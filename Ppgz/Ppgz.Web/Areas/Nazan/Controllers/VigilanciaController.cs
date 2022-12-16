using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;
using ScaleWrapper;
namespace Ppgz.Web.Areas.Nazan.Controllers
{
    public class VigilanciaController : Controller
    {

        private static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["ScaleGNZN"].ConnectionString;

        public ActionResult Vigilancia()
        {
            //ViewBag.Resss = "";
            //ViewBag.Res = "";
            //var id = string.Format("{0}", DateTime.Now.ToString("yyyy-MM-dd"));
            //var resss = DbScaleGNZN.GetDataTable(@"select
            //IdCtrl,convert(int,IdCita) IdCita, IdProv, NombreProv, FechaCita, HoraCita24, convert(int,CantidadCita) CantidadCita, Marca, Estatus, Gafete, IdGuardia, FechaIngreso, fechaSalida, MercanciaNoNazan, ParesEnValesSalida
            //                 from GNZN_Control_vigilancia");
            //ViewBag.Resss = resss;
            //var r = DbScaleGNZN.GetDataTable("select * from GNZN.DBO.GNZN_DOCUMENTOS");
            //ViewBag.Res = r;

            DataSet ds = DbScaleGNZN.GetDataSet(@"select
                                        IdCtrl,convert(int,IdCita) IdCita, convert(int,IdProv) IdProv, NombreProv, 
                                        FechaCita, HoraCita24, convert(int,CantidadCita) CantidadCita, Marca, Estatus, Gafete, 
                                        IdGuardia, FechaIngreso, fechaSalida, MercanciaNoNazan, ParesEnValesSalida
                                         from GNZN.DBO.GNZN_Control_vigilancia GO
                                select * from GNZN.DBO.GNZN_DOCUMENTOS GO");
            if (ds.Tables[0].Rows.Count > 0)
            {
                ViewBag.Resss = ds.Tables[0];
            }
            if (ds.Tables[1].Rows.Count > 0)
            {
                ViewBag.Res = ds.Tables[1];
            }
            
            
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