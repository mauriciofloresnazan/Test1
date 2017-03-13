using System.Collections.Generic;
using System.Data.SqlClient;
using System.EnterpriseServices.Internal;
using System.Linq;
using System.Reflection.Emit;
using System.Web.Mvc;
using System.Web.Util;
<<<<<<< HEAD
using Ppgz.Repository;
=======
using MySql.Data.MySqlClient;
>>>>>>> b76980783290e9421e738925f9a1dd95b935ab26
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
        public ActionResult PagosPendientes(int proveedorId)
        {
            Entities db = new Entities();

            string[] tiposMovimientos = new string[] {"10","21","4"};

            var datos  = db.cuentasxpagars.Where(c => tiposMovimientos.Contains(c.TipoMovimiento) && c.Referencia == null).ToList();

            var result = db.cuentasxpagars
                .Where(c => tiposMovimientos.Contains(c.TipoMovimiento) && c.Referencia == null && c.ProveedoresId == proveedorId)
                .GroupBy(c => c.ProveedoresId)
                   .Select(r => new {  total = r.Sum(i => decimal.Parse( i.Importe)) });
            ViewBag.data = result;

            ViewBag.proveedor = _proveedorManager.Find(proveedorId);
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