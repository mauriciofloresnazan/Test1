using System;
using System.Linq;
using System.Net.Mime;
using System.Web.Mvc;
using Ppgz.Repository;
using Ppgz.Services;
using Ppgz.Web.Infrastructure;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class ImpresionEtiquetasController : Controller
    {
        

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-IMPRESIONETIQUETAS")]
        public ActionResult Index()
        {
            var commonManager = new CommonManager();

            var cuenta = commonManager.GetCuentaUsuarioAutenticado();
        
            var proveedorManager = new ProveedorManager();
            var proveedores = proveedorManager.FindByCuentaId(cuenta.Id).ToList();

            var proveedoresIds = proveedores.Select(p => p.Id).ToArray();

            var db = new Entities();
            

            ViewBag.Etiquetas = db.etiquetas.Where(e=> proveedoresIds.Contains(e.ProveedorId)).ToList();

            return View();
        }


        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-IMPRESIONETIQUETAS")]
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
            
            var fileName = string.Format("Etiqueta_{0}.csv",etiqueta.NumeroOrden );

            return File(fileBytes, MediaTypeNames.Application.Pdf, fileName);
        }
    }
}