using System;
using System.Data.Entity.Infrastructure;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Ppgz.Web.Areas.Nazan.Models;
using Ppgz.Web.Infrastructure;
using Ppgz.Web.Infrastructure.Nazan;
using Ppgz.Web.Models;

namespace Ppgz.Web.Areas.Nazan.Controllers
{
    [Authorize]
	public class AdministrarUsuariosNazanController : Controller
	{
		
		private readonly UsuarioNazanManager _usuarioNazanManager = new UsuarioNazanManager();

		private readonly PerfilNazanManager _perfilNazanManager = new PerfilNazanManager();
		

		//
		// GET: /Nazan/
		[Authorize(Roles = "SUPERADMIN,NAZAN-ADMINISTRARUSUARIOSNAZAN-LISTAR,NAZAN-ADMINISTRARUSUARIOSNAZAN-MODIFICAR")]
		public ActionResult Index()
		{
			ViewBag.Usuarios = _usuarioNazanManager.FindAll();

			return View();
		}

		[Authorize(Roles = "SUPERADMIN,NAZAN-ADMINISTRARUSUARIOSNAZAN-MODIFICAR")]
		public ActionResult Crear()
		{
			ViewBag.Perfiles = 
				new SelectList(_perfilNazanManager.FindAll(), "Id", "Nombre"); ;

			return View();
		}

		[Authorize(Roles = "SUPERADMIN,NAZAN-ADMINISTRARUSUARIOSNAZAN-MODIFICAR")]
		[ValidateAntiForgeryToken]
		[HttpPost]
		public ActionResult Crear(UsuarioNazanViewModel model)
		{
			ViewBag.Perfiles =
				  new SelectList(_perfilNazanManager.FindAll(), "Id", "Nombre"); ;

			if (!ModelState.IsValid) return View(model);

			try
			{
				_usuarioNazanManager
					.Create(model.UserName,model.Nombre,model.Apellido,
					model.Email,model.Password, model.Perfil);

				TempData["FlashSuccess"] = "Usuario creado con éxito.";
				return RedirectToAction("Index");
			}
			catch (Exception exception)
			{

				ModelState.AddModelError(string.Empty, exception.Message);

				return View(model);
			}
	
		}

		[Authorize(Roles = "SUPERADMIN,NAZAN-ADMINISTRARUSUARIOSNAZAN-MODIFICAR")]
		public ActionResult Editar(string id)
		{
			ViewBag.Perfiles =
				  new SelectList(_perfilNazanManager.FindAll(), "Id", "Nombre"); ;

			var usuario = _usuarioNazanManager.Find(id);

			if (usuario == null)
			{
				//TODO ACTUALIZAR MENSAJE AL RESOURCE
				TempData["FlashError"] = "Usuario incorrecto.";
				return RedirectToAction("Index");
			}

			var usuarioNazanViewModel = new UsuarioNazanViewModel()
			{
				UserName = usuario.UserName,
				Nombre = usuario.Nombre,
				Apellido = usuario.Apellido,
				Email = usuario.Email,
				Perfil = usuario.PerfilId
			};

			return View(usuarioNazanViewModel);
		}

		[Authorize(Roles = "SUPERADMIN,NAZAN-ADMINISTRARUSUARIOSNAZAN-MODIFICAR")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Editar(string id, UsuarioNazanViewModel model)
		{
			var usuario = _usuarioNazanManager.Find(id);

			if (usuario == null)
			{
				//TODO ACTUALIZAR MENSAJE AL RESOURCE
				TempData["FlashError"] = "Usuario incorrecto.";
				return RedirectToAction("Index");
			}

			try
			{
				_usuarioNazanManager.Update(
					id,
					model.Nombre,
					model.Apellido,
                    model.Email,
                    model.Perfil,
					model.Password);

				TempData["FlashSuccess"] = "Usuario actualizado con éxito.";
				return RedirectToAction("Index");
			}
			catch (RetryLimitExceededException)
			{
				ModelState.AddModelError("", ResourceErrores.RegistroGeneral);
			}

			ViewBag.Perfiles =
				  new SelectList(_perfilNazanManager.FindAll(), "Id", "Nombre"); ;

			return View(model);
		}

		[Authorize(Roles = "SUPERADMIN,NAZAN-ADMINISTRARUSUARIOSNAZAN-MODIFICAR")]
		public ActionResult Eliminar(string id)
		{
			var usuario = _usuarioNazanManager.Find(id);

			if (usuario == null)
			{
				//TODO ACTUALIZAR MENSAJE AL RESOURCE
				TempData["FlashError"] = "Usuario incorrecto.";
				return RedirectToAction("Index");
			}

			if (usuario.Id == User.Identity.GetUserId())
			{
				//TODO ACTUALIZAR MENSAJE AL RESOURCE
				TempData["FlashError"] = "No puede eliminar su propio usuario.";
				return RedirectToAction("Index");
			}

			if (usuario.UserName.ToLower() == "superadmin")
			{

				//TODO ACTUALIZAR MENSAJE AL RESOURCE
				TempData["FlashError"] = "No puede eliminar el usuario SuperAdmin.";
				return RedirectToAction("Index");
			}

			try
			{

				_usuarioNazanManager.Remove(id);
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

			TempData["FlashSuccess"] = "Usuario eliminado con éxito.";
			return RedirectToAction("Index");
		}
	}
}