using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Ppgz.Repository;

namespace Ppgz.Web.Areas.Nazan.Controllers
{
    public class AdministrarFacturasController : Controller
    {
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARFACTURAS")]
        public ActionResult Index(string fecha = null)
        {

            var fechaInicio = DateTime.Today.AddMonths(-3);

            if (!string.IsNullOrWhiteSpace(fecha))
            {
                fechaInicio = DateTime.ParseExact(fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture).AddMonths(-3);
            }


            var fechaFin = fechaInicio.AddMonths(3);

            var db = new Entities();

            ViewBag.Facturas = db.facturas.Where(f => f.Fecha >= fechaInicio && f.Fecha <= fechaFin).ToList();

            ViewBag.FechaInicio = fechaInicio;
            
            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARFACTURAS")]
        public FileResult DescargarXml(int facturaId)
        {
            var db = new Entities();

            var factura = db.facturas.Find(facturaId);

            if (factura == null)
            {
                throw new Exception("Factura incorrecta");
            }

            var fileBytes = System.IO.File.ReadAllBytes(factura.XmlRuta);

            var fileName = factura.Uuid + ".xml";

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Text.Xml, fileName);


        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARFACTURAS")]
        public FileResult DescargarPdf(int facturaId)
        {
            var db = new Entities();

            var factura = db.facturas.Find(facturaId);

            if (factura == null)
            {
                throw new Exception("Factura incorrecta");
            }

            var fileBytes = System.IO.File.ReadAllBytes(factura.PdfRuta);

            var fileName = factura.Uuid + ".pdf";

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, fileName);


        }
    
    
    }
}