using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Ppgz.Web.Infrastructure;
using Ppgz.Web.Infrastructure.Nazan;
using Ppgz.Web.Infrastructure.Proveedor;

namespace Ppgz.Web.Areas.Nazan.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class CuentasPagarController : Controller
    {
        private readonly PerfilProveedorManager _perfilProveedorManager = new PerfilProveedorManager();
        private readonly CommonManager _commonManager = new CommonManager();

        //
        // GET: /Nazan/CuentasPagar/
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-CUENTASPAGAR-LISTAR,NAZAN-CUENTASPAGAR-MODIFICAR")]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-CUENTASPAGAR-LISTAR,NAZAN-CUENTASPAGAR-MODIFICAR")]
        public ActionResult Pagos()
        {
            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-CUENTASPAGAR-LISTAR,NAZAN-CUENTASPAGAR-MODIFICAR")]
        public ActionResult PagosPendientes()
        {
            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-CUENTASPAGAR-LISTAR,NAZAN-CUENTASPAGAR-MODIFICAR")]
        public ActionResult PagosPendientesDetalle()
        {
            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-CUENTASPAGAR-LISTAR,NAZAN-CUENTASPAGAR-MODIFICAR")]
        public ActionResult Devoluciones()
        {
            return View();
        }
        
        
	}
}