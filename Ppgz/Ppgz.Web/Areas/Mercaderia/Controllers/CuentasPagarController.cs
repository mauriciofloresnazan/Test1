using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using Ppgz.Repository;
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


            var result = db.cuentasxpagars
                .Where(
                    c =>
                        tiposMovimientos.Contains(c.TipoMovimiento) && c.Referencia == null &&
                        c.ProveedoresId == proveedorId).ToList();

            decimal total = 0;

            foreach (var data in result)
            {
                total = total + decimal.Parse(data.Importe);
            }

            ViewBag.importeTotal = total.ToString("f2");

            ViewBag.proveedor = _proveedorManager.Find(proveedorId);
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

            ViewBag.proveedor = _proveedorManager.Find(proveedorId);
            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult Devoluciones(int proveedorId)
        {
            var commonManager = new CommonManager();


            MySqlParameter[] parametes = {
                    new MySqlParameter("id", proveedorId)
                };


            const string sql = @"
            SELECT cp.*, p.Rfc, p.NombreProveedor 
            FROM   cuentasxpagar cp
                   JOIN proveedores p ON p.Id = cp.Proveedoresid
            WHERE  cp.Proveedoresid = @id AND cp.TipoMovimiento = 'KR';";

            ViewBag.devoluciones = commonManager.QueryToTable(sql, parametes);
            return View();
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

            StreamWriter wr = new StreamWriter(@"c:\\temp\" + nombreXls + ".xls");
            try
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    wr.Write(dt.Columns[i].ToString().ToUpper() + "\t");
                }
                wr.WriteLine();

                //write rows to excel file
                for (int i = 0; i < (dt.Rows.Count); i++)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (dt.Rows[i][j] != null)
                        {
                            wr.Write(Convert.ToString(dt.Rows[i][j]) + "\t");
                        }
                        else
                        {
                            wr.Write("\t");
                        }
                    }
                    //go to next line
                    wr.WriteLine();
                }
                //close file
                wr.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}