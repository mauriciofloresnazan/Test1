using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using ClosedXML.Excel;
using Ppgz.Repository;
using Ppgz.Services;
using Ppgz.Web.Infrastructure;
using OrdenCompraManager = Ppgz.Services.OrdenCompraManager;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class ControlCitasController : Controller
    {


        readonly ProveedorManager _proveedorManager = new ProveedorManager();
        readonly CommonManager _commonManager = new CommonManager();
        readonly CuentasPorPagarManager _cuentasPorPagarManager = new CuentasPorPagarManager();

        //
        // GET: /Mercaderia/ControlCitas/
        [Authorize(Roles = "MAESTRO-MERCADERIA")]
        public ActionResult Index()
        {

            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            ViewBag.proveedores = _proveedorManager.FindByCuentaId(cuenta.Id);
            
            return View();
        }



        [Authorize(Roles = "MAESTRO-MERCADERIA")]
        public ActionResult OrdenDeCompra(int proveedorId)
        {
            ViewBag.IdProveedor = proveedorId;

            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA")]
        public JsonResult OrdenDeCompraDetalle(string Documento = "4500916565")
        {
            var ordenCompraManager = new OrdenCompraManager();
            var orden = ordenCompraManager.FindDetalleByDocumento(Documento);

            return Json(orden,JsonRequestBehavior.AllowGet);
        }


        [Authorize(Roles = "MAESTRO-MERCADERIA")]
        public ActionResult BuscarOrden(int proveedorId)
        {
            var proveedor = _proveedorManager.Find(proveedorId);

            if (proveedor == null)
            {
                // TODO
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }

            ViewBag.proveedorId = proveedorId;
            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult BuscarOrden(int proveedorId, string numeroDocumento)
        {
            try
            {
                var ordenCompraManager = new OrdenCompraManager();
                var orden = ordenCompraManager.FindOrdenCompraWithAvailableDates(numeroDocumento, proveedorId);

                if (orden["orden"] == null)
                {
                    TempData["FlashError"] = "Numero de documento incorrecto";
                    return RedirectToAction("BuscarOrden", new { proveedorId });
                }
                 System.Web.HttpContext.Current.Session["orden"]  = orden;
                return RedirectToAction("FechaCita");

            }
            catch (BusinessException businessEx)
            {
                TempData["FlashError"] =  businessEx.Message;
                return View();
            }
            catch (Exception e)
            {
                var log = CommonManager.BuildMessageLog(
                    TipoMensaje.Error,
                    ControllerContext.Controller.ValueProvider.GetValue("controller").RawValue.ToString(),
                    ControllerContext.Controller.ValueProvider.GetValue("action").RawValue.ToString(),
                    e.ToString(), Request);

                CommonManager.WriteAppLog(log, TipoMensaje.Error);
                return View();
            }
        }

        public ActionResult FechaCita()
        {
            if (System.Web.HttpContext.Current.Session["orden"] == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Dates = ((Hashtable)System.Web.HttpContext.Current.Session["orden"])["Dates"];
            return View();
        }
                
   
        public ActionResult Asn(string fecha = null)
        {
            if (System.Web.HttpContext.Current.Session["fecha"] == null)
            {

                if (string.IsNullOrWhiteSpace(fecha))
                {
                    return RedirectToAction("Index");
                }

                System.Web.HttpContext.Current.Session["fecha"] = DateTime.ParseExact(fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            if (System.Web.HttpContext.Current.Session["orden"] == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Orden = ((Hashtable)System.Web.HttpContext.Current.Session["orden"]);

            ViewBag.Detalles =
                ((List<ordencompradetalle>) ((Hashtable) System.Web.HttpContext.Current.Session["orden"])["detalle"])
                    .Select(
                        d =>
                            new
                            {
                                d.NumeroMaterial,
                                d.Centro,
                                d.Almacen,
                                d.DescripcionMaterial,
                                d.CantidadPedido,
                                d.Id,
                                d.ordencompra
                            });

            return View();
        }

        public void DescargarPlantilla()
        {
            if (System.Web.HttpContext.Current.Session["orden"] == null)
            {
                return;
            }

            var detalles =((List<ordencompradetalle>)((Hashtable)System.Web.HttpContext.Current.Session["orden"])["detalle"]);

            var dt = new DataTable();
            dt.Columns.Add("NumeroMaterial");
            dt.Columns.Add("Descripcion");
            dt.Columns.Add("Cantidad");


            foreach (var detalle in detalles)
            {
                dt.Rows.Add(detalle.NumeroMaterial, detalle.DescripcionMaterial, detalle.CantidadBase);

            }
            
            var orden =((ordencompra)((Hashtable)System.Web.HttpContext.Current.Session["orden"])["orden"]);
            // var detalle = _ordenCompraManager.FindDetalleByDocumento();


            FileManager.ExportExcel(dt, orden.NumeroDocumento + "_plantilla", HttpContext);
        }

               
        [HttpPost]
        public ActionResult CargarDesdePlantilla(FormCollection collection)
        {
            if (System.Web.HttpContext.Current.Session["orden"] == null)
            {
                return RedirectToAction("Index");
            }

            var file = Request.Files[0];

            if (file != null && file.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                TempData["FlashError"] = "Archivo incorrecto";
                return RedirectToAction("Asn");
            }

            if (file == null)
            {
                TempData["FlashError"] = "Archivo incorrecto";
                return RedirectToAction("Asn");
            }

            var fileName = Path.GetFileName(file.FileName);
            


            if (fileName == null)
            {
                TempData["FlashError"] = "Archivo incorrecto";
                return RedirectToAction("Asn");
            }

            
            var detalles =((List<ordencompradetalle>)((Hashtable)System.Web.HttpContext.Current.Session["orden"])["detalle"]);


            var wb = new XLWorkbook(file.InputStream);


            var ws = wb.Worksheet(1);

            try
            {
                for (var i = 2; i < detalles.Count + 2; i++)
                {
                    var numeroMaterial = ws.Row(i).Cell(1).Value.ToString();
                    var cantidad = decimal.Parse(ws.Row(i).Cell(3).Value.ToString());

                    var detalle = detalles.FirstOrDefault(d => d.NumeroMaterial == numeroMaterial);
                    if (detalle != null)
                    {
                        detalle.CantidadComprometida = cantidad;
                    }
                }
            }
            catch (Exception)
            {

                TempData["FlashError"] = "Archivo incorrecto";
                return RedirectToAction("Asn");
            }



            var test = wb.Worksheet(1).Row(1).Cell(1).Value;
            //var path = Path.Combine(Server.MapPath("~/Uploads/"), fileName);

            //file.SaveAs(path);

            //return "~/Uploads/" + fileName;

         /*   var rows = wb.Worksheet(1);
             var rngNumbers = ws.Range("F4:F6");

              foreach (var cell in rngNumbers.Cells())
                {
                    string formattedString = cell.GetFormattedString();
                    cell.DataType = XLCellValues.Text;
                    cell.Value = formattedString + " Dollars";
                }
            }
            
            TempData["FlashSuccess"] = "Archivo cargado exitosamente";*/
            return RedirectToAction("Asn",new { fecha = System.Web.HttpContext.Current.Session["fecha"] });
        }

	}
}