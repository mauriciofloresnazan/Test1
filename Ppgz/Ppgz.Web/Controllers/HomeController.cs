using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Ppgz.Web.Models;

namespace Ppgz.Web.Controllers
{
    public class HomeController : Controller
    {
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
            /*

            var context = new ApplicationDbContext();
            context.Roles.Add(new IdentityRole()
            {
                Name = "MAESTRO"
            });
            context.SaveChanges();*/

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
    }
}