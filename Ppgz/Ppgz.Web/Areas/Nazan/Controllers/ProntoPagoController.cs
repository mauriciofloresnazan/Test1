﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Ppgz.Repository;
using Ppgz.Services;
using Ppgz.Web.Infrastructure;
using Ppgz.Web.Models.ProntoPago;
using SapWrapper;
using System.Data.Entity;
//using Data = System.Collections.Generic.KeyValuePair<string, string>;

namespace Ppgz.Web.Areas.Nazan.Controllers
{
    public class ProntoPagoController : Controller
    {
        readonly Entities _db = new Entities();
        
        //Inicializacion de servicios para Pronto Pago - Factoraje
        readonly CuentaManager _cuentaManager = new CuentaManager();
        readonly ConfiguracionesFManager _configuracionesFManager = new ConfiguracionesFManager();
        readonly DescuentoFManager _descuentoFManager = new DescuentoFManager();
        readonly FacturaFManager _facturaFManager = new FacturaFManager();
        readonly ProveedorManager _proveedorManager = new ProveedorManager();
        readonly ProveedorFManager _proveedorFManager = new ProveedorFManager();
        readonly SolicitudFManager _solicitudFManager = new SolicitudFManager();
        readonly LogsFactoraje _logsFactoraje = new LogsFactoraje();

        internal string SociedadActiva
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["sociedadactiva"] != null)
                {
                    return (string)System.Web.HttpContext.Current.Session["sociedadactiva"];
                }
                return null;
            }
            set
            {
                System.Web.HttpContext.Current.Session["sociedadactiva"] = value;
            }
        }

        /*-------------BEGIN DASHBOARD SECTION--------------*/
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO,NAZAN-PRONTOPAGO-APROBADOR")]
        public ActionResult Index()
        {            
            string listadoSociedades = CommonManager.GetConfiguraciones().Single(c => c.Clave == "sociedades.lista").Valor;
            string[] lsociedad = listadoSociedades.Trim().Split(',').ToArray();
            string[,] lSociedades = new string[lsociedad.Count(), 2];

            for (int i = 0; i < lsociedad.Count(); i++)
            {
                lSociedades[i,0] = lsociedad[i];
                lSociedades[i,1] = CommonManager.GetConfiguraciones().Single(c => c.Clave == "sociedades.nombre." + lsociedad[i]).Valor; ;
            }

            ViewBag.Sociedades = lSociedades;
            return View();
        }
        /*-------------BEGIN DASHBOARD SECTION--------------*/

        /*-------------BEGIN DASHBOARD SECTION--------------*/
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO,NAZAN-PRONTOPAGO-APROBADOR")]
        public ActionResult Index2(string sociedad)
        {
            SociedadActiva = sociedad;

            //Se contabilizan los estatusfactoraje <estatus, cantudad>
            var lse = GetSolicitudEstatus();
            ViewBag.SolicitudesEstatus = lse;
            ViewBag.ParaEnviar = lse.Where(x => x.EstatusNombre == "Lista Para Propuesta").FirstOrDefault().Cantidad.ToString();

            //Se cambia el estatus de solicitudes anteriores
            var solicitudesAnteriores = _solicitudFManager.GetSolicitudesFactoraje();
            DateTime startOfWeek = DateTime.Today.AddDays(-1 * (int)(DateTime.Today.DayOfWeek));
            solicitudesAnteriores = solicitudesAnteriores.Where(x => x.Fecha < startOfWeek && x.Estatus != 3 && x.Estatus != 7).ToList();

            if (solicitudesAnteriores.Count > 0)
            {
                foreach (var sa in solicitudesAnteriores)
                {
                    _solicitudFManager.UpdateEstatusSolicitud(sa.Id, 3);
                }
            }

            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO,NAZAN-PRONTOPAGO-APROBADOR")]
        public ActionResult DashboardEnviarPropuestas()
        {
            var listpropuestas = new List<solicitudesfactoraje>();
            int idestatus = _db.estatusfactoraje.Where(x => x.Nombre == "Lista Para Propuesta" && x.Activo == 1).FirstOrDefault().idEstatusFactoraje;
            listpropuestas = _solicitudFManager.GetSolicitudesByEstatus(idestatus);
            listpropuestas = listpropuestas.Where(x => x.Sociedad == SociedadActiva).ToList();

            TempData["FlashError"] = "Error al enviar solicitudes";
            TempData["FlashSuccess"] = "Solicitudes enviadas con exito";
            return RedirectToAction("Index");
        }

        public JsonResult GetPieChartData()
        {
            //Se contabilizan los estatus de las solicitudesfactoraje
            var result = GetSolicitudEstatus();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public List<KeyValueCustom> GetSolicitudEstatus()
        {
            //Obtenemos una lista de las SolicitudesFactoraje de la semana actual
            var lsolicitudesf = _solicitudFManager.GetSolicitudesFactoraje();
            DateTime startOfWeek = DateTime.Today.AddDays(-1 * (int)(DateTime.Today.DayOfWeek));
            lsolicitudesf = lsolicitudesf.Where(x => x.Fecha > startOfWeek && x.Sociedad == SociedadActiva).ToList();

            //Obtenemos una lista de los estatusfactoraje contabilizados 
            var lestatus = _db.estatusfactoraje.ToList();
            var estatusSolicitudes = new List<KeyValueCustom>();

            foreach (var item in lestatus)
            {
                int count = 0;
                Double monto = 0;
                monto = lsolicitudesf.Where(x => x.EstatusNombre == item.Nombre).Sum(x => x.Total);
                count = lsolicitudesf.Where(x => x.EstatusNombre == item.Nombre).Count();
                var element = new KeyValueCustom(item.Nombre, count, monto);
                estatusSolicitudes.Add(element);
            }
            KeyValueCustom aespecial =  estatusSolicitudes.Find(x => x.EstatusNombre == "Aprobacion Especial");
            KeyValueCustom poraprobacion = estatusSolicitudes.Find(x => x.EstatusNombre == "Por Aprobacion");
            poraprobacion.Cantidad = poraprobacion.Cantidad + aespecial.Cantidad;

            estatusSolicitudes.Remove(aespecial);
            estatusSolicitudes.Add(new KeyValueCustom("Total", lsolicitudesf.Count()+1, lsolicitudesf.Sum(x => x.Total)));
            var lse = estatusSolicitudes.OrderByDescending(x => x.Cantidad).ToList();
            lse.Where(x => x.EstatusNombre == "Total").FirstOrDefault().Cantidad = lsolicitudesf.Count();
            return lse;
        }
        /*--------------END DASHBOARD SECTION---------------*/

        /*-------------BEGIN PROVEEDORES SECTION--------------*/
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO,NAZAN-PRONTOPAGO-APROBADOR")]
        public ActionResult Proveedores()
        {
            //Buscamos los proveedores que aplican Pronto Pago
            var cuentas = _cuentaManager.FindAllFactoraje();
            List<localPF> lproveedores = new List<localPF>();

            foreach (var cuenta in cuentas)
            {
                foreach (var proveedor in cuenta.proveedores)
                {
                    localPF p = new localPF
                    {
                        CuentaId = cuenta.Id,
                        IdProveedor = proveedor.Id,
                        Nombre = proveedor.Nombre1,
                        Numero = proveedor.NumeroProveedor,
                        Rfc = proveedor.Rfc,
                        Correo = proveedor.Correo
                    };

                    lproveedores.Add(p);
                }
            }

            //Obtenemos el porcentaje y dia de pago por proveedor, si no tiene, lo toma de configuracion
            foreach (var item in lproveedores)
            {
                proveedorfactoraje _pf = _proveedorFManager.GetProveedorById(item.IdProveedor);
                if (_pf != null)
                {
                    var index = lproveedores.FindIndex(c => c.IdProveedor == item.IdProveedor);
                    lproveedores[index].DiaDePago = _pf.DiaDePago;
                    lproveedores[index].Porcentaje = _pf.Porcentaje;
                    lproveedores[index].Correo = _pf.Correo;
                }
                else
                {
                    var index = lproveedores.FindIndex(c => c.IdProveedor == item.IdProveedor);
                    lproveedores[index].DiaDePago = 0;
                    lproveedores[index].Porcentaje = 0;
                }
            }

            //Toma porcentaje y dia de pago default de la configuracion
            ViewBag.DiaDePago = CommonManager.GetConfiguraciones().Single(c => c.Clave == "prontopago.default.day").Valor;
            ViewBag.Porcentaje = CommonManager.GetConfiguraciones().Single(c => c.Clave == "prontopago.default.percent").Valor;
            ViewBag.ProveedoresF = lproveedores;
            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO,NAZAN-PRONTOPAGO-APROBADOR")]
        public ActionResult ActualizarProveedor(int idCuenta, int idProveedor, int diadepago, int porcentaje, string correo)
        {
            bool result = false;

            //Validamos el rango de los datos y cambiamos los valores en servicio
            if ((diadepago > 0 && diadepago < 8) || (porcentaje >= 0))
            {
                result = _proveedorFManager.UpdateProveedorFactoraje(idProveedor, diadepago, porcentaje, correo);
            }

            if (result)
            {
                _logsFactoraje.InsertLog(SociedadActiva, this.User.Identity.Name.ToString(), "Actualizar Proveedor", idProveedor, "Actualizacion de proveedor " + idProveedor + ": porcentaje " + porcentaje + "%, dia de pago " + diadepago + ", correo electrónico " + correo);
                TempData["FlashSuccess"] = "Actualización realizada correctamente.";                
            }
            else
                TempData["FlashError"] = "Datos incorrectos";

            return RedirectToAction("Proveedores");
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO,NAZAN-PRONTOPAGO-APROBADOR")]
        public ActionResult EliminarProveedor(int idProveedor, int idCuenta)
        {
            bool result = false;

            /* ELIMINAR CUENTA PRONTOPAGO
            bool updatecuenta = false;
            var cuenta = _cuentaManager.Find(idCuenta);
            if (cuenta != null)
            {
                cuenta.Factoraje = false;

                _db.Entry(cuenta).State = EntityState.Modified;
                _db.SaveChanges();
                updatecuenta = true;
            }
            result = updatecuenta ? _proveedorFManager.DeleteProveedorFactoraje(idProveedor) : false;
            */

            //Se elimina proveedor de ProvedoresFactoraje (porcentaje y diadepago)
            result = _proveedorFManager.DeleteProveedorFactoraje(idProveedor);

            if (result)
            {
                _logsFactoraje.InsertLog(SociedadActiva,this.User.Identity.Name.ToString(), "Eliminar configuracion", idProveedor, "Eliminar configuracion de proveedor " + idProveedor);
                TempData["FlashSuccess"] = "Eliminado correctamente.";                
            }
            else
                TempData["FlashError"] = "Ocurrio un error al eliminar";

            return RedirectToAction("Proveedores");
        }
        /*--------------END PROVEEDORES SECTION---------------*/

        /*-------------BEGIN SOLICITUDES SECTION--------------*/
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO,NAZAN-PRONTOPAGO-APROBADOR")]
        public ActionResult Solicitudes()
        {
            //Obtenemos todas las solicitudes de pronto pago
            var solicitudesF = _solicitudFManager.GetSolicitudesFactoraje();
            DateTime startOfWeek = DateTime.Today.AddDays(-1 * (int)(DateTime.Today.DayOfWeek));
            solicitudesF = solicitudesF.Where(x => x.Fecha > startOfWeek && x.Sociedad == SociedadActiva).ToList();

            //Int32.TryParse(CommonManager.GetConfiguraciones().Single(c => c.Clave == "prontopago.default.percent").Valor, out int p);
            //foreach(var item in solicitudesF)
            //{
            //    var index = solicitudesF.FindIndex(c => c.Id == item.Id);
            //    solicitudesF[index].Tasa = (item.Tasa == 0 ) ?  p : item.Tasa;                
            //}

            ViewBag.SolicitudesF = solicitudesF;
            
            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO,NAZAN-PRONTOPAGO-APROBADOR")]
        public ActionResult SolicitudesEnviarPropuestas(string selectedlist)
        {
            List<string> split = selectedlist.Split(',').ToList();

            //Sacamos las solicitudes seleccionadas
            //var listpropuestas = _db.solicitudesfactoraje.Where(x => split.Contains(x.idSolicitudesFactoraje.ToString())).ToList();
            var listpropuestas =  _solicitudFManager.GetSolicitudesFactoraje().Where(x => split.Contains(x.Id.ToString())).ToList();

            if (listpropuestas.Count() > 0)
            {
                List<string> docs = new List<string>();
                List<DateTime> fechasDocs = new List<DateTime>();
                List<string> numerosProveedor = new List<string>();
                string sociedad = listpropuestas.FirstOrDefault().Sociedad.ToString();

                foreach (var item in listpropuestas)
                {
                    //Obtenemos los parametros de F110
                    var numeroProveedor = _proveedorManager.Find(item.IdProveedor).NumeroProveedor;
                    var facturasSolicitud = _facturaFManager.GetFacturasBySolicitud(item.Id);
                    var descuentosSolicitud = _descuentoFManager.GetDescuentosBySolicitud(item.Id);
                                        
                    List<string> documentosSolicitud =     facturasSolicitud.Select(x => x.NumeroDocumento).ToList();
                    List<string> descuentosList = descuentosSolicitud.Select(x => x.NumeroDocumento).ToList();
                    documentosSolicitud.AddRange(descuentosList);
                    //Agregamos la nota de credito al final 
                    documentosSolicitud.Add(item.NumeroGenerado.ToString());

                    docs.AddRange(documentosSolicitud);

                    List<DateTime> fechasList = facturasSolicitud.Select(x => x.FechaFactura).ToList();
                    List<DateTime> descuentosFechasList = descuentosSolicitud.Select(x => x.FechaDescuento).ToList();
                    fechasList.AddRange(descuentosFechasList);
                    fechasList.Add(item.Fecha);

                    fechasDocs.AddRange(fechasList);   

                    foreach(var element in documentosSolicitud)
                    {
                        numerosProveedor.Add(numeroProveedor);
                    }
                }
                
                int DiaPago;
                string dp = CommonManager.GetConfiguraciones().Single(c => c.Clave == "prontopago.default.day").Valor.ToUpper();
                switch (dp)
                {
                    case "LUNES":
                        DiaPago = 1;
                        break;
                    case "MARTES":
                        DiaPago = 2;
                        break;
                    case "MIERCOLES":
                        DiaPago = 3;
                        break;
                    case "JUEVES":
                        DiaPago = 4;
                        break;
                    case "VIERNES":
                        DiaPago = 5;
                        break;
                    case "SABADO":
                        DiaPago = 6;
                        break;
                    default:
                        DiaPago = 0;
                        break;
                }

                //Obtenemos la fecha del dia de pago configurado
                DateTime fechaDiaPago = DateTime.Today.AddDays(-1 * (int)(DateTime.Today.DayOfWeek));
                fechaDiaPago = fechaDiaPago.AddDays(DiaPago);

                string[] paramdocs = docs.ToArray();                                                
                DateTime[] paramdates = fechasDocs.ToArray();
                string[] proveedores = numerosProveedor.ToArray();

                SapProntoPagoManager spp = new SapProntoPagoManager();
                DataTable[] dt = spp.EnviarPropuesta(fechaDiaPago, proveedores, paramdocs, paramdates, sociedad);

                string documentos = "";
                for (int i = 0; i < paramdocs.Count(); i++)
                {
                    documentos = documentos + ", " + paramdocs[i];
                }

                if (dt != null && dt[0].Rows.Count > 0)
                {
                    //dt[0].Rows[0][3].ToString().ToLower().Substring(0, 5) == "error"
                    _logsFactoraje.InsertLog(SociedadActiva,this.User.Identity.Name.ToString(), "Enviar Propuestas", listpropuestas.Count(), "Envia propuesta con documentos: " + documentos + ". ");
                    TempData["FlashError"] = "Error SAP: "+ dt[0].Rows[0][3].ToString();
                    return RedirectToAction("Solicitudes");
                }
                else
                {
                    string errorlist = " ";
                    if (dt != null)
                    {
                        bool err = false;
                        List<string> numProErrs = new List<string>();

                        for (int i = 0; i < paramdocs.Count(); i++)
                        {
                            if (String.IsNullOrEmpty(dt[1].Rows[i][3].ToString()))
                            {
                                errorlist = errorlist + dt[1].Rows[i][0].ToString() + ", ";
                                numProErrs.Add(dt[1].Rows[i][1].ToString());
                                err = true;
                            }
                        }

                        if (!err)
                        {
                            
                            var commonManager = new CommonManager();

                            _logsFactoraje.InsertLog(SociedadActiva, this.User.Identity.Name.ToString(), "Enviar Propuestas", listpropuestas.Count(), "Envia propuesta con documentos: " + documentos + ". ");

                            foreach (var propuesta in listpropuestas)
                            {
                                _solicitudFManager.UpdateEstatusSolicitud(propuesta.Id, 7);
                                
                                var proveedorFactoraje = _proveedorFManager.GetProveedorById(_solicitudFManager.GetSolicitudById(propuesta.Id).IdProveedor);

                                bool enviado = false;
                                if (proveedorFactoraje != null)
                                {
                                    if (!String.IsNullOrEmpty(proveedorFactoraje.Correo))
                                    {
                                        try
                                        {
                                            commonManager.SendNotificacionP("Portal de Proveedores del Grupo Nazan - Solicitud Procesada", "La solicitud con id " + propuesta.Id + " ha sido procesada.", proveedorFactoraje.Correo);
                                            enviado = true;
                                        }
                                        catch 
                                        {
                                            enviado = false;
                                            TempData["FlashError"] = "Error enviando correo factoraje";
                                        }
                                    }
                                }

                                var proveedor = _db.proveedores.Where(c => c.Id == propuesta.IdProveedor).FirstOrDefault();
                                if (proveedor != null && enviado == false) 
                                {
                                    try
                                    {
                                        commonManager.SendNotificacionP("Portal de Proveedores del Grupo Nazan - Solicitud Procesada", "La solicitud con id " + propuesta.Id + " ha sido procesada.", proveedor.Correo);
                                    }
                                    catch
                                    {
                                        TempData["FlashError"] = "Error enviando correo";
                                    }
                                }
                                   
                            }

                            TempData["FlashSuccess"] = "Enviadas con exito";                                
                        }
                        else
                        {
                            errorlist = errorlist.Replace(" ", "");
                            List<string> spliterrorlist = errorlist.Split(',').ToList();
                            foreach (var propuesta in listpropuestas)
                            {
                                if (numProErrs.Contains(propuesta.Numero.ToString()))
                                {
                                    //Validamos facturas, descuentos, NC marcadas con error
                                    var facturasfactoraje = _facturaFManager.GetFacturasBySolicitud(propuesta.Id);
                                    var descuentosfactoraje = _descuentoFManager.GetDescuentosBySolicitud(propuesta.Id);
                                    
                                    List<string> documentosS = facturasfactoraje.Select(x => x.NumeroDocumento).ToList();
                                    List<string> descuentosList = descuentosfactoraje.Select(x => x.NumeroDocumento).ToList();
                                    documentosS.AddRange(descuentosList);
                                    //Agregamos la nota de credito al final 
                                    documentosS.Add(propuesta.NumeroGenerado.ToString());

                                    int ndocs = documentosS.Count();

                                    if (ndocs <= spliterrorlist.Count())
                                    {
                                        bool cambiarEstatus = false;
                                        foreach(var document in documentosS)
                                        {
                                            if (!spliterrorlist.Contains(document))
                                                cambiarEstatus = true;
                                        }

                                        if (cambiarEstatus)
                                        {
                                            _solicitudFManager.UpdateEstatusSolicitud(propuesta.Id, 8);
                                        }
                                    }
                                    else
                                    {
                                        _solicitudFManager.UpdateEstatusSolicitud(propuesta.Id, 8);
                                    }
                                }
                                else
                                {
                                    _solicitudFManager.UpdateEstatusSolicitud(propuesta.Id, 7);
                                }
                                
                            }

                            _logsFactoraje.InsertLog(SociedadActiva,this.User.Identity.Name.ToString(), "Enviar Propuestas", listpropuestas.Count(), "Envia propuestas: " + selectedlist + ". Documentos: " + documentos + ". Revisar documentos: " + errorlist);
                            TempData["FlashError"] = "Revisar facturas: " + errorlist;
                        }
                    }
                    else
                    {
                        _logsFactoraje.InsertLog(SociedadActiva,this.User.Identity.Name.ToString(), "Enviar Propuestas", listpropuestas.Count(), "Envia propuesta con documentos: " + documentos + ". ");
                        TempData["FlashError"] = "Error al enviar propuesta de pago";
                    }
                }
            }
            else
            {
                TempData["FlashError"] = "Debes seleccionar una solicitud";
            }

            return RedirectToAction("Solicitudes");
        }        
        /*--------------END SOLICITUDES SECTION---------------*/

        /*-------------BEGIN SOLICITUD DETALLE SECTION--------------*/
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO,NAZAN-PRONTOPAGO-APROBADOR")]
        public ActionResult SolicitudDetalle(int id)
        {
            //Obtenemos las facturas, descuentos por solicitud
            var solicitud = _solicitudFManager.GetSolicitudById(id);
            var proveedor = _proveedorManager.Find(solicitud.IdProveedor);
            var facturas = _facturaFManager.GetFacturasBySolicitud(id);
            List<descuentofactoraje> descuentos = _descuentoFManager.GetDescuentosBySolicitud(id);            

            List<FacturaView> descuentosSAP = GetDescuentosByProveedor(proveedor.Id);
            int i = 0;
            foreach(var item in descuentosSAP)
            {
                var df = descuentos.Where(d => d.NumeroDocumento.Contains(item.numeroDocumento)).FirstOrDefault();
                if (df == null)
                {
                    i -= 1;
                    descuentofactoraje element = new descuentofactoraje()
                    {
                        idDescuentosFactoraje = i,
                        idSolicitudesFactoraje = solicitud.idSolicitudesFactoraje,
                        NumeroDocumento = item.numeroDocumento,
                        Monto = item.importe,
                        EstatusFactoraje = solicitud.EstatusFactoraje,
                        Descripcion = item.descripcion
                    };
                    descuentos.Add(element);
                }
            }

            //Se obtienen los calculos iniciales 
            TotalView totalView = new TotalView(id);

            //Traemos el prestamo del proveedor
            SapProveedorManager sapProveedorManager = new SapProveedorManager();
            double prestamo = sapProveedorManager.GetPrestamo(proveedor.NumeroProveedor, SociedadActiva);

            //Quitamos la nota de credito de los descuentos
            int notadecredito = (int)_solicitudFManager.GetSolicitudById(id).NumeroGenerado;
            var nc = descuentos.Where(x => x.NumeroDocumento == notadecredito.ToString() && x.idSolicitudesFactoraje == id).FirstOrDefault();
            descuentos.Remove(nc);

            ViewBag.MontoOriginal = totalView.MontoOriginal.ToString("C");
            ViewBag.DescuentosTotal = totalView.DescuentosTotal.ToString("C");
            ViewBag.DescuentoProntoPago = totalView.Interes.ToString("C");
            ViewBag.TotalSolicitado = totalView.TotalSolicitado.ToString("C");

            ViewBag.Solicitud = solicitud;
            ViewBag.Proveedor = proveedor;
            ViewBag.Facturas = facturas;
            ViewBag.Descuentos = descuentos;
            ViewBag.Prestamo = prestamo;
            ViewBag.AprobarActivo = 1;
            if (!this.User.IsInRole("NAZAN-PRONTOPAGO-APROBADOR") && solicitud.EstatusFactoraje == 6)
            {
                ViewBag.AprobarActivo = 0;
            }

            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO,NAZAN-PRONTOPAGO-APROBADOR")]
        public ActionResult ActualizarPorcentaje(int idFactura, int porcentaje, int solid)
        {
            if (_solicitudFManager.GetSolicitudById(solid).EstatusFactoraje == 3 ||
                _solicitudFManager.GetSolicitudById(solid).EstatusFactoraje == 4 ||
                _solicitudFManager.GetSolicitudById(solid).EstatusFactoraje == 7)
            {
                TempData["FlashError"] = "La solicitud no puede ser modificada";
                return RedirectToAction("SolicitudDetalle", "ProntoPago", new { @id = solid });
            }
            var result = _facturaFManager.ActualizarPorcentaje(idFactura, porcentaje);
            var solicitufactoraje = _solicitudFManager.UpdateEstatusSolicitud(solid, 6);

            _logsFactoraje.InsertLog(SociedadActiva,this.User.Identity.Name.ToString(), "Actualiza Porcentaje", solid, "Porcentaje de factura" + idFactura);

            return RedirectToAction("SolicitudDetalle", "ProntoPago", new { @id = solid }); 
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO,NAZAN-PRONTOPAGO-APROBADOR")]
        public ActionResult ObtenerTotalFactoraje(string facturas, string descuentos, int solicitudId)
        {
            //Se obtienen las facturas y descuentos con check
            string[] facturasList = facturas.Split(',');
            string[] descuentosList = descuentos.Split(',');

            //Se obtienen los calculos segun las facturas y descuentos con check
            TotalView totalView = new TotalView(solicitudId, facturasList, descuentosList);
            //int dias = 0;
            ViewBag.MontoOriginal = totalView.MontoOriginal.ToString("C");
            ViewBag.DescuentosTotal = totalView.DescuentosTotal.ToString("C");
            ViewBag.DescuentoProntoPago = totalView.Interes.ToString("C");
            ViewBag.TotalSolicitado = totalView.TotalSolicitado.ToString("C");

            return PartialView("_totalProntoPago");
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO,NAZAN-PRONTOPAGO-APROBADOR")]
        public ActionResult AprobarSolicitud(string facturas, string descuentos, int solicitudId, int _proveedorid)
        {
            if (_solicitudFManager.GetSolicitudById(solicitudId).EstatusFactoraje == 3 ||
                _solicitudFManager.GetSolicitudById(solicitudId).EstatusFactoraje == 4 ||
                _solicitudFManager.GetSolicitudById(solicitudId).EstatusFactoraje == 7 )
            {
                TempData["FlashError"] = "La solicitud no puede ser modificada";
                return RedirectToAction("SolicitudDetalle", "ProntoPago", new { @id = solicitudId });
            }

            var estatusSS = _solicitudFManager.GetSolicitudById(solicitudId);
            if(estatusSS.EstatusFactoraje == 6 && !this.User.IsInRole("NAZAN-PRONTOPAGO-APROBADOR"))
            {
                TempData["FlashError"] = "No se pudo aprobar solicitud.";
                return RedirectToAction("SolicitudDetalle", "ProntoPago", new { @id = solicitudId });
            }

            string[] facturasList = facturas.Split(',');
            string[] descuentosList = descuentos.Split(',');

            TotalView totalView1 = new TotalView(solicitudId, facturasList, descuentosList);
            if (totalView1.TotalSolicitado <0)
            {
                TempData["FlashError"] = "No se pudo aprobar solicitud, el total solicitado es negativo";
                return RedirectToAction("SolicitudDetalle", "ProntoPago", new { @id = solicitudId });
            }

            //Traemos los descuentos guardados a eliminar
            List<descuentofactoraje> descuentosguardados = _descuentoFManager.GetDescuentosBySolicitud(solicitudId).Where(x => descuentosList.Contains(x.NumeroDocumento)).ToList();
            List<string> arrdescuentos = descuentosguardados.Select(x => x.idDescuentosFactoraje.ToString()).ToList();

            //ELIMINA FACTURAS Y DESCUENTOS SIN CHECK
            _facturaFManager.DeleteFacturas(facturasList, solicitudId);
            _descuentoFManager.DeleteDescuentos(arrdescuentos.ToArray(), solicitudId);

            //Sacamos los descuentos que vienen de SAP y tienen check
            List<FacturaView> descuentosSAP = GetDescuentosByProveedor(_proveedorid);
            var dg = _descuentoFManager.GetDescuentosBySolicitud(solicitudId);
            foreach (var item in descuentosSAP)
            {
                var df = descuentosList.Where(d => d.Contains(item.numeroDocumento)).FirstOrDefault();
                if (df != null)
                {
                    var dgresult = dg.Where(d => d.NumeroDocumento == item.numeroDocumento).FirstOrDefault();
                    if (dgresult == null)
                    {
                        descuentofactoraje element = new descuentofactoraje()
                        {
                            idSolicitudesFactoraje = solicitudId,
                            NumeroDocumento = item.numeroDocumento,
                            Monto = item.importe,
                            EstatusFactoraje = estatusSS.EstatusFactoraje,
                            Descripcion = item.descripcion
                        };
                        _descuentoFManager.InsDescuentoFactoraje(element);
                        descuentosguardados.Add(element);
                    }
                }
            }

            //Se obtienen los calculos de las facturas y descuentos con check para actualizar la solicitud
            TotalView totalView = new TotalView(solicitudId, facturasList, descuentosList);

            //Se crea la solicitud actualizada
            var nuevaSolicitud = new solicitudesfactoraje()
            {
                idSolicitudesFactoraje = solicitudId,
                DescuentoPP =    totalView.DescuentoProntoPago,
                Descuentos =     totalView.DescuentosTotal,
                MontoOriginal =  totalView.MontoOriginal,
                MontoAFacturar = totalView.TotalSolicitado,
            };

            //Se actualiza la solicitud de pronto pago 
            var result = _solicitudFManager.UpdateSolicitud(nuevaSolicitud);
            if (result == nuevaSolicitud.idSolicitudesFactoraje)
            {
                _solicitudFManager.UpdateEstatusSolicitud(nuevaSolicitud.idSolicitudesFactoraje, 5);
            }

            _logsFactoraje.InsertLog(SociedadActiva,this.User.Identity.Name.ToString(), "Aprobación de solicitud", nuevaSolicitud.idSolicitudesFactoraje, "Aprobación de solicitud con facturas: " + facturas + ". Descuentos: " + descuentos);

            
            var commonManager = new CommonManager();

            var proveedorFactoraje = _proveedorFManager.GetProveedorById(_proveedorid);
            bool enviado = false;
            if (proveedorFactoraje != null)
            {
                if (!String.IsNullOrEmpty(proveedorFactoraje.Correo))
                {
                    try
                    {
                        commonManager.SendNotificacionP("Portal de Proveedores del Grupo Nazan - Solicitud Aprobada", "La solicitud con id " + solicitudId + " ha sido aprobada y esta lista para cargar la nota de credito.", proveedorFactoraje.Correo);
                        enviado = true;
                    }
                    catch
                    {
                        enviado = false;
                        TempData["FlashError"] = "Error enviando correo factoraje";
                    }
                }
            }

            var proveedor = _db.proveedores.Where(c => c.Id == _proveedorid).FirstOrDefault();
            if (proveedor != null && enviado==false)
            {
                try
                {
                    commonManager.SendNotificacionP("Portal de Proveedores del Grupo Nazan - Solicitud Aprobada", "La solicitud con id " + solicitudId + " ha sido aprobada y esta lista para cargar la nota de credito.", proveedor.Correo);
                }
                catch 
                {
                    TempData["FlashError"] = "Error enviando correo";
                }
            }

            return RedirectToAction("Solicitudes");
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO,NAZAN-PRONTOPAGO-APROBADOR")]
        public ActionResult RechazarSolicitud(int solicitudId)
        {
            var SolicitudRechazada = _solicitudFManager.GetSolicitudById(solicitudId);

            if (SolicitudRechazada.EstatusFactoraje == 3 ||
                SolicitudRechazada.EstatusFactoraje == 4 ||
                SolicitudRechazada.EstatusFactoraje == 7)
            {
                TempData["FlashError"] = "La solicitud no puede ser rechazada";
                return RedirectToAction("SolicitudDetalle", "ProntoPago", new { @id = solicitudId });
            }
            else
            {
                //Cambia el estatus en solicitudes, facturas y descuentos por rechazada
                var result = _solicitudFManager.UpdateEstatusSolicitud(solicitudId, 3);

                _logsFactoraje.InsertLog(SociedadActiva,this.User.Identity.Name.ToString(), "Rechazar Solicitud", solicitudId, "Cambio a estatus Rechazada");
                var commonManager = new CommonManager();

                var proveedorFactoraje = _proveedorFManager.GetProveedorById(SolicitudRechazada.IdProveedor);
                bool enviado = false;
                if (proveedorFactoraje != null)
                {
                    if (!String.IsNullOrEmpty(proveedorFactoraje.Correo))
                    {
                        try
                        {
                            commonManager.SendNotificacionP("Portal de Proveedores del Grupo Nazan - Solicitud Rechazada", "La solicitud con id " + solicitudId + " ha sido rechazada.", proveedorFactoraje.Correo);
                            enviado = true;
                        }
                        catch
                        {
                            enviado = false;
                            TempData["FlashError"] = "Error enviando correo factoraje";
                        }
                    }
                }


                var proveedor = _db.proveedores.Where(c => c.Id == SolicitudRechazada.IdProveedor).FirstOrDefault();
                if (proveedor != null && enviado == false)
                {
                    try
                    {
                        commonManager.SendNotificacionP("Portal de Proveedores del Grupo Nazan - Solicitud Rechazada", "La solicitud con id " + solicitudId + " ha sido rechazada.", proveedor.Correo);
                    }
                    catch
                    {
                        TempData["FlashError"] = "Error enviando correo";
                    }
                }
                TempData["FlashSuccess"] = "Solicitud rechazada";                
                return RedirectToAction("Solicitudes");
            }
        }

        public List<FacturaView> GetDescuentosByProveedor(int proveedorId)
        {
            var proveedor = _proveedorManager.Find(proveedorId);

            if (proveedor == null)
            {
                TempData["FlashError"] = "Ocurrio un error.";
                List<FacturaView> _descuentos = null;
                return _descuentos;
            }
            
            //Traemos los pagos pendientes
            var partidasManager = new PartidasManager();
            var proveedorFManager = new ProveedorFManager();

            var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
            var dsPagosPendientes = partidasManager.GetPartidasAbiertas(proveedor.NumeroProveedor, sociedad, DateTime.Today);

            //Armamos la lista con las cuentas por pagas es decir mayores a cero 
            List<Web.Models.ProntoPago.FacturaView> _listDescuentos = new List<Web.Models.ProntoPago.FacturaView>();

            for (int i = 0; i < dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows.Count; i++)
            {
                Web.Models.ProntoPago.FacturaView item = new Web.Models.ProntoPago.FacturaView()
                {
                    referencia = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["XBLNR"].ToString(),
                    importe = Convert.ToDouble(dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["DMBTR"].ToString()),
                    ml = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["WAERS"].ToString(),
                    vencimiento = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["FECHA_PAGO"].ToString(),
                    tipoMovimiento = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["BLART"].ToString(),
                    fechaDocumento = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["BLDAT"].ToString(),
                    descripcion = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["SGTXT"].ToString(),
                    pagar = false,
                    idProveedor = proveedor.Id.ToString(),
                    numeroDocumento = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["BELNR"].ToString()
                };
                if (item.importe < 0)
                {
                    item.pagar = true;
                }
                if (item.importe < 0)
                    _listDescuentos.Add(item);
            }
                        
            return _listDescuentos;
        }

        public JsonResult InsDescuentoFactoraje(descuentofactoraje model)
        {
            int result = _descuentoFManager.InsDescuentoFactoraje(model);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO,NAZAN-PRONTOPAGO-APROBADOR")]
        public ActionResult GetTotal(string facturas, string descuentos, int solicitudId, int _proveedorid)
        {
            List<descuentofactoraje> _ldescuentos = _descuentoFManager.GetDescuentosBySolicitud(solicitudId);

            List<FacturaView> descuentosSAP = GetDescuentosByProveedor(_proveedorid);
            int i = 0;
            foreach (var item in descuentosSAP)
            {
                var df = _ldescuentos.Where(d => d.NumeroDocumento.Contains(item.numeroDocumento)).FirstOrDefault();
                if (df == null)
                {
                    i -= 1;
                    descuentofactoraje element = new descuentofactoraje()
                    {
                        idDescuentosFactoraje = i,
                        idSolicitudesFactoraje = solicitudId,
                        NumeroDocumento = item.numeroDocumento,
                        Monto = item.importe,
                        //EstatusFactoraje = solicitud.EstatusFactoraje,
                        Descripcion = item.descripcion
                    };
                    _ldescuentos.Add(element);
                }
            }

            //Se obtienen las facturas y descuentos (numerodocumento) con check
            string[] facturasList = facturas.Split(',');
            string[] descuentosList = descuentos.Split(',');

            //Se obtienen los calculos segun las facturas y descuentos con check
            TotalView totalView = new TotalView(solicitudId, facturasList, descuentosList, _ldescuentos);
            //int dias = 0;
            ViewBag.MontoOriginal = totalView.MontoOriginal.ToString("C");
            ViewBag.DescuentosTotal = totalView.DescuentosTotal.ToString("C");
            ViewBag.DescuentoProntoPago = totalView.Interes.ToString("C");
            ViewBag.TotalSolicitado = totalView.TotalSolicitado.ToString("C");

            return PartialView("_totalProntoPago");
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO,NAZAN-PRONTOPAGO-APROBADOR")]
        public ActionResult GuardarSolicitudM(string facturas, string descuentos, int solicitudId, int _proveedorid, int _estatus)
        {
            if (_solicitudFManager.GetSolicitudById(solicitudId).EstatusFactoraje == 3 ||
                _solicitudFManager.GetSolicitudById(solicitudId).EstatusFactoraje == 4 ||
                _solicitudFManager.GetSolicitudById(solicitudId).EstatusFactoraje == 7)
            {
                TempData["FlashError"] = "La solicitud no puede ser modificada";
                return RedirectToAction("SolicitudDetalle", "ProntoPago", new { @id = solicitudId });
            }

            int estatusactual = _solicitudFManager.GetSolicitudById(solicitudId).EstatusFactoraje;
            string[] facturasList = facturas.Split(',');
            string[] descuentosList = descuentos.Split(',');

            //Traemos los descuentos guardados a eliminar
            List<descuentofactoraje> descuentosguardados = _descuentoFManager.GetDescuentosBySolicitud(solicitudId).Where(x => descuentosList.Contains(x.NumeroDocumento)).ToList();
            List<string> arrdescuentos = descuentosguardados.Select(x => x.idDescuentosFactoraje.ToString()).ToList();
            
            //ELIMINA FACTURAS Y DESCUENTOSFACTORAJE SIN CHECK
            _facturaFManager.DeleteFacturas(facturasList, solicitudId);
            _descuentoFManager.DeleteDescuentos(arrdescuentos.ToArray(), solicitudId);
                       
            //Sacamos los descuentos que vienen de SAP y tienen check
            List<FacturaView> descuentosSAP = GetDescuentosByProveedor(_proveedorid);
            foreach (var item in descuentosSAP)
            {
                var df = descuentosList.Where(d => d.Contains(item.numeroDocumento)).FirstOrDefault();
                var dg = descuentosguardados.Where(x => x.NumeroDocumento == df).FirstOrDefault();
                if (df != null && dg==null)
                {
                    descuentofactoraje element = new descuentofactoraje()
                    {
                        idSolicitudesFactoraje = solicitudId,
                        NumeroDocumento = item.numeroDocumento,
                        Monto = item.importe,
                        EstatusFactoraje = estatusactual,
                        Descripcion = item.descripcion,
                        FechaDescuento = DateTime.ParseExact(item.fechaDocumento, "yyyyMMdd", CultureInfo.InvariantCulture)
                    };
                    _descuentoFManager.InsDescuentoFactoraje(element);
                    descuentosguardados.Add(element);
                }
            }

            TotalView totalView = new TotalView(solicitudId, facturasList, descuentosList, descuentosguardados);

            //Se crea la solicitud actualizada
            var nuevaSolicitud = new solicitudesfactoraje()
            {
                idSolicitudesFactoraje = solicitudId,
                DescuentoPP = totalView.DescuentoProntoPago,
                Descuentos = totalView.DescuentosTotal,
                MontoOriginal = totalView.MontoOriginal,
                MontoAFacturar = totalView.TotalSolicitado,
            };

            //Se actualiza la solicitud de pronto pago 
            var result = _solicitudFManager.UpdateSolicitud(nuevaSolicitud);
            if (result == nuevaSolicitud.idSolicitudesFactoraje)
            {
                _solicitudFManager.UpdateEstatusSolicitud(nuevaSolicitud.idSolicitudesFactoraje, _estatus);
            }

            _logsFactoraje.InsertLog(SociedadActiva,this.User.Identity.Name.ToString(), "Guarda Solicitud", solicitudId, "Cambio a estatus Aprobacion Especial. Con las facturas: " + facturas + "y Descuentos: " + descuentos);
            return RedirectToAction("Solicitudes");
        }
        
        /*--------------END SOLICITUD DETALLE SECTION---------------*/
        
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO,NAZAN-PRONTOPAGO-APROBADOR")]
        public ActionResult Configuraciones()
        {
            var configuraciones = _configuracionesFManager.GetConfiguraciones();
            ViewBag.Configuraciones = configuraciones;

            return View();
        }
        
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO,NAZAN-PRONTOPAGO-APROBADOR")]
        public ActionResult Logs(string fechaFrom, string fechaTo)
        {
            if (!String.IsNullOrEmpty(fechaFrom) && !String.IsNullOrEmpty(fechaTo))
            {
                var fechaf = DateTime.ParseExact(fechaFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                var fechat = DateTime.ParseExact(fechaTo, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                ViewBag.Logs = _db.logfactoraje.Where(c => c.Fecha >= fechaf && c.Fecha <= fechat && c.TipoObjeto.ToString() == SociedadActiva).ToList();
            }
            else
            {
                var fecha = DateTime.Today.AddMonths(-3);
                ViewBag.Logs = _db.logfactoraje.Where(c => c.Fecha > fecha && c.TipoObjeto.ToString() == SociedadActiva).ToList();
            }

            return View();
        }
        
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO,NAZAN-PRONTOPAGO-APROBADOR")]
        public ActionResult Reporte(string fechaFrom, string fechaTo)
        {
            if (!String.IsNullOrEmpty(fechaFrom) && !String.IsNullOrEmpty(fechaTo))
            {
                var fechaf = DateTime.ParseExact(fechaFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                var fechat = DateTime.ParseExact(fechaTo, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                fechat = fechat.AddHours(11);
                ViewBag.Solicitudes = _solicitudFManager.GetSolicitudesFactoraje().Where(c => c.Fecha >= fechaf && c.Fecha <= fechat && c.Sociedad == SociedadActiva).ToList();                
            }
            else
            {
                fechaTo = DateTime.Now.ToString("dd/MM/yyyy");
                fechaFrom = DateTime.Today.AddMonths(-3).ToString("dd/MM/yyyy");

                var fecha = DateTime.Today.AddMonths(-3);
                ViewBag.Solicitudes = _solicitudFManager.GetSolicitudesFactoraje().Where(c => c.Fecha > fecha && c.Sociedad == SociedadActiva).ToList();
            }

            ViewBag.FechaFrom = fechaFrom;
            ViewBag.FechaTo = fechaTo;

            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO,NAZAN-PRONTOPAGO-APROBADOR")]
        public ActionResult UpdateConfiguracion(int id, string key, string value)
        {
            bool result = false;
            key = key.ToLower();

            if(key == "prontopago.default.day")
            {
                string[] week = { "LUNES", "MARTES", "MIERCOLES", "JUEVES", "VIERNES", "SABADO", "DOMINGO" };

                if (week.Contains(value.ToUpper()))
                {
                    result = _configuracionesFManager.UpdateConfiguracion(id, key, value);
                }
            }
            else if (key == "prontopago.default.percent")
            {
                int percent = 0;
                Int32.TryParse(value, out percent);
                if (percent >= 0 && percent <= 100)
                {
                    result = _configuracionesFManager.UpdateConfiguracion(id, key, value);
                }
            }
            else
            {
                result = _configuracionesFManager.UpdateConfiguracion(id, key, value);
            }

            TempData["FlashSuccess"] = result ? "Configuracion guardad correctamente." : "Ocurrio un error al guardar la configuracion";
            return RedirectToAction("Configuraciones");
        }        

        public class localPF
        {
            public int CuentaId { get; set; }
            public int IdProveedor { get; set; }
            public int DiaDePago { get; set; }
            public string Nombre { get; set; }
            public string Numero { get; set; }
            public int Porcentaje { get; set; }
            public string Rfc { get; set; }
            public string Correo { get; set; }
        }

        public class KeyValueCustom
        {
            public string EstatusNombre { get; set; }
            public int Cantidad { get; set; }
            public Double Monto { get; set; }

            public KeyValueCustom(string EstatusNombre, int cantidad, Double monto)
            {
                this.EstatusNombre = EstatusNombre;
                this.Cantidad = cantidad;
                this.Monto = monto;
            }
        }
    }    
}