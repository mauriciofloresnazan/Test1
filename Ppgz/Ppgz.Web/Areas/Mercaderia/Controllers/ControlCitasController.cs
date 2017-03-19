using System.Web.Mvc;
using Ppgz.Web.Infrastructure;
using Ppgz.Services;

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

            var ordenCompraManager = new Ppgz.Services.OrdenCompraManager();
            var orden = ordenCompraManager.FindDetalleByDocumento(Documento);

            return Json(orden,JsonRequestBehavior.AllowGet);

        }

	}
}