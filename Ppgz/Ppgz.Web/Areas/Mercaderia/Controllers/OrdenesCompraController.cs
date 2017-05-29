using System;
using System.IO;
using System.Web.Mvc;
using ClosedXML.Excel;
using Ppgz.Services;
using Ppgz.Web.Infrastructure;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class OrdenesCompraController : Controller
    {
        private readonly OrdenCompraManager  _ordenCompraManager = new OrdenCompraManager();
        private readonly CommonManager _commonManager = new CommonManager();


        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-ORDENESCOMPRA")]
        public ActionResult Index()
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();
    
            ViewBag.data= _ordenCompraManager.FindOrdenesDecompraActivasByCuenta(cuenta.Id);

            return View();
        }


        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-ORDENESCOMPRA")]
        public void Descargar(string numeroDocumento, int proveedorId)
        {
            var orden = _ordenCompraManager.FindOrdenConDetalles(proveedorId, numeroDocumento);
        

            var workbook = new XLWorkbook(Server.MapPath(@"~/App_Data/plantillaoc.xlsx"));
            var ws = workbook.Worksheet(1);

            var proveedorManager = new ProveedorManager();
            var proveedor = proveedorManager.Find(proveedorId);

            ws.Cell(3, "B").Value = string.Format("{0} {1} {2} {3}", proveedor.Nombre1, proveedor.Nombre2, proveedor.Nombre3, proveedor.Nombre4);
            ws.Cell(3, "D").Value = proveedor.Rfc;
            ws.Cell(5, "B").Value = numeroDocumento;
            ws.Cell(6, "B").Value = orden.FechaEntrega.ToString("dd/MM/yyyy");

            var row = 9;
            foreach (var detalle in orden.Detalles)
            {
                ws.Cell(row, "A").Value = detalle.NumeroMaterial;
                ws.Cell(row, "B").Value = detalle.Descripcion;
                ws.Cell(row, "C").Value = detalle.Centro;
                ws.Cell(row, "D").Value = detalle.CantidadPedido;
                ws.Cell(row, "E").Value = detalle.PrecioNeto;

                row++;
            }

            FileManager.ExportExcel(workbook, "ORDEN" + numeroDocumento, HttpContext);
        }
    }
}