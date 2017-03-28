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
			var orden = ordenCompraManager.FindDetalleByDocumento(Documento,"");

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

			checkoutCitas.ListaDeOrdenes.IdProveedor(proveedorId.ToString(), "set");
			
			ViewBag.proveedorId = proveedorId;
			
			return View();
		}

		[Authorize(Roles = "MAESTRO-MERCADERIA")]
		[ValidateAntiForgeryToken]
		[HttpPost]
		public ActionResult BuscarOrden(int proveedorId, string numeroDocumento)
		{

			int tmpProveedorId = 0;

			try
			{
		
				if (proveedorId == 0)
				{
					tmpProveedorId = Int32.Parse(checkoutCitas.ListaDeOrdenes.IdProveedor("", "get").ToString());

				}
				else
				{
					tmpProveedorId = proveedorId;

				}

			   // checkoutCitas.ListaDeOrdenes.IdProveedor("", "get");

				var ordenCompraManager = new OrdenCompraManager();
				var orden = ordenCompraManager.FindOrdenCompraWithAvailableDates(numeroDocumento, tmpProveedorId);

				if (orden["orden"] == null)
				{
					TempData["FlashError"] = "Numero de documento incorrecto";
					return RedirectToAction("BuscarOrden", new { @proveedorId = tmpProveedorId });
				}

				if (checkoutCitas.ListaDeOrdenes.getsetOrdenTemp(numeroDocumento) == "1")
				{
					TempData["FlashError"] = "El numero de documento ya se encuentra en la lista";
					return RedirectToAction("BuscarOrden", new { @proveedorId = tmpProveedorId });

				}
				
				System.Web.HttpContext.Current.Session["orden"]  = orden;

				Int64 countElementos = checkoutCitas.ListaDeOrdenes.countElementosEnLista();

				if (countElementos == 0)
				{
					return RedirectToAction("FechaCita");
				}
				else
				{
					return RedirectToAction("Asn");

				}

			}
			catch (BusinessException businessEx)
			{
				TempData["FlashError"] =  businessEx.Message;
				return RedirectToAction("BuscarOrden", new { @proveedorId = tmpProveedorId });
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
				
   
		public ActionResult Asn(string fecha = null,string orden = "0")
		{

			Int64 countElementos = checkoutCitas.ListaDeOrdenes.countElementosEnLista();

			if (orden == "0")//Esto indica que es una nueva Orden, no existente en la Lista
			{

				if (countElementos == 0)
				{

					if (System.Web.HttpContext.Current.Session["fecha"] == null)
					{

						if (string.IsNullOrWhiteSpace(fecha))
						{
							return RedirectToAction("Index");
						}

						checkoutCitas.ListaDeOrdenes.FechaOrden(fecha, "set");

						System.Web.HttpContext.Current.Session["fecha"] = DateTime.ParseExact(checkoutCitas.ListaDeOrdenes.FechaOrden("", "get"), "dd/MM/yyyy",
							CultureInfo.InvariantCulture);
					}
				}
				else
				{
					System.Web.HttpContext.Current.Session["fecha"] = DateTime.ParseExact(checkoutCitas.ListaDeOrdenes.FechaOrden("", "get"), "dd/MM/yyyy",CultureInfo.InvariantCulture);
					
				}


				if (System.Web.HttpContext.Current.Session["orden"] == null)
				{
					return RedirectToAction("Index");
				}

				ViewBag.ProveedorId = checkoutCitas.ListaDeOrdenes.IdProveedor("", "get");

				ViewBag.origen = "1";

				ViewBag.Orden = ((Hashtable) System.Web.HttpContext.Current.Session["orden"]);

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
									d.ordencompra,

								});

				checkoutCitas.ListaDeOrdenes.Agregar(checkoutCitas.ListaDeOrdenes.getsetOrdenTemp("", 1),
					checkoutCitas.ListaDeOrdenes.convertirHashTable(
						(Hashtable) System.Web.HttpContext.Current.Session["orden"]));

			}
			else
			{

				System.Web.HttpContext.Current.Session["fecha"] = DateTime.ParseExact(checkoutCitas.ListaDeOrdenes.FechaOrden("", "get"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
				ViewBag.ProveedorId = checkoutCitas.ListaDeOrdenes.IdProveedor("", "get");
				ViewBag.origen = "2";
				ViewBag.NumeroDocumento = orden;
				ViewBag.Detalles =
					checkoutCitas.ListaDeOrdenes.getListaDeItems(orden);


			}
			;

			//checkoutCitas.ListaDeOrdenes.convertirHashTable((Hashtable) System.Web.HttpContext.Current.Session["orden"]);

			
			
			//checkoutCitas.ListaDeOrdenes.getListaDeItems(checkoutCitas.ListaDeOrdenes.getsetOrdenTemp("", 1));

			//checkoutCitas.ListaDeOrdenes.updElementoEnLista("4501140098", "000000000013327801", "12", "10");

			//checkoutCitas.ListaDeOrdenes.getListaDeOrdenes();

			return View();
		}


		[HttpPost]
		[AcceptVerbs(HttpVerbs.Post)]
		public  ActionResult updItemOrden(string Orden, string Item, string OldValue, string NewValue)
		{

			int Res = checkoutCitas.ListaDeOrdenes.updElementoEnLista(Orden, Item, OldValue, NewValue);


			return Json(Res, JsonRequestBehavior.DenyGet);

		}

		public ActionResult ListaDeOrdenes(string orden = "0")
		{


			ViewBag.ListaDeOrdenes = checkoutCitas.ListaDeOrdenes.getListaDeOrdenes();

			
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
				dt.Rows.Add(detalle.NumeroMaterial, detalle.DescripcionMaterial, Decimal.ToInt32(decimal.Parse(detalle.CantidadPedido)));

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
		                if (cantidad > decimal.Parse(detalle.CantidadPedido))
		                {
		                    throw new BusinessException(string.Format("Error en la cantidad del Material {0}", detalle.NumeroMaterial));
		                }
		                detalle.CantidadComprometida = cantidad;
		            }
		        }
		    }
		    catch (BusinessException businessException)
		    {
				TempData["FlashError"] = businessException.Message;
				return RedirectToAction("Asn");
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