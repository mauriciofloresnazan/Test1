using System;
using System.Data.Entity;
using System.Web.Mvc;
using Ppgz.Repository;
using Ppgz.Web.Areas.Nazan.Models;
using Ppgz.Web.Infrastructure;
using Ppgz.Web.Infrastructure.Nazan;

namespace Ppgz.Web.Areas.Nazan.Controllers
{
	[Authorize]
	public class AdministrarProveedoresController : Controller
	{
		readonly CuentaManager _cuentaManager = new CuentaManager();
		//
		// GET: /Nazan/AdministrarProveedores/
		[Authorize(Roles = "SUPERADMIN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-LISTAR,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
		public ActionResult Index()
		{
			var cuentas = _cuentaManager.FinAll();

			ViewBag.cuentas = cuentas;
			return View();
		}

		[Authorize(Roles = "SUPERADMIN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
		public ActionResult Registrar()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "SUPERADMIN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
		
		public ActionResult Registrar(CuentaViewModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					_cuentaManager.Create(
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


		[Authorize(Roles = "SUPERADMIN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
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

		
		[Authorize(Roles = "SUPERADMIN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
		public ActionResult ModificarUsuarioMaestro(int id, string usuarioId)
		{
			var entities = new Entities();
			var cuenta = entities.cuentas.Find(id);
			var usuario = entities.aspnetusers.Find(usuarioId);
			
			var perfilProveedorManager = new PerfilProveedorManager();
			var perfilMaestro = perfilProveedorManager.GetMaestroByUsuarioTipo(CuentaManager.GetTipoByString(cuenta.Tipo));
			usuario.PerfilId = perfilMaestro.Id;

			entities.Entry(usuario).State = EntityState.Modified;
			entities.SaveChanges();

			cuenta.ResponsableUsuarioId = usuario.Id;
			entities.Entry(cuenta).State = EntityState.Modified;
			entities.SaveChanges();

			TempData["FlashSuccess"] = "Cuenta Actualizada con éxito.";
			return RedirectToAction("Editar", new { id = id });
		}


        [Authorize(Roles = "SUPERADMIN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
        public JsonResult BuscarProveedor(string numeroProveedor)
        {
            var sapManager = new SapManager();

            try
            {
                var proveedor = sapManager.GetProveedor(numeroProveedor);
                return Json(proveedor); ;

            }
            catch (Exception exception)
            {
                return Json(new { error = exception.Message}) ;
            }

        }

	}
}