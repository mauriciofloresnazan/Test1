using System;
using System.Web.Mvc;
using System.Globalization;
using Ppgz.Repository;
using Ppgz.Services;
using Ppgz.Web.Areas.Nazan.Models;
using Ppgz.Web.Infrastructure;
using System.Linq;
using ClosedXML.Excel;
using System.Collections;
using System.Data;
using System.Collections.Generic;
using System.Text;
using ScaleWrapper;
using System.Web;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.IO;
using System.Drawing;
using Microsoft.AspNet.Identity;

namespace Ppgz.Web.Areas.Nazan.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class ReporteMuestrasController : Controller
    {
        private readonly Entities _db = new Entities();

        readonly ProveedorManager _proveedorManager = new ProveedorManager();
        readonly CuentaManager _cuentaManager = new CuentaManager();
        readonly CommonManager _commonManager = new CommonManager();
        internal proveedore ProveedorActivo
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["proveedoractivo"] != null)
                {
                    return (proveedore)System.Web.HttpContext.Current.Session["proveedoractivo"];
                }
                return null;
            }
            set
            {
                System.Web.HttpContext.Current.Session["proveedoractivo"] = value;
            }
        }

        internal CuentaConUsuarioMaestro CuentaActiva
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["cuentaactiva"] != null)
                {
                    return (CuentaConUsuarioMaestro)System.Web.HttpContext.Current.Session["cuentaactiva"];
                }
                return null;
            }
            set
            {
                System.Web.HttpContext.Current.Session["cuentaactiva"] = value;
            }
        }

        internal string SociedadActiva
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["sociedadactiva"] != null)
                {
                    return (string)System.Web.HttpContext.Current.Session["sociedadactiva"];
                }
                return null;
            }
            set
            {
                System.Web.HttpContext.Current.Session["sociedadactiva"] = value;
            }
        }

        private static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        //private static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["portalqa"].ConnectionString;

        public ActionResult Inicio()
        {
            var resultado = TempData["resultado"];
            ViewBag.Res = resultado;
            return View();
        }


        public ActionResult ModificarMuestras()
        {
            var resultado = TempData["resultado"];
            ViewBag.Res = resultado;
            return View();
        }
        public ActionResult AutorizarMuestras()
        {
            var resultado = TempData["resultado"];
            ViewBag.Res = resultado;
            return View();
        }

        public ActionResult ModificarM(int numero)
        {
            var user = User.Identity.GetUserName().ToUpper();
            var res = Db.GetDataTable(@"SELECT id ,imagen,NombreProveedor,contacto,MarcaAgrupa,
Estilo,Color,Acabado,NombreMaterial,MaterialSuela,
altura,Tallas,EM,Costo,ComentarioFinal,FechaCarga,
Estatus,canalm,id_d      
FROM muestras, canalmuestras  WHERE id = id_muestras and id_muestras='" + numero + "' and canalm='" + user + "'");
            ViewBag.Res = res;
            foreach (DataRow rows in res.Rows)
            {
                var canalm = rows.ItemArray[16];
                var ca = Convert.ToString(canalm);
                if (ca =="30"|| ca == "40" || ca == "50" || ca == "60" || ca == "70")
                {
                    TempData["resultado"] = ViewBag.Res;
                    TempData["FlashError"] = "Esta muestra ya se autorizo anteriormente";
                    return RedirectToAction("ModificarMuestras");
                }
                else { 
                MySqlCommand command = new MySqlCommand(@"UPDATE canalmuestras SET Estatus = '20'  WHERE id_muestras = '" + numero + "' and canalm='"+user+"' ");
                MySqlConnection con = new MySqlConnection(ConnectionString);
                con.Open();
                command.Connection = con;
                command.ExecuteNonQuery();
                TempData["resultado"] = ViewBag.Res;
                TempData["FlashSuccess"] = "Muestra actualizada correctamente";
                return RedirectToAction("ModificarMuestras");
                }
            }
            TempData["FlashError"] = "No existe este id para este canal";
            return RedirectToAction("ModificarMuestras");
        }

        public ActionResult Autorizar(int numero)
        {
            var user = User.Identity.GetUserName().ToUpper();
            var res = Db.GetDataTable(@"SELECT id ,imagen,NombreProveedor,contacto,MarcaAgrupa,
Estilo,Color,Acabado,NombreMaterial,MaterialSuela,
altura,Tallas,EM,Costo,ComentarioFinal,FechaCarga,
Estatus,canalm,id_d      
FROM muestras, canalmuestras  WHERE id = id_muestras and id_muestras='" + numero + "' and canalm='" + user + "'");
            ViewBag.Res = res;
            foreach (DataRow rows in res.Rows)
            {
                var canalm = rows.ItemArray[16];
                var ca = Convert.ToString(canalm);
                if (ca == "30" || ca == "40" || ca == "50" || ca == "60" || ca == "70")
                {
                    TempData["resultado"] = ViewBag.Res;
                    TempData["FlashError"] = "Esta muestra ya se autorizo anteriormente";
                    return RedirectToAction("AutorizarMuestras");
                }
                else
                {
                    MySqlCommand command = new MySqlCommand(@"UPDATE canalmuestras SET Estatus = '30'  WHERE id_muestras = '" + numero + "' and canalm='" + user + "' ");
                    MySqlConnection con = new MySqlConnection(ConnectionString);
                    con.Open();
                    command.Connection = con;
                    command.ExecuteNonQuery();
                    TempData["resultado"] = ViewBag.Res;
                    TempData["FlashSuccess"] = "Muestra autorizada correctamente";
                    return RedirectToAction("AutorizarMuestras");
                }
                    
               
            }
            TempData["FlashError"] = "No existe este id para este canal";
            return RedirectToAction("AutorizarMuestras");
        }
        public ActionResult IngresarMuestras(int numero)
        {
            var user = User.Identity.GetUserName().ToUpper();
            var res = Db.GetDataTable(@"SELECT id ,imagen,NombreProveedor,contacto,MarcaAgrupa,
Estilo,Color,Acabado,NombreMaterial,MaterialSuela,
altura,Tallas,EM,Costo,ComentarioFinal,FechaCarga,
Estatus,canalm,id_d      
FROM muestras, canalmuestras  WHERE id = id_muestras and id_muestras='" + numero + "' and canalm='"+user+"'");
            ViewBag.Res = res;
            foreach (DataRow rows in res.Rows)
            {
                var id = rows.ItemArray[18];
                var ids = Convert.ToString(id);
                var canalm = rows.ItemArray[16];
                var ca = Convert.ToString(canalm);
                if (ca== "0")
                {
                    MySqlCommand command = new MySqlCommand(@"UPDATE canalmuestras SET Estatus = '10'  WHERE id_muestras = '" + numero + "' and canalm='"+user+"' ");
                    MySqlConnection con = new MySqlConnection(ConnectionString);
                    con.Open();
                    command.Connection = con;
                    command.ExecuteNonQuery();
                    TempData["resultado"] = ViewBag.Res;
                    TempData["FlashSuccess"] = "Muestra ingresada correctamente";
                    return RedirectToAction("Inicio");
                }
                else
                {
                    TempData["resultado"] = ViewBag.Res;
                    TempData["FlashError"] = "El id de la muestra ya esta ingresado";
                    return RedirectToAction("Inicio");
                }

            }
            TempData["FlashError"] = "No existe este id para este canal";
            return RedirectToAction("Inicio");
        }
        public ActionResult Seleccionar()
        {
            ViewBag.Proveedores = _proveedorManager.FindByCuenta();
            return View();
        }
        public ActionResult SeleccionarA()
        {
            ViewBag.Proveedores = _proveedorManager.FindByCuenta();
            return View();
        }
        public ActionResult Muestras(string fechaDesde = null, string fechaHasta = null)
        {

         var user=User.Identity.GetUserName().ToUpper();
            if (!String.IsNullOrEmpty(fechaDesde) && !String.IsNullOrEmpty(fechaHasta))
            {

                var fechaf = DateTime.ParseExact(fechaDesde, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                var fechat = DateTime.ParseExact(fechaHasta, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");

                ViewBag.Res = Db.GetDataTable(@"SELECT id ,imagen,NombreProveedor,contacto,MarcaAgrupa,
Estilo,Color,Acabado,NombreMaterial,MaterialSuela,
altura,Tallas,EM,Costo,ComentarioFinal,FechaCarga,
Estatus    
FROM muestras, canalmuestras       
WHERE id = id_muestras and Estatus in (10,20) and canalm='"+user+"' and muestras.FechaCarga >='" + fechaf+ "' and  muestras.FechaCarga <= '"+fechat+"'");
            }
            else
            {

                ViewBag.Res = Db.GetDataTable(@"SELECT id ,imagen,NombreProveedor,contacto,MarcaAgrupa,
Estilo,Color,Acabado,NombreMaterial,MaterialSuela,
altura,Tallas,EM,Costo,ComentarioFinal,FechaCarga,
Estatus    
FROM muestras, canalmuestras       
WHERE id = id_muestras and Estatus in (10,20) and canalm='"+user+"'") ;
            }
            return View();
        }

        public ActionResult MuestrasA(string fechaDesde = null, string fechaHasta = null)
        {
            var user = User.Identity.GetUserName().ToUpper();
            if (!String.IsNullOrEmpty(fechaDesde) && !String.IsNullOrEmpty(fechaHasta))
            {

                var fechaf = DateTime.ParseExact(fechaDesde, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                var fechat = DateTime.ParseExact(fechaHasta, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");

                ViewBag.Res = Db.GetDataTable(@"SELECT id ,imagen,NombreProveedor,contacto,MarcaAgrupa,
Estilo,Color,Acabado,NombreMaterial,MaterialSuela,
altura,Tallas,EM,Costo,ComentarioFinal,FechaCarga,
Estatus,canalm   
FROM muestras, canalmuestras       
WHERE id = id_muestras and Estatus in (30,40,50,60,70) and canalm='"+user+"' and FechaCarga >='" + fechaf + "' and  FechaCarga <= '" + fechat + "' ");
            }
            else
            {

                ViewBag.Res = Db.GetDataTable(@"SELECT id ,imagen,NombreProveedor,contacto,MarcaAgrupa,
Estilo,Color,Acabado,NombreMaterial,MaterialSuela,
altura,Tallas,EM,Costo,ComentarioFinal,FechaCarga,
Estatus,canalm   
FROM muestras, canalmuestras       
WHERE id = id_muestras and Estatus in (30,40,50,60,70) and canalm='" + user + "'");
            }
            return View();
        }
        public ActionResult Resultado(string Id)
        {

            var Res = Db.GetDataTable("SELECT * FROM muestras where id in (" + Id + ") ");
            ViewBag.Res = Res;
            TempData["resultado"] = Id;
            TempData["FlashSuccess"] = "Excel generado correctamente";
            return View();
        }
        public ActionResult ResultadoA(string Id)
        {

            var Res = Db.GetDataTable("SELECT * FROM muestras where id in (" + Id + ") ");
            ViewBag.Res = Res;
            TempData["resultado"] = Id;
            TempData["FlashSuccess"] = "Excel generado correctamente";
            return View();
        }
        public ActionResult Descargar()
        {
            var i= TempData["resultado"];
            if (i == null) {
                TempData["FlashError"] = " ya se descargo esta informacion,debes seleccionar informacion nuevamente";
                return RedirectToAction("Muestras");
            }
            var workbook = new XLWorkbook(Server.MapPath(@"~/App_Data/plantillamuestras.xlsx"));
            var ws = workbook.Worksheet(1);

            var Res = Db.GetDataTable("SELECT * FROM muestras where id in (" + i + ") ");
            var row = 3;
            foreach (DataRow rows in Res.Rows)
            {

                byte[] Imagen = (byte[])rows.ItemArray[1];
                string valor = Convert.ToBase64String(Imagen);
                MemoryStream ms = new MemoryStream(Imagen);

                ws.Cell(row, "A").Value = rows.ItemArray[0];
                var images = ws.AddPicture(new MemoryStream((byte[])rows["imagen"]))
                                        .MoveTo(ws.Cell(row, 2))
                                        .WithSize(180, 102);

                ws.Cell(row, "C").Value = rows.ItemArray[2];
                ws.Cell(row, "D").Value = rows.ItemArray[3];
                ws.Cell(row, "E").Value = rows.ItemArray[4];
                ws.Cell(row, "F").Value = rows.ItemArray[6];
                ws.Cell(row, "G").Value = rows.ItemArray[7];
                ws.Cell(row, "H").Value = rows.ItemArray[8];
                ws.Cell(row, "I").Value = rows.ItemArray[9];
                ws.Cell(row, "J").Value = rows.ItemArray[10];
                ws.Cell(row, "K").Value = rows.ItemArray[11];
                ws.Cell(row, "L").Value = rows.ItemArray[12];
                ws.Cell(row, "M").Value = rows.ItemArray[13];
                ws.Cell(row, "N").Value = rows.ItemArray[14];
                ws.Cell(row, "O").Value = rows.ItemArray[15];
                row++;
            }
            FileManager.ExportExcel(workbook, "Muestras", HttpContext);


            return View();
        }

        public ActionResult DescargarA()
        {

            var i = TempData["resultado"];
            if (i == null)
            {
                TempData["FlashError"] = " ya se descargo esta informacion,debes seleccionar informacion nuevamente";
                return RedirectToAction("MuestrasA");
            }
            var workbook = new XLWorkbook(Server.MapPath(@"~/App_Data/plantillamuestras.xlsx"));
            var ws = workbook.Worksheet(1);

            var Res = Db.GetDataTable("SELECT * FROM muestras where id in (" + i + ") ");
            var row = 3;
            foreach (DataRow rows in Res.Rows)
            {

                byte[] Imagen = (byte[])rows.ItemArray[1];
                string valor = Convert.ToBase64String(Imagen);
                MemoryStream ms = new MemoryStream(Imagen);

                ws.Cell(row, "A").Value = rows.ItemArray[0];
                var images = ws.AddPicture(new MemoryStream((byte[])rows["imagen"]))
                                        .MoveTo(ws.Cell(row, 2))
                                        .WithSize(180, 102);

                ws.Cell(row, "C").Value = rows.ItemArray[2];
                ws.Cell(row, "D").Value = rows.ItemArray[3];
                ws.Cell(row, "E").Value = rows.ItemArray[4];
                ws.Cell(row, "F").Value = rows.ItemArray[6];
                ws.Cell(row, "G").Value = rows.ItemArray[7];
                ws.Cell(row, "H").Value = rows.ItemArray[8];
                ws.Cell(row, "I").Value = rows.ItemArray[9];
                ws.Cell(row, "J").Value = rows.ItemArray[10];
                ws.Cell(row, "K").Value = rows.ItemArray[11];
                ws.Cell(row, "L").Value = rows.ItemArray[12];
                ws.Cell(row, "M").Value = rows.ItemArray[13];
                ws.Cell(row, "N").Value = rows.ItemArray[14];
                ws.Cell(row, "O").Value = rows.ItemArray[15];
                row++;
            }
            FileManager.ExportExcel(workbook, "Muestras", HttpContext);


            return View();
        }

    }
}


       