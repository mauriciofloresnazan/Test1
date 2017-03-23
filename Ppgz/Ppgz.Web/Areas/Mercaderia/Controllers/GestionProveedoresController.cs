﻿using System;
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

        //
        // GET: /Mercaderia/GestionProveedores/
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
                    proveedor.NumeroProveedor,
                    proveedor.NumeroTelefono, proveedor.Correo
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