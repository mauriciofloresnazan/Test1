using System;
using System.Data.Entity.Infrastructure;
using System.Web.Mvc;
using Ppgz.Web.Areas.Nazan.Models;
using Ppgz.Web.Infrastructure.Nazan;

namespace Ppgz.Web.Areas.Nazan.Controllers
{
	[Authorize]
	public class AdministrarPerfilesNazanController : Controller
	{
		private readonly PerfilNazanManager  _perfilNazanManager = new PerfilNazanManager();
		
		//
		// GET: /Nazan/AdministrarPerfilesNazan/
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPERFILESNAZAN-LISTAR,NAZAN-ADMINISTRARPERFILESNAZAN-MODIFICAR")]
		public ActionResult Index()
		{
			ViewBag.Perfiles = _perfilNazanManager.FindAll();

			return View();
		}
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPERFILESNAZAN-MODIFICAR")]
		public ActionResult Crear()
		{
			
			var roles = _perfilNazanManager.GetRoles();
			
			var model = new PefilNazanViewModel
			{
				Roles = new MultiSelectList(roles, "Id", "Name")
			};
			return View(model);
		}

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPERFILESNAZAN-MODIFICAR")]
		[ValidateAntiForgeryToken]
		[HttpPost]
		public ActionResult Crear(PefilNazanViewModel model)
		{
			var roles = _perfilNazanManager.GetRoles();
			model.Roles = new MultiSelectList(roles, "Id", "Name");
		
			if (!ModelState.IsValid) return View(model);

			if (_perfilNazanManager.FindByNombre(model.Nombre.Trim()) != null)
			{
				ModelState.AddModelError(string.Empty, Errores.PerfilNazanNombreExistente);
				return View(model);
			}

			_perfilNazanManager
				.Create(model.Nombre, model.RolesIds);

		

			TempData["FlashSuccess"] = "Perfil creado con éxito.";
			return RedirectToAction("Index");
		}

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPERFILESNAZAN-MODIFICAR")]
		public ActionResult Editar(int id)
		{
			var perfil = _perfilNazanManager.Find(id);

			if (perfil == null)
			{
			//TODO ACTUALIZAR MENSAJE AL RESOURCE
			TempData["FlashError"] = "Perfil incorrecto.";
			return RedirectToAction("Index");
				
			}


			var roles = _perfilNazanManager.GetRoles();
			
			var model = new PefilNazanViewModel
			{
				Nombre = perfil.Nombre,
				Roles = new MultiSelectList(roles, "Id", "Name")
				
			};

			return View(model);
		}


        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPERFILESNAZAN-MODIFICAR")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Editar(int id, PefilNazanViewModel model)
		{
			
			var perfil = _perfilNazanManager.Find(id);

			if (perfil == null)
			{
			//TODO ACTUALIZAR MENSAJE AL RESOURCE
			TempData["FlashError"] = "Perfil incorrecto.";
			return RedirectToAction("Index");
				
			}

			try
			{
				_perfilNazanManager.Update(
					id,
					model.Nombre,
					model.RolesIds);

				TempData["FlashSuccess"] = "Perfil actualizado con éxito.";
				return RedirectToAction("Index");
			}
			catch (RetryLimitExceededException)
			{
				ModelState.AddModelError("", ResourceErrores.RegistroGeneral);
			}
			catch (Exception exception)
			{
				//TODO ACTUALIZAR MENSAJE AL RESOURCE
				TempData["FlashError"] = exception.Message;
				return RedirectToAction("Index");
			}            
			var roles = _perfilNazanManager.GetRoles();
			
			model.Roles = new MultiSelectList(roles, "Id", "Name");

			return View(model);
		}

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPERFILESNAZAN-MODIFICAR")]
		public ActionResult Eliminar(int id)
		{
	  
			var perfil = _perfilNazanManager.Find(id);

			if (perfil == null)
			{
				//TODO ACTUALIZAR MENSAJE AL RESOURCE
				TempData["FlashError"] = "Perfil incorrecto.";
				return RedirectToAction("Index");
			}
			
			try
			{

				_perfilNazanManager.Remove(id);
			}
			catch (RetryLimitExceededException)
			{
				//TODO ACTUALIZAR MENSAJE AL RESOURCE
				TempData["FlashError"] = "INTENTOS EXEDIDOS";
				return RedirectToAction("Index");
			}
			catch (Exception exception)
			{
				//TODO ACTUALIZAR MENSAJE AL RESOURCE
				TempData["FlashError"] = exception.Message;
				return RedirectToAction("Index");
			}

			TempData["FlashSuccess"] = "Perfil eliminado con éxito.";
			return RedirectToAction("Index");
		}
	}
}