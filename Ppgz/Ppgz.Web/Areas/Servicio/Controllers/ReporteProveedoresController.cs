using System;
using System.Web.Mvc;
using ClosedXML.Excel;
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


                if (ViewBag.nivelservicio == null)
                {
                    TempData["FlashError"] = "No hay datos para este proveedor";
                    return RedirectToAction("Index");
                }
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

            var nivelServicio = _reporteProveedorManager.FindNivelSerNiveleseervicio(proveedor.Id);


            var workbook = new XLWorkbook(Server.MapPath(@"~/App_Data/plantillareporte.xlsx"));
            var ws = workbook.Worksheet(1);

            ws.Cell(4, "C").Value = string.Format("{0} {1} {2} {3}", proveedor.Nombre1, proveedor.Nombre2, proveedor.Nombre3, proveedor.Nombre4);
            ws.Cell(5, "C").Value = proveedor.Rfc;

            if (nivelServicio != null)
            {
                ws.Cell(9, "C").Value = nivelServicio.UltimoMes / 100;
                ws.Cell(10, "C").Value = nivelServicio.TemporadaActual / 100;
                ws.Cell(11, "C").Value = nivelServicio.AcumuladoAnual / 100;

                ws.Cell(9, "F").Value = nivelServicio.PedidoAtrasado;
                ws.Cell(10, "F").Value = nivelServicio.PedidoEnTiempo;
                ws.Cell(11, "F").Value = nivelServicio.PedidoTotal;
            }

            ws.Cell(4, "M").Value = detalles[0].FechaProceso.ToString("dd/MM/yyyy");

            ws.Cell(14, "D").Value = string.Format("Ventas ({0})", DateTime.Today.AddMonths(-2).ToString("MM/yyyy"));
            ws.Cell(14, "E").Value = string.Format("Ventas ({0})", DateTime.Today.AddMonths(-1).ToString("MM/yyyy"));
            ws.Cell(14, "F").Value = string.Format("Ventas ({0})", DateTime.Today.ToString("MM/yyyy"));

            var row = 15;

            foreach (var detalle in detalles)
            {


                ws.Cell(row, "B").Value = detalle.Material.TrimStart('0');
                ws.Cell(row, "C").Value = detalle.NombreMaterial;
                ws.Cell(row, "D").Value = detalle.CantidadVentas2;
                ws.Cell(row, "E").Value = detalle.CantidadVentas1;
                ws.Cell(row, "F").Value = detalle.CantidadVentas;
                ws.Cell(row, "G").Value = detalle.CantidadTotal;
                ws.Cell(row, "H").Value = detalle.CalculoTotal;
                ws.Cell(row, "I").Value = detalle.InvTienda;
                ws.Cell(row, "J").Value = detalle.InvTransito;
                ws.Cell(row, "K").Value = detalle.InvCedis;
                ws.Cell(row, "L").Value = detalle.PedidosPendiente;
                ws.Cell(row, "M").Value = detalle.EstadoMaterial;
                row++;
            }

            FileManager.ExportExcel(workbook, "REP_" + proveedor.Rfc + "_" + detalles[0].FechaProceso.ToString("ddMMyyyy"), HttpContext);




        }

    }
}