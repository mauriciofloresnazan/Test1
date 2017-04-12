using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Ppgz.Repository;
using Ppgz.Web.Infrastructure;
using Ppgz.Web.Infrastructure.Nazan;
using Ppgz.Web.Models;

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

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public void CreateRole()
        {
           CommonManager commonManager= new CommonManager();

           var table = commonManager.QueryToTable("SELECT * FROM cuentas;");
           


        }

        public void AddToRole()
        {/*
            var context = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            userManager.AddToRole(User.Identity.GetUserId(), "NAZAN-ADMINSITRARUSUARIOSNAZAN-LISTAR");*/
        }

        [Authorize(Roles = "MAESTRO")]
        public void TestRole()
        {
            /*var context = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            userManager.AddToRole(User.Identity.GetUserId(), "MAESTRO");
            Response.Write("TENGO ACCESO MAESTRO");*/
        }

        public void Terminos()
        {
            Response.Write("DEBE ACEPTAR");
        }

        public void TestMail()
        {
            var commonManager = new CommonManager();
            commonManager.SendHtmlMail(
                "Prueba","hola<br>juan<br>godoy","g.juanch14@gmail.com");
        }

    }
}