using System;
using System.Linq;
using System.Net.Mime;
using System.Web.Mvc;
using Ppgz.Repository;
using Ppgz.Web.Infrastructure;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class ImpresionEtiquetasController : Controller
    {

        // GET: /Mercaderia/ImpresionEtiquetas/
        [Authorize(Roles = "MAESTRO-MERCADERIA")]
        public ActionResult Index()
        {
            var db = new Entities();

            ViewBag.Etiquetas = db.etiquetas.ToList();

            return View();
        }
        public FileResult Descargar(int id)
        {
            var db = new Entities();
            var etiqueta = db.etiquetas.Find(id);
            if (etiqueta == null)
            {
                // TODO
                throw new Exception("Etiqueta Incorrecta");
            }
             var fileBytes = System.IO.File.ReadAllBytes(etiqueta.Archivo);
            
            var fileName = etiqueta.OrdenCompraNumeroDoc +  ".txt";

            return File(fileBytes, MediaTypeNames.Application.Pdf, fileName);
        }
    }
}