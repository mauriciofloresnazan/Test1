using System;
using System.Data.Entity.Infrastructure;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Ppgz.Web.Infrastructure;
using Ppgz.Web.Infrastructure.Nazan;
using Ppgz.Web.Models;
using Ppgz.Web.Models.Nazan;
using UsuarioManager = Ppgz.Web.Infrastructure.Nazan.UsuarioManager;

namespace Ppgz.Web.Controllers
{
	[Authorize]
	public class NazanController : Controller
	{
		readonly UserManager<ApplicationUser> _applicationUserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

		private readonly PerfilManager _perfilManager = new PerfilManager();

		private readonly UsuarioManager _usuarioManager = new UsuarioManager();


		[Authorize(Roles = "NAZAN-ADMINISTRARPERFILESNAZAN-LISTAR,NAZAN-ADMINISTRARPERFILESNAZAN-MODIFICAR")]
		public ActionResult AdministrarPerfilesNazan()
		{
			ViewBag.Perfiles = _perfilManager.FindAll();

			return View();
		}

		//
		// GET: /Nazan/
		[Authorize(Roles = "NAZAN-ADMINISTRARUSUARIOSNAZAN-LISTAR,NAZAN-ADMINISTRARUSUARIOSNAZAN-MODIFICAR")]
		public ActionResult AdministrarUsuariosNazan()
		{
			ViewBag.Usuarios = _usuarioManager.FindAll();

			return View();
		}

		[Authorize(Roles = "NAZAN-ADMINISTRARUSUARIOSNAZAN-MODIFICAR")]
		public ActionResult CrearUsuarioNazan()
		{
			//TODO
			ViewBag.Perfiles = new SelectList(new[]{
					"POR DEFINIR",
				});

			return View();
		}

		[Authorize(Roles = "NAZAN-ADMINISTRARUSUARIOSNAZAN-MODIFICAR")]
		[ValidateAntiForgeryToken]
		[HttpPost]
		public ActionResult CrearUsuarioNazan(UsuarioViewModel model)
		{
			//TODO
			ViewBag.Perfiles = new SelectList(new[]{
					"POR DEFINIR",
				});

			if (!ModelState.IsValid) return View(model);

			if (_applicationUserManager.FindByName(model.UserName) != null)
			{
				ModelState.AddModelError(string.Empty, ResourceErrores.NombreUsuarioExistente);
				return View(model);
			}

			var usuario = new ApplicationUser()
			{
				UserName = model.UserName,
				Nombre = model.Nombre,
				Apellido = model.Apellido,
				Email = model.Email,
				Tipo = TipoUsuario.Nazan,
				Activo = true
			};

			var result = _applicationUserManager.Create(usuario, model.Password);

			if (result.Succeeded)
			{
				TempData["FlashSuccess"] = "Usuario creado con éxito.";
				return RedirectToAction("AdministrarUsuariosNazan");
			}

			ModelState.AddModelError("", ResourceErrores.RegistroGeneral);

			return View(model);
		}

		[Authorize(Roles = "NAZAN-ADMINISTRARUSUARIOSNAZAN-MODIFICAR")]
		public ActionResult EditarUsuarioNazan(string id)
		{
			//TODO
			ViewBag.Perfiles = new SelectList(new[]{
					"POR DEFINIR",
				});

			var usuario = _usuarioManager.Find(id);

			if (usuario == null)
			{
				//TODO ACTUALIZAR MENSAJE AL RESOURCE
				TempData["FlashError"] = "Usuario incorrecto.";
				return RedirectToAction("AdministrarUsuariosNazan");
			}

			var usuarioViewModel = new UsuarioViewModel()
			{
				UserName = usuario.UserName,
				Nombre = usuario.Nombre,
				Apellido = usuario.Apellido,
				Email = usuario.Email
			};

			return View(usuarioViewModel);
		}

		[Authorize(Roles = "NAZAN-ADMINISTRARUSUARIOSNAZAN-MODIFICAR")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult EditarUsuarioNazan(string id, UsuarioViewModel model)
		{
			var usuario = _usuarioManager.Find(id);

			if (usuario == null)
			{
				//TODO ACTUALIZAR MENSAJE AL RESOURCE
				TempData["FlashError"] = "Usuario incorrecto.";
				return RedirectToAction("AdministrarUsuariosNazan");
			}
			
			try
			{
				_usuarioManager.Update(
					id,
					model.Nombre,
					model.Apellido,
					model.Email,
					model.Password);

				TempData["FlashSuccess"] = "Usuario actualizado con éxito.";
				return RedirectToAction("AdministrarUsuariosNazan");
			}
			catch (RetryLimitExceededException)
			{
				ModelState.AddModelError("", ResourceErrores.RegistroGeneral);
			}

			//TODO
			ViewBag.Perfiles = new SelectList(new[]{
					"POR DEFINIR",
				});

			return View(model);
		}

		[Authorize(Roles = "NAZAN-ADMINISTRARUSUARIOSNAZAN-MODIFICAR")]
		public ActionResult EliminarUsuarioNazan(string id)
		{
			var usuario = _usuarioManager.Find(id);

			if (usuario == null)
			{
				//TODO ACTUALIZAR MENSAJE AL RESOURCE
				TempData["FlashError"] = "Usuario incorrecto.";
				return RedirectToAction("AdministrarUsuariosNazan");
			}

			if (usuario.Id == User.Identity.GetUserId())
			{
				//TODO ACTUALIZAR MENSAJE AL RESOURCE
				TempData["FlashError"] = "No puede eliminar su propio usuario.";
				return RedirectToAction("AdministrarUsuariosNazan");
			}

			if (usuario.UserName.ToLower() == "superadmin")
			{

				//TODO ACTUALIZAR MENSAJE AL RESOURCE
				TempData["FlashError"] = "No puede eliminar el usuario SuperAdmin.";
				return RedirectToAction("AdministrarUsuariosNazan");
			}

			try
			{

				_usuarioManager.Remove(id);
			}
			catch (RetryLimitExceededException)
			{
				//TODO ACTUALIZAR MENSAJE AL RESOURCE
				TempData["FlashError"] = "INTENTOS EXEDIDOS";
				return RedirectToAction("AdministrarUsuariosNazan");
			}
			catch (Exception exception)
			{
				//TODO ACTUALIZAR MENSAJE AL RESOURCE
				TempData["FlashError"] = exception.Message;
				return RedirectToAction("AdministrarUsuariosNazan");
			}

			TempData["FlashSuccess"] = "Usuario eliminado con éxito.";
			return RedirectToAction("AdministrarUsuariosNazan");
		}
	}
}