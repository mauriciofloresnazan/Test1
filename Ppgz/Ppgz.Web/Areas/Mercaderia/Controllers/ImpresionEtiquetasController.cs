using System.Collections;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Ppgz.Repository;
using Ppgz.Services;
using Ppgz.Web.Infrastructure;
using System;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class ImpresionEtiquetasController : Controller
    {

        private readonly CommonManager _commonManager = new CommonManager();
        private readonly ProveedorManager _proveedorManager = new ProveedorManager();
        private readonly EtiquetasManager _etiquetasManager = new EtiquetasManager();
        private readonly OrdenCompraManager _ordenesCompraManager = new OrdenCompraManager();

        private const string NombreVarSession = "etiqueta_csv";

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-IMPRESIONETIQUETAS")]
        public ActionResult Index()
        {
            /*var commonManager = new CommonManager();

            var cuenta = commonManager.GetCuentaUsuarioAutenticado();
        
            var proveedorManager = new ProveedorManager();
            var proveedores = proveedorManager.FindByCuentaId(cuenta.Id).ToList();

            var proveedoresIds = proveedores.Select(p => p.Id).ToArray();

            var db = new Entities();
            

            ViewBag.Etiquetas = db.etiquetas.Where(e=> proveedoresIds.Contains(e.ProveedorId)).ToList();*/

            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            ViewBag.proveedores = _proveedorManager.FindByCuentaId(cuenta.Id);

 

            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-IMPRESIONETIQUETAS")]
        public ActionResult Generar(int proveedorId)
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();
            var proveedor = _proveedorManager.Find(proveedorId, cuenta.Id);
            if (proveedor == null)
            {
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index"); 
            }

            ViewBag.Proveedor = proveedor;
            ViewBag.Ordenes = _ordenesCompraManager.FindOrdenesDecompraImprimir(proveedor.NumeroProveedor);
            return View();
        }

/*
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-IMPRESIONETIQUETAS")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Generar(int proveedorId, bool nazan, string [] ordenes)
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            var proveedor = _proveedorManager.Find(proveedorId, cuenta.Id);

            if (proveedor == null)
            {
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }

            var resultado = _etiquetasManager.GetArchivoCsv(proveedorId, cuenta.Id, nazan, ordenes);

            TempData["resultado"] = resultado;
            TempData["proveedor"] = proveedor;

            return RedirectToAction("Resultado"); 
        }*/

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-IMPRESIONETIQUETAS")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Generar(int proveedorId, bool nazan, string orden1, string orden2 = "", string orden3 = "", string orden4 = "", string orden5 = "")
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            var proveedor = _proveedorManager.Find(proveedorId, cuenta.Id);

            if (proveedor == null)
            {
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }

            var ordenes = new[]
            {
                orden1, orden2, orden3, orden4, orden5
            };

            var resultado = _etiquetasManager.GetArchivoCsv(proveedorId, cuenta.Id, nazan, ordenes.Where(o => !string.IsNullOrEmpty(o)).ToArray());

            TempData["resultado"] = resultado;
            TempData["proveedor"] = proveedor;

            return RedirectToAction("Resultado");
        }


        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-IMPRESIONETIQUETAS")]
        public ActionResult Resultado()
        {
            if (System.Web.HttpContext.Current.Session[NombreVarSession] != null)
            {
                System.Web.HttpContext.Current.Session.Remove(NombreVarSession);
            }

            var resultado = TempData["resultado"] as Hashtable;
            var proveedor = TempData["proveedor"] as proveedore;

            if (resultado == null || proveedor == null)
            {
                return RedirectToAction("Index");
            }


            ViewBag.Proveedor = proveedor;
            ViewBag.Resultado = resultado["return"];

            ViewBag.PuedeDescargar = false;

            if (resultado["csv"] == null) return View();
            if (string.IsNullOrWhiteSpace(resultado["csv"].ToString())) return View();

            System.Web.HttpContext.Current.Session[NombreVarSession] = resultado["csv"];
            ViewBag.PuedeDescargar = true;
            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-IMPRESIONETIQUETAS")]
        public FileContentResult Descargar()
        {
            var csv = System.Web.HttpContext.Current.Session["etiqueta_csv"].ToString(); 

            return File(new UTF8Encoding().GetBytes(csv), "text/csv", "etiquetas.csv");
        }

    }
}