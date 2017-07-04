using System;
using System.Collections;
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
using System.Threading.Tasks;

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


    }
}