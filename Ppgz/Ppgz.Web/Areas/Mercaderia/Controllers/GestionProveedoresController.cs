using System;
using System.Linq;
using System.Web.Mvc;
using DocumentFormat.OpenXml.Drawing.Charts;
using Ppgz.Services;
using Ppgz.Web.Areas.Mercaderia.Models;
using Ppgz.Web.Infrastructure;
using Ppgz.Web.Infrastructure.Nazan;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class GestionProveedoresController : Controller
    {
        private readonly CommonManager _commonManager = new CommonManager();
        private readonly CuentaManager _cuentaManager = new CuentaManager();
        private readonly ProveedorManager _proveedorManager = new ProveedorManager();
        private readonly MensajesInstitucionalesManager _mensajesInstitucionalesManager = new MensajesInstitucionalesManager();

        //
        // GET: /Mercaderia/GestionProveedores/
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-GESTIONPROVEEDORES-LISTAR")]
        public ActionResult Index()
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            var proveedores = _proveedorManager.FindByCuentaId(cuenta.Id);
            var mensajes = _mensajesInstitucionalesManager.FindPublicadosByCuentaId(cuenta.Id);
            //var datosCuenta = _cuentaManager.Find(cuenta.Id);

            ViewBag.Proveedores = proveedores;
            ViewBag.Mensajes = mensajes;
            ViewBag.DatosCuenta = cuenta;

            return View();
        }

        public JsonResult BuscarProveedor(int id)
        {
            try
            {
                // TODO cambiar en manejador para buscar por proveedor id y cuenta id
                var proveedor = _proveedorManager.Find(id);
                // TODO PASAR MENSAJE AL RESOURCE



                return proveedor == null ? Json(new { error = "Prvoeedor incorrecto" }) : Json(new
                {
                    RFC = proveedor.Rfc,
                    Nombre = proveedor.Nombre1,
                    Direccion = proveedor.Region,
                    Contacto = proveedor.Nombre1,
                    NumeroProveedor = proveedor.NumeroProveedor,
                    NumeroTelefono = proveedor.NumeroTelefono,
                    Correo = proveedor.Correo
                });
            }
            catch (BusinessException businessEx)
            {
                return Json(new { error = businessEx.Message });
            }
            catch (Exception e)
            {
                var log = CommonManager.BuildMessageLog(
                    TipoMensaje.Error,
                    ControllerContext.Controller.ValueProvider.GetValue("controller").RawValue.ToString(),
                    ControllerContext.Controller.ValueProvider.GetValue("action").RawValue.ToString(),
                    e.ToString(), Request);

                CommonManager.WriteAppLog(log, TipoMensaje.Error);
                return Json(new { error = e.Message });
            }
        }
    }
}