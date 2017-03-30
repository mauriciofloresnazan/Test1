using System;
using System.Data;
using System.Diagnostics;
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
		readonly OrdenCompraManager _ordenCompraManager = new OrdenCompraManager();

		private const string NombreVarSession = "controlCita";
		internal void LimpiarCita()
		{
			if (System.Web.HttpContext.Current.Session[NombreVarSession] != null)
			{
				System.Web.HttpContext.Current.Session.Remove(NombreVarSession);
			}
		}

		internal CurrentCita CurrentCita
		{
			get
			{
				if (System.Web.HttpContext.Current.Session[NombreVarSession] == null)
				{
					return null;
				}
				return (CurrentCita)System.Web.HttpContext.Current.Session[NombreVarSession];
			}
		}

		internal void InitCurrentCita(int cuentaId, int proveedorId, DateTime fecha)
		{
			System.Web.HttpContext.Current.Session[NombreVarSession] = new CurrentCita(cuentaId, proveedorId, fecha);
		}

		//
		// GET: /Mercaderia/ControlCitas/
		[Authorize(Roles = "MAESTRO-MERCADERIA")]
		public ActionResult Index()
		{
			LimpiarCita();
			
			var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

			ViewBag.proveedores = _proveedorManager.FindByCuentaId(cuenta.Id);
			
			return View();
		}

		internal proveedore GetProveedor(int proveedorId)
		{

			// TODO
			const string proveedorIcorrectoMsg = "Proveedor incorrecto";

			if (CurrentCita != null)
			{
				if (proveedorId != CurrentCita.Proveedor.Id)
				{
					throw new BusinessException(proveedorIcorrectoMsg);
				}
			}

			var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

			var proveedor = _proveedorManager.Find(proveedorId, cuenta.Id);
			
			if (proveedor == null)
			{
				throw new BusinessException(proveedorIcorrectoMsg);
			}

			return proveedor;
		}

		[Authorize(Roles = "MAESTRO-MERCADERIA")]
		public ActionResult BuscarOrden(int proveedorId)
		{
		   
			/*

			if (System.Web.HttpContext.Current.Session["controlCita"] == null)
			{
				System.Web.HttpContext.Current.Session["controlCita"] = new CurrentCita(cuenta.Id, proveedorId);

			} else if (System.Web.HttpContext.Current.Session["controlCita"] != null)
			{

				int proveedorIdtmp = ((CurrentCita) System.Web.HttpContext.Current.Session["controlCita"]).Proveedor.Id;

				if (proveedorId != proveedorIdtmp)
				{
					System.Web.HttpContext.Current.Session["controlCita"] = new CurrentCita(cuenta.Id, proveedorId);

				}

			}*/
			try
			{
				ViewBag.proveedor = GetProveedor(proveedorId); 
			}
			catch (BusinessException exception)
			{
				TempData["FlashError"] = exception.Message;
				return RedirectToAction("Index");
			}
		
			ViewBag.CurrentCita = CurrentCita;
			
			return View();
		}

		[Authorize(Roles = "MAESTRO-MERCADERIA")]
		[ValidateAntiForgeryToken]
		[HttpPost]
		public ActionResult BuscarOrden(int proveedorId, string numeroDocumento)
		{
			try
			{
				var proveedor = GetProveedor(proveedorId);
				ViewBag.proveedor = proveedor;
				TempData["proveedor"] = proveedor;
			}
			catch (BusinessException exception)
			{
				TempData["FlashError"] = exception.Message;
				return RedirectToAction("Index");
			}

			var ordenCompra = _ordenCompraManager.FindOrdenCompra(numeroDocumento, proveedorId);
			if (ordenCompra == null)
			{
				TempData["FlashError"] = "Numero de documento incorrecto";
				return RedirectToAction("BuscarOrden", new { proveedorId });
			}
			
			//TODO
			if (CurrentCita != null)
			{
				/*if (CurrentCita.HasOrden(numeroDocumento))
				{
					// TODO
					TempData["FlashError"] = "El numero de documento ya se encuentra en la lista";
					return RedirectToAction("BuscarOrden", new { proveedorId });
				}-*/

                RedirectToAction("AgregarOrden", new  { proveedorId, numeroDocumento, fecha = string.Empty });
			    
			}

			TempData["orden"] = ordenCompra;
			ViewBag.CurrentCita = CurrentCita;
			
			return RedirectToAction("FechaCita");


		}

		public ActionResult FechaCita()
		{
			if (CurrentCita != null)
			{
				return RedirectToAction("Index");
			}


			if (TempData["proveedor"] == null)
			{
				return RedirectToAction("Index");
			}

			if (TempData["orden"] == null)
			{
				return RedirectToAction("Index");
			}
			var proveedor = (proveedore)TempData["proveedor"];
			var ordenCompra = (ordencompra)TempData["orden"];

			ViewBag.Fechas = _ordenCompraManager
				.GetAvailableDatesByOrdenCompra(ordenCompra.NumeroDocumento, proveedor.Id);

            ViewBag.OrdenCompra = ordenCompra;
			ViewBag.proveedor = proveedor;

			return View();
		}


		[ValidateAntiForgeryToken]
		[HttpPost]
		public ActionResult AgregarOrden(int proveedorId, string numeroDocumento, string fecha)
		{
			
			try
			{
				var proveedor = GetProveedor(proveedorId);
				var cuenta = _commonManager.GetCuentaUsuarioAutenticado();
				var ordenCompra = _ordenCompraManager.FindOrdenCompra(numeroDocumento, proveedorId);

			    if (ordenCompra == null)
			    {
                    return RedirectToAction("Index");
			    }

                var ordencompradetalles = _ordenCompraManager.FindDetalle(numeroDocumento, proveedorId);

                ordenCompra.ordencompradetalles = ordencompradetalles;
				if (CurrentCita == null)
				{
					InitCurrentCita(
                        cuenta.Id, 
                        proveedor.Id, 
                        DateTime.ParseExact(fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture));
				}

				Debug.Assert(CurrentCita != null, "CurrentCita != null");
				if (CurrentCita.HasOrden(numeroDocumento))
				{

					TempData["FlashError"] = "El numero de documento ya se encuentra en la lista";
                    return RedirectToAction("BuscarOrden", new { proveedorId });
				}

				CurrentCita.AddOrden(ordenCompra);
				return RedirectToAction("Asn",new{ numeroDocumento = ordenCompra.NumeroDocumento });

			}
			catch (BusinessException exception)
			{
				TempData["FlashError"] = exception.Message;
				return RedirectToAction("Index");
			}
		}
   
		public ActionResult Asn(string numeroDocumento)
		{

			if (CurrentCita == null)
			{
				return RedirectToAction("Index");
			}

			if(!CurrentCita.HasOrden(numeroDocumento))
			{
				return RedirectToAction("ListaDeOrdenes");
			}

            ViewBag.OrdenCompra = CurrentCita.GetOrden(numeroDocumento);
			ViewBag.CurrentCita = CurrentCita;
			return View();
		
		}


		[HttpPost]
		[AcceptVerbs(HttpVerbs.Post)]
		public  ActionResult updItemOrden(string Orden, string Item, string OldValue, string NewValue)
		{

			//int Res = checkoutCitas.ListaDeOrdenes.updElementoEnLista(Orden, Item, OldValue, NewValue);

			int Res = ((CurrentCita) System.Web.HttpContext.Current.Session["controlCita"]).UpdateElementoEnLista(Orden,Item,Int32.Parse(OldValue),Int32.Parse(NewValue));

			return Json(Res, JsonRequestBehavior.DenyGet);

		}

		public ActionResult ListaDeOrdenes(string orden = "0")
		{

		    if (CurrentCita == null)
            {
                return RedirectToAction("Index");
		    }
            
		    ViewBag.CurrentCita = CurrentCita;
			
			return View();
		}

		public void DescargarPlantilla(string numeroDocumento)
		{

		    if (CurrentCita == null)
		    {
		        return;
		    }

		    var ordenCompra = CurrentCita.GetOrden(numeroDocumento);

		    if (ordenCompra == null)
		    {
		        return;
		    }
 		   
			var dt = new DataTable();
			dt.Columns.Add("NumeroMaterial");
			dt.Columns.Add("Descripcion");
			dt.Columns.Add("Cantidad");

            foreach (var detalle in ordenCompra.ordencompradetalles)
			{
				dt.Rows.Add(detalle.NumeroMaterial, detalle.DescripcionMaterial, Decimal.ToInt32(decimal.Parse(detalle.CantidadPedido)));
			}
			
            FileManager.ExportExcel(dt, numeroDocumento + "_plantilla", HttpContext);
		}

			   
		[HttpPost]
		public ActionResult CargarDesdePlantilla(FormCollection collection)
        {
            var numeroDocumento = collection["numeroDocumento"];

			if (CurrentCita == null)
			{
				return RedirectToAction("Index");
			}

            if (!CurrentCita.HasOrden(numeroDocumento))
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

		    var ordenCompra = CurrentCita.GetOrden(numeroDocumento);
            var detalles = ordenCompra.ordencompradetalles;


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
                return RedirectToAction("Asn", new { numeroDocumento });
			}

            //TODO
            TempData["FlashSuccess"] = "Archivo cargado exitosamente";
            return RedirectToAction("Asn", new { numeroDocumento });
		}

	}
}