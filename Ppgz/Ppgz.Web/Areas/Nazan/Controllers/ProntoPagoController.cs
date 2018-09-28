using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Ppgz.Repository;
using Ppgz.Services;

namespace Ppgz.Web.Areas.Nazan.Controllers
{
    public class ProntoPagoController : Controller
    {
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO")]
        public ActionResult Index()
        {
            return View();
        }
    }
}