using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ClosedXML.Excel;
using MySql.Data.MySqlClient;
using Ppgz.CitaWrapper;
using Ppgz.CitaWrapper.Entities;
using Ppgz.Repository;
using Ppgz.Services;
using Ppgz.Web.Infrastructure;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
	[Authorize]
	[TerminosCondiciones]
	public class ControlCitasController : Controller
	{
		readonly ProveedorManager _proveedorManager = new ProveedorManager();
		readonly CommonManager _commonManager = new CommonManager();

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
			set
			{
				System.Web.HttpContext.Current.Session[NombreVarSession] = value;
			}
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


		//
		// GET: /Mercaderia/ControlCitas/
		[Authorize(Roles = "MAESTRO-MERCADERIA")]
		public ActionResult SeleccionarProveedor(int proveedorId, string centro)
		{
			if (CurrentCita != null)
			{
				LimpiarCita();
			}

			try
			{
				var cuenta = _commonManager.GetCuentaUsuarioAutenticado();
				CurrentCita = new CurrentCita(cuenta.Id, proveedorId, centro);
				return RedirectToAction("BuscarOrden");
			}
			catch (Exception exception)
			{
				TempData["FlashError"] = exception.Message;
				return RedirectToAction("Index"); 
			}
		}

		
		[Authorize(Roles = "MAESTRO-MERCADERIA")]
		public ActionResult BuscarOrden(int proveedorId = 0)
		{
			try
			{
				if (CurrentCita == null)
				{
					return RedirectToAction("Index");
				}

			}
			catch (BusinessException exception)
			{
				TempData["FlashError"] = exception.Message;
				return RedirectToAction("Index");
			}

			ViewBag.proveedor = CurrentCita.Proveedor; 
			ViewBag.CurrentCita = CurrentCita;
			
			return View();
		}

		[Authorize(Roles = "MAESTRO-MERCADERIA")]
		[ValidateAntiForgeryToken]
		[HttpPost]
		public ActionResult BuscarOrden(string numeroDocumento)
		{

			if (CurrentCita == null)
			{
				return RedirectToAction("Index");
			}
			

			var proveedor = CurrentCita.Proveedor;
			

		   /* var ordenCompra = CurrentCita.GetOrdenActivaDisponible(numeroDocumento);
			
			if (ordenCompra == null)
			{
				// TODO pasar a resource
				TempData["FlashError"] = "Numero de documento incorrecto";
				return RedirectToAction("BuscarOrden", new { proveedor.Id });
			}*/
			
			//TODO
			if (CurrentCita.Fecha != null)
			{
				try
				{
					CurrentCita.AddPreAsn(numeroDocumento);
					return RedirectToAction("Asn", new {numeroDocumento });

				}
				catch (CurrentCita.OrdenDuplicadaException)
				{
					TempData["FlashError"] = "El numero de documento ya se encuentra en la lista";
					return RedirectToAction("BuscarOrden", new {proveedor.Id});
				}
				catch (CurrentCita.OrdenSinDetalleException)
				{
					TempData["FlashError"] = "La orden no contiene items para entregar, por favor seleccione otra orden";
					return RedirectToAction("BuscarOrden", new {proveedor.Id});
				}
				catch (CurrentCita.NumeroDocumentoException)
				{
					TempData["FlashError"] = "Numero de documento incorrecto";
					return RedirectToAction("BuscarOrden", new { proveedor.Id });
				}
				catch (CurrentCita.FechaException)
				{
					TempData["FlashError"] = "La orden no puede ser entregada en la fecha de la cita";
					return RedirectToAction("BuscarOrden", new { proveedor.Id });
				}
				catch (CurrentCita.OrdenCentroException)
				{
					TempData["FlashError"] = "La orden no contiene items para el Almacén seleccionado";
					return RedirectToAction("BuscarOrden", new { proveedor.Id });
				}
			}

			if (CurrentCita.GetOrdenActivaDisponible(numeroDocumento) == null)
			{
				TempData["FlashError"] = "Numero de documento incorrecto";
				return RedirectToAction("BuscarOrden", new { proveedor.Id });
				
			}


			TempData["numeroDocumento"] = numeroDocumento;
			ViewBag.CurrentCita = CurrentCita;
			
			return RedirectToAction("FechaCita");
		}

		public ActionResult FechaCita()
		{

			if (CurrentCita == null)
			{
				return RedirectToAction("Index");
			}

			if (CurrentCita.Fecha != null)
			{
				// TODO redireccionar a la seccion de busqueda o al checkout
				return RedirectToAction("Index");
			}


			if (TempData["numeroDocumento"] == null)
			{
				return RedirectToAction("Index");
			}
			var numeroDocumento = (string) TempData["numeroDocumento"];

			var proveedor = CurrentCita.Proveedor;

			var orden = CurrentCita.GetOrdenActivaDisponible(numeroDocumento);

			if (orden == null)
			{
				TempData["FlashError"] = "Numero de documento incorrecto";
				return RedirectToAction("BuscarOrden");
				
			}


			ViewBag.Fechas = orden.FechasPermitidas;

			ViewBag.NumeroDocumento = numeroDocumento;
			ViewBag.proveedor = proveedor;

			ViewBag.CurrentCita = CurrentCita;
			return View();
		}


		


		[ValidateAntiForgeryToken]
		[HttpPost]
		public ActionResult AgregarPrimeraOrden(string numeroDocumento, string fecha)
		{

			if (CurrentCita == null)
			{
				return RedirectToAction("Index");
			}
			
			
			var proveedor = CurrentCita.Proveedor;

			try
			{
				CurrentCita.SetFecha(DateTime.ParseExact(fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture), numeroDocumento);

				CurrentCita.AddPreAsn(numeroDocumento);

				return RedirectToAction("Asn", new {numeroDocumento});

			}
			catch (CurrentCita.OrdenDuplicadaException)
			{
				TempData["FlashError"] = "El numero de documento ya se encuentra en la lista";
				return RedirectToAction("BuscarOrden", new {proveedor.Id});
			}
			catch (CurrentCita.OrdenSinDetalleException)
			{
				TempData["FlashError"] = "La orden no contiene items para entregar, por favor seleccione otra orden";
				return RedirectToAction("BuscarOrden", new {proveedor.Id});
			}
			catch (CurrentCita.NumeroDocumentoException)
			{
				TempData["FlashError"] = "Numero de documento incorrecto";
				return RedirectToAction("BuscarOrden", new {proveedor.Id});
			}
			catch (BusinessException exception)
			{
				TempData["FlashError"] = exception.Message;
				return RedirectToAction("BuscarOrden", new {proveedor.Id});
			}
			catch (CurrentCita.FechaException)
			{
				TempData["FlashError"] = "La orden no puede ser entregada en la fecha de la cita";
				return RedirectToAction("BuscarOrden", new {proveedor.Id});
			}
			catch (CurrentCita.OrdenCentroException)
			{
				TempData["FlashError"] = "La orden no contiene items para el Almacén seleccionado";
				return RedirectToAction("BuscarOrden", new { proveedor.Id });
			}
		}
   
		public ActionResult Asn(string numeroDocumento)
		{

			if (CurrentCita == null)
			{
				return RedirectToAction("Index");
			}


			if (CurrentCita.Fecha == null)
			{
				return RedirectToAction("BuscarOrden");
			}

			if(!CurrentCita.HasPreAsn(numeroDocumento))
			{
				return RedirectToAction("ListaDeOrdenes");
			}

			ViewBag.OrdenCompra = CurrentCita.GetPreAsn(numeroDocumento);
			ViewBag.CurrentCita = CurrentCita;
			return View();
		
		}



		[HttpPost]
		public JsonResult AsnActualizarDetalle(string numeroDocumento, string numeroPosicion,  string numeroMaterial, int cantidad)
		{
			try
			{
				CurrentCita.UpdateDetail(numeroDocumento, numeroPosicion, numeroMaterial, cantidad);
			}
			catch (Exception exception)
			{
				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return Json(exception.Message);
			}
			return Json("Actualizado correctamente");
		}

	


		public ActionResult ListaDeOrdenes()
		{

			if (CurrentCita == null)
			{
				return RedirectToAction("Index");
			}

			if (CurrentCita.Fecha == null)
			{
				return RedirectToAction("BuscarOrden");
			}

			ViewBag.CurrentCita = CurrentCita;
			
			return View();
		}

		public ActionResult EliminarOrden(string numeroDocumento)
		{
			CurrentCita.RemovePreAsn(numeroDocumento);

			TempData["FlashSuccess"] = "Orden eliminada exitosamente";
			return RedirectToAction("ListaDeOrdenes");
		}

		public void DescargarPlantilla(string numeroDocumento)
		{

			if (CurrentCita == null)
			{
				return;
			}
			if (CurrentCita.Fecha == null)
			{
				return;
			}

			var ordenCompra = CurrentCita.GetPreAsn(numeroDocumento);

			if (ordenCompra == null)
			{
				return;
			}

			var dt = new DataTable();
			dt.Columns.Add("NumeroPosicion");
			dt.Columns.Add("NumeroMaterial");
			dt.Columns.Add("Descripcion");
			dt.Columns.Add("Cantidad");

			foreach (var detalle in ordenCompra.Detalles)
			{
				if (detalle.CantidadPermitida > 0)
				{
					dt.Rows.Add(detalle.NumeroPosicion, detalle.NumeroMaterial,
						detalle.DescripcionMaterial, detalle.CantidadPermitida);
					
				}
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
			if (CurrentCita.Fecha == null)
			{
				return RedirectToAction("BuscarOrden");
			}
			if (!CurrentCita.HasPreAsn(numeroDocumento))
			{
				return RedirectToAction("Index");
			}

			var file = Request.Files[0];

			if (file != null && file.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
			{
				TempData["FlashError"] = "Archivo incorrecto";
				return RedirectToAction("Asn", new { numeroDocumento });
			}

			if (file == null)
			{
				TempData["FlashError"] = "Archivo incorrecto";
				return RedirectToAction("Asn", new { numeroDocumento });
			}

			var fileName = Path.GetFileName(file.FileName);
			


			if (fileName == null)
			{
				TempData["FlashError"] = "Archivo incorrecto";
				return RedirectToAction("Asn", new { numeroDocumento });
			}

			/*var ordenCompra = CurrentCita.GetPreAsn(numeroDocumento);
			var detalles = ordenCompra.Detalles;
			*/

			var wb = new XLWorkbook(file.InputStream);


			var ws = wb.Worksheet(1);

			try
			{
				for (var i = 2; i < ws.RowsUsed().ToList().Count + 1; i++)
				{
					var numeroPosicion = ws.Row(i).Cell(1).Value.ToString();
					var numeroMaterial = ws.Row(i).Cell(2).Value.ToString();
					var cantidad = int.Parse(ws.Row(i).Cell(4).Value.ToString());

					try
					{
						CurrentCita.UpdateDetail(numeroDocumento, numeroPosicion, numeroMaterial, cantidad);
					}
					catch (Exception exception)
					{
						throw new BusinessException(string.Format("Material {0}: {1}", numeroMaterial, exception.Message));
					}


/*                    var detalle = detalles.FirstOrDefault(d => d.NumeroPosicion == numeroPosicion && d.NumeroMaterial == numeroMaterial);

					if (detalle != null)
					{
						if (cantidad > detalle.CantidadPermitida)
						{
							throw new BusinessException(string.Format("Error en la cantidad del Material {0}", detalle.NumeroMaterial));
						}
						detalle.Cantidad  = cantidad;
					}*/
				}
			}
			catch (BusinessException businessException)
			{
				TempData["FlashError"] = businessException.Message;
				return RedirectToAction("Asn", new { numeroDocumento });
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



		public ActionResult SeleccionarRieles()
		{
			if (CurrentCita == null)
			{
				return RedirectToAction("Index");
			}
			
			if (CurrentCita.Fecha == null)
			{
				return RedirectToAction("BuscarOrden");
			}

			if (CurrentCita.Cantidad < 1)
			{
				TempData["FlashError"] = "Debe incluir al menos un (1) PAR para poder agendar la Cita";
				return RedirectToAction("BuscarOrden");
			}

			var date = ((DateTime) CurrentCita.Fecha).Date;
			var parameters = new List<MySqlParameter>()
			{
				new MySqlParameter
				{
					ParameterName = "pTotal",
					Direction = ParameterDirection.Output,
					MySqlDbType = MySqlDbType.VarChar
				},
				new MySqlParameter("pFecha", date)
			};

			Db.ExecuteProcedureOut(parameters, "config_appointment");
			
			ViewBag.CurrentCita = CurrentCita;

			var db = new Entities();

			var horarioRieles = db.horariorieles.Where(h => h.Fecha == date.Date).ToList();

			ViewBag.HorarioRieles = horarioRieles;
			return View();
		}




		[Authorize(Roles = "MAESTRO-MERCADERIA")]
		[ValidateAntiForgeryToken]
		[HttpPost]
		public ActionResult Agendar(int[] rielesIds)
		{

			var preCita = new PreCita()
			{
				Cantidad = CurrentCita.Cantidad,
				Centro = CurrentCita.Centro,
				Fecha = (DateTime) CurrentCita.Fecha,
				ProveedorId = CurrentCita.Proveedor.Id,
				UsuarioId = _commonManager.GetUsuarioAutenticado().Id,
				Asns = new List<Asn>(),
				HorarioRielesIds = rielesIds.ToList()
			};

			foreach (var preAsn in CurrentCita.GetPreAsns())
			{
				foreach (var preAsnDetail in preAsn.Detalles.Where(preAsnDetail => preAsnDetail.Cantidad > 0))
				{
					preCita.Asns.Add(new Asn
					{
						Cantidad = preAsnDetail.Cantidad,
						NombreMaterial = preAsnDetail.DescripcionMaterial,
						NumeroMaterial = preAsnDetail.NumeroMaterial,
						NumeroPosicion = preAsnDetail.NumeroPosicion,
						OrdenNumeroDocumento = preAsn.NumeroDocumento
					});
				}
			}
			try
			{

				CitaManager.RegistrarCita(preCita);
			}
			catch (Exception exception)
			{

				//TODO
				TempData["FlashError"] = exception.Message;
				return RedirectToAction("Citas");
			}

			//TODO
			TempData["FlashSuccess"] = "Ha terminado de configurar su cita exitosamente";
			return RedirectToAction("Citas");
		}


		[Authorize(Roles = "MAESTRO-MERCADERIA")]
		public ActionResult Citas()
		{
			var cuenta = _commonManager.GetCuentaUsuarioAutenticado();
			
			var proveedoresIds = _proveedorManager.FindByCuentaId(cuenta.Id).Select(p => p.Id).ToList();
			var db = new Entities();

			var fecha = DateTime.Today.Date;
			var citas = db.citas.Where(c => proveedoresIds.Contains(c.ProveedorId) && c.FechaCita >= fecha).ToList();

			ViewBag.Citas = citas;

			return View();
		}
		public ActionResult CitaDetalle(int citaId)
		{
			var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

			var proveedoresIds = _proveedorManager.FindByCuentaId(cuenta.Id).Select(p => p.Id).ToList();

			var db = new Entities();
			var cita = db.citas.FirstOrDefault(c => c.Id == citaId && proveedoresIds.Contains(c.ProveedorId));

			if (cita == null)
			{
				//TODO
				TempData["FlashError"] = "Cita incorrecta";
				return RedirectToAction("Citas");
			}

			ViewBag.Cita = cita;

			return View();
		}

		[Authorize(Roles = "MAESTRO-MERCADERIA")]
		public ActionResult CancelarCita(int citaId)
		{
			var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

			var proveedoresIds = _proveedorManager.FindByCuentaId(cuenta.Id).Select(p => p.Id).ToList();
			
			var db = new Entities();

			var cita = db.citas.FirstOrDefault(c => proveedoresIds.Contains(c.ProveedorId) && c.Id == citaId);

			if (cita == null)
			{
				//TODO
				TempData["FlashError"] = "Cita incorrecta";
				return RedirectToAction("Citas");
			}

			if (!RulesManager.PuedeCancelarCita(cita.FechaCita))
			{
				//TODO
				TempData["FlashError"] = "La cita no puede ser Cancelada";
				return RedirectToAction("Citas");
			}

			CitaManager.CancelarCita(citaId);

			//TODO
			TempData["FlashSuccess"] = "Cita cancelada exitosamente";
			return RedirectToAction("Citas");
		}

		[Authorize(Roles = "MAESTRO-MERCADERIA")]
		[ValidateAntiForgeryToken]
		[HttpPost]
		public ActionResult ActualizarCita(int citaId, FormCollection collection)
		{
			var db = new Entities();

			var cuenta = _commonManager.GetCuentaUsuarioAutenticado();
			var proveedoresIds = _proveedorManager.FindByCuentaId(cuenta.Id).Select(p => p.Id).ToList();
			var cita = db.citas.FirstOrDefault(c => proveedoresIds.Contains(c.ProveedorId) && c.Id == citaId);
			
			if (cita == null)
			{
				//TODO
				TempData["FlashError"] = "Cita incorrecta";
				return RedirectToAction("Citas");
			}

			foreach (var hriel in cita.horariorieles.ToList())
			{
				hriel.CitaId = null;
				hriel.Disponibilidad = true;
				db.Entry(hriel).State = EntityState.Modified;
			}

			if (!RulesManager.PuedeEditarCita(cita.FechaCita))
			{
				//TODO
				TempData["FlashError"] = "La cita no puede ser Editada";
				return RedirectToAction("Citas");
			}
			
			foreach (var element in collection)
			{
				if (element.ToString().IndexOf("asnid-", StringComparison.Ordinal) == 0)
				{
					var asnId = int.Parse(element.ToString().Replace("asnid-", string.Empty));
					var cantidad = int.Parse(collection[element.ToString()]);

					var asn = db.asns.FirstOrDefault(a => a.Id == asnId && a.CitaId == citaId);

					if (asn == null)
					{
						//TODO
						TempData["FlashError"] = "Asns incorrectos en la edición.";
						return RedirectToAction("Citas");
					}
					if (cantidad > 0)
					{
						asn.Cantidad = cantidad;
						db.Entry(asn).State = EntityState.Modified;
					}
					else
					{
						db.Entry(asn).State = EntityState.Deleted;
					}
				}
				if (element.ToString().IndexOf("horarioriel", StringComparison.Ordinal) == 0)
				{
					var horarioRielId = int.Parse(collection[element.ToString()]);
					var horarioRiel = db.horariorieles.Find(horarioRielId);
					if (horarioRiel == null)
					{
						//TODO
						TempData["FlashError"] = "Rieles incorrectos en la edición.";
						return RedirectToAction("Citas");
					}

					horarioRiel.Disponibilidad = false;
					horarioRiel.CitaId = citaId;
					db.Entry(horarioRiel).State = EntityState.Modified;
				}


			}

			db.SaveChanges();

			cita = db.citas.FirstOrDefault(c => proveedoresIds.Contains(c.ProveedorId) && c.Id == citaId);
			if (cita != null)
			{
				cita.CantidadTotal = cita.asns.Sum(a => a.Cantidad);
				cita.RielesOcupados = (sbyte) cita.horariorieles.Count;
				db.Entry(cita).State = EntityState.Modified;
			}
			

			db.SaveChanges();

			TempData["FlashSuccess"] = "Cita actualizada exitosamente";
			return RedirectToAction("Citas");
		}


		public JsonResult CalcularRieles(int cantidad)
		{
				return Json(new { rieles = RulesManager.GetCantidadRieles(cantidad) });
			
		}
	
	}
}