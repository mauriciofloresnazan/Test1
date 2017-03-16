using Ppgz.Web.Infrastructure;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Ppgz.Services;

namespace Ppgz.Web
{
	public class MvcApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
			//Configuración de log4net
			log4net.Config.XmlConfigurator.Configure();
		}

		protected void Application_Error(object sender, EventArgs e)
		{
			HttpContext httpContext = ((MvcApplication)sender).Context;
			string currentController = string.Empty;
			string currentAction = string.Empty;
			RouteData currentRouteData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));
			if (currentRouteData != null)
			{
				if (currentRouteData.Values["controller"] != null && !string.IsNullOrEmpty(currentRouteData.Values["controller"].ToString()))
					currentController = currentRouteData.Values["controller"].ToString();
				if (currentRouteData.Values["action"] != null && !string.IsNullOrEmpty(currentRouteData.Values["action"].ToString()))
					currentAction = currentRouteData.Values["action"].ToString();
			}
			Exception ex = Server.GetLastError();
			RouteData routeData = new RouteData();
			string action = string.Empty;
			if (ex is HttpException)
			{
				HttpException httpEx = ex as HttpException;
				switch (httpEx.GetHttpCode())
				{
					case 404:
						action = "NotFound";
						break;
					case 500:
						action = "InternalServer";
						break;
					case 403:
					case 401:
						action = "NotAuthorize";
						break;
					default:
						action = "GenericError";
						break;
				}
			}
			else
			{
				action = "GenericError";
				if (ex is BusinessException)
				{
					CommonManager.WriteBusinessLog(CommonManager.BuildMessageLog(TipoMensaje.Error, currentController, currentAction, ex.ToString()), TipoMensaje.Error);
				}
				if (ex is Exception)
				{
					CommonManager.WriteAppLog(CommonManager.BuildMessageLog(TipoMensaje.Error, currentController, currentAction, ex.ToString()), TipoMensaje.Error);
				}
			}
			httpContext.ClearError();
			httpContext.Response.Clear();
			httpContext.Response.StatusCode = ex is HttpException ? ((HttpException)ex).GetHttpCode() : 0;
			httpContext.Response.TrySkipIisCustomErrors = true;
			routeData.Values["controller"] = "Error";
			routeData.Values["action"] = action;
			routeData.Values["exception"] = new HandleErrorInfo(ex, currentController, currentAction);
			IController CommonManagerController = new Controllers.ErrorController();
			HttpContextWrapper wrapper = new HttpContextWrapper(httpContext);
			RequestContext rc = new RequestContext(wrapper, routeData);
			CommonManagerController.Execute(rc);
		}
	}
}
