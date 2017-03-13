using System.Collections.Generic;
using System.Data.SqlClient;
using System.EnterpriseServices.Internal;
using System.Linq;
using System.Reflection.Emit;
using System.Web.Mvc;
using System.Web.Util;
using MySql.Data.MySqlClient;
using Ppgz.Web.Infrastructure;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class CuentasPagarController : Controller
    {

        readonly ProveedorManager _proveedorManager = new ProveedorManager();
        readonly CommonManager _commonManager = new CommonManager();
        readonly  CuentasPorPagarManager _cuentasPorPagarManager = new CuentasPorPagarManager();

        //
        // GET: /Nazan/CuentasPagar/
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult Index()
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();


            ViewBag.proveedores = _proveedorManager.FindByCuentaId(cuenta.Id);

            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult Pagos(int proveedorId)
        {
            ViewBag.pagos  = _cuentasPorPagarManager.FindPagosByProveedorId(proveedorId);


            /* var commonManager = new CommonManager();

  
            MySqlParameter[] parametes = {
                    new MySqlParameter("id", proveedorId)
                };


            const string sql = @"
            SELECT * 
            FROM   cuentasxpagar 
            WHERE  Proveedoresid = @id AND Referencia IS NULL;";

            ViewBag.pagos = commonManager.QueryToTable(sql, parametes);
            */


            ViewBag.proveedor = _proveedorManager.Find(proveedorId);
            return View();
        }



        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public JsonResult PagoDetalle(string numeroCompensacion)
        {
            var transacciones = _cuentasPorPagarManager.FindPagoDetalleByNumeroCompensacion(numeroCompensacion).ToList();
            
            var jsonData = new
            {
                data = from transaccion in transacciones select transaccion
            };
            return Json(jsonData, JsonRequestBehavior.AllowGet);

        }



        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult PagosPendientes()
        {
            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult PagosPendientesDetalle()
        {
            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult Devoluciones()
        {
            return View();
        }
        
    }
}