using System;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Ppgz.Services;
using Ppgz.Web.Areas.Nazan.Models;
using Ppgz.Web.Infrastructure;
using Ppgz.Web.Infrastructure.Nazan;

namespace Ppgz.Web.Areas.Nazan.Controllers
{
    [Authorize]
    public class AdministrarMensajesInstitucionalesController : Controller
    {
        private readonly MensajesInstitucionalesManager _mensajesInstitucionalesManager = new MensajesInstitucionalesManager();

        internal string CargarPdf(HttpPostedFileBase file)
        {
            if (file != null && file.ContentType != "application/pdf")
            {
                throw new BusinessException(MensajesResource.ERROR_MensajesInstitucionales_PdfInvalido);
            }

            if (file == null)
            {
                throw new BusinessException(MensajesResource.ERROR_MensajesInstitucionales_PdfInvalido);
            }

            var fileName = Path.GetFileName(file.FileName);

            if (fileName == null)
            {
                throw new BusinessException(MensajesResource.ERROR_MensajesInstitucionales_PdfInvalido);
            }

            var path = Path.Combine(Server.MapPath("~/Uploads/"), fileName);

            file.SaveAs(path);

            return "~/Uploads/" + fileName;
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARMENSAJESINSTITUCIONALES-LISTAR,NAZAN-ADMINISTRARMENSAJESINSTITUCIONALES-MODIFICAR")]
        public ActionResult Index()
        {
            ViewBag.mensajes = _mensajesInstitucionalesManager.FindAll();
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

            try
            {
                if (!String.IsNullOrEmpty(Request["pdf-mensaje"]))
                {
                    if (Request.Files.Count == 0)
                    {
                        ModelState.AddModelError(string.Empty, MensajesResource.ERROR_MensajesInstitucionales_PdfInvalido);
                        return View(model);
                    }

                    var pdfUrl = CargarPdf(Request.Files[0]);

                    _mensajesInstitucionalesManager.CrearPdf(
                        model.Titulo,
                        pdfUrl,
                        DateTime.ParseExact(model.FechaPublicacion, "d/M/yyyy", CultureInfo.InvariantCulture),
                        DateTime.ParseExact(model.FechaCaducidad, "d/M/yyyy", CultureInfo.InvariantCulture),
                        MensajesInstitucionalesManager.GetEnviadoAByString(model.TipoProveedor));
                }
                else // Mensaje solo texto
                {
                    _mensajesInstitucionalesManager.CrearTexto(
                       model.Titulo,
                       model.Contenido,
                       DateTime.ParseExact(model.FechaPublicacion, "d/M/yyyy", CultureInfo.InvariantCulture),
                       DateTime.ParseExact(model.FechaCaducidad, "d/M/yyyy", CultureInfo.InvariantCulture),
                       MensajesInstitucionalesManager.GetEnviadoAByString(model.TipoProveedor));
                }
                
                TempData["FlashSuccess"] = MensajesResource.INFO_MensajesInstitucionales_CreadoCorrectamente;
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

                ModelState.AddModelError(string.Empty, e.Message);
                return View(model);
            }
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARMENSAJESINSTITUCIONALES-MODIFICAR")]
        public ActionResult Editar(int id)
        {
            var mensaje = _mensajesInstitucionalesManager.Find(id);

            if (mensaje == null)
            {
                TempData["FlashError"] = MensajesResource.ERROR_MensajesInstitucionales_IdIncorrecto;
                return RedirectToAction("Index");
            }

            var mensajeModel = new MensajeViewModel()
            {
                Contenido = mensaje.Contenido,
                FechaCaducidad = mensaje.FechaCaducidad.ToString("dd/MM/yyyy"),
                FechaPublicacion = mensaje.FechaPublicacion.ToString("dd/MM/yyyy"),
                TipoProveedor = mensaje.EnviadoA,
                Titulo = mensaje.Titulo,
                Pdf = mensaje.Archivo
            };

            return View(mensajeModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARMENSAJESINSTITUCIONALES-MODIFICAR")]
        public ActionResult Editar(int id, MensajeViewModel model)
        {
            var mensaje = _mensajesInstitucionalesManager.Find(id);

            if (mensaje == null)
            {
                TempData["FlashError"] = MensajesResource.ERROR_MensajesInstitucionales_IdIncorrecto;
                return RedirectToAction("Index");
            }

            try
            {
                if (!String.IsNullOrEmpty(Request["pdf-mensaje"]))
                {
                    if (Request.Files.Count == 0)
                    {
                        ModelState.AddModelError(string.Empty, MensajesResource.ERROR_MensajesInstitucionales_PdfInvalido);
                        return View(model);
                    }

                    var pdfUrl = CargarPdf(Request.Files[0]);

                    _mensajesInstitucionalesManager.ActualizarPdf(
                        id,
                        model.Titulo,
                        pdfUrl,
                        DateTime.ParseExact(model.FechaPublicacion, "d/M/yyyy", CultureInfo.InvariantCulture),
                        DateTime.ParseExact(model.FechaCaducidad, "d/M/yyyy", CultureInfo.InvariantCulture),
                        MensajesInstitucionalesManager.GetEnviadoAByString(model.TipoProveedor));
                }
                else // Mensaje solo texto
                {
                    _mensajesInstitucionalesManager.ActualizarTexto(
                        id,
                        model.Titulo,
                        model.Contenido,
                        DateTime.ParseExact(model.FechaPublicacion, "d/M/yyyy", CultureInfo.InvariantCulture),
                        DateTime.ParseExact(model.FechaCaducidad, "d/M/yyyy", CultureInfo.InvariantCulture),
                        MensajesInstitucionalesManager.GetEnviadoAByString(model.TipoProveedor));
                }

                TempData["FlashSuccess"] = MensajesResource.INFO_MensajesInstitucionales_CreadoCorrectamente;
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

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARMENSAJESINSTITUCIONALES-MODIFICAR")]
        public ActionResult Eliminar(int id)
        {
            try
            {
                _mensajesInstitucionalesManager.Eliminar(id);
                TempData["FlashSuccess"] = MensajesResource.INFO_MensajesInstitucionales_EliminadoCorrectamente;
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