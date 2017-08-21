using System;
using System.Web.Mvc;
using Ppgz.Services;
using Ppgz.Web.Infrastructure;

namespace Ppgz.Web.Controllers
{
    
    [Authorize]
    public class HomeController : Controller
    {
    
        [TerminosCondiciones]
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Test()
        {
            return Content(DateTime.Now.Hour.ToString());
        }

        public FileResult Ayuda()
        {
            var commonManager = new CommonManager();
            var usuario = commonManager.GetUsuarioAutenticado();

            var fileBytes = usuario.Tipo == UsuarioManager.Tipo.Nazan
                ? System.IO.File.ReadAllBytes(Server.MapPath(@"~/App_Data/manual_administrador.pdf"))
                : System.IO.File.ReadAllBytes(Server.MapPath(@"~/App_Data/manual_usuario.pdf"));

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, "manual.pdf");
        }
        public FileResult AyudaCedis()
        {
            var commonManager = new CommonManager();
            var usuario = commonManager.GetUsuarioAutenticado();

            var fileBytes = System.IO.File.ReadAllBytes(Server.MapPath(@"~/App_Data/Manual-CEDIS.doc"));

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, "manual-cedis.doc");
        }

        [AllowAnonymous]
        public ActionResult Privacidad()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Terminos()
        {
            ViewBag.Politicas = System.IO.File.ReadAllText(Server.MapPath(@"~/App_Data/politicas.txt"));
            return View();
        }
    }
}