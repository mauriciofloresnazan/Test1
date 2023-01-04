using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Ppgz.Repository;
using Ppgz.Services;

namespace Ppgz.Web.Areas.Nazan.Controllers
{
    public class AdministrarFacturasController : Controller
    {
        private readonly FacturaManager _facturaManager = new FacturaManager();

        public ActionResult ProcesarFacturas()
        {
            _facturaManager.ProcesarFacturasAtoradas();

            return View();
        }
        public ActionResult ProcesarFacturasM()
        {
            _facturaManager.ProcesarFacturasM();

            return View();
        }
        public ActionResult ProcesarFacturasServicios()
        {
            _facturaManager.ProcesarFacturasAtoradasServicio();

            return View();
        }


        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARFACTURAS")]
        public ActionResult Index(string fecha = null)
        {
            
            var fechaInicio = DateTime.Today.AddMonths(-1);

            if (!string.IsNullOrWhiteSpace(fecha))
            {
                fechaInicio = DateTime.ParseExact(fecha, "MM/yyyy", CultureInfo.InvariantCulture);
                fechaInicio = new DateTime(fechaInicio.Year, fechaInicio.Month, 1);
            }


            var fechaFin = fechaInicio.AddMonths(1);

            var db = new Entities();

            FacturaManager obj_fact = new FacturaManager();
            List<factura> lstfacturas = new List<factura>();
            try
            {
                DataSet dsfacturas = obj_fact.GetFacturasbyFecha(fechaInicio.ToString("yyyy-MM-dd"), fechaFin.ToString("yyyy-MM-dd"));
                if (dsfacturas.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow drf in dsfacturas.Tables[0].Rows)
                    {
                        factura fact = new factura();
                        int auxInt = 0;
                        bool isParsable = Int32.TryParse(drf["Id"].ToString(), out auxInt);
                        fact.Id = auxInt;
                        isParsable = Int32.TryParse(drf["proveedor_id"].ToString(), out auxInt);
                        fact.proveedor_id = auxInt;
                        fact.Uuid = drf["Uuid"].ToString();
                        fact.Serie = drf["Serie"].ToString();
                        fact.Folio = drf["Folio"].ToString();
                        fact.Fecha = Convert.ToDateTime(drf["Fecha"].ToString());
                        decimal auxtotal;
                        isParsable = decimal.TryParse(drf["Total"].ToString(), out auxtotal);
                        fact.Total = auxtotal;
                        fact.XmlRuta = drf["XmlRuta"].ToString();
                        fact.PdfRuta = drf["PdfRuta"].ToString();
                        fact.Estatus = drf["Estatus"].ToString();
                        fact.Comentario = drf["Comentario"].ToString();
                        fact.NumeroGenerado = drf["NumeroGenerado"].ToString();
                        fact.Procesado = Convert.ToBoolean(drf["Procesado"].ToString());
                        fact.numeroProveedor = drf["numeroProveedor"].ToString();
                        fact.FechaPortal = Convert.ToDateTime(drf["FechaPortal"].ToString());
                        fact.EstatusOriginal = drf["EstatusOriginal"].ToString();
                        fact.RFCReceptor = drf["RFCReceptor"].ToString();
                        fact.TipoFactura = drf["TipoFactura"].ToString();
                        fact.MetodoPago = drf["MetodoPago"].ToString();
                        fact.formapago = drf["formapago"].ToString();
                        isParsable = Int32.TryParse(drf["idPro"].ToString(), out auxInt);
                        fact.proveedore = new proveedore();
                        fact.proveedore.Id = auxInt;
                        fact.proveedore.Rfc = drf["Rfc"].ToString();
                        fact.proveedore.Nombre1 = drf["Nombre1"].ToString();
                        fact.proveedore.Nombre2 = drf["Nombre2"].ToString();
                        fact.proveedore.Nombre3 = drf["Nombre3"].ToString();
                        fact.proveedore.Nombre4 = drf["Nombre4"].ToString();
                        lstfacturas.Add(fact);

                    }
                    ViewBag.Facturas = lstfacturas;
                }
            }
            catch (Exception ex)
            {
                TempData["FlashError"] = ex.Message;
                return RedirectToAction("Home");
            }
            //ViewBag.Facturas = db.facturas.Where(f => f.Fecha >= fechaInicio && f.Fecha <= fechaFin).ToList();

            ViewBag.FechaInicio = fechaInicio;
            
            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARFACTURAS")]
        public FileResult DescargarXml(int facturaId)
        {
            var db = new Entities();

            var factura = db.facturas.Find(facturaId);

            if (factura == null)
            {
                throw new Exception("Factura incorrecta");
            }

            var fileBytes = System.IO.File.ReadAllBytes(factura.XmlRuta);

            var fileName = factura.Uuid + ".xml";

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Text.Xml, fileName);


        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARFACTURAS")]
        public FileResult DescargarPdf(int facturaId)
        {
            var db = new Entities();

            var factura = db.facturas.Find(facturaId);

            if (factura == null)
            {
                throw new Exception("Factura incorrecta");
            }

            var fileBytes = System.IO.File.ReadAllBytes(factura.PdfRuta);

            var fileName = factura.Uuid + ".pdf";

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, fileName);


        }


        public ActionResult Actualizar(int facturaId, string numeroGenerado, string estatus, string comentario)
        {
            var db = new Entities();

            var factura = db.facturas.Find(facturaId);

            if (factura == null)
            {

                TempData["FlashError"] = "Factura incorrecta";
                return RedirectToAction("Index");
            }

            if (numeroGenerado != "" & numeroGenerado.Any(char.IsDigit))
            {
                factura.NumeroGenerado = numeroGenerado;
                factura.Procesado = true;
            }
            

            factura.Estatus = estatus;
            factura.Comentario = comentario;
            db.Entry(factura).State = EntityState.Modified;
            
            db.SaveChanges();

            TempData["FlashSuccess"] = "Actualización realizada correctamente.";
            return RedirectToAction("Index");
        }
    }
}