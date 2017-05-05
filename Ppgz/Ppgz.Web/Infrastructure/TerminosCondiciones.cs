using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace Ppgz.Web.Infrastructure
{
    public class TerminosCondiciones: ActionFilterAttribute  
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        
        var commonManager = new CommonManager();

        var usuario = commonManager.GetUsuarioAutenticado();
        if (usuario.Tipo == "PROVEEDOR" || usuario.Tipo == "MAESTRO-PROVEEDOR")
        if (usuario.TerminosCondicionesFecha == null)
        {
            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Politicas", action = "Index", area = "" }));
        }

        base.OnActionExecuting(filterContext);
    }
}
 
}