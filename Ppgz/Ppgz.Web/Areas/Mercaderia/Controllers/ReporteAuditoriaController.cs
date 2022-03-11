using Ppgz.Repository;
using Ppgz.Services;
using Ppgz.Web.Infrastructure;
using Ppgz.Web.Models.ProntoPago;
using SapWrapper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static SapWrapper.SapFacturaManager;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
	[Authorize]
	[TerminosCondiciones]
	public class ReporteAuditoriaController : Controller
	{
		readonly CommonManager _commonManager = new CommonManager();
		readonly ProveedorManager _proveedorManager = new ProveedorManager();
		readonly FacturaManager _facturaManager = new FacturaManager();
        readonly LogsFactoraje _logsFactoraje = new LogsFactoraje();

        internal proveedore ProveedorCxp
		{
			get
			{
				if (System.Web.HttpContext.Current.Session["proveedorcxp"] != null)
				{
					return (proveedore)System.Web.HttpContext.Current.Session["proveedorcxp"];
				}
				return null;
			}
			set
			{
				System.Web.HttpContext.Current.Session["proveedorcxp"] = value;
			}
		}
        internal string SociedadActiva
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["sociedadactiva"] != null)
                {
                    return (string)System.Web.HttpContext.Current.Session["sociedadactiva"];
                }
                return null;
            }
            set
            {
                System.Web.HttpContext.Current.Session["sociedadactiva"] = value;
            }
        }
		
		public ActionResult Index()
		{
			var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

			ViewBag.proveedores = _proveedorManager.FindByCuentaId(cuenta.Id);

			return View();
		}

		
		public ActionResult SeleccionarProveedor(int proveedorId, string sociedad)
		{

			var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

			var proveedor = _proveedorManager.Find(proveedorId, cuenta.Id);

			if (proveedor == null)
			{
				// TODO pasar a recurso
				TempData["FlashError"] = "Proveedor incorrecto";
				return RedirectToAction("Index");
			}

			ProveedorCxp = proveedor;
            SociedadActiva = sociedad;
            return RedirectToAction("Pagos");
		}

   
        public ActionResult Pagos(int proveedorId,string NumeroProveedor, string sociedad, string fechaDesde = null, string fechaHasta = null)
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            var proveedor = _proveedorManager.Find(proveedorId, cuenta.Id);

            if (proveedor == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }

            
            var dateFechaDesde = DateTime.Today.AddMonths(-3);
            if (!string.IsNullOrWhiteSpace(fechaDesde))
            {
                dateFechaDesde = DateTime.ParseExact(fechaDesde, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            var dateFechaHasta = DateTime.Today;
            if (!string.IsNullOrWhiteSpace(fechaHasta))
            {
                dateFechaHasta = DateTime.ParseExact(fechaHasta, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            var db = new Entities();

            ///var fecha = DateTime.Today.AddMonths(-3);

            ViewBag.penalizacion = db.Penalizacionauditores.Where(c => c.FechaPenalizacion > dateFechaDesde & c.FechaPenalizacion < dateFechaHasta & c.procesado == true & c.NumeroProveedor == NumeroProveedor).OrderByDescending(c => c.FechaPenalizacion).ToList();
            ProveedorCxp = proveedor;
            SociedadActiva = sociedad;
            ViewBag.Proveedor = proveedor;
            ViewBag.FechaDesde = dateFechaDesde;
            ViewBag.FechaHasta = dateFechaHasta;

            return View();

        }
      
		
    }
}