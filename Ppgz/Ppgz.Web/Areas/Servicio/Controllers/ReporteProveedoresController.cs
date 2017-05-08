using System;
using System.Data;
using System.Web.Mvc;
using Ppgz.Services;
using Ppgz.Web.Infrastructure;

namespace Ppgz.Web.Areas.Servicio.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class ReporteProveedoresController : Controller
    {
      
        private readonly CommonManager _commonManager = new CommonManager();
        readonly ProveedorManager _proveedorManager = new ProveedorManager();
        readonly ReporteProveedorManager _reporteProveedorManager = new ReporteProveedorManager();

        [Authorize(Roles = "MAESTRO-SERVICIO,SERVICIO-REPORTESPROVEEDORES")]
        public ActionResult Index()
        {
 
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            ViewBag.proveedores = _proveedorManager.FindByCuentaId(cuenta.Id);

           

            return View();
        }

        [Authorize(Roles = "MAESTRO-SERVICIO,SERVICIO-REPORTESPROVEEDORES")]
        public ActionResult Reportes(int proveedorId = 0)
        {
            int id = proveedorId > 0 ? proveedorId : Convert.ToInt32(System.Web.HttpContext.Current.Session["proveedorId"]);

            if (id == 0)
                return RedirectToAction("Index");

            ViewBag.proveedor = _proveedorManager.Find(id);
            try
            {
                ViewBag.reportes = _reporteProveedorManager.FindReporteProveedor(ViewBag.proveedor.NumeroProveedor);

                ViewBag.nivelservicio = _reporteProveedorManager.FindNivelSerNiveleseervicio(ViewBag.proveedor.NumeroProveedor);

            }
            catch (Exception ex)
            {
                if (ex.Message == "NOT_DATA_FOUND")
                {
                    TempData["FlashError"] = "No hay datos para este proveedor"; 
                    return RedirectToAction("Index");
                }


                throw;
            }

            return View();
        }

        [Authorize(Roles = "MAESTRO-SERVICIO,SERVICIO-REPORTESPROVEEDORES")]
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
            dt.Columns.Add("Unidad de Medida");
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
                    detalle.FechaProceso.ToString("dd/MM/yyyy"),
                    detalle.UnidadMedida,
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