using System;
using System.Web.Mvc;
using Ppgz.Services;
using Ppgz.Web.Areas.Nazan.Models;
using Ppgz.Web.Infrastructure;

namespace Ppgz.Web.Areas.Nazan.Controllers
{
	[Authorize]
	public class AdministrarUsuariosNazanController : Controller
	{
        private readonly UsuarioManager _usuarioManager = new UsuarioManager();

        private readonly PerfilManager _perfilManager = new PerfilManager();

		//
		// GET: /Nazan/
		[Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARUSUARIOSNAZAN-LISTAR,NAZAN-ADMINISTRARUSUARIOSNAZAN-MODIFICAR")]
		public ActionResult Index()
		{
            ViewBag.Usuarios = _usuarioManager.FindAllNazan();
			return View();
		}

		[Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARUSUARIOSNAZAN-MODIFICAR")]
		public ActionResult Crear()
		{
			ViewBag.Perfiles =
                new SelectList(_perfilManager.FindPerfilesNazan(), "Id", "Nombre");
			return View();
		}

		[Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARUSUARIOSNAZAN-MODIFICAR")]
		[ValidateAntiForgeryToken]
		[HttpPost]
		public ActionResult Crear(UsuarioNazanViewModel model)
		{
			ViewBag.Perfiles =
				  new SelectList(_perfilManager.FindPerfilesNazan(), "Id", "Nombre"); 

			if (!ModelState.IsValid) return View(model);

			try
			{
				_usuarioManager.CrearNazan(
                    model.UserName,
                    model.Nombre,
                    model.Apellido,
					model.Email,
                    model.Telefono,
                    null,//TODO
                    true,
                    model.Perfil,
                    model.Password);

				TempData["FlashSuccess"] =MensajesResource.INFO_UsuarioNazan_CreadoCorrectamente;
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

		[Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARUSUARIOSNAZAN-MODIFICAR")]
		public ActionResult Editar(string id)
		{
			ViewBag.Perfiles =
                  new SelectList(_perfilManager.FindPerfilesNazan(), "Id", "Nombre"); 

			var usuario = _usuarioManager.Find(id);

			if (usuario == null)
			{
				TempData["FlashError"] = MensajesResource.ERROR_UsuarioNazan_IdIncorrecto;
				return RedirectToAction("Index");
			}

			var usuarioNazanViewModel = new UsuarioNazanViewModel()
			{
				UserName = usuario.UserName,
				Nombre = usuario.Nombre,
				Apellido = usuario.Apellido,
				Email = usuario.Email,
                Telefono = usuario.PhoneNumber,
				Perfil = usuario.PerfilId
			};

			return View(usuarioNazanViewModel);
		}

		[Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARUSUARIOSNAZAN-MODIFICAR")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Editar(string id, UsuarioNazanViewModel model)
		{
			var usuario = _usuarioManager.Find(id);

			if (usuario == null)
			{
				TempData["FlashError"] = MensajesResource.ERROR_UsuarioNazan_IdIncorrecto;
				return RedirectToAction("Index");
			}

			try
			{
				_usuarioManager.Actualizar(
					id,
					model.Nombre,
					model.Apellido,
					model.Email,
					model.Telefono,null,
					model.Password);

                _usuarioManager.ActualizarPerfil(id, model.Perfil);

				TempData["FlashSuccess"] = MensajesResource.INFO_UsuarioNazan_ActualizadoCorrectamente;
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

		[Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARUSUARIOSNAZAN-MODIFICAR")]
		public ActionResult Eliminar(string id)
		{
			var usuario = _usuarioManager.Find(id);

			if (usuario == null)
			{
				TempData["FlashError"] = MensajesResource.ERROR_UsuarioNazan_IdIncorrecto;
				return RedirectToAction("Index");
			}

			try
			{
				_usuarioManager.Eliminar(id);
				TempData["FlashSuccess"] = MensajesResource.INFO_UsuarioNazan_EliminadoCorrectamente;
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
	}
}