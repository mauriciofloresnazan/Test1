using System;
using System.Linq;
using System.Web.Mvc;
using Ppgz.Repository;
using Ppgz.Services;
using Ppgz.Web.Infrastructure;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class ComprobantesReciboController : Controller
    {
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-COMPROBANTESRECIBO")]
        public ActionResult Index()
        {
            var commonManager = new CommonManager();

            var cuenta = commonManager.GetCuentaUsuarioAutenticado();

            var proveedorManager = new ProveedorManager();
            var proveedores = proveedorManager.FindByCuentaId(cuenta.Id);
            var proveedoresNum = proveedores.Select(p => p.NumeroProveedor).ToArray();
            var proveedoresIds = proveedores.Select(p => p.Id).ToArray();

            var db = new Entities();
            ViewBag.CrsF = db.crsforaneos.Where(cr => proveedoresNum.Contains(cr.Proveedor)).ToList();
            ViewBag.Crs = db.crs.Where(cr => proveedoresIds.Contains(cr.cita.ProveedorId)).ToList();

            return View();
        }
        
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-COMPROBANTESRECIBO")]
        public FileResult Descargar(int id)
        {
            var db = new Entities();
            var cr = db.crs.Find(id);
            if (cr == null)
            {
                // TODO
                throw new Exception("CR Incorrecto");
            }
             var fileBytes = System.IO.File.ReadAllBytes(cr.ArchivoCR);
            
            var fileName = string.Format("CR_{0}_{1}.pdf", cr.cita.Id, ((DateTime)cr.Fecha).ToString("dd/MM/yyyy"));

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, fileName);


        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-COMPROBANTESRECIBO")]
        public FileResult DescargarForaneo(int id)
        {
            var db = new Entities();
            var cr = db.crsforaneos.Find(id);
            if (cr == null)
            {
                // TODO
                throw new Exception("CR Incorrecto");
            }
            string fechaf = ((DateTime)cr.Fecha).ToString("dd/MM/yyyy");
            fechaf = fechaf.Replace(@"/", "");

            var fileBytes = System.IO.File.ReadAllBytes(cr.ArchivoCR);
            var fileName = string.Format("CRF_{0}_{1}.pdf", cr.Proveedor, fechaf);

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, fileName);


        }

    }
}