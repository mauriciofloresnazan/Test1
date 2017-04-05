using System;
using System.Data;
using System.Web.Mvc;
using Ppgz.Services;
using Ppgz.Web.Infrastructure;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class ReporteProveedoresController : Controller
    {
      
        private readonly CommonManager _commonManager = new CommonManager();
        readonly ProveedorManager _proveedorManager = new ProveedorManager();
        readonly ReporteProveedorManager _reporteProveedorManager = new ReporteProveedorManager();
        //
        // GET: /Mercaderia/ReporteProveedores/
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-REPORTES-LISTAR")]
        public ActionResult Index()
        {
 
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            ViewBag.proveedores = _proveedorManager.FindByCuentaId(cuenta.Id);

            return View();
        }

        public string PersitedProviderId(string proveedorId)
        {
            int result = 0;
            if (int.TryParse(proveedorId, out result))
                System.Web.HttpContext.Current.Session["proveedorId"] = result;

            System.Web.HttpContext.Current.Session["razonsocial"] = _proveedorManager.Find(result).Rfc;

            return System.Web.HttpContext.Current.Session["proveedorId"].ToString();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-REPORTES")]
        public ActionResult Reportes(int proveedorId = 0)
        {
            int id = proveedorId > 0 ? proveedorId : Convert.ToInt32(System.Web.HttpContext.Current.Session["proveedorId"]);

            if (id == 0)
                return RedirectToAction("Index");

            ViewBag.proveedor = _proveedorManager.Find(id);

            ViewBag.reportes = _reporteProveedorManager.FindReporteProveedor(ViewBag.proveedor.NumeroProveedor);

            return View();
        }

        public ReporteProveedoresController()
        {
            if (!(Convert.ToInt32(System.Web.HttpContext.Current.Session["proveedorId"]) > 0))
                System.Web.HttpContext.Current.Session["proveedorId"] = 0;
        }

        public void Descargar(string numeroProveedor)
        {
            var detalles = _reporteProveedorManager.FindReporteProveedor(numeroProveedor);

            var proveedor = _proveedorManager.FindByNumeroProveedor(numeroProveedor);
            

            var dt = new DataTable();
            dt.Columns.Add("Id Proveedor");
            dt.Columns.Add("Nombre Proveedor");
            dt.Columns.Add("Material");
            dt.Columns.Add("Nombre Material");
            dt.Columns.Add("Fecha Proceso");
            dt.Columns.Add("Ventas Actual - 2 meses");
            dt.Columns.Add("Ventas Actual - 1 meses");
            dt.Columns.Add("Ventas Actual");
            dt.Columns.Add("Total Ventas");
            dt.Columns.Add("Calculo Total");
            dt.Columns.Add("Inventario TDA");
            dt.Columns.Add("Inventario Tránsito");
            dt.Columns.Add("Inventario CEDIS");
            dt.Columns.Add("Pedidos Pendientes(TDA + CEDIS)");
            dt.Columns.Add("Estatus Material");


            foreach (var detalle in detalles)
            {
                dt.Rows.Add(
                    detalle.NumeroProveedor,
                    detalle.NombreProveedor,
                    detalle.Material,
                    detalle.NombreMaterial,
                    detalle.FechaProceso,
                    detalle.CantidadVentas2,
                    detalle.CantidadVentas1,
                    detalle.CantidadVentas,
                    detalle.CantidadTotal,
                    detalle.CalculoTotal,
                    detalle.InvTienda,
                    detalle.InvTransito,
                    detalle.InvCedis,
                    detalle.PedidosPendiente,
                    detalle.EstadoMaterial);

            }

            FileManager.ExportExcel(dt,proveedor.NumeroProveedor, HttpContext);
        }
    }
}