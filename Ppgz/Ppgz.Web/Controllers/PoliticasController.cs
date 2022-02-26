using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Ppgz.Repository;
using Ppgz.Web.Infrastructure;
using Ppgz.Web.Models;
using Ppgz.Services;

namespace Ppgz.Web.Controllers
{

    [Authorize]
    public class PoliticasController : Controller
    {
        //
        // GET: /Politicas/
        public ActionResult Index()
        {
            var commonManager  = new CommonManager();
            
            var usuario = commonManager.GetUsuarioAutenticado();

            ViewBag.Usuario = usuario;

            ViewBag.Politicas = System.IO.File.ReadAllText(Server.MapPath(@"~/App_Data/politicas.txt"));

            return View();
        }

        public ActionResult CambiarPassword()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarPassword(CambiarPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var commonManager = new CommonManager();

            var usuario = commonManager.GetUsuarioAutenticado();

            var usuarioManager = new UsuarioManager();
            try
            {
                usuarioManager.Actualizar(usuario.Id, null, null, null, null, null, model.Password);

                TempData["FlashSuccess"] = "Contraseña actualizada exitosamente";
                return RedirectToAction("Aceptar", "Politicas");
            }
            catch (Exception exception)
            {

                ModelState.AddModelError(string.Empty, exception.Message);

                return View(model);
            }
        }
            public ActionResult Aceptar()
        {
            var entities = new Entities();

            var usuarioId = User.Identity.GetUserId();

            var usuario = entities.AspNetUsers
                .Single(u => u.Id == usuarioId);

            if(usuario.TerminosCondicionesFecha == null)
                usuario.TerminosCondicionesFecha =  DateTime.Now;
            
            
            entities.Entry(usuario).State = EntityState.Modified;
            entities.SaveChanges();

            return RedirectToAction("Index", "Home");
        }
	}
}