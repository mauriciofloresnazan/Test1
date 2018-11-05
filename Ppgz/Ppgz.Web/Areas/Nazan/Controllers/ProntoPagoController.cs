using System;
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

        /*-------------BEGIN DASHBOARD SECTION--------------*/
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO,NAZAN-PRONTOPAGO-APROBADOR")]
        public ActionResult Index()
        {
            //Se contabilizan los estatusfactoraje <estatus, cantudad>
            var lse = GetSolicitudEstatus();
            ViewBag.SolicitudesEstatus = lse;
            ViewBag.ParaEnviar = lse.Where(x => x.EstatusNombre == "Lista Para Propuesta").FirstOrDefault().Cantidad.ToString();

            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO,NAZAN-PRONTOPAGO-APROBADOR")]
        public ActionResult DashboardEnviarPropuestas()
        {
            var listpropuestas = new List<solicitudesfactoraje>();
            int idestatus = _db.estatusfactoraje.Where(x => x.Nombre == "Lista Para Propuesta" && x.Activo == 1).FirstOrDefault().idEstatusFactoraje;
            listpropuestas = _solicitudFManager.GetSolicitudesByEstatus(idestatus);

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
            lsolicitudesf = lsolicitudesf.Where(x => x.Fecha > startOfWeek).ToList();

            //Obtenemos una lista de los estatusfactoraje contabilizados 
            var lestatus = _db.estatusfactoraje.ToList();
            var estatusSolicitudes = new List<KeyValueCustom>();

            foreach (var item in lestatus)
            {
                int count = 0;
                count = lsolicitudesf.Where(x => x.EstatusNombre == item.Nombre).Count();
                var element = new KeyValueCustom(item.Nombre, count);
                estatusSolicitudes.Add(element);
            }
            KeyValueCustom aespecial =  estatusSolicitudes.Find(x => x.EstatusNombre == "Aprobacion Especial");
            KeyValueCustom poraprobacion = estatusSolicitudes.Find(x => x.EstatusNombre == "Por Aprobacion");
            poraprobacion.Cantidad = poraprobacion.Cantidad + aespecial.Cantidad;

            estatusSolicitudes.Remove(aespecial);
            estatusSolicitudes.Add(new KeyValueCustom("Total", lsolicitudesf.Count()+1));
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
                        Rfc = proveedor.Rfc
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
        public ActionResult ActualizarProveedor(int idCuenta, int idProveedor, int diadepago, int porcentaje)
        {
            bool result = false;

            //Validamos el rango de los datos y cambiamos los valores en servicio
            if ((diadepago > 0 && diadepago < 8) || (porcentaje >= 0))
            {
                result = _proveedorFManager.UpdateProveedorFactoraje(idProveedor, diadepago, porcentaje);
            }

            if (result)
                TempData["FlashSuccess"] = "Actualización realizada correctamente.";
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
                TempData["FlashSuccess"] = "Eliminado correctamente.";
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
            solicitudesF = solicitudesF.Where(x => x.Fecha > startOfWeek).ToList();

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
            var listpropuestas = _db.solicitudesfactoraje.Where(x => split.Contains(x.idSolicitudesFactoraje.ToString())).ToList();

            if (listpropuestas.Count() > 0)
            {
                foreach (var item in listpropuestas)
                {
                    //Obtenemos los parametros de F110
                    var numeroProveedor = _proveedorManager.Find(item.IdProveedor).NumeroProveedor;
                    var facturasSolicitud = _facturaFManager.GetFacturasBySolicitud(item.idSolicitudesFactoraje);
                    string[] facturasList = facturasSolicitud.Select(x => x.NumeroDocumento).ToArray();
                    DateTime[] fechasList = facturasSolicitud.Select(x => x.FechaFactura).ToArray();

                    //Obtenemos el día prontopago configurado
                    var pf = _proveedorFManager.GetProveedorById(item.IdProveedor);
                    int DiaPago;
                    if (pf != null)
                    {
                        DiaPago = pf.DiaDePago;
                    }
                    else
                    {
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
                    }

                    //Obtenemos la fecha del dia de pago configurado
                    DateTime fechaDiaPago = DateTime.Today.AddDays(-1 * (int)(DateTime.Today.DayOfWeek));
                    fechaDiaPago = fechaDiaPago.AddDays(DiaPago);

                    SapProntoPagoManager spp = new SapProntoPagoManager();
                    DataTable dt = spp.EnviarPropuesta(fechaDiaPago, numeroProveedor, facturasList, fechasList);
                    if (dt != null)
                    {
                        _solicitudFManager.UpdateEstatusSolicitud(item.idSolicitudesFactoraje, 7);
                        TempData["FlashSuccess"] = "Enviadas con exito";
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
                        Monto = item.importe.ToString(),
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
            double prestamo = sapProveedorManager.GetPrestamo(proveedor.NumeroProveedor);

            ViewBag.MontoOriginal = totalView.MontoOriginal.ToString("C");
            ViewBag.DescuentosTotal = totalView.DescuentosTotal.ToString("C");
            ViewBag.DescuentoProntoPago = totalView.Interes.ToString("C");
            ViewBag.TotalSolicitado = totalView.TotalSolicitado.ToString("C");

            ViewBag.Solicitud = solicitud;
            ViewBag.Proveedor = proveedor;
            ViewBag.Facturas = facturas;
            ViewBag.Descuentos = descuentos;
            ViewBag.Prestamo = prestamo;

            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO,NAZAN-PRONTOPAGO-APROBADOR")]
        public ActionResult ActualizarPorcentaje(int idFactura, int porcentaje, int solid)
        {
            if (_solicitudFManager.GetSolicitudById(solid).EstatusFactoraje == 3 ||
                _solicitudFManager.GetSolicitudById(solid).EstatusFactoraje == 7)
            {
                TempData["FlashError"] = "La solicitud no puede ser modificada";
                return RedirectToAction("SolicitudDetalle", "ProntoPago", new { @id = solid });
            }
            var result = _facturaFManager.ActualizarPorcentaje(idFactura, porcentaje);
            var solicitufactoraje = _solicitudFManager.UpdateEstatusSolicitud(solid, 6);

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

            //Traemos los descuentos guardados a eliminar
            List<descuentofactoraje> descuentosguardados = _descuentoFManager.GetDescuentosBySolicitud(solicitudId).Where(x => descuentosList.Contains(x.NumeroDocumento)).ToList();
            List<string> arrdescuentos = descuentosguardados.Select(x => x.idDescuentosFactoraje.ToString()).ToList();

            //ELIMINA FACTURAS Y DESCUENTOS SIN CHECK
            _facturaFManager.DeleteFacturas(facturasList, solicitudId);
            _descuentoFManager.DeleteDescuentos(arrdescuentos.ToArray(), solicitudId);

            //Sacamos los descuentos que vienen de SAP y tienen check
            List<FacturaView> descuentosSAP = GetDescuentosByProveedor(_proveedorid);
            foreach (var item in descuentosSAP)
            {
                var df = descuentosList.Where(d => d.Contains(item.numeroDocumento)).FirstOrDefault();
                if (df != null)
                {
                    descuentofactoraje element = new descuentofactoraje()
                    {
                        idSolicitudesFactoraje = solicitudId,
                        NumeroDocumento = item.numeroDocumento,
                        Monto = item.importe.ToString(),
                        EstatusFactoraje = estatusSS.EstatusFactoraje,
                        Descripcion = item.descripcion
                    };
                    _descuentoFManager.InsDescuentoFactoraje(element);
                    descuentosguardados.Add(element);
                }
            }

            //Se obtienen los calculos de las facturas y descuentos con check para actualizar la solicitud
            TotalView totalView = new TotalView(solicitudId, facturasList, descuentosList);

            //Se crea la solicitud actualizada
            var nuevaSolicitud = new solicitudesfactoraje()
            {
                idSolicitudesFactoraje = solicitudId,
                DescuentoPP = Convert.ToInt32(totalView.DescuentoProntoPago),
                Descuentos = Convert.ToInt32(totalView.DescuentosTotal),
                MontoOriginal = Convert.ToInt32(totalView.MontoOriginal),
                MontoAFacturar = Convert.ToInt32(totalView.TotalSolicitado),
            };

            //Se actualiza la solicitud de pronto pago 
            var result = _solicitudFManager.UpdateSolicitud(nuevaSolicitud);
            if (result == nuevaSolicitud.idSolicitudesFactoraje)
            {
                _solicitudFManager.UpdateEstatusSolicitud(nuevaSolicitud.idSolicitudesFactoraje, 5);
            }

            return RedirectToAction("Solicitudes");
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO,NAZAN-PRONTOPAGO-APROBADOR")]
        public ActionResult RechazarSolicitud(int solicitudId)
        {
            //Cambia el estatus en solicitudes, facturas y descuentos por rechazada
            var result = _solicitudFManager.UpdateEstatusSolicitud(solicitudId, 3);

            return RedirectToAction("Solicitudes");
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
                        Monto = item.importe.ToString(),
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
                        Monto = item.importe.ToString(),
                        EstatusFactoraje = estatusactual,
                        Descripcion = item.descripcion
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
                DescuentoPP = Convert.ToInt32(totalView.DescuentoProntoPago),
                Descuentos = Convert.ToInt32(totalView.DescuentosTotal),
                MontoOriginal = Convert.ToInt32(totalView.MontoOriginal),
                MontoAFacturar = Convert.ToInt32(totalView.TotalSolicitado),
            };

            //Se actualiza la solicitud de pronto pago 
            var result = _solicitudFManager.UpdateSolicitud(nuevaSolicitud);
            if (result == nuevaSolicitud.idSolicitudesFactoraje)
            {
                _solicitudFManager.UpdateEstatusSolicitud(nuevaSolicitud.idSolicitudesFactoraje, _estatus);
            }

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
                ViewBag.Logs = _db.logfactoraje.Where(c => c.Fecha >= fechaf && c.Fecha <= fechat).ToList();
            }
            else
            {
                var fecha = DateTime.Today.AddMonths(-3);
                ViewBag.Logs = _db.logfactoraje.Where(c => c.Fecha > fecha).ToList();
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
                ViewBag.Solicitudes = _solicitudFManager.GetSolicitudesFactoraje().Where(c => c.Fecha >= fechaf && c.Fecha <= fechat).ToList();
            }
            else
            {
                var fecha = DateTime.Today.AddMonths(-3);
                ViewBag.Solicitudes = _solicitudFManager.GetSolicitudesFactoraje().Where(c => c.Fecha > fecha).ToList();
            }

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
        }

        public class KeyValueCustom
        {
            public string EstatusNombre { get; set; }
            public int Cantidad { get; set; }

            public KeyValueCustom(string EstatusNombre, int cantidad)
            {
                this.EstatusNombre = EstatusNombre;
                this.Cantidad = cantidad;
            }
        }
    }    
}