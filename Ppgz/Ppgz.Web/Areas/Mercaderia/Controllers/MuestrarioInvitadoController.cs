using BarcodeLib;
using ClosedXML.Excel;
using MySql.Data.MySqlClient;
using Ppgz.Repository;
using Ppgz.Services;
using Ppgz.Web.Infrastructure;
using Ppgz.Web.Models.ProntoPago;
using SapWrapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static SapWrapper.SapFacturaManager;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class MuestrarioInvitadoController : Controller
    {
        readonly CommonManager _commonManager = new CommonManager();
        readonly ProveedorManager _proveedorManager = new ProveedorManager();
        readonly FacturaManager _facturaManager = new FacturaManager();
        readonly LogsFactoraje _logsFactoraje = new LogsFactoraje();

        internal proveedore ProveedorCxp
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["proveedorcxp"] != null)
                {
                    return (proveedore)System.Web.HttpContext.Current.Session["proveedorcxp"];
                }
                return null;
            }
            set
            {
                System.Web.HttpContext.Current.Session["proveedorcxp"] = value;
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
        public ActionResult InicioC()
        {
            var resultado = TempData["resultado"];
            ViewBag.Res = resultado;
            return View();
        }
        public ActionResult InicioP()
        {
            var resultado = TempData["resultado"];
            ViewBag.Res = resultado;
            return View();
        }
        public ActionResult InicioA()
        {
            var resultado = TempData["resultado"];
            ViewBag.Res = resultado;
            return View();
        }
        public ActionResult InicioE()
        {
            var resultado = TempData["resultado"];
            ViewBag.Res = resultado;
            return View();
        }
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-MUESTRASINVITADOS")]
        public ActionResult MuestrasInvitado(string marca, string fechaDesde = null, string fechaHasta = null)
        {

            if (!String.IsNullOrEmpty(fechaDesde) && !String.IsNullOrEmpty(fechaHasta))
            {
                var fechaf = DateTime.ParseExact(fechaDesde, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                var fechat = DateTime.ParseExact(fechaHasta, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");

                ViewBag.Res = Db.GetDataTable(@"SELECT id ,imagen,NombreProveedor,contacto,MarcaAgrupa,
Estilo,Color,Acabado,NombreMaterial,MaterialSuela,
altura,Tallas,EM,Costo,ComentarioFinal,FechaCarga,
Estatus,canalm,id_d    
FROM muestras, canalmuestras       
WHERE id = id_muestras and Estatus=0 and MarcaAgrupa='" + marca + "' and FechaCarga >='" + fechaf + "' and FechaCarga <= '" + fechat + "'");
            }
            else
            {
                ViewBag.Res = Db.GetDataTable(@"SELECT id ,imagen,NombreProveedor,contacto,MarcaAgrupa,
Estilo,Color,Acabado,NombreMaterial,MaterialSuela,
altura,Tallas,EM,Costo,ComentarioFinal,FechaCarga,
Estatus,canalm,id_d     
FROM muestras, canalmuestras       
WHERE id = id_muestras and Estatus=0 and MarcaAgrupa='" + marca + "'");
            }
            ViewBag.marca = marca;
            return View();
        }
        public ActionResult Estatus(string marca,string fechaDesde = null, string fechaHasta = null)
        {
            if (!String.IsNullOrEmpty(fechaDesde) && !String.IsNullOrEmpty(fechaHasta))
            {

                var fechaf = DateTime.ParseExact(fechaDesde, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                var fechat = DateTime.ParseExact(fechaHasta, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");

                ViewBag.Res = Db.GetDataTable(@"SELECT id ,imagen,NombreProveedor,contacto,MarcaAgrupa,
Estilo,Color,Acabado,NombreMaterial,MaterialSuela,
altura,Tallas,EM,Costo,ComentarioFinal,FechaCarga,
Estatus,canalm,id_d     
FROM muestras, canalmuestras       
WHERE id = id_muestras and MarcaAgrupa='" + marca + "' and Estatus in(30,40) and FechaCarga >='" + fechaf + "' and  FechaCarga <= '" + fechat + "' ");
            }
            else
            {

                ViewBag.Res = Db.GetDataTable(@"SELECT id ,imagen,NombreProveedor,contacto,MarcaAgrupa,
Estilo,Color,Acabado,NombreMaterial,MaterialSuela,
altura,Tallas,EM,Costo,ComentarioFinal,FechaCarga,
Estatus,canalm,id_d     
FROM muestras, canalmuestras       
WHERE id = id_muestras and Estatus in (20,30) and MarcaAgrupa='" + marca+"'");
            }

            return View();
        }


        public ActionResult EstatusA(string marca, string fechaDesde = null, string fechaHasta = null)
        {
        
            if (!String.IsNullOrEmpty(fechaDesde) && !String.IsNullOrEmpty(fechaHasta))
            {

                var fechaf = DateTime.ParseExact(fechaDesde, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                var fechat = DateTime.ParseExact(fechaHasta, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");

                ViewBag.Res = Db.GetDataTable(@"SELECT id ,imagen,NombreProveedor,contacto,MarcaAgrupa,
Estilo,Color,Acabado,NombreMaterial,MaterialSuela,
altura,Tallas,EM,Costo,ComentarioFinal,FechaCarga,
Estatus,canalm,id_d    
FROM muestras, canalmuestras       
WHERE id = id_muestras and MarcaAgrupa='" + marca+ "' and Estatus = 40 and FechaCarga >='" + fechaf + "' and  FechaCarga <= '" + fechat + "' ");
            }
            else
            {

                ViewBag.Res = Db.GetDataTable(@"SELECT id ,imagen,NombreProveedor,contacto,MarcaAgrupa,
Estilo,Color,Acabado,NombreMaterial,MaterialSuela,
altura,Tallas,EM,Costo,ComentarioFinal,FechaCarga,
Estatus,canalm,id_d     
FROM muestras, canalmuestras       
WHERE id = id_muestras and MarcaAgrupa='" + marca + "' and Estatus=40" );
            }
            return View();
        }
        public ActionResult CrearInvitado(HttpPostedFileBase foto, string proveedornombre1, string proveedornumero,string marcaagrupa,string Canal, string proveedorcuenta, string Estilo, string Color, string Acabado, string NombreMaterial, string MaterialSuela, string Altura,
            string RangoTallas, string EM, string Costo)
        {
            var fecha = string.Format("{0}", DateTime.Now.ToString("yyyy-MM-dd"));
            if (foto != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    foto.InputStream.CopyTo(ms);
                    byte[] array = ms.GetBuffer();


                    MySqlCommand command = new MySqlCommand(@"INSERT INTO muestras (`imagen`,`NombreProveedor`, `Contacto`,`MarcaAgrupa`, `Canal`, `Estilo`, `Color`
                , `Acabado`, `NombreMaterial`, `MaterialSuela`, `Altura`, `Tallas`, `EM`, `Costo`, `ComentarioFinal`, `FechaCarga`, `Estatusm`
              ) VALUES (@Photo, '" + proveedornombre1 + "','" + proveedornumero + "','" + marcaagrupa + "','" + Canal + "','" + Estilo + "','" + Color + "'," +
                          "'" + Acabado + "','" + NombreMaterial + "','" + MaterialSuela + "','" + Altura + "','" + RangoTallas + "','" + EM + "','" + Costo + "','','" + fecha + "','" + 0 + "')");
                    MySqlConnection con = new MySqlConnection(ConnectionString);
                    command.Parameters.Add("@Photo",
          MySqlDbType.LongBlob, array.Length).Value = array;
                    con.Open();
                    command.Connection = con;
                    command.ExecuteNonQuery();
                }
            }

            var res = Db.GetDataTable("select id from muestras where NombreProveedor = '" + proveedornombre1 + "' order by id desc  limit 1");
            foreach (DataRow rows in res.Rows)
            {
                var id = rows.ItemArray[0];

                MySqlCommand command = new MySqlCommand(@"INSERT INTO canalmuestras (`id_muestras`,`canalm`, `Estatus`, `Cantidad` ,`Fecha_Estatus`,impresion
              ) VALUES ('" + id + "','" + Canal + "','" + 0 + "','" + 1 + "','" + fecha + "','" + 0 + "')");
                MySqlConnection con = new MySqlConnection(ConnectionString);
                con.Open();
                command.Connection = con;
                command.ExecuteNonQuery();

            }
            TempData["FlashSuccess"] = "Muestra registrada correctamente";
            return RedirectToAction("MuestrasInvitado", new { marca = marcaagrupa });
        }




        public ActionResult Modificar(string idproveedor, HttpPostedFileBase foto, string nombreproveedor, string contacto, string marca, string Estilo, string Color, string Acabado, string NombreMaterial, string MaterialSuela, string Altura,
    string tallas, string EM, string Costo)
        {
            var fecha = string.Format("{0}", DateTime.Now.ToString("yyyy-MM-dd"));
            if (foto != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    foto.InputStream.CopyTo(ms);
                    byte[] array = ms.GetBuffer();


                    MySqlCommand command = new MySqlCommand(@"UPDATE muestras SET imagen = @Photo, NombreProveedor = '" + nombreproveedor + "', Contacto ='" + contacto + "', MarcaAgrupa = '" + marca + "', Estilo ='" + Estilo + "',Color = '" + Color + "', Acabado= '" + Acabado + "', " + " " +
                        " NombreMaterial = '" + NombreMaterial + "', MaterialSuela= '" + MaterialSuela + "', Altura = '" + Altura + "', Tallas = '" + tallas + "',EM = '" + EM + "', Costo = '" + Costo + "' WHERE id = '" + idproveedor + "' ");
                    MySqlConnection con = new MySqlConnection(ConnectionString);
                    command.Parameters.Add("@Photo",
          MySqlDbType.LongBlob, array.Length).Value = array;
                    con.Open();
                    command.Connection = con;
                    command.ExecuteNonQuery();
                }
            }
            TempData["FlashSuccess"] = "Muestra actualizada correctamente";
            return RedirectToAction("Estatus", new { marca = marca });
        }


        public ActionResult ImprimirEtiqueta(string marca,string fechaDesde = null, string fechaHasta = null)
        {
            if (!String.IsNullOrEmpty(fechaDesde) && !String.IsNullOrEmpty(fechaHasta))
            {
               
                var fechaf = DateTime.ParseExact(fechaDesde, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                var fechat = DateTime.ParseExact(fechaHasta, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");

                ViewBag.Res = Db.GetDataTable(@"SELECT id ,imagen,NombreProveedor,contacto,MarcaAgrupa,
Estilo,Color,Acabado,NombreMaterial,MaterialSuela,
altura,Tallas,EM,Costo,ComentarioFinal,FechaCarga,
Estatus,canalm,id_d    
FROM muestras, canalmuestras       
WHERE id = id_muestras and Impresion=0 and MarcaAgrupa='" + marca + "' and FechaCarga >='" + fechaf + "' and  FechaCarga <= '" + fechat + "' ");
            }
            else
            {

                ViewBag.Res = Db.GetDataTable(@"SELECT id ,imagen,NombreProveedor,contacto,MarcaAgrupa,
Estilo,Color,Acabado,NombreMaterial,MaterialSuela,
altura,Tallas,EM,Costo,ComentarioFinal,FechaCarga,
Estatus,canalm,id_d 
FROM muestras, canalmuestras       
WHERE id = id_muestras and Impresion=0 and MarcaAgrupa='" + marca + "'");
            }
            TempData["marca"] = marca;
            return View();
        }
        public ActionResult Resultado(string Id,string Ids)
        {
            TempData["resultado"] = Id;
            TempData["resultadoi"] = Ids;
            ViewBag.marca= TempData["marca"];
            TempData["FlashSuccess"] = "Excel generado correctamente";
            return View();
        }

        public ActionResult Impresion()
        {
            var ids = TempData["resultado"];
            var id = TempData["resultadoi"];
            if (ids == null)
            {
                TempData["FlashError"] = " ya se descargo esta informacion,debes seleccionar informacion nuevamente";
                return RedirectToAction("Muestras");
            }
            Barcode codigo = new Barcode();
            codigo.IncludeLabel = true;
            codigo.Alignment = AlignmentPositions.LEFT;
            codigo.LabelFont = new Font(FontFamily.GenericMonospace, 14, FontStyle.Regular);

            
            var row = 1;
            var ros = 1;
            var r = 3;
            var ro = 9;

            var c = 1;
            var ross = 1;
            var rs = 3;
            var rss = 9;

            var res = Db.GetDataTable("SELECT * FROM muestras where id in (" + ids + ") ");
                var workbook = new XLWorkbook(Server.MapPath(@"~/App_Data/plantillaetiquetas.xlsx"));
                var ws = workbook.Worksheet(1);


                foreach (DataRow rows in res.Rows)
                {

                    byte[] Imagen = (byte[])rows.ItemArray[1];
                    var etiqu =Convert.ToString(rows.ItemArray[0]);
                    string valor = Convert.ToBase64String(Imagen);
                    MemoryStream ms = new MemoryStream(Imagen);
                    Image img = codigo.Encode(TYPE.CODE128, etiqu, Color.Black, Color.White, 200, 100);
                    ImageConverter converter = new ImageConverter();
                    var i = (byte[])converter.ConvertTo(img, typeof(byte[]));
                    var images = ws.AddPicture(new MemoryStream((byte[])rows["imagen"]))
                                            .MoveTo(ws.Cell(r, "A"))
                                            .WithSize(50,50);
                r++;
                    var imagess = ws.AddPicture(new MemoryStream(i))
                                           .MoveTo(ws.Cell(ro, "A"))
                                           .WithSize(50, 50);
                ro++;
                ws.Cell(row++, "B").Value = "PROVEEDOR";
                    ws.Cell(row++, "B").Value = "CONTACTO";
                    ws.Cell(row++, "B").Value = "ID PRODUCTO";
                    ws.Cell(row++, "B").Value = "MARCA";
                    ws.Cell(row++, "B").Value = "ESTILO";
                    ws.Cell(row++, "B").Value = "COLOR";
                    ws.Cell(row++, "B").Value = "ACABADO";
                    ws.Cell(row++, "B").Value = "NOMBRE" +
                    " MATERIAL";
                    ws.Cell(row++, "B").Value = "MATERIAL" +
                    " SUELA";
                    ws.Cell(row++, "B").Value = "ALTURA";
                    ws.Cell(row++, "B").Value = "RANGO " +
                    "TALLAS";
                    ws.Cell(row++, "B").Value = "E/M";
                    ws.Cell(row++, "B").Value = "COSTO";
                ws.Cell(ros++, "C").Value = rows.ItemArray[2];
                ws.Cell(ros++, "C").Value = rows.ItemArray[3];
                ws.Cell(ros++, "C").Value = rows.ItemArray[0];
                ws.Cell(ros++, "C").Value = rows.ItemArray[4];
                ws.Cell(ros++, "C").Value = rows.ItemArray[6];
                ws.Cell(ros++, "C").Value = rows.ItemArray[7];
                ws.Cell(ros++, "C").Value = rows.ItemArray[8];
                ws.Cell(ros++, "C").Value = rows.ItemArray[9];
                ws.Cell(ros++, "C").Value = rows.ItemArray[10];
                ws.Cell(ros++, "C").Value = rows.ItemArray[11];
                ws.Cell(ros++, "C").Value = rows.ItemArray[12];
                ws.Cell(ros++, "C").Value = rows.ItemArray[13];
                ws.Cell(ros++, "C").Value = rows.ItemArray[14];
                var images1 = ws.AddPicture(new MemoryStream((byte[])rows["imagen"]))
                                        .MoveTo(ws.Cell(rs, "D"))
                                        .WithSize(50, 50);
                rs++;
                var imagess2 = ws.AddPicture(new MemoryStream(i))
                                       .MoveTo(ws.Cell(rss, "D"))
                                       .WithSize(50, 50);
                rss++;
                ws.Cell(c++, "E").Value = "PROVEEDOR";
                ws.Cell(c++, "E").Value = "CONTACTO";
                ws.Cell(c++, "E").Value = "ID PRODUCTO";
                ws.Cell(c++, "E").Value = "MARCA";
                ws.Cell(c++, "E").Value = "ESTILO";
                ws.Cell(c++, "E").Value = "COLOR";
                ws.Cell(c++, "E").Value = "ACABADO";
                ws.Cell(c++, "E").Value = "NOMBRE MATERIAL";
                ws.Cell(c++, "E").Value = "MATERIAL SUELA";
                ws.Cell(c++, "E").Value = "ALTURA";
                ws.Cell(c++, "E").Value = "RANGO TALLAS";
                ws.Cell(c++, "E").Value = "E/M";
                ws.Cell(c++, "E").Value = "COSTO";
                ws.Cell(ross++, "F").Value = rows.ItemArray[2];
                ws.Cell(ross++, "F").Value = rows.ItemArray[3];
                ws.Cell(ross++, "F").Value = rows.ItemArray[0];
                ws.Cell(ross++, "F").Value = rows.ItemArray[4];
                ws.Cell(ross++, "F").Value = rows.ItemArray[6];
                ws.Cell(ross++, "F").Value = rows.ItemArray[7];
                ws.Cell(ross++, "F").Value = rows.ItemArray[8];
                ws.Cell(ross++, "F").Value = rows.ItemArray[9];
                ws.Cell(ross++, "F").Value = rows.ItemArray[10];
                ws.Cell(ross++, "F").Value = rows.ItemArray[11];
                ws.Cell(ross++, "F").Value = rows.ItemArray[12];
                ws.Cell(ross++, "F").Value = rows.ItemArray[13];
                ws.Cell(ross++, "F").Value = rows.ItemArray[14];
                row++;
                ros++;
                r++;r++;r++;r++;r++;r++;r++;r++;r++;r++;r++;r++;r++;
                ro++;ro++;ro++;ro++;ro++;ro++;ro++;ro++;ro++;ro++;ro++;ro++;ro++;
                c++;
                ross++;
                rs++; rs++; rs++; rs++; rs++; rs++; rs++; rs++; rs++; rs++; rs++; rs++; rs++;
                rss++; rss++; rss++; rss++; rss++; rss++; rss++; rss++; rss++; rss++; rss++; rss++; rss++;
               
            }

            MySqlCommand command = new MySqlCommand(@"UPDATE canalmuestras SET impresion = '" + 1 + "'  WHERE id_d in(" + id + ")");
            MySqlConnection con = new MySqlConnection(ConnectionString);
            con.Open();
            command.Connection = con;
            command.ExecuteNonQuery();


            FileManager.ExportExcel(workbook, "etiquetasMuestras", HttpContext);

            return View();
        }


        public ActionResult Seleccionar(string marca, string fechaDesde = null, string fechaHasta = null)
        {

            if (!String.IsNullOrEmpty(fechaDesde) && !String.IsNullOrEmpty(fechaHasta))
            {

                var fechaf = DateTime.ParseExact(fechaDesde, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");
                var fechat = DateTime.ParseExact(fechaHasta, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd");

                ViewBag.Res = Db.GetDataTable(@"select * from muestras where Estatusm=0 and MarcaAgrupa='" + marca + "'  and FechaCarga >='" + fechaf + "' and  FechaCarga <= '" + fechat + "' ");
            }
            else
            {

                ViewBag.Res = Db.GetDataTable(@"select * from muestras where Estatusm=0 and MarcaAgrupa='" + marca + "'");
            }
            TempData["Marc"] = marca;
            return View();
        }

        public ActionResult Actualizar(string Canal, int idproveedor,string proveedorid)
        {

           
            var Res1 = Db.GetDataTable(@"SELECT id_muestras,GROUP_CONCAT(canalm) as canales
FROM canalmuestras
where id_muestras='"+idproveedor+"'  and canalm like '%"+Canal+"%'");


            var marca = TempData["Marc"];
            foreach (DataRow rows in Res1.Rows)
            {
              string[]  letters = {"Mayoreo","Menudeo","Wosh","Kipon" };
                var canales = rows.ItemArray[1];
                var ca = Convert.ToString(canales);
                string[] vcanal = ca.Split(',');

                var fecha = string.Format("{0}", DateTime.Now.ToString("yyyy-MM-dd"));
                foreach (var can in vcanal)
                {
                    if (can == "Mayoreo")
                    {

                    }else if(can == "Menudeo")
                    {
                       
                    }else if(can == "Wosh")
                    {

                    }else if(can == "Kipon")
                    {

                    }

                    else
                    {
                        MySqlCommand command = new MySqlCommand(@"INSERT INTO canalmuestras (`id_muestras`,`canalm`, `Estatus`, `Cantidad` ,`Fecha_Estatus`,impresion
              ) VALUES ('" + idproveedor + "','" + Canal + "','" + 0 + "','" + 1 + "','" + fecha + "','" + 0 + "')");
                        MySqlConnection con = new MySqlConnection(ConnectionString);
                        con.Open();
                        command.Connection = con;
                        command.ExecuteNonQuery();

                        TempData["FlashSuccess"] = "Muestra registrada correctamente";
                        return RedirectToAction("Seleccionar", new { marca = marca });
                    }

                }
                    
            }
            TempData["Flasherror"] = "Esta Muestra ya fue registrada para este canal";
            return RedirectToAction("Seleccionar", new { marca = marca });
        }

    }
}