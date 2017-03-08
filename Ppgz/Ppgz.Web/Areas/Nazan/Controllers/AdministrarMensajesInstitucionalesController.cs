using System;
using System.Globalization;
using System.IO;
using System.Web.Mvc;
using Ppgz.Web.Areas.Nazan.Models;
using Ppgz.Web.Infrastructure.Nazan;

namespace Ppgz.Web.Areas.Nazan.Controllers
{
	[Authorize]
	public class AdministrarMensajesInstitucionalesController : Controller
	{
		private readonly MensajesInstitucionalesManager  _mensajesInstitucionalesManager = new MensajesInstitucionalesManager();

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARMENSAJESINSTITUCIONALES-LISTAR,NAZAN-ADMINISTRARMENSAJESINSTITUCIONALES-MODIFICAR")]
		public ActionResult Index()
		{
			ViewBag.mensajes  = _mensajesInstitucionalesManager.FindAll();
			
			return View();
		}
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARMENSAJESINSTITUCIONALES-MODIFICAR")]
		public ActionResult Crear()
		{
			return View();
		}

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARMENSAJESINSTITUCIONALES-MODIFICAR")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Crear(MensajeViewModel model, FormCollection collection)
		{
			if (!ModelState.IsValid) return View(model);

			if (!String.IsNullOrEmpty(Request["pdf-mensaje"]))
			{
				if (Request.Files.Count == 0)
				{
					ModelState.AddModelError(string.Empty, "Debe agregar un contenido textual para el mensaje o un Archivo PDF.");
					return View(model);
				}

				var file = Request.Files[0];

				if (file != null && file.ContentType != "application/pdf")
				{
					ModelState.AddModelError(string.Empty, "El archivo debe ser de tipo PDF.");
					return View(model);
				}

				if (file == null) return RedirectToAction("Index");

				var fileName = Path.GetFileName(file.FileName);

				if (fileName == null) return RedirectToAction("Index");

				var path = Path.Combine(Server.MapPath("~/Uploads/"), fileName);
				file.SaveAs(path);

				try
				{
					_mensajesInstitucionalesManager.CrearPdf(
						model.Titulo,
						"~/Uploads/" + fileName,
						DateTime.ParseExact(model.FechaPublicacion, "d/M/yyyy", CultureInfo.InvariantCulture),
						DateTime.ParseExact(model.FechaCaducidad, "d/M/yyyy", CultureInfo.InvariantCulture),
						MensajesInstitucionalesManager.GetEnviadoAByString(model.TipoProveedor));

					TempData["FlashSuccess"] = "Mensaje creado con éxito.";
					return RedirectToAction("Index");
				}
				catch (Exception)
				{
					//TODO ACTUALIZAR MENSAJE AL RESOURCE
                    TempData["FlashError"] = MensajesResource.ERROR_General;
					return RedirectToAction("Index");
				}
			}

			if (string.IsNullOrWhiteSpace(model.Contenido))
			{
				// TODO CARLOS Y JUAN DELGADO
				ModelState.AddModelError(string.Empty, "Debe agregar un contenido textual para el mensaje o un Archivo PDF.");

				return View(model);
			}

			try
			{
				_mensajesInstitucionalesManager.CrearTexto(
					model.Titulo,
					model.Contenido,
					DateTime.ParseExact(model.FechaPublicacion, "d/M/yyyy", CultureInfo.InvariantCulture),
					DateTime.ParseExact(model.FechaCaducidad, "d/M/yyyy", CultureInfo.InvariantCulture),
					MensajesInstitucionalesManager.GetEnviadoAByString(model.TipoProveedor));

				TempData["FlashSuccess"] = "Mensaje creado con éxito.";
				return RedirectToAction("Index");
			}
			catch (Exception exception)
			{
						
				//TODO ACTUALIZAR MENSAJE AL RESOURCE
                ModelState.AddModelError(string.Empty, exception.Message);
                return View(model);
			}
		}

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARMENSAJESINSTITUCIONALES-MODIFICAR")]
		   public ActionResult Editar(int id)
		{
			var mensaje = _mensajesInstitucionalesManager.Find(id);
			
			if (mensaje == null)
			{
				//TODO ACTUALIZAR MENSAJE AL RESOURCE
				TempData["FlashError"] = "Mensaje incorrecto.";
				return RedirectToAction("Index");
			}
			
			var mensajeModel = new MensajeViewModel()
			{
				Contenido = mensaje.Contenido,
				FechaCaducidad = mensaje.FechaCaducidad.ToString("dd/MM/yyyy"),
				FechaPublicacion = mensaje.FechaPublicacion.ToString("dd/MM/yyyy"),
				TipoProveedor = mensaje.EnviadoA,
				Titulo = mensaje.Titulo
			};

			return View(mensajeModel);
		}

		[HttpPost, ActionName("Editar")]
		[ValidateAntiForgeryToken]
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARMENSAJESINSTITUCIONALES-MODIFICAR")]
		public ActionResult EditarPost(int id, MensajeViewModel model)
		{
			
			if (!String.IsNullOrEmpty(Request["pdf-mensaje"]))
			{
				if (Request.Files.Count == 0)
				{
					ModelState.AddModelError(string.Empty, "Debe agregar un contenido textual para el mensaje o un Archivo PDF.");
					return View(model);
				}

				var file = Request.Files[0];

				if (file != null && file.ContentType != "application/pdf")
				{
					ModelState.AddModelError(string.Empty, "El archivo debe ser de tipo PDF.");
					return View(model);
				}


				if (file == null) return RedirectToAction("Index");
				var fileName = Path.GetFileName(file.FileName);

				if (fileName == null) return RedirectToAction("Index");
				var path = Path.Combine(Server.MapPath("~/Uploads/"), fileName);
				file.SaveAs(path);

				model.Pdf = "~/Uploads/" + fileName;
				try
				{
					_mensajesInstitucionalesManager.ActualizarPdf(
						id,
						model.Titulo,
						model.Pdf,
						DateTime.ParseExact(model.FechaPublicacion, "d/M/yyyy", CultureInfo.InvariantCulture),
						DateTime.ParseExact(model.FechaCaducidad, "d/M/yyyy", CultureInfo.InvariantCulture),
						MensajesInstitucionalesManager.GetEnviadoAByString(model.TipoProveedor));

					// TODO MENSAJES
					TempData["FlashSuccess"] = "Mensaje creado con éxito.";
					return RedirectToAction("Index");
				}
				catch (Exception)
				{
					//TODO ACTUALIZAR MENSAJE AL RESOURCE
					TempData["FlashError"] = MensajesResource.ERROR_General;
					return RedirectToAction("Index");
				}
			}
			if (string.IsNullOrWhiteSpace(model.Contenido))
			{
				// TODO JUAN DELGADO CARLOS CAMPOS
				ModelState.AddModelError(string.Empty, "Debe agregar un contenido textual para el mensaje o un Archivo PDF.");
				return View(model);
			}

			try
			{
							
				_mensajesInstitucionalesManager.ActualizarTexto(
					id,
					model.Titulo,
					model.Contenido,
					DateTime.ParseExact(model.FechaPublicacion, "d/M/yyyy", CultureInfo.InvariantCulture),
					DateTime.ParseExact(model.FechaCaducidad, "d/M/yyyy", CultureInfo.InvariantCulture),
					MensajesInstitucionalesManager.GetEnviadoAByString(model.TipoProveedor));

				// TODO MENSAJES
				TempData["FlashSuccess"] = "Mensaje creado con éxito.";
				return RedirectToAction("Index");
			}
			catch (Exception)
			{
				//TODO ACTUALIZAR MENSAJE AL RESOURCE
				TempData["FlashError"] = MensajesResource.ERROR_General;
				return RedirectToAction("Index");
			}
		}

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARMENSAJESINSTITUCIONALES-MODIFICAR")]
		public ActionResult Eliminar(int id)
		{
			try
			{
				_mensajesInstitucionalesManager.Eliminar(id);
			}
			catch (Exception exception)
			{
				 //TODO ACTUALIZAR MENSAJE AL RESOURCE
				TempData["FlashError"] = exception.Message;
				return RedirectToAction("Index");
			}
			return RedirectToAction("Index");
		}
	
	}
}