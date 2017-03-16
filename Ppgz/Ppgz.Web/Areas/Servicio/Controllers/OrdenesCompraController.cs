using System.Web.Mvc;
using Ppgz.Services;
using Ppgz.Web.Infrastructure;

namespace Ppgz.Web.Areas.Servicio.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class OrdenesCompraController : Controller
    {
        private readonly PerfilManager _perfilProveedorManager = new PerfilManager();
        private readonly CommonManager _commonManager = new CommonManager();

        //
        // GET: /Mercaderia/OrdenesCompra/
        [Authorize(Roles = "MAESTRO-SERVICIO,SERVICIO-ORDENESCOMPRA-LISTAR,SERVICIO-ORDENESCOMPRA-MODIFICAR")]
        //
        // GET: /Servicio/OrdenesCompra/
        public ActionResult Index()
        {
            var perfiles = _perfilProveedorManager
                .FindPerfilProveedorByCuentaId(_commonManager.GetCuentaUsuarioAutenticado().Id);

            //perfiles.Add(_perfilProveedorManager.GetMaestroByUsuarioTipo(CuentaManager.Tipo.MERCADERIA));

            ViewBag.Perfiles = perfiles;
            return View();
        }
        [Authorize(Roles = "MAESTRO-SERVICIO,SERVICIO-ORDENESCOMPRA-LISTAR,SERVICIO-ORDENESCOMPRA-MODIFICAR")]
        [HttpPost]
        public ActionResult Visualizar(int id)
        {
            //var mensajes = _mensajesInstitucionalesManager.FindUsuarioMensajes(User.Identity.GetUserId());

            //if (mensajes.Any(i => i.MensajeId == id))
            //{
            //    return Content("Actualizado"); ;
            //}
            //_mensajesInstitucionalesManager.Visualizar(User.Identity.GetUserId(), id);
            return Content("Actualizado");
        }
	}
}