using System.Data;
using System.Web.Mvc;
using Ppgz.Web.Infrastructure;
using OrdenCompraManager = Ppgz.Services.OrdenCompraManager;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class OrdenesCompraController : Controller
    {
        private readonly OrdenCompraManager  _ordenCompraManager = new OrdenCompraManager();
        private readonly CommonManager _commonManager = new CommonManager();
        //
        // GET: /Mercaderia/ComprobantesRecibo/
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-ORDENESCOMPRA-LISTAR,MERCADERIA-ORDENESCOMPRA-MODIFICAR")]
        public ActionResult Index()
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();
    
            ViewBag.data= _ordenCompraManager.FindOrdenesDecompraActivasByCuenta(cuenta.Id);

            return View();
        }
        public void Descargar(string numeroDocumento, int proveedorId)
        {
            var detalles = _ordenCompraManager.FindDetalle(numeroDocumento, proveedorId);

            var dt = new DataTable();
            dt.Columns.Add("Orde de Compra");
            dt.Columns.Add("Material");
            dt.Columns.Add("Descripcion");
            dt.Columns.Add("Centro");
            dt.Columns.Add("Almacen");
            dt.Columns.Add("Cantidad");
            dt.Columns.Add("Precio");
            

            foreach (var detalle in detalles)
            {
                dt.Rows.Add(
                    detalle.NumeroDocumento,
                    detalle.NumeroMaterial,
                    detalle.DescripcionMaterial,
                    detalle.Centro,
                    detalle.Almacen, 
                    detalle.CantidadPedido,
                    detalle.PrecioNeto);

            }
            
            FileManager.ExportExcel(dt, "ORDEN" + numeroDocumento, HttpContext);
        }
    }
}