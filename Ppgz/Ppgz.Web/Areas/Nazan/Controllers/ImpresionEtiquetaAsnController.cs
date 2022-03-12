using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using MySql.Data.MySqlClient;
using Ppgz.CitaWrapper;
using Ppgz.Repository;
using Ppgz.Web.Infrastructure;
using ScaleWrapper;

namespace Ppgz.Web.Areas.Nazan.Controllers
{
    public class ImpresionEtiquetaAsnController : Controller
    {

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS")]
        public ActionResult Citas()
        {
            var db = new Entities();
            var fecha = DateTime.Today.Date;
            var fechas = DateTime.Today;
            var citas = db.citas.Where(c => c.FechaCita >= fecha && c.TipoCita == "Cita Por Asn").ToList();
            ViewBag.Citas = citas;
            return View();
        }


        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-IMPRESIONETIQUETAS")]
        public ActionResult Resultado(int citaId)
        {

            var db2 = new Entities();
            var proveedor = TempData["proveedor"] as proveedore;
            ViewBag.Proveedor = proveedor;
            ViewBag.PuedeDescargar = false;
            ViewBag.PuedeDescargar = true;
            List<string> etiquetas = new List<string>();
            string etiquetasPrint = "";

            var db = new Entities();
            var archivo1 = db.EnviarEtiquetas.Where(et => et.cita == citaId).ToList();
            foreach (var et in archivo1)
            {

                etiquetas.Add(@"^XA
^JZN
^PR9			
^PQ1
^FO0,10^A0N ,45,25^FD" + et.carga + @"^FS
^FO0,65^A0N ,150,70^FD" + et.cita + @"^FS
^FO230,5^BY,2.5^BAN,170Y^FD" + et.recibo + @"^FS
^FO5,195^A0N ,45,25^FDPares:^FS
^FO60,190^A0N ,55,55^FD " + et.pares + @"^FS
^FO230,218^A0N ,30,25^FDTienda:^FS
^FO300,218^A0N ,30,25^FD " + et.tienda + @"^FS
^FO400,218^A0N ,30,25^FDIDProv:^FS
^FO500,218^A0N ,30,25^FD" + et.Id_Proveedor + @"^FS
^FO200,250^BY,2.1^BAN,122Y^FD" + et.caja + @"^FS
^FO0,240^A0N ,25,25^FD Color:^FS
^FO0,265^A0N ,35,35^FD" + et.color + @"^FS
^FO0,310^A0N ,25,25^FDEstilo:^FS
^FO80,305^A0N ,35,35^FD" + et.estilo + @"^FS
^FO0,340^A0N ,25,25^FDFactura:^FS
^FO0,370^A0N ,35,35^FD" + et.factura + @"^FS
^XZ");
            }


            foreach (string etiqueta in etiquetas)
            {
                etiquetasPrint = etiquetasPrint + etiqueta;
            }

            ViewBag.etiquetas = etiquetas;
            ViewBag.etiquetasPrint = etiquetasPrint;
            TempData["texto"] = etiquetasPrint;
            return View();
        }
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-IMPRESIONETIQUETAS")]
        public ActionResult Individual(int citaId)
        {

            var db = new Entities();
            var db2 = new Entities();

            ViewBag.etiq = db2.EnviarEtiquetas.Where(csa => csa.cita == citaId).ToList();

            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-IMPRESIONETIQUETAS")]
        public ActionResult Resultadoind(string cantidades, string materiales)
        {
            string[] ordenes = cantidades.Split(',');
            var proveedor = TempData["proveedor"] as proveedore;
            ViewBag.Proveedor = proveedor;
            ViewBag.PuedeDescargar = false;
            ViewBag.PuedeDescargar = true;
            List<string> etiquetas = new List<string>();
            string etiquetasPrint = "";
            foreach (var ordencompra in ordenes)
            {

                var db = new Entities();
                var archivo1 = db.EnviarEtiquetas.Where(et => et.caja == ordencompra).FirstOrDefault();


                etiquetas.Add(@"^XA
^JZN
^PR9			
^PQ1
^FO0,10^A0N ,45,25^FD" + archivo1.carga + @"^FS
^FO0,65^A0N ,150,70^FD" + archivo1.cita + @"^FS
^FO230,5^BY,2.5^BAN,170Y^FD" + archivo1.recibo + @"^FS
^FO5,195^A0N ,45,25^FDPares:^FS
^FO60,190^A0N ,55,55^FD" + archivo1.pares + @"^FS
^FO230,218^A0N ,30,25^FDTienda:^FS
^FO300,218^A0N ,30,25^FD " + archivo1.tienda + @"^FS
^FO400,218^A0N ,30,25^FDIDProv:^FS
^FO500,218^A0N ,30,25^FD" + archivo1.Id_Proveedor + @"^FS
^FO200,250^BY,2.1^BAN,122Y^FD" + ordencompra + @"^FS
^FO0,240^A0N ,25,25^FD Color:^FS
^FO0,265^A0N ,35,35^FD" + archivo1.color + @"^FS
^FO0,310^A0N ,25,25^FDEstilo:^FS
^FO80,305^A0N ,35,35^FD" + archivo1.estilo + @"^FS
^FO0,340^A0N ,25,25^FDFactura:^FS
^FO0,370^A0N ,35,35^FD" + archivo1.factura + @"^FS
^XZ");

            }

            foreach (string etiqueta in etiquetas)
            {
                etiquetasPrint = etiquetasPrint + etiqueta;
            }

            ViewBag.etiquetas = etiquetas;
            ViewBag.etiquetasPrint = etiquetasPrint;
            TempData["texto"] = etiquetasPrint;


            return View();
        }
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-IMPRESIONETIQUETAS")]
        public FileContentResult ImpresionEtiquetas()
        {
            var texto = TempData["texto"].ToString();

            return File(TextToByte(texto), System.Net.Mime.MediaTypeNames.Application.Octet, "Impresion Etiquetas.txt");
        }
        public static byte[] TextToByte(string texto)
        {
            // convert string to stream
            byte[] byteArray = Encoding.ASCII.GetBytes(texto);
            return byteArray;
        }

    }
}