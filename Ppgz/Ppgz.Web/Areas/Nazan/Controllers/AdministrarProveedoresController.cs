using System;
using System.Web.Mvc;
using Ppgz.Repository;
using Ppgz.Web.Areas.Nazan.Models;
using Ppgz.Web.Infrastructure;
using Ppgz.Web.Infrastructure.Nazan;
using Ppgz.Web.Models;

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
	}
}