using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Ppgz.Web.Infrastructure;
using Ppgz.Web.Infrastructure.Nazan;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
    [Authorize]
    [TerminosCondiciones]
	public class MensajesInstitucionalesController : Controller
	{
		private readonly CommonManager _commonManager = new CommonManager();
		private readonly MensajesInstitucionalesManager _mensajesInstitucionalesManager = new MensajesInstitucionalesManager();


        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-MENSAJESINSTITUCIONALES")]
		public ActionResult Index()
		{
			//CUENTAS
			var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

			var mensajes = _mensajesInstitucionalesManager.FindPublicadosByCuentaId(cuenta.Id);

			var mensajesUsuario = _mensajesInstitucionalesManager.FindCuentaMensajes(cuenta.Id);

			ViewBag.mensajes = mensajes;

			ViewBag.mensajesUsuario = Json(mensajesUsuario);

			return View();
		}

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-MENSAJESINSTITUCIONALES")]
		[HttpPost]
		public ActionResult Visualizar(int id)
        {
            //CUENTAS
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

			var mensajes = _mensajesInstitucionalesManager.FindCuentaMensajes(cuenta.Id);

			if (mensajes.Any(i => i.MensajeId == id))
			{
				return Content("Actualizado"); 
			}

			_mensajesInstitucionalesManager.Visualizar(cuenta.Id, id, User.Identity.GetUserId());

			return Content("Actualizado");
		}

		
	}
}