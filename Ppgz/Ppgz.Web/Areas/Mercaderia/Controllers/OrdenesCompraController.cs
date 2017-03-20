using System.Web.Mvc;
using MySql.Data.MySqlClient;
using Ppgz.Repository;
using Ppgz.Services;
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
        public void Descargar(string NumeroDocumento)
        {
           // var detalle = _ordenCompraManager.FindDetalleByDocumento();
            //todo pasar a un manejador
            var commonManager = new CommonManager();
            MySqlParameter[] parametes = {
                    new MySqlParameter("NumeroDocumento", NumeroDocumento)
                };

            const string sql = @"
            SELECT * 
            FROM   ordencompradetalle
            WHERE  NumeroDocumento = @NumeroDocumento;";

            var dt = commonManager.QueryToTable(sql, parametes);
            
            FileManager.ExportExcel(dt, NumeroDocumento, HttpContext);
        }
    }
}