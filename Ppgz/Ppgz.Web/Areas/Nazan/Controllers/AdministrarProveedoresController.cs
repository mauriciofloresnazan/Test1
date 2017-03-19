﻿using System;
using System.Web.Mvc;
using Ppgz.Services;
using Ppgz.Web.Areas.Nazan.Models;
using Ppgz.Web.Infrastructure;

namespace Ppgz.Web.Areas.Nazan.Controllers
{
	[Authorize]
	public class AdministrarProveedoresController : Controller
	{
		readonly CuentaManager _cuentaManager = new CuentaManager();
		readonly ProveedorManager _proveedorManager = new ProveedorManager();
		//
		// GET: /Nazan/AdministrarProveedores/
		[Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-LISTAR,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
		public ActionResult Index()
		{
			var cuentas = _cuentaManager.FindAllWithUsuarioMaestro();

			ViewBag.cuentas = cuentas;
			return View();
		}

		[Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
		public ActionResult Registrar()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
		
		public ActionResult Registrar(CuentaViewModel model)
		{
            if (!ModelState.IsValid) return View(model);
            try
            {
                _cuentaManager.Crear(
                                    model.TipoCuenta,
                                    model.NombreCuenta,
                                    model.UserName,
                                    model.ResponsableNombre,
                                    model.ResponsableApellido,
                                    model.ResponsableCargo,
                                    model.ResponsableEmail,
                                    model.ResponsableTelefono,
                                    model.ResponsablePassword);

                TempData["FlashSuccess"] = MensajesResource.INFO_Cuenta_CreadaCorrectamente;
                return RedirectToAction("Index");
            }
            catch (BusinessException businessEx)
            {
                ModelState.AddModelError(string.Empty, businessEx.Message);

                return View(model);
            }
            catch (Exception e)
            {
                var log = CommonManager.BuildMessageLog(
                    TipoMensaje.Error,
                    ControllerContext.Controller.ValueProvider.GetValue("controller").RawValue.ToString(),
                    ControllerContext.Controller.ValueProvider.GetValue("action").RawValue.ToString(),
                    e.ToString(), Request);

                CommonManager.WriteAppLog(log, TipoMensaje.Error);

                return View(model);
            }
		}

		[Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
		public ActionResult Editar(int id)
		{
            var cuentaConUsuarioMaestro = _cuentaManager.FindWithUsuarioMaestro(id);
			if (cuentaConUsuarioMaestro == null)
			{
				TempData["FlashError"] = MensajesResource.ERROR_Cuenta_IdIncorrecto;
				return RedirectToAction("Index");
			}

            ViewBag.cuentaConUsuarioMaestro = cuentaConUsuarioMaestro;
			return View();
		}

		[Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
		public JsonResult BuscarProveedor(string numeroProveedor)
		{
		    try
		    {
                var proveedor = _proveedorManager.FindProveedorEnSap(numeroProveedor);
                return proveedor == null ? Json(new { error = MensajesResource.ERROR_Proveedor_IdIncorrecto }) : Json(proveedor);
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


		[Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
		public ActionResult AsociarProveedor(int id, string numeroProveedor)
		{
			try
            {
                _cuentaManager.AsociarProveedorSapEnCuenta(id, numeroProveedor);

				TempData["FlashSuccess"] = "Proveedor asociado con éxito.";
				return RedirectToAction("Editar", new {  id });
			}
            catch (BusinessException businessEx)
            {
                TempData["FlashError"] = businessEx.Message;
                return RedirectToAction("Editar", new { id });

            }
            catch (Exception e)
            {
                var log = CommonManager.BuildMessageLog(
                    TipoMensaje.Error,
                    ControllerContext.Controller.ValueProvider.GetValue("controller").RawValue.ToString(),
                    ControllerContext.Controller.ValueProvider.GetValue("action").RawValue.ToString(),
                    e.ToString(), Request);

                CommonManager.WriteAppLog(log, TipoMensaje.Error);

                TempData["FlashError"] = MensajesResource.ERROR_General;
                return RedirectToAction("Editar", new { id });
            }

		}
		[Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
		public ActionResult Eliminar(int id)
        {
            try
            {
                _proveedorManager.Eliminar(id);

                TempData["FlashSuccess"] = MensajesResource.INFO_Proveedor_EliminadoCorrectamente;
                return RedirectToAction("Index");
            }
            catch (BusinessException businessEx)
            {
                TempData["FlashError"] = businessEx.Message;
                return RedirectToAction("Index");

            }
            catch (Exception e)
            {
                var log = CommonManager.BuildMessageLog(
                    TipoMensaje.Error,
                    ControllerContext.Controller.ValueProvider.GetValue("controller").RawValue.ToString(),
                    ControllerContext.Controller.ValueProvider.GetValue("action").RawValue.ToString(),
                    e.ToString(), Request);

                CommonManager.WriteAppLog(log, TipoMensaje.Error);

                TempData["FlashError"] = MensajesResource.ERROR_General;
                return RedirectToAction("Index");
            }
			
		}
		
		public ActionResult DesasociarProveedor(int cuentaId, int proveedorId)
		{
			// TODO MEJORAR
			try
			{
                _cuentaManager.EliminarProveedorEnCuenta(cuentaId, proveedorId);


				TempData["FlashSuccess"] = "Proveedor desvinculado con éxito.";
                return RedirectToAction("Editar", new { id = cuentaId });
			}
			catch (Exception exception)
			{
				TempData["FlashError"] = exception.Message;
                return RedirectToAction("Editar", new { id = cuentaId });
			}

		}

	}
}