using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MySql.Data.MySqlClient;
using Ppgz.Repository;
using Ppgz.Services;
using ScaleWrapper;
namespace Ppgz.Web.Areas.Nazan.Controllers
{
    public class VigilanciaController : Controller
    {

        private static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["ScaleGNZN"].ConnectionString;

        public ActionResult Vigilancia()
        {
            var id = string.Format("{0}", DateTime.Now.ToString("yyyy-MM-dd"));
            var resss = DbScaleGNZN.GetDataTable("select * from GNZN_Control_vigilancia");
            ViewBag.Resss = resss;
            var r = DbScaleGNZN.GetDataTable("select * from GNZN.DBO.GNZN_DOCUMENTOS");
            ViewBag.Res = r;
            return View();
        }


        public ActionResult EnviarDatos(string gafete, string guardia, string Mercancia, HttpPostedFileBase foto, int idproveedor)
        {
            var fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            SqlCommand command = new SqlCommand(@"UPDATE GNZN_Control_vigilancia SET Gafete = '" + gafete + "'," +
                 "idGuardia='" + guardia + "',FechaIngreso='" + fecha + "',MercanciaNoNazan='" + Mercancia + "'  WHERE IdCtrl =" + idproveedor + "");
            SqlConnection con = new SqlConnection(ConnectionString);
            con.Open();
            command.Connection = con;
            command.ExecuteNonQuery();

            TempData["FlashSuccess"] = "Entrada registrada exitosamente";
            return RedirectToAction("Vigilancia");
        }


    }
}