using System;
using System.Collections;
using System.Web.Mvc;
using Ppgz.Services;
using Ppgz.Web.Infrastructure;
using OrdenCompraManager = Ppgz.Services.OrdenCompraManager;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class ControlCitasController : Controller
    {


        readonly ProveedorManager _proveedorManager = new ProveedorManager();
        readonly CommonManager _commonManager = new CommonManager();
        readonly CuentasPorPagarManager _cuentasPorPagarManager = new CuentasPorPagarManager();

        //
        // GET: /Mercaderia/ControlCitas/
        [Authorize(Roles = "MAESTRO-MERCADERIA")]
        public ActionResult Index()
        {

            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            ViewBag.proveedores = _proveedorManager.FindByCuentaId(cuenta.Id);
            
            return View();
        }



        [Authorize(Roles = "MAESTRO-MERCADERIA")]
        public ActionResult OrdenDeCompra(int proveedorId)
        {
            ViewBag.IdProveedor = proveedorId;

            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA")]
        public JsonResult OrdenDeCompraDetalle(string Documento = "4500916565")
        {
            var ordenCompraManager = new OrdenCompraManager();
            var orden = ordenCompraManager.FindDetalleByDocumento(Documento);

            return Json(orden,JsonRequestBehavior.AllowGet);
        }


        [Authorize(Roles = "MAESTRO-MERCADERIA")]
        public ActionResult BuscarOrden(int proveedorId)
        {
            var proveedor = _proveedorManager.Find(proveedorId);

            if (proveedor == null)
            {
                // TODO
                TempData["FlashError"] = "Proveedor incorrecto";
            }

            ViewBag.proveedorId = proveedorId;
            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult BuscarOrden(int proveedorId, string numeroDocumento)
        {
            try
            {
                var ordenCompraManager = new OrdenCompraManager();
                var orden = ordenCompraManager.FindOrdenCompraWithAvailableDates(numeroDocumento, proveedorId);

                Session["orden"] = orden;
                return RedirectToAction("FechaCita");

            }
            catch (BusinessException businessEx)
            {
                TempData["FlashError"] =  businessEx.Message;
                return View();
            }
            catch (Exception e)
            {
                var log = CommonManager.BuildMessageLog(
                    TipoMensaje.Error,
                    ControllerContext.Controller.ValueProvider.GetValue("controller").RawValue.ToString(),
                    ControllerContext.Controller.ValueProvider.GetValue("action").RawValue.ToString(),
                    e.ToString(), Request);

                CommonManager.WriteAppLog(log, TipoMensaje.Error);
                return View();
            }
        }

        public ActionResult FechaCita()
        {
            if (Session["orden"] == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Dates = ((Hashtable) Session["orden"])["Dates"];
            return View();
        }

        public ActionResult Asn()
        {
            if (Session["orden"] == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Orden = ((Hashtable)Session["orden"]);

            return View();
        }
	}
}