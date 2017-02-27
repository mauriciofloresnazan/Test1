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

			var mensajesUsuario = _mensajesInstitucionalesManager.FindUsuarioMensajes(User.Identity.GetUserId());

			ViewBag.mensajes = mensajes;

			ViewBag.mensajesUsuario = Json(mensajesUsuario);

			return View();
		}

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-MENSAJESINSTITUCIONALES")]
		[HttpPost]
		public ActionResult Visualizar(int id)
		{
			var mensajes = _mensajesInstitucionalesManager.FindUsuarioMensajes(User.Identity.GetUserId());

			if (mensajes.Any(i => i.MensajeId == id))
			{
				return Content("Actualizado"); ;
			}

			_mensajesInstitucionalesManager.Visualizar(User.Identity.GetUserId(), id);

			return Content("Actualizado");
		}

		
	}
}