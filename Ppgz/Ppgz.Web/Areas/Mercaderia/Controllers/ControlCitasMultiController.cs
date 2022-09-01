﻿using System;
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
using System.Web.Script.Serialization;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class ControlCitasMultiController : Controller
    {
        readonly ProveedorManager _proveedorManager = new ProveedorManager();
        readonly CommonManager _commonManager = new CommonManager();

        private const string NombreVarSession = "controlCita";
        private const string NombreVarSession2 = "SociedadCita";
        internal void LimpiarCita()
        {
            if (System.Web.HttpContext.Current.Session[NombreVarSession] != null)
            {
                System.Web.HttpContext.Current.Session.Remove(NombreVarSession);
            }
            if (System.Web.HttpContext.Current.Session[NombreVarSession2] != null)
            {
                System.Web.HttpContext.Current.Session.Remove(NombreVarSession2);
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

        internal string SociedadCita
        {
            get
            {
                if (System.Web.HttpContext.Current.Session[NombreVarSession2] == null)
                {
                    return null;
                }
                return (string)System.Web.HttpContext.Current.Session[NombreVarSession2];
            }
            set
            {
                System.Web.HttpContext.Current.Session[NombreVarSession2] = value;
            }
        }
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        public ActionResult Index()
        {
            LimpiarCita();

            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            ViewBag.proveedores = _proveedorManager.FindByCuentaId(cuenta.Id);

            ViewBag.Almacenes = CommonManager.GetConfiguraciones().Single(c => c.Clave == "warehouse.warehouses").Valor.Split(',');

            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        public ActionResult SeleccionarProveedor(int proveedorId, string centro, string sociedad)
        {
            if (CurrentCita != null)
            {
                LimpiarCita();
            }

            try
            {
                var cuenta = _commonManager.GetCuentaUsuarioAutenticado();
                CurrentCita = new CurrentCita(cuenta.Id, proveedorId, centro, sociedad);
                SociedadCita = sociedad;

                return RedirectToAction("BuscarOrden");
            }
            catch (Exception exception)
            {
                TempData["FlashError"] = exception.Message;
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
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

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult BuscarOrden(string numeroDocumento)
        {
            string[] orden = numeroDocumento.Split(',');
            foreach (var documento in orden)
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
                        foreach (var documentos in orden)
                        {
                            CurrentCita.AddPreAsn(documentos);

                        }
                        return RedirectToAction("ListaDeOrdenes", new { numeroDocumento });
                    }
                    catch (CurrentCita.OrdenDuplicadaException)
                    {
                        TempData["FlashError"] = "El número de documento ya se encuentra en la lista";
                        return RedirectToAction("BuscarOrden", new { proveedor.Id });
                    }
                    catch (CurrentCita.OrdenSinDetalleException)
                    {
                        TempData["FlashError"] = "La orden no contiene items para entregar, por favor seleccione otra orden";
                        return RedirectToAction("BuscarOrden", new { proveedor.Id });
                    }
                    catch (CurrentCita.NumeroDocumentoException)
                    {
                        TempData["FlashError"] = "Número de documento incorrecto";
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

                if (CurrentCita.GetOrdenActivaDisponible(documento) == null)
                {
                    TempData["FlashError"] = "Número de documento incorrecto";
                    return RedirectToAction("BuscarOrden", new { proveedor.Id });

                }

            }
            TempData["numeroDocumento"] = numeroDocumento;
            ViewBag.CurrentCita = CurrentCita;

            return RedirectToAction("FechaCita");
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
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
            var proveedor = CurrentCita.Proveedor;
            var numeroDocumento = (string)TempData["numeroDocumento"];


            string[] ordenes = numeroDocumento.Split(',');
            foreach (var ordencompra in ordenes)
            {
                var orden = CurrentCita.GetOrdenActivaDisponible(ordencompra);

                if (orden == null)
                {
                    TempData["FlashError"] = "Número de documento incorrecto";
                    return RedirectToAction("BuscarOrden");

                }


                ViewBag.Fechas = orden.FechasPermitidas;

                ViewBag.NumeroDocumento = numeroDocumento;
                ViewBag.proveedor = proveedor;

                ViewBag.CurrentCita = CurrentCita;
            }
            return View();
        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        public ActionResult VisualizarDisponibilidad(string numeroDocumento, string fecha, string fechaspermitidas)
        {

            /*
             * 
             * Se valida que los horarios de los riales de la fecha seleccionada esten habilitados y se consultan
             * 
             * */

            var date = DateTime.ParseExact(fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var FechasPermitidas = new JavaScriptSerializer().Deserialize<List<DateTime>>(fechaspermitidas);

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
            var db = new Entities();

            var horarioRieles = db.horariorieles.Where(h => h.Fecha == date.Date).ToList();

            ViewBag.HorarioRieles = horarioRieles;
            ViewBag.date = date;
            ViewBag.numeroDocumento = numeroDocumento;
            ViewBag.CurrentCita = CurrentCita;
            ViewBag.Fechas = FechasPermitidas;

            //fin




            return View();
        }




        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        public ActionResult AgregarPrimeraOrden(string numeroDocumento, string fecha)
        {
            string[] ordenes = numeroDocumento.Split(',');
            foreach (var ordencompra in ordenes)
            {
                if (CurrentCita == null)
                {
                    return RedirectToAction("Index");
                }


                var proveedor = CurrentCita.Proveedor;

                try
                {
                    CurrentCita.SetFecha(DateTime.ParseExact(fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture), ordencompra);

                    CurrentCita.AddPreAsn(ordencompra);



                }
                catch (CurrentCita.OrdenDuplicadaException)
                {
                    TempData["FlashError"] = "El número de documento ya se encuentra en la lista";
                    return RedirectToAction("BuscarOrden", new { proveedor.Id });
                }
                catch (CurrentCita.OrdenSinDetalleException)
                {
                    TempData["FlashError"] = "La orden no contiene items para entregar, por favor seleccione otra orden";
                    return RedirectToAction("BuscarOrden", new { proveedor.Id });
                }
                catch (CurrentCita.NumeroDocumentoException)
                {
                    TempData["FlashError"] = "Número de documento incorrecto";
                    return RedirectToAction("BuscarOrden", new { proveedor.Id });
                }
                catch (BusinessException exception)
                {
                    TempData["FlashError"] = exception.Message;
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
            TempData["resultado"] = numeroDocumento;
            return RedirectToAction("Asn");
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        public ActionResult Asn()
        {
            foreach (var preAsn in CurrentCita.GetPreAsns())
            {
                var pr = preAsn.NumeroDocumento;
                if (pr == null)

                    return RedirectToAction("Index");


                if (CurrentCita == null)
                {
                    return RedirectToAction("Index");
                }


                if (CurrentCita.Fecha == null)
                {
                    return RedirectToAction("BuscarOrden");
                }

                if (!CurrentCita.HasPreAsn(pr))
                {
                    return RedirectToAction("ListaDeOrdenes");
                }

                ViewBag.OrdenCompra = CurrentCita.GetPreAsn(pr);
                ViewBag.CurrentCita = CurrentCita;

            }
            return View();

        }

        [HttpPost]
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        public JsonResult AsnActualizarDetalle(string numeroDocumento, string numeroPosicion, string numeroMaterial, int cantidad)
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


        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
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


        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        public ActionResult EliminarOrden(string numeroDocumento)
        {
            string[] ordenes = numeroDocumento.Split(',');
            foreach (var ordencompra in ordenes)
            {
                CurrentCita.RemovePreAsn(ordencompra);
            }
            TempData["FlashSuccess"] = "Orden eliminada exitosamente";
            return RedirectToAction("ListaDeOrdenes");
        }


        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        public void DescargarPlantilla(FormCollection collection)
        {
            var numeroDocumento = collection["numeroDocumento"];
            string[] ordenes = numeroDocumento.Split(',');
            foreach (var ordencompra in ordenes)
            {
                if (CurrentCita == null)
                {
                    return;
                }
                if (CurrentCita.Fecha == null)
                {
                    return;
                }

                var ordenCompra = CurrentCita.GetPreAsn(ordencompra);

                if (ordenCompra == null)
                {
                    return;
                }

                var dt = new DataTable();
                dt.Columns.Add("NumeroDocumento");
                dt.Columns.Add("NumeroPosicion");
                dt.Columns.Add("NumeroMaterial");
                dt.Columns.Add("Descripcion");
                dt.Columns.Add("Cantidad");
                foreach (var preAsn in CurrentCita.GetPreAsns())
                {
                    foreach (var preAsnDetail in preAsn.Detalles.Where(preAsnDetail => preAsnDetail.Cantidad > 0))
                    {
                        if (preAsnDetail.CantidadPermitida > 0)
                        {
                            dt.Rows.Add(preAsn.NumeroDocumento, preAsnDetail.NumeroPosicion, preAsnDetail.NumeroMaterial,
                                preAsnDetail.DescripcionMaterial, preAsnDetail.CantidadPermitida);

                        }
                    }
                }
                FileManager.ExportExcel(dt, CurrentCita.Proveedor.Nombre1 + CurrentCita.Proveedor.Nombre2 + CurrentCita.Proveedor.Nombre3 + CurrentCita.Proveedor.Nombre4 + "_plantilla", HttpContext);
            }
        }


        [HttpPost]
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        public ActionResult CargarDesdePlantilla()
        {

            foreach (var preAsn in CurrentCita.GetPreAsns())
            {
                var pr = preAsn.NumeroDocumento;



                if (CurrentCita == null)
                {
                    return RedirectToAction("Index");
                }
                if (CurrentCita.Fecha == null)
                {
                    return RedirectToAction("BuscarOrden");
                }
                if (!CurrentCita.HasPreAsn(pr))
                {
                    return RedirectToAction("Index");
                }

                var file = Request.Files[0];

                if (file != null && file.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    TempData["FlashError"] = "Archivo incorrecto";
                    return RedirectToAction("Asn", new { pr });
                }

                if (file == null)
                {
                    TempData["FlashError"] = "Archivo incorrecto";
                    return RedirectToAction("Asn", new { pr });
                }

                var fileName = Path.GetFileName(file.FileName);



                if (fileName == null)
                {
                    TempData["FlashError"] = "Archivo incorrecto";
                    return RedirectToAction("Asn", new { pr });
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
                        var numeroOrden = ws.Row(i).Cell(1).Value.ToString();
                        var numeroPosicion = ws.Row(i).Cell(2).Value.ToString();
                        var numeroMaterial = ws.Row(i).Cell(3).Value.ToString();
                        var cantidad = int.Parse(ws.Row(i).Cell(5).Value.ToString());

                        try
                        {
                            CurrentCita.UpdateDetail(numeroOrden, numeroPosicion, numeroMaterial, cantidad);
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
                    return RedirectToAction("Asn", new { pr });
                }
                catch (Exception)
                {
                    TempData["FlashError"] = "Archivo incorrecto";
                    return RedirectToAction("Asn", new { pr });
                }
            }
            //TODO
            TempData["FlashSuccess"] = "Archivo cargado exitosamente";
            return RedirectToAction("Asn");

        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        public ActionResult SeleccionarRieles()
        {
            //Validacion de cantidad mínima
            //const string sql = @"SELECT Clave, Valor FROM configuraciones WHERE Clave = 'warehouse.min-pairs.per-meet' AND Habilitado = 1";
            var result = CommonManager.GetConfiguraciones().Single(c => c.Clave == "warehouse.min-pairs.per-meet");
            //var result = Db.GetDataTable(sql);

            if (String.IsNullOrEmpty(result.Valor))
            {
                ViewBag.CantidadMinimaCita = 270;
            }
            else
            {
                ViewBag.CantidadMinimaCita = Convert.ToInt32(result.Valor);
            }

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

            var date = ((DateTime)CurrentCita.Fecha).Date;

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
        public ActionResult Agendar(int[] rielesIds, DateTime FechaCreacion)
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
                Fecha = (DateTime)CurrentCita.Fecha,
                ProveedorId = CurrentCita.Proveedor.Id,
                UsuarioId = _commonManager.GetUsuarioAutenticado().Id,
                Asns = new List<Asn>(),
                HorarioRielesIds = rielesIds.ToList(),
                FechaCreacion = (DateTime)FechaCreacion,
                Sociedad = SociedadCita
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
                        OrdenNumeroDocumento = preAsn.NumeroDocumento,
                        Tienda = preAsn.Tienda,

                        TiendaOrigen = preAsn.TiendaOrigen,
                        CantidadSolicitada = preAsnDetail.CantidadPedido,
                        InOut = preAsn.InOut,
                        Precio = preAsnDetail.Precio,
                        UnidadMedida = preAsnDetail.UnidadMedida,
                        NumeroSurtido = preAsn.NumeroOrdenSurtido,
                        NumeroMaterial2 = preAsnDetail.NumeroMaterial2,

                        Centro = preAsn.Centro
                    });
                }
            }
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
                CitaManager.RegistrarCita(preCita);

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
            //Test para probar las reglas de las fechas
            //DateTime myDT = new DateTime(2018, 1, 5);
            //DateTime orden = new DateTime(2018, 1, 22);

            //var tes = RulesManager.Regla4(myDT);
            //var tes2 = RulesManager.Regla3(myDT, orden);

            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();



            var proveedoresIds = _proveedorManager.FindByCuentaId(cuenta.Id).Select(p => p.Id).ToList();
            var db = new Entities();

            var fecha = DateTime.Today.Date;
            var citas = db.citas.Where(c => proveedoresIds.Contains(c.ProveedorId) && c.FechaCita >= fecha && c.TipoCita == null).ToList();

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
            if (!RulesManager.PuedeEditarCita(cita.FechaCita, cita.FechaCreacion))
            {
                //TODO
                TempData["FlashError"] = "La cita no puede ser Editada";
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

            if (!RulesManager.PuedeEditarCita(cita.FechaCita, cita.FechaCreacion))
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
                        asnIds.Add(asnId);
                        asn.Cantidad = cantidad;
                        db.Entry(asn).State = EntityState.Modified;
                    }
                    else
                    {

                        //Cuando el valor de la ASN llega a Cero se envia a Scale para su elimicacion y a Sap para desmarcar
                        try
                        {
                            CitaManager.EliminarAsnScale(asn);

                        }
                        finally
                        {
                            var asnAborrar = new asn();
                            asnAborrar.OrdenNumeroDocumento = asn.OrdenNumeroDocumento;
                            asnAborrar.NumeroPosicion = asn.NumeroPosicion;

                            db.Entry(asn).State = EntityState.Deleted;

                            CitaManager.DesmarcarEnActualizacion(asnAborrar);
                        }

                    }
                }
                if (element.ToString().IndexOf("horarioriel", StringComparison.Ordinal) == 0)
                {

                    //Fix para manejar multiples rieles
                    var rieles = collection[element.ToString()];
                    if (rieles.Contains(","))
                    {
                        var arrayRieles = rieles.Split(',');
                        foreach (var riel in arrayRieles)
                        {
                            var horarioRielId = int.Parse(riel);
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
                    else
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


            }

            db.SaveChanges();

            cita = db.citas.FirstOrDefault(c => proveedoresIds.Contains(c.ProveedorId) && c.Id == citaId);
            if (cita != null)
            {
                cita.CantidadTotal = cita.asns.Sum(a => a.Cantidad);
                cita.RielesOcupados = (sbyte)cita.horariorieles.Count;
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