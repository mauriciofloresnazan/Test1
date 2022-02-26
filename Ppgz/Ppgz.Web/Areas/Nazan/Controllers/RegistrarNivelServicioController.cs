using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Web.Mvc;
using ClosedXML.Excel;
using Ppgz.Services;

namespace Ppgz.Web.Areas.Nazan.Controllers
{
	[Authorize]
	public class RegistrarNivelServicioController : Controller
	{
        readonly ReporteProveedorManager _reporteProveedorManager = new ReporteProveedorManager();

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-NIVELSERVICIO")]
		public ActionResult Index()
        {

            var lasNivelServicio = _reporteProveedorManager.LastNivleServicio();

            ViewBag.UltimaActualizacion = lasNivelServicio == null ? 
                "No hay registros cargados" : 
                lasNivelServicio.Fecha.ToString("dd/MM/yyyy");

            return View();
        }


        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-NIVELSERVICIO")]
        public FileResult Descargar()
        {

            var archivo = Server.MapPath("~/Uploads/formato_nivel_servicio.xlsx");

            var fileBytes = System.IO.File.ReadAllBytes(archivo);

            var fileName = "formato_nivel_servicio.xlsx";

            return File(fileBytes, MediaTypeNames.Application.Pdf, fileName);


        }

      
        [HttpPost]
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-NIVELSERVICIO")]
        public ActionResult CargarExcel(FormCollection collection)
        {
            //var codigoProveedor = collection["codigoProveedor"];

            var file = Request.Files[0];

            if (file != null && file.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                TempData["FlashError"] = "Archivo incorrecto";
                return RedirectToAction("Index");
            }

            if (file == null)
            {
                TempData["FlashError"] = "Archivo incorrecto";
                return RedirectToAction("Index");
            }

            var fileName = Path.GetFileName(file.FileName);


            if (fileName == null)
            {
                TempData["FlashError"] = "Archivo incorrecto";
                return RedirectToAction("Index");
            }

            //niveleseervicio _nivelservicio = new niveleseervicio();;
           // var reporteProveedor = _reporteProveedorManager.FindReporteProveedor(codigoProveedor);
            //niveleseervicio
            var wb = new XLWorkbook(file.InputStream);

            var ws = wb.Worksheet(1);



            for (var i = 2; i < ws.RowsUsed().ToList().Count + 1; i++)
            {
                try
                {

                    var numeroProveedor = ws.Row(i).Cell(1).Value.ToString();

                    if (string.IsNullOrWhiteSpace(numeroProveedor))
                    {
                        throw new Exception();
                    }
                    var ultimoMes = decimal.Parse(ws.Row(i).Cell(2).Value.ToString());
                    var temporadaActual = decimal.Parse(ws.Row(i).Cell(3).Value.ToString());
                    var acumuladoAnual = decimal.Parse(ws.Row(i).Cell(4).Value.ToString());
                    var pedidoAtrasado = decimal.Parse(ws.Row(i).Cell(5).Value.ToString());
                    var pedidoEntiempo = decimal.Parse(ws.Row(i).Cell(6).Value.ToString());
                    var pedidoTotal = decimal.Parse(ws.Row(i).Cell(7).Value.ToString());
                }
                catch (Exception)
                {
                    TempData["FlashError"] = "Archivo incorrecto";
                    return RedirectToAction("Index");

                }


                //_reporteProveedorManager.CrearNivelServicio(numeroProveedor, ultimoMes, temporadaActual, acumuladoAnual, pedidoAtrasado, pedidoEntiempo, pedidoTotal);

            }




            try
            {
                for (var i = 2; i < ws.RowsUsed().ToList().Count + 1; i++)
                {
                    
                   
                        var numeroProveedor = ws.Row(i).Cell(1).Value.ToString();
                        var ultimoMes = decimal.Parse(ws.Row(i).Cell(2).Value.ToString());
                        var temporadaActual = decimal.Parse(ws.Row(i).Cell(3).Value.ToString());
                        var acumuladoAnual = decimal.Parse(ws.Row(i).Cell(4).Value.ToString());
                        var pedidoAtrasado = decimal.Parse(ws.Row(i).Cell(5).Value.ToString());
                        var pedidoEntiempo = decimal.Parse(ws.Row(i).Cell(6).Value.ToString());
                        var pedidoTotal = decimal.Parse(ws.Row(i).Cell(7).Value.ToString());

                        _reporteProveedorManager.CrearNivelServicio(numeroProveedor, ultimoMes, temporadaActual, acumuladoAnual, pedidoAtrasado, pedidoEntiempo, pedidoTotal);
                   
                }
            }
            catch (BusinessException businessException)
            {
                TempData["FlashError"] = businessException.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["FlashError"] = ex.Message;
                return RedirectToAction("Index");
            }

            //TODO
            TempData["FlashSuccess"] = "Archivo cargado exitosamente";
            return RedirectToAction("Index");
        }
    }
}