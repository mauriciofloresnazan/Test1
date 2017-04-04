using System.Web.Mvc;

namespace Ppgz.Web.Controllers
{
	public class ErrorController : Controller
	{
		[Authorize]
		public ActionResult Index()
		{
			return RedirectToAction("NotAuthorizeErrorPage", "Error");
		}

		/// <summary> Pantalla de error genérica.</summary>
		/// <param name="exception">Indica el tipo de error ocurrido</param>
		/// <returns>Vista de Error</returns>
		[AllowAnonymous]
		public ViewResult GenericError(HandleErrorInfo exception)
		{
			ViewBag.Title = "Error desconocido";
			ViewData["Content"] = "Ha ocurrido un error inesperado. Por favor, póngase en contacto con el administrador del sistema.";
			return View("Error", exception);
		}

		/// <summary> Pantalla de error, en este caso se indica al usuario que hay un error 404.</summary>
		/// <param name="exception">Indica el tipo de error ocurrido (404)</param>
		/// <returns>Vista de error</returns>
		[Authorize]
		public ActionResult NotFound(HandleErrorInfo exception)
		{
			/*ViewBag.Title = "Página no encontrada";
			ViewData["Content"] = "La página solicitada no existe";
			return View("Error", exception);
            */
            TempData["FlashError"] = "La página solicitada no existe";
                
            return RedirectToAction("Index", "Home");
		}

		/// <summary> Pantalla de error, en este caso se indica al usuario que hay un error 500.</summary>
		/// <param name="exception">Indica el tipo de error ocurrido (500)</param>
		/// <returns>Vista de error</returns>
		[Authorize]
		public ViewResult InternalServer(HandleErrorInfo exception)
		{
			ViewBag.Title = "Internal Server";
			ViewData["Content"] = "Ha ocurrido un error inesperado. Por favor, póngase en contacto con el administrador del sistema.";
			return View("Error", exception);
		}

		/// <summary> Pantalla de error, en este caso se indica al usuario que no tiene acceso al módulo.</summary>
		/// <returns>Vista de error</returns>
		[Authorize]
		public ActionResult NotAuthorize()
		{
			ViewBag.Title = "Acceso Denegado";
			ViewData["Content"] = "No tienes acceso a esta página.";
			return View("Error");
		}

		/// <summary> Redirecciona a la pantalla en caso de que se acceda a este método.</summary>
		/// <returns>Pantalla Principal de la aplicación</returns>
		[Authorize]
		public ActionResult NotAuthorizeErrorPage()
		{
			return RedirectToAction("Index", "Home");
		}
	}
}
