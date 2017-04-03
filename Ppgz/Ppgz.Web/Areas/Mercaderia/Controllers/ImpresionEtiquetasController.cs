using System;
using System.Linq;
using System.Web.Mvc;
using Ppgz.Repository;
using Ppgz.Web.Infrastructure;
using OrdenCompraManager = Ppgz.Services.OrdenCompraManager;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class ImpresionEtiquetasController : Controller
    {
        private readonly OrdenCompraManager _ordenCompraManager = new OrdenCompraManager();
        private readonly CommonManager _commonManager = new CommonManager();
        // GET: /Mercaderia/ImpresionEtiquetas/
        [Authorize(Roles = "MAESTRO-MERCADERIA")]
        public ActionResult Index()
        {
            var db = new Entities();

            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            ViewBag.data = _ordenCompraManager.FindOrdenesDecompraActivasByCuenta(cuenta.Id);

            ViewBag.Crs = db.crs.ToList();

            return View();
        }
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
            
            var fileName = cr.Codigo +  ".pdf";

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, fileName);


        }

    }
}