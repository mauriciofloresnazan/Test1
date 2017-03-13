using System;
using System.Collections;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Ppgz.Repository;
using Ppgz.Web.Areas.Nazan.Models;
using Ppgz.Web.Infrastructure;
using Ppgz.Web.Infrastructure.Proveedor;

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
			var cuentas = _cuentaManager.FinAll();

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
			if (ModelState.IsValid)
			{
				try
				{
					_cuentaManager.Crear(
						model.ProveedorNombre,
						CuentaManager.GetTipoByString(model.TipoProveedor),
						model.UserName,
						model.ResponsableNombre,
						model.ResponsableApellido,
						model.ResponsableCargo,
						model.ResponsableEmail,
						model.ResponsableTelefono,
						model.ResponsablePassword);

					// TODO JUAN DELGADO
					TempData["FlashSuccess"] = "Cuenta Creada con éxito.";
					return RedirectToAction("Index");
				}
				catch (Exception exception)
				{

					ModelState.AddModelError(string.Empty, exception.Message);

					return View(model);
				}

			}

			return View(model);

		}


		[Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
		public ActionResult Editar(int id)
		{
			var cuenta = _cuentaManager.Find(id);

			if (cuenta == null)
			{
				//TODO ACTUALIZAR MENSAJE AL RESOURCE
				TempData["FlashError"] = "Proveedor incorrecto.";
				return RedirectToAction("Index");
			}

			ViewBag.Cuenta = cuenta;
			return View();
		}


		[Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
		public ActionResult ModificarUsuarioMaestro(int id, string usuarioId)
		{
			var entities = new Entities();
			var cuenta = entities.cuentas.Find(id);
			var usuario = entities.AspNetUsers.Find(usuarioId);
			
			var perfilProveedorManager = new PerfilProveedorManager();
			var perfilMaestro = perfilProveedorManager.GetMaestroByUsuarioTipo(CuentaManager.GetTipoByString(cuenta.Tipo));
			usuario.PerfilId = perfilMaestro.Id;

			entities.Entry(usuario).State = EntityState.Modified;
			entities.SaveChanges();

			cuenta.ResponsableUsuarioId = usuario.Id;
			entities.Entry(cuenta).State = EntityState.Modified;
			entities.SaveChanges();

			TempData["FlashSuccess"] = "Cuenta Actualizada con éxito.";
			return RedirectToAction("Editar", new { id });
		}


		[Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
		public JsonResult BuscarProveedor(string numeroProveedor)
		{
			try
			{
				var proveedor = _proveedorManager.FindByCodigoProveedor(numeroProveedor);

				var table = new Hashtable();
				table["Codigo"] = proveedor.CodigoProveedor;
				table["RFC"] = proveedor.Rfc;
				table["Nombre"] = proveedor.NombreProveedor;
				table["Cidudad"] = proveedor.Ciudad;
				table["Estado"] = proveedor.Estado;
				table["Email"] = proveedor.email;
				table["Direccion 1"] = proveedor.direccion1;
				table["Dirección 2"] = proveedor.direccion2;
				table["Dirección 3"] = proveedor.direccion3;
				return Json(table);
			}
			catch (Exception exception)
			{
				return Json(new { error = exception.Message });
			}
		}


		[Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
		public ActionResult AsociarProveedor(int id, string numeroProveedor)
		{
			try
			{
				var entities = new Entities();

				var proveedor = entities.proveedores.SingleOrDefault(a => a.CodigoProveedor == numeroProveedor); ;

				var cuenta = _cuentaManager.Find(id);

				proveedor.CuentaId = cuenta.Id;

				entities.Entry(proveedor).State  = EntityState.Modified;
				entities.SaveChanges();

				TempData["FlashSuccess"] = "Proveedor asociado con éxito.";
				return RedirectToAction("Editar", new { id = id });
			}
			catch (Exception exception)
			{
				TempData["FlashError"] = exception.Message;
				return RedirectToAction("Editar", new { id = id });
			}

		}
		[Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
		public ActionResult Eliminar(int id)
		{

			//TODO ACTUALIZAR MENSAJE AL RESOURCE
			TempData["FlashError"] = "En revision";
			return RedirectToAction("Index");
			
		}
		
		public ActionResult DesasociarProveedor(int id)
		{
			// TODO MEJORAR


			var entities = new Entities();
			var proveedor = entities.proveedores.SingleOrDefault(a => a.Id == id);

			var cuentaId = proveedor.CuentaId; 
			try
			{
				proveedor.CuentaId = null;
				
				entities.Entry(proveedor).State = EntityState.Modified ;
				entities.SaveChanges();

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