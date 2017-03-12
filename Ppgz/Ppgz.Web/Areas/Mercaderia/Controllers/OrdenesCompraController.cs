using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Ppgz.Web.Infrastructure;
using Ppgz.Web.Infrastructure.Nazan;
using Ppgz.Web.Infrastructure.Proveedor;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class OrdenesCompraController : Controller
    {
        private readonly PerfilProveedorManager _perfilProveedorManager = new PerfilProveedorManager();
        private readonly CommonManager _commonManager = new CommonManager();

        //
        // GET: /Mercaderia/OrdenesCompra/
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-ORDENESCOMPRA-LISTAR,MERCADERIA-ORDENESCOMPRA-MODIFICAR")]
        public ActionResult Index()
        {
            var perfiles = _perfilProveedorManager
                .FindByCuentaId(_commonManager.GetCuentaUsuarioAutenticado().Id);

            //perfiles.Add(_perfilProveedorManager.GetMaestroByUsuarioTipo(CuentaManager.Tipo.MERCADERIA));

            ViewBag.Perfiles = perfiles;
            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-ORDENESCOMPRA-LISTAR,MERCADERIA-ORDENESCOMPRA-MODIFICAR")]
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