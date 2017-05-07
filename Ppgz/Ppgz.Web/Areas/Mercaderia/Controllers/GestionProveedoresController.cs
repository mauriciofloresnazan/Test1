using System;
using System.Web.Mvc;
using Ppgz.Services;
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


        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-GESTIONPROVEEDORES-LISTAR")]
        public ActionResult Index()
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();
            var proveedores = _proveedorManager.FindByCuentaId(cuenta.Id);
            var mensajes = _mensajesInstitucionalesManager.FindPublicadosSinLeerByCuentaId(cuenta.Id);
            var cuentaConUsuarioMaestro = _cuentaManager.FindWithUsuarioMaestro(cuenta.Id);

            ViewBag.Proveedores = proveedores;
            ViewBag.Mensajes = mensajes;
            ViewBag.CuentaConUsuarioMaestro = cuentaConUsuarioMaestro;

            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-GESTIONPROVEEDORES-LISTAR")]
        public JsonResult BuscarProveedor(int id)
        {
            try
            {
                // TODO cambiar en manejador para buscar por proveedor id y cuenta id
                var proveedor = _proveedorManager.Find(id);
                // TODO PASAR MENSAJE AL RESOURCE

                return proveedor == null ? Json(new { error = "Prvoeedor incorrecto" }) : Json(new
                {
                    proveedor.Rfc,
                    Nombre = string.Format
                        ("{0} {1} {2} {3}",
                            proveedor.Nombre1,
                            proveedor.Nombre2,
                            proveedor.Nombre3,
                            proveedor.Nombre4),
                    Teléfono = proveedor.NumeroTelefono,
                    Email = proveedor.Correo,
                    Vendedor = proveedor.VendedorResponsable,
                    proveedor.Region,
                    proveedor.Poblacion,
                    proveedor.Apartado,
                    proveedor.Distrito,
                    proveedor.CodigoPostal,
                    Direccion = proveedor.Calle,
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