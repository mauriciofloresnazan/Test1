﻿using Ppgz.Repository;
using Ppgz.Services;
using Ppgz.Web.Infrastructure;
using Ppgz.Web.Models.ProntoPago;
using SapWrapper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using static SapWrapper.SapFacturaManager;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class ProntoPagoController : Controller
    {
        readonly CommonManager _commonManager = new CommonManager();
        readonly ProveedorManager _proveedorManager = new ProveedorManager();
        readonly FacturaManager _facturaManager = new FacturaManager();
        readonly LogsFactoraje _logsFactoraje = new LogsFactoraje();

        internal proveedore ProveedorCxp
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["proveedorcxp"] != null)
                {
                    return (proveedore)System.Web.HttpContext.Current.Session["proveedorcxp"];
                }
                return null;
            }
            set
            {
                System.Web.HttpContext.Current.Session["proveedorcxp"] = value;
            }
        }
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

        //**********CU002-Seleccion razon social*****************
        public ActionResult CargarNotaCredito(int idSolicitudesFactoraje, int proveedorId)
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();
            var proveedor = _proveedorManager.Find(proveedorId, cuenta.Id);

            SolicitudFManager solicitudFManager = new SolicitudFManager();
            solicitudesfactoraje solicitud = solicitudFManager.GetSolicitudById(idSolicitudesFactoraje);




            double iva = (solicitud.MontoAFacturar * .16);
            iva = Math.Round(iva, 2);

            double subtotal = solicitud.MontoAFacturar - iva;
            ViewBag.Iva = iva;
            ViewBag.Total = solicitud.MontoAFacturar;
            ViewBag.SubTotal = Math.Round(subtotal, 2);
            ViewBag.MontoNota = solicitud.DescuentoPP;


            ViewBag.Proveedor = proveedor;
            ViewBag.idSolicitudesFactoraje = idSolicitudesFactoraje;
            ViewBag.Solicitud = solicitud;


            return View();
        }
        [HttpPost]
        public ActionResult CargarNotaCredito(NotaCreditoView notaCreditoView)
        {
            SapFacturaManager sapFacturaManager = new SapFacturaManager();
            ProveedorManager proveedorManager = new ProveedorManager();
            SolicitudFManager solicitudFManager = new SolicitudFManager();


            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();
            var proveedor = _proveedorManager.Find(notaCreditoView.proveedorId, cuenta.Id);
            ViewBag.Proveedor = proveedor;
            ViewBag.idSolicitudesFactoraje = notaCreditoView.idSolicitudesFactoraje;

            if (proveedor == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }



            //Iniciamos proceso de archivos
            if (!ModelState.IsValid) return View();

            var tempXmlPath = Path.Combine(Server.MapPath("~/Uploads/"), "temp-" + Guid.NewGuid() + ".xml");
            var tempPdfPath = Path.Combine(Server.MapPath("~/Uploads/"), "temp-" + Guid.NewGuid() + ".pdf");

            try
            {
                if (notaCreditoView.notaCreditoPdf == null)
                {
                    ModelState.AddModelError(string.Empty, "Archivos incorrectos");
                    return View();
                }

                if (notaCreditoView.notaCreditoXml == null)
                {
                    ModelState.AddModelError(string.Empty, "Archivos incorrectos");
                    return View();
                }

                var xmlFiles = notaCreditoView.notaCreditoXml;
                xmlFiles.SaveAs(tempXmlPath);
                XmlDocument doc = new XmlDocument();
                doc.Load(tempXmlPath);//Leer el XML

                //agregamos un Namespace, que usaremos para buscar que el nodo no exista:
                XmlNamespaceManager nsm = new XmlNamespaceManager(doc.NameTable);
                nsm.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/3");
                XmlNode nodeComprobante = doc.SelectSingleNode("//cfdi:Comprobante", nsm);

                if (nodeComprobante == null)
                {

                    //Validamos el xml
                    var xmlFile = notaCreditoView.notaCreditoXml;
                    ValidarXmlVersion4(xmlFile);

                    //Validamos el pdf
                    var pdfFile = notaCreditoView.notaCreditoPdf;
                    ValidarPdfVersion4(pdfFile);

                    xmlFile.SaveAs(tempXmlPath);
                    pdfFile.SaveAs(tempPdfPath);


                    try
                    {

                        //Paso 1: Validamos la factura
                        factura facturaModel = _facturaManager.CargarFacturaFactorajeV4(proveedor.Id, cuenta.Id, tempXmlPath, tempPdfPath);

                        string numeroProveedor = proveedor.NumeroProveedor;
                        DateTime fechaFactura = facturaModel.Fecha;
                        string importe = facturaModel.Total.ToString();
                        string cabecera = facturaModel.Folio.ToString() + "PP" + "de" + facturaModel.numeroProveedor.ToString();
                        string posicion = facturaModel.Folio.ToString() + "PP" + "de" + facturaModel.numeroProveedor.ToString();
                        string folio = facturaModel.Serie.ToString() + facturaModel.Folio.ToString();

                        //Validamos que el importe de la factura 
                        var Solicitud = solicitudFManager.GetSolicitudById(notaCreditoView.idSolicitudesFactoraje);
                        double MontoFacturar = Solicitud.DescuentoPP;
                        double diferencia = MontoFacturar - Convert.ToDouble(facturaModel.Total);

                        if (diferencia > .5 || diferencia < -.5)
                        {
                            TempData["FlashError"] = "El monto de la factura no coincide con el monto a pagar.";
                            return RedirectToAction("VerSolicitudes", new { proveedorId = proveedor.Id });
                        }

                        //Paso 2: Ejecutamos la funcion de SAP
                        Resultado cargarNotaCredito = sapFacturaManager.CrearNotaCredito(numeroProveedor, fechaFactura, importe, cabecera, posicion, folio, Solicitud.Sociedad);

                        if (cargarNotaCredito.Estatus == "1")
                        {
                            //Paso 3: Guardamos la factura en la bd
                            facturaModel.NumeroGenerado = cargarNotaCredito.FacturaNumero;
                            _facturaManager.GuardaFacturaFactoraje(facturaModel);

                            //Paso 4: Actualizamos el estatus de la solicitud
                            solicitudFManager.UpdateEstatusSolicitud(notaCreditoView.idSolicitudesFactoraje, 4);

                            solicitudesfactoraje sf = solicitudFManager.GetSolicitudById(notaCreditoView.idSolicitudesFactoraje);
                            sf.NumeroGenerado = Convert.ToInt32(facturaModel.NumeroGenerado);
                            solicitudFManager.UpdateSolicitud(sf);

                            _logsFactoraje.InsertLog(SociedadActiva, this.User.Identity.Name.ToString(), "Carga Nota Credito", notaCreditoView.idSolicitudesFactoraje, "Carga nota de credito");

                            TempData["FlashSuccess"] = "Nota de credito registrada satisfactoriamente.";

                        }
                        else
                        {

                            TempData["FlashError"] = "Error al cargar la nota de credito en SAP. " + cargarNotaCredito.ErrorTable.ToString();

                        }
                    }
                    catch (Exception ioe)
                    {

                        TempData["FlashError"] = "Error al cargar la nota de credito en SAP. " + ioe.Message.ToString();

                    }

                    return RedirectToAction("VerSolicitudes", new { proveedorId = proveedor.Id });
                }
                else
                {
                    //Validamos el xml
                    var xmlFile = notaCreditoView.notaCreditoXml;
                    ValidarXml(xmlFile);

                    //Validamos el pdf
                    var pdfFile = notaCreditoView.notaCreditoPdf;
                    ValidarPdf(pdfFile);

                    xmlFile.SaveAs(tempXmlPath);
                    pdfFile.SaveAs(tempPdfPath);



                    try
                    {

                        //Paso 1: Validamos la factura
                        factura facturaModel = _facturaManager.CargarFacturaFactoraje(proveedor.Id, cuenta.Id, tempXmlPath, tempPdfPath);

                        string numeroProveedor = proveedor.NumeroProveedor;
                        DateTime fechaFactura = facturaModel.Fecha;
                        string importe = facturaModel.Total.ToString();
                        string cabecera = facturaModel.Folio.ToString() + "PP" + "de" + facturaModel.numeroProveedor.ToString();
                        string posicion = facturaModel.Folio.ToString() + "PP" + "de" + facturaModel.numeroProveedor.ToString();
                        string folio = facturaModel.Serie.ToString() + facturaModel.Folio.ToString();

                        //Validamos que el importe de la factura 
                        var Solicitud = solicitudFManager.GetSolicitudById(notaCreditoView.idSolicitudesFactoraje);
                        double MontoFacturar = Solicitud.DescuentoPP;
                        double diferencia = MontoFacturar - Convert.ToDouble(facturaModel.Total);

                        if (diferencia > .5 || diferencia < -.5)
                        {
                            TempData["FlashError"] = "El monto de la factura no coincide con el monto a pagar.";
                            return RedirectToAction("VerSolicitudes", new { proveedorId = proveedor.Id });
                        }

                        //Paso 2: Ejecutamos la funcion de SAP
                        Resultado cargarNotaCredito = sapFacturaManager.CrearNotaCredito(numeroProveedor, fechaFactura, importe, cabecera, posicion, folio, Solicitud.Sociedad);

                        if (cargarNotaCredito.Estatus == "1")
                        {
                            //Paso 3: Guardamos la factura en la bd
                            facturaModel.NumeroGenerado = cargarNotaCredito.FacturaNumero;
                            _facturaManager.GuardaFacturaFactoraje(facturaModel);

                            //Paso 4: Actualizamos el estatus de la solicitud
                            solicitudFManager.UpdateEstatusSolicitud(notaCreditoView.idSolicitudesFactoraje, 4);

                            solicitudesfactoraje sf = solicitudFManager.GetSolicitudById(notaCreditoView.idSolicitudesFactoraje);
                            sf.NumeroGenerado = Convert.ToInt32(facturaModel.NumeroGenerado);
                            solicitudFManager.UpdateSolicitud(sf);

                            _logsFactoraje.InsertLog(SociedadActiva, this.User.Identity.Name.ToString(), "Carga Nota Credito", notaCreditoView.idSolicitudesFactoraje, "Carga nota de credito");

                            TempData["FlashSuccess"] = "Nota de credito registrada satisfactoriamente.";

                        }
                        else
                        {

                            TempData["FlashError"] = "Error al cargar la nota de credito en SAP. " + cargarNotaCredito.ErrorTable.ToString();

                        }
                    }
                    catch (Exception ioe)
                    {

                        TempData["FlashError"] = "Error al cargar la nota de credito en SAP. " + ioe.Message.ToString();

                    }

                    return RedirectToAction("VerSolicitudes", new { proveedorId = proveedor.Id });
                }
            }



            catch (BusinessException businessEx)
            {

                ModelState.AddModelError(string.Empty, businessEx.Message);
                TempData["FlashError"] = businessEx.Message;
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

                ModelState.AddModelError(string.Empty, e.Message);
                TempData["FlashError"] = e.Message;
                return View();
            }
            finally
            {

                if (System.IO.File.Exists(tempPdfPath))
                    System.IO.File.Delete(tempPdfPath);

                if (System.IO.File.Exists(tempXmlPath))
                    System.IO.File.Delete(tempXmlPath);
            }

            
        }


        internal void ValidarPdfVersion4(HttpPostedFileBase file)
        {
            if (file != null && file.ContentType != "application/pdf")
            {
                throw new BusinessException("Pdf Invalido");
            }

            if (file == null)
            {
                throw new BusinessException("Pdf Invalido");
            }
            /*
            var fileName = Path.GetFileName(file.FileName);

            if (fileName == null)
            {
                throw new BusinessException(MensajesResource.ERROR_MensajesInstitucionales_PdfInvalido);
            }

            var path = Path.Combine(Server.MapPath("~/Uploads/"), fileName);

            file.SaveAs(path);

            return "~/Uploads/" + fileName;*/
        }
        internal void ValidarXmlVersion4(HttpPostedFileBase file)
        {
            if (file != null && file.ContentType != "text/xml")
            {
                throw new BusinessException("El archivo no puede ser procesado");
            }

            if (file == null)
            {
                throw new BusinessException("El archivo no puede ser procesado");
            }




            _facturaManager.ValidarFacturaVersion4(file.InputStream);

            /*
            var fileName = Path.GetFileName(file.FileName);

            if (fileName == null)
            {
                throw new BusinessException(MensajesResource.ERROR_MensajesInstitucionales_PdfInvalido);
            }

            var path = Path.Combine(Server.MapPath("~/Uploads/"), fileName);

            file.SaveAs(path);

            return "~/Uploads/" + fileName;*/
        }


        internal void ValidarPdf(HttpPostedFileBase file)
        {
            if (file != null && file.ContentType != "application/pdf")
            {
                throw new BusinessException("Pdf Invalido");
            }

            if (file == null)
            {
                throw new BusinessException("Pdf Invalido");
            }
            /*
            var fileName = Path.GetFileName(file.FileName);

            if (fileName == null)
            {
                throw new BusinessException(MensajesResource.ERROR_MensajesInstitucionales_PdfInvalido);
            }

            var path = Path.Combine(Server.MapPath("~/Uploads/"), fileName);

            file.SaveAs(path);

            return "~/Uploads/" + fileName;*/
        }
        internal void ValidarXml(HttpPostedFileBase file)
        {
            if (file != null && file.ContentType != "text/xml")
            {
                throw new BusinessException("El archivo no puede ser procesado");
            }

            if (file == null)
            {
                throw new BusinessException("El archivo no puede ser procesado");
            }

            _facturaManager.ValidarFactura(file.InputStream);

            /*
            var fileName = Path.GetFileName(file.FileName);

            if (fileName == null)
            {
                throw new BusinessException(MensajesResource.ERROR_MensajesInstitucionales_PdfInvalido);
            }

            var path = Path.Combine(Server.MapPath("~/Uploads/"), fileName);

            file.SaveAs(path);

            return "~/Uploads/" + fileName;*/
        }
        public ActionResult GuardarSolicitud(string facturas, int proveedorId)
        {
            string[] facturasList = facturas.Split(',');

            if (facturas == "," || facturas == "")
            {
                TempData["FlashError"] = "No se ha seleccionado ninguna factura.";
                return RedirectToAction("NuevaSolicitud", new { proveedorId = proveedorId });
            }

            SolicitudFManager solicitudFManager = new SolicitudFManager();
            List<localsolicitud> solicitudesList = solicitudFManager.GetSolicitudesFactoraje();
            //Filtramos las solicitudes de la semana actual
            DateTime startOfWeek = DateTime.Today.AddDays(-1 * (int)(DateTime.Today.DayOfWeek));
            solicitudesList = solicitudesList.Where(x => x.Fecha > startOfWeek).ToList();
            //Validamos que no se tenga abierta una solicitud
            solicitudesList = solicitudesList.Where(x => x.IdProveedor == proveedorId && (x.Estatus != 3 && x.Estatus != 7 && x.Estatus != 8)).ToList();

            //Si hay alguna solicitud en estatus diferente de 3, se le informa al usuario
            if (solicitudesList != null && solicitudesList.Count > 0)
            {
                foreach (var item in solicitudesList)
                {
                    if (item.Sociedad == SociedadActiva)
                    {
                        TempData["FlashError"] = "Solo se puede tener una solicitud abierta.";
                        return RedirectToAction("NuevaSolicitud", new { proveedorId = proveedorId });
                    }
                }
            }

            //var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;

            var partidasManager = new PartidasManager();
            var dsPagosPendientes = partidasManager.GetPartidasAbiertas(ProveedorCxp.NumeroProveedor, SociedadActiva, DateTime.Today);

            var proveedorFManager = new ProveedorFManager();
            ProveedorFManager.localPF provedorFactoraje = proveedorFManager.GetProveedoresFactoraje().Where(x => x.IdProveedor == proveedorId).FirstOrDefault();
            int porcentaje = 0;
            if (provedorFactoraje != null)
            {
                porcentaje = provedorFactoraje.Porcentaje;
            }
            else
            {
                porcentaje = Convert.ToInt32(CommonManager.GetConfiguraciones().Single(c => c.Clave == "prontopago.default.percent").Valor);
            }

            TotalView totalView = new TotalView(dsPagosPendientes, porcentaje, proveedorId, facturasList);
            //SolicitudFManager solicitudFManager = new SolicitudFManager();
            FacturaFManager facturaFManager = new FacturaFManager();
            DescuentoFManager descuentoFManager = new DescuentoFManager();

            //Insertamos la solicitud factoraje
            totalView.SolicitudFactoraje.Sociedad = SociedadActiva;
            int SolicitudId = solicitudFManager.InsSolicitud(totalView.SolicitudFactoraje);

            //Insertamos las facturas del factoraje
            foreach (facturasfactoraje item in totalView.FacturasFactoraje)
            {
                item.idSolicitudesFactoraje = SolicitudId;
                facturaFManager.InsFacturaFactoraje(item);
            }

            string dfs = "";
            //Insertamos los descuentos del factoraje
            foreach (descuentofactoraje item in totalView.DescuentosFactoraje)
            {
                item.idSolicitudesFactoraje = SolicitudId;
                descuentoFManager.InsDescuentoFactoraje(item);
                dfs = dfs + item.NumeroDocumento + ", ";
            }


            _logsFactoraje.InsertLog(SociedadActiva, this.User.Identity.Name.ToString(), "Crea Solicitud", SolicitudId, "Crea solicitud proveedor: " + proveedorId + " con facturas: " + facturas + ". Descuentos: " + dfs);

            return RedirectToAction("VerSolicitudes", new { proveedorId = proveedorId });
        }

        public ActionResult ObtenerTotalFactoraje(string facturas, int proveedorId)
        {
            string[] facturasList = facturas.Split(',');

            //var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
            var partidasManager = new PartidasManager();
            var dsPagosPendientes = partidasManager.GetPartidasAbiertas(ProveedorCxp.NumeroProveedor, SociedadActiva, DateTime.Today);

            var proveedorFManager = new ProveedorFManager();
            ProveedorFManager.localPF provedorFactoraje = proveedorFManager.GetProveedoresFactoraje().Where(x => x.IdProveedor == proveedorId).FirstOrDefault();

            var configuracionesFManager = new ConfiguracionesFManager();
            //configuracionesFManager.GetConfiguracionById();

            int porcentaje = 0;
            if (provedorFactoraje != null)
            {
                porcentaje = provedorFactoraje.Porcentaje;
            }
            else
            {
                porcentaje = Convert.ToInt32(CommonManager.GetConfiguraciones().Single(c => c.Clave == "prontopago.default.percent").Valor);
            }

            TotalView totalView = new TotalView(dsPagosPendientes, porcentaje, proveedorId, facturasList);
            //int dias = 0;
            ViewBag.MontoOriginal = totalView.MontoOriginal.ToString("C");
            ViewBag.DescuentosTotal = totalView.DescuentosTotal.ToString("C");
            ViewBag.DescuentoProntoPago = totalView.Interes.ToString("C");
            ViewBag.TotalSolicitado = totalView.TotalSolicitado.ToString("C");

            return PartialView("_totalProntoPago");
        }
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult VerSolicitud(int proveedorId, int idSolicitudesFactoraje)
        {
            SolicitudFManager solicitudFManager = new SolicitudFManager();
            FacturaFManager facturaFManager = new FacturaFManager();
            //Traemos la solicitud
            solicitudesfactoraje solicitud = solicitudFManager.GetSolicitudById(idSolicitudesFactoraje);
            List<facturasfactoraje> facturas = facturaFManager.GetFacturasBySolicitud(solicitud.idSolicitudesFactoraje);

            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            var proveedor = _proveedorManager.Find(proveedorId, cuenta.Id);

            if (proveedor == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }

            ProveedorCxp = proveedor;


            //Traemos los pagos pendientes
            var partidasManager = new PartidasManager();
            var proveedorFManager = new ProveedorFManager();

            //var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
            var dsPagosPendientes = partidasManager.GetPartidasAbiertas(ProveedorCxp.NumeroProveedor, SociedadActiva, DateTime.Today);

            //Se obtiene porcentaje de factoraje por proveedor.
            ProveedorFManager.localPF provedorFactoraje = proveedorFManager.GetProveedoresFactoraje().Where(x => x.IdProveedor == proveedorId).FirstOrDefault();
            int porcentaje = 0;
            if (provedorFactoraje != null)
            {
                porcentaje = provedorFactoraje.Porcentaje;
            }
            else
            {
                porcentaje = Convert.ToInt32(CommonManager.GetConfiguraciones().Single(c => c.Clave == "prontopago.default.percent").Valor);
            }

            string DiaPago = CommonManager.GetConfiguraciones().Single(c => c.Clave == "prontopago.default.day").Valor;
            DateTime FechaPago = DateTime.Now;

            switch (DiaPago)
            {
                case "LUNES":
                    FechaPago = GetFechaPago(DayOfWeek.Monday);
                    break;
                case "MARTES":
                    FechaPago = GetFechaPago(DayOfWeek.Tuesday);
                    break;
                case "MIERCOLES":
                    FechaPago = GetFechaPago(DayOfWeek.Wednesday);
                    break;
                case "JUEVES":
                    FechaPago = GetFechaPago(DayOfWeek.Thursday);
                    break;
                case "VIERNES":
                    FechaPago = GetFechaPago(DayOfWeek.Friday);
                    break;
                case "SABADO":
                    FechaPago = GetFechaPago(DayOfWeek.Saturday);
                    break;
                case "DOMINGO":
                    FechaPago = GetFechaPago(DayOfWeek.Sunday);
                    break;
            }

            //Armamos la lista con las cuentas por pagas es decir mayores a cero 
            List<Web.Models.ProntoPago.FacturaView> _list = new List<Web.Models.ProntoPago.FacturaView>();
            List<Web.Models.ProntoPago.FacturaView> _listDescuentos = new List<Web.Models.ProntoPago.FacturaView>();

            //Traemos los descuentos seleccionados (descuentosfactoraje)
            var descuentosmanager = new DescuentoFManager();
            List<descuentofactoraje> _df = descuentosmanager.GetDescuentosBySolicitud(idSolicitudesFactoraje);

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
                    idProveedor = ProveedorCxp.Id.ToString(),
                    numeroDocumento = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["BELNR"].ToString(),
                    porcentaje = porcentaje
                };

                if (facturas.Where(x => x.NumeroDocumento == item.numeroDocumento).FirstOrDefault() != null)
                {
                    item.pagar = true;
                }


                //if (item.importe < 0)
                //{
                //	item.pagar = true;
                //}
                if (item.importe > 0 && DateTime.ParseExact(item.vencimiento, "yyyyMMdd", CultureInfo.InvariantCulture) >= FechaPago)
                {
                    _list.Add(item);
                }
                else
                {
                    descuentofactoraje descuentoresult = _df.Where(d => d.NumeroDocumento.Contains(item.numeroDocumento)).FirstOrDefault();
                    if (descuentoresult != null)
                    {
                        _listDescuentos.Add(item);
                    }
                }
            }

            foreach (var item in _df)
            {
                FacturaView descuentoresult = _listDescuentos.Where(d => d.numeroDocumento.Contains(item.NumeroDocumento)).FirstOrDefault();

                if (descuentoresult == null)
                {
                    FacturaView d = new FacturaView()
                    {
                        numeroDocumento = item.NumeroDocumento,
                        descripcion = item.Descripcion,
                        importe = item.Monto
                    };
                    _listDescuentos.Add(d);
                }
            }
            //Traemos el prestamo del proveedor
            SapProveedorManager sapProveedorManager = new SapProveedorManager();
            double prestamo = sapProveedorManager.GetPrestamo(proveedor.NumeroProveedor, SociedadActiva);
            ViewBag.Prestamo = prestamo;

            ViewBag.PagosPendientes = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"];
            ViewBag.Proveedor = proveedor;
            ViewBag.Title = "Nueva solicitud";
            ViewBag.SubTitle = "En esta sección podrán crear una solicitud.";
            ViewBag.Facturas = _list;
            ViewBag.Descuentos = _listDescuentos;

            //

            ViewBag.MontoOriginal = solicitud.MontoOriginal.ToString("C");
            ViewBag.DescuentosTotal = solicitud.Descuentos.ToString("C");
            ViewBag.DescuentoProntoPago = solicitud.DescuentoPP.ToString("C");
            ViewBag.TotalSolicitado = solicitud.MontoAFacturar.ToString("C");
            ViewBag.DisableItems = 1;
            ViewBag.SoloLectura = 1;
            return View("Solicitud");
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult NuevaSolicitud(int proveedorId)
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            var proveedor = _proveedorManager.Find(proveedorId, cuenta.Id);

            if (proveedor == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }

            ProveedorCxp = proveedor;


            //Traemos los pagos pendientes
            var partidasManager = new PartidasManager();
            var proveedorFManager = new ProveedorFManager();

            ///var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
            var dsPagosPendientes = partidasManager.GetPartidasAbiertas(ProveedorCxp.NumeroProveedor, SociedadActiva, DateTime.Today);

            //Se obtiene porcentaje de factoraje por proveedor.
            ProveedorFManager.localPF provedorFactoraje = proveedorFManager.GetProveedoresFactoraje().Where(x => x.IdProveedor == proveedorId).FirstOrDefault();
            int porcentaje = 0;
            if (provedorFactoraje != null)
            {
                porcentaje = provedorFactoraje.Porcentaje;
            }
            else
            {
                porcentaje = Convert.ToInt32(CommonManager.GetConfiguraciones().Single(c => c.Clave == "prontopago.default.percent").Valor);
            }

            string DiaPago = CommonManager.GetConfiguraciones().Single(c => c.Clave == "prontopago.default.day").Valor;
            DateTime FechaPago = DateTime.Now;

            switch (DiaPago)
            {
                case "LUNES":
                    FechaPago = GetFechaPago(DayOfWeek.Monday);
                    break;
                case "MARTES":
                    FechaPago = GetFechaPago(DayOfWeek.Tuesday);
                    break;
                case "MIERCOLES":
                    FechaPago = GetFechaPago(DayOfWeek.Wednesday);
                    break;
                case "JUEVES":
                    FechaPago = GetFechaPago(DayOfWeek.Thursday);
                    break;
                case "VIERNES":
                    FechaPago = GetFechaPago(DayOfWeek.Friday);
                    break;
                case "SABADO":
                    FechaPago = GetFechaPago(DayOfWeek.Saturday);
                    break;
                case "DOMINGO":
                    FechaPago = GetFechaPago(DayOfWeek.Sunday);
                    break;
            }

            //Armamos la lista con las cuentas por pagas es decir mayores a cero 
            List<Web.Models.ProntoPago.FacturaView> _list = new List<Web.Models.ProntoPago.FacturaView>();
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
                    idProveedor = ProveedorCxp.Id.ToString(),
                    numeroDocumento = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["BELNR"].ToString(),
                    porcentaje = porcentaje
                };
                if (item.importe < 0)
                {
                    item.pagar = true;
                }
                if (item.importe > 0 && DateTime.ParseExact(item.vencimiento, "yyyyMMdd", CultureInfo.InvariantCulture) >= FechaPago)
                {
                    _list.Add(item);
                }
                else
                {
                    //if (DateTime.ParseExact(item.vencimiento, "yyyyMMdd", CultureInfo.InvariantCulture) >= FechaPago)
                    if (item.importe < 0)
                    {
                        _listDescuentos.Add(item);
                    }

                }
            }

            //Traemos el prestamo del proveedor
            SapProveedorManager sapProveedorManager = new SapProveedorManager();
            double prestamo = sapProveedorManager.GetPrestamo(proveedor.NumeroProveedor, SociedadActiva);
            ViewBag.Prestamo = prestamo;

            ViewBag.PagosPendientes = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"];
            ViewBag.Proveedor = proveedor;
            ViewBag.Title = "Nueva solicitud";
            ViewBag.SubTitle = "En esta sección podrán crear una solicitud.";
            ViewBag.Facturas = _list;
            ViewBag.Descuentos = _listDescuentos;
            ViewBag.DisableItems = 0;

            return View("Solicitud");
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult VerSolicitudes(int proveedorId, string sociedad)
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            var proveedor = _proveedorManager.Find(proveedorId, cuenta.Id);

            if (proveedor == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }

            ProveedorCxp = proveedor;
            SociedadActiva = sociedad;
            ViewBag.Proveedor = proveedor;

            //Obtenemos las solicitudes del proveedor
            SolicitudFManager solicitudFManager = new SolicitudFManager();
            List<localsolicitud> solicitudesList = solicitudFManager.GetSolicitudesFactoraje();
            solicitudesList = solicitudesList.Where(x => x.IdProveedor == proveedorId && x.Sociedad == SociedadActiva).ToList();
            //Filtramos solicitudes por semana actual
            DateTime startOfWeek = DateTime.Today.AddDays(-1 * (int)(DateTime.Today.DayOfWeek));
            solicitudesList = solicitudesList.Where(x => x.Fecha > startOfWeek).ToList();
            return View(solicitudesList);
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult Index()
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            ViewBag.proveedores = _proveedorManager.FindByCuentaId(cuenta.Id);

            return View();
        }
        //**********Termina CU002-Seleccion razon social*****************


        //**********CU003-Facturas pendientes de pago*****************
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult SeleccionarProveedor(int proveedorId, string sociedad)
        {

            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            var proveedor = _proveedorManager.Find(proveedorId, cuenta.Id);

            if (proveedor == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }

            ProveedorCxp = proveedor;
            SociedadActiva = sociedad;
            return RedirectToAction("Pagos");
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult Pagos()
        {

            if (ProveedorCxp == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }

            var partidasManager = new PartidasManager();

            //var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
            var dsPagosPendientes = partidasManager.GetPartidasAbiertas(ProveedorCxp.NumeroProveedor, SociedadActiva, DateTime.Today);

            ViewBag.PagosPendientes = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"];

            ViewBag.Proveedor = ProveedorCxp;

            //Armamos la lista
            List<Web.Models.ProntoPago.FacturaView> _list = new List<Web.Models.ProntoPago.FacturaView>();
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
                    idProveedor = ProveedorCxp.Id.ToString(),
                    numeroDocumento = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["BELNR"].ToString()
                };
                if (item.importe < 0)
                {
                    item.pagar = true;
                }
                _list.Add(item);
            }

            return View(_list);
        }
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult RegistraSolicitud(List<Web.Models.ProntoPago.FacturaView> model)
        {

            foreach (var item in model)
            {
                if (item.importe < 0)
                {
                    item.pagar = true;
                }
            }
            return RedirectToAction("Pagos");
        }
        public DateTime GetFechaPago(DayOfWeek day)
        {
            DateTime result = DateTime.Now.AddDays(1);
            while (result.DayOfWeek != day)
                result = result.AddDays(1);
            return result;
        }
        //**********Termina CU003-Facturas pendientes de pago*****************
    }
}