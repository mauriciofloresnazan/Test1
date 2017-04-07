using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using ClosedXML.Excel;
using Ppgz.Repository;
using Ppgz.Services;
using Ppgz.Web.Areas.Nazan.Models;
using Ppgz.Web.Infrastructure;

namespace Ppgz.Web.Areas.Nazan.Controllers
{
	[Authorize]
	public class RegistrarNivelServicioController : Controller
	{
        readonly ReporteProveedorManager _reporteProveedorManager = new ReporteProveedorManager();
        //
        // GET: /Nazan/RegistrarNivelServicio/
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-REGISTRARNIVELSERVICIO")]
		public ActionResult Index()
		{
            return View();
        }



        public FileResult Descargar()
        {

            var archivo = Server.MapPath("~/Uploads/formato_nivel_servicio.xlsx");

            var fileBytes = System.IO.File.ReadAllBytes(archivo);

            var fileName = "formato_nivel_servicio.xlsx";

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, fileName);


        }

      
        [HttpPost]
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
                TempData["FlashError"] = "Archivo incorrecto";
                return RedirectToAction("Index");
            }

            //TODO
            TempData["FlashSuccess"] = "Archivo cargado exitosamente";
            return RedirectToAction("Index");
        }
    }
}