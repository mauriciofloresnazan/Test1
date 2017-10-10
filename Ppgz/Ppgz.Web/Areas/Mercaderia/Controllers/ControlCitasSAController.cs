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
using Ppgz.Repository;
using Ppgz.Services;
using Ppgz.Web.Infrastructure;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
	[Authorize]
	[TerminosCondiciones]
	public class ControlCitasSAController : Controller
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
		
		[Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
		public ActionResult Index()
		{
			LimpiarCita();
			
			var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            var proveedoresSA = _proveedorManager.FindByCuentaId(cuenta.Id);

            ViewBag.proveedores = proveedoresSA;

            ViewBag.Almacenes = CommonManager.GetConfiguraciones().Single(c => c.Clave == "warehouse.warehouses").Valor.Split(',');

            DateTime thisDay = DateTime.Today;

            ViewBag.FechasPermitidas = RulesManager.GetFechasPermitidas(thisDay, proveedoresSA[0].cuenta.EsEspecial, false);

            return View();
		}
		
		[Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
		public ActionResult SeleccionarProveedor(int proveedorId, int cantidad, string fecha)
		{
			if (CurrentCita != null)
			{
				LimpiarCita();
			}

			try
			{
				var cuenta = _commonManager.GetCuentaUsuarioAutenticado();
				CurrentCita = new CurrentCita(cuenta.Id, proveedorId);
                CurrentCita.CantidadSinASN = cantidad;
                CurrentCita.SetFecha(DateTime.ParseExact(fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture));

                return RedirectToAction("SeleccionarRieles");
			}
			catch (Exception exception)
			{
				TempData["FlashError"] = exception.Message;
				return RedirectToAction("Index"); 
			}
		}
		
		

		
		

		[Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
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

			if (CurrentCita.CantidadSinASN < 1)
			{
				TempData["FlashError"] = "Debe incluir al menos un (1) PAR para poder agendar la Cita";
				return RedirectToAction("BuscarOrden");
			}

			var date = ((DateTime) CurrentCita.Fecha).Date;

		    try
		    {
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
		    }
		    catch (Exception exception)
		    {
                TempData["FlashError"] = exception.Message;
                return RedirectToAction("ListaDeOrdenes");
		    }
			
			ViewBag.CurrentCita = CurrentCita;

			var db = new Entities();

			var horarioRieles = db.horariorieles.Where(h => h.Fecha == date.Date).ToList();

			ViewBag.HorarioRieles = horarioRieles;
			return View();
		}

	    public JsonResult VerificarRieles(string fecha)
	    {
            var date = DateTime.ParseExact(fecha, "ddMMyyyy", CultureInfo.InvariantCulture);

            var db = new Entities();
            var horarioRieles = db.horariorieles.Where(hr => hr.Fecha == date).ToList();


            return Json(
                horarioRieles.Select(hr => new
                {
                    hr.Id,
                    hr.Disponibilidad
                }));
	    }


		[Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
		[ValidateAntiForgeryToken]
		[HttpPost]
		public ActionResult Agendar(int[] rielesIds)
		{
			if (CurrentCita.Fecha == null)
			{
                // Todo mejorar
				return RedirectToAction("Citas");
			}

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

			//foreach (var preAsn in CurrentCita.GetPreAsns())
			//{
			//	foreach (var preAsnDetail in preAsn.Detalles.Where(preAsnDetail => preAsnDetail.Cantidad > 0))
			//	{
			//		preCita.Asns.Add(new Asn
			//		{
			//			Cantidad = preAsnDetail.Cantidad,
			//			NombreMaterial = preAsnDetail.DescripcionMaterial,
			//			NumeroMaterial = preAsnDetail.NumeroMaterial,
			//			NumeroPosicion = preAsnDetail.NumeroPosicion,
			//			OrdenNumeroDocumento = preAsn.NumeroDocumento,
   //                     Tienda = preAsn.Tienda,

   //                     TiendaOrigen = preAsn.TiendaOrigen,
   //                     CantidadSolicitada = preAsnDetail.CantidadPedido,
   //                     InOut = preAsn.InOut,
   //                     Precio = preAsnDetail.Precio,
   //                     UnidadMedida = preAsnDetail.UnidadMedida,
   //                     NumeroSurtido = preAsn.NumeroOrdenSurtido,
   //                     NumeroMaterial2 = preAsnDetail.NumeroMaterial2,

   //                     Centro = preAsn.Centro
			//		});
			//	}
			//}
		    try
		    {
		        /*
                var url = CommonManager.GetConfiguraciones().Single(c => c.Clave == "wfc.url.cita.add").Valor;;
                var client = new RestClient(url);
			    var request = new RestRequest(string.Empty, Method.POST);

                var microsoftDateFormatSettings = new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
                };

                var json = JsonConvert.SerializeObject(preCita, microsoftDateFormatSettings);
                request.AddParameter("application/json", json, ParameterType.RequestBody);

                var response = (RestResponse)client.Execute(request);
                
                TempData["FlashSuccess"] = response.Content + ", " + response.StatusDescription;
                return RedirectToAction("SeleccionarRieles");
                */
		        CitaManager.RegistrarCitaSinASN(preCita);
		        
		    }
		    catch (ScaleException exception)
		    {
                LimpiarCita();
                TempData["FlashError"] = exception.Message;
                return RedirectToAction("Citas");
		    }
			catch (Exception exception)
			{

				//TODO
				TempData["FlashError"] = exception.Message;
				return RedirectToAction("SeleccionarRieles");
			}

			//TODO
            LimpiarCita();
			TempData["FlashSuccess"] = "Ha terminado de configurar su cita exitosamente";
			return RedirectToAction("Citas");
		}
		
		[Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
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

		[Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
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
		
		[Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
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
		
		[ValidateAntiForgeryToken]
		[HttpPost]
		[Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
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

		    var asnIds = new List<int>();

			foreach (var element in collection)
			{
				if (element.ToString().IndexOf("asnid-", StringComparison.Ordinal) == 0)
				{
					var asnId = int.Parse(element.ToString().Replace("asnid-", string.Empty));
                    asnIds.Add(asnId);

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

		    CitaManager.ActualizarCantidadScale(asnIds.ToArray());

			TempData["FlashSuccess"] = "Cita actualizada exitosamente";
			return RedirectToAction("Citas");
		}
		
		[Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
		public JsonResult CalcularRieles(int cantidad)
		{
				return Json(new { rieles = RulesManager.GetCantidadRieles(cantidad) });
			
		}
	
	}
}