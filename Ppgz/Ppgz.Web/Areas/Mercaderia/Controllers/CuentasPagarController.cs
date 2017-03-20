using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using Ppgz.Repository;
using Ppgz.Services;
using Ppgz.Web.Infrastructure;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class CuentasPagarController : Controller
    {

        readonly CommonManager _commonManager = new CommonManager();
        readonly CuentasPorPagarManager _cuentasPorPagarManager = new CuentasPorPagarManager();
        readonly ProveedorManager _proveedorManager = new ProveedorManager();
        public CuentasPagarController()
        {
            if (!(Convert.ToInt32(System.Web.HttpContext.Current.Session["proveedorId"]) > 0))
                System.Web.HttpContext.Current.Session["proveedorId"] = 0;
        }

        public void Descargar(int id)
        {

            var commonManager = new CommonManager();

            MySqlParameter[] parametes = {
					new MySqlParameter("id", id)
				};


            const string sql = @"
			SELECT * 
			FROM   cuentasxpagar 
			WHERE TipoMovimiento in(10,21,4) AND ProveedoresId = @id;";

            var dt = commonManager.QueryToTable(sql, parametes);

            ExportExcel(dt, id.ToString());


        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult Devoluciones(int proveedorId = 0)
        {


            int id = proveedorId > 0 ? proveedorId : Convert.ToInt32(System.Web.HttpContext.Current.Session["proveedorId"]);

            if (id == 0)
                return RedirectToAction("Index");
            var commonManager = new CommonManager();
            MySqlParameter[] parametes = {
					new MySqlParameter("id", id)
				};
            const string sql = @"
				SELECT * 
				FROM   devoluciones d
				JOIN   proveedores p ON p.Id = d.ProveedorId
				WHERE  p.Id = @id";
            //ViewBag.devoluciones = commonManager.QueryToTable(sql, parametes);
            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult DevolucionesDetalle(int id)
        {
            var commonManager = new CommonManager();
            MySqlParameter[] parametes = {
					new MySqlParameter("id", id)
				};
            const string sql = @"
				SELECT * 
				FROM   devoluciones d
				WHERE  d.Id = @id";
            ViewBag.devolucion = commonManager.QueryToTable(sql, parametes).Rows[0];



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

            return View();
        }

        public void DevolucionesDetalleDescargar(int id)
        {
            var commonManager = new CommonManager();
            MySqlParameter[] parametes = {
					new MySqlParameter("id", id)
				};
            const string sql = @"
				SELECT * 
				FROM   devoluciones d
				WHERE  d.Id = @id";
            var data = commonManager.QueryToTable(sql, parametes).Rows[0];

            var cantidad = int.Parse(data["cantidad"].ToString());

            var dt = new DataTable();

            dt.Columns.Add(new DataColumn("Articulo"));
            dt.Columns.Add(new DataColumn("Descripcion"));
            dt.Columns.Add(new DataColumn("Total"));
            dt.Columns.Add(new DataColumn("Cantidad"));

            while (cantidad > 0)
            {

                dt.Rows.Add(new[] { data["Material"], data["Descripcion"], 230.5, 1 });


                cantidad--;
            }
            ExportExcel(dt, id.ToString());
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

        }

        public void ExportExcel(DataTable dt, string nombreXls)
        {

            var grid = new GridView();
            grid.DataSource = dt;
            grid.DataBind();

            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + nombreXls + ".xls");
            Response.ContentType = "application/ms-excel";

            Response.Charset = "";
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);

            grid.RenderControl(htw);

            Response.Output.Write(sw.ToString());
            Response.Flush();
            Response.End();

            return;

        }

        // GET: /Nazan/CuentasPagar/
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult Index()
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();


            ViewBag.proveedores = _proveedorManager.FindByCuentaId(cuenta.Id);

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
        public ActionResult Pagos(int proveedorId = 0)
        {
            int id = proveedorId > 0 ? proveedorId : Convert.ToInt32(System.Web.HttpContext.Current.Session["proveedorId"]);

            if (id == 0)
                return RedirectToAction("Index");

            ViewBag.pagos = _cuentasPorPagarManager.FindPagosByProveedorId(id);
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
            ViewBag.proveedor = _proveedorManager.Find(id);
            return View();
        }
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult PagosDetalle(string numeroCompensacion)
        {
            var data = _cuentasPorPagarManager.FindPagoDetalleByNumeroCompensacion(numeroCompensacion).ToList();

            ViewBag.pagos = data;

            var id = Convert.ToInt32(System.Web.HttpContext.Current.Session["proveedorId"]);

            ViewBag.proveedor = _proveedorManager.Find(id);

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

            return View();
        }


        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult PagosPendientes(int proveedorId = 0)
        {
            int id = proveedorId > 0 ? proveedorId : Convert.ToInt32(System.Web.HttpContext.Current.Session["proveedorId"]);

            if (id == 0)
                return RedirectToAction("Index");
            Entities db = new Entities();
            string[] tiposMovimientos = new string[] { "10", "21", "4" };
            var result = db.cuentasxpagars
                .Where(
                    c =>
                        tiposMovimientos.Contains(c.TipoMovimiento) && c.Referencia == null &&
                        c.ProveedoresId == id).ToList();

            decimal total = 0;
            foreach (var data in result)
            {
                total = total + decimal.Parse(data.Importe);
            }
            ViewBag.importeTotal = total.ToString("f2");
            ViewBag.proveedor = _proveedorManager.Find(id);
            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult PagosPendientesDetalle(int proveedorId)
        {
            Entities db = new Entities();

            string[] tiposMovimientos = new string[] { "10", "21", "4" };


            var result = db.cuentasxpagars
                .Where(
                    c =>
                        tiposMovimientos.Contains(c.TipoMovimiento) && c.Referencia == null &&
                        c.ProveedoresId == proveedorId).ToList();



            ViewBag.data = result;
            var total = result.Aggregate<cuentasxpagar, decimal>(0, (current, data) => current + decimal.Parse(data.Importe));
            ViewBag.importeTotal = total.ToString("f2");

            ViewBag.proveedor = _proveedorManager.Find(proveedorId);
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


    }
}