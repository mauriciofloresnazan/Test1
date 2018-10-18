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
using System.Web.Script.Serialization;

using Ppgz.Web.Models.ProntoPago;

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
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO")]
        public ActionResult Index()
        {
            //Se contabilizan los estatusfactoraje <estatus, cantudad>
            var lse = GetSolicitudEstatus();
            ViewBag.SolicitudesEstatus = lse;
            ViewBag.ParaEnviar = lse.Where(x => x.EstatusNombre == "Lista Para Propuesta").FirstOrDefault().Cantidad.ToString();

            return View();
        }

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
        /*--------------END DASHBOARD SECTION---------------*/

        /*-------------BEGIN PROVEEDORES SECTION--------------*/
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO")]
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

        public ActionResult ActualizarProveedor(int idCuenta, int idProveedor, int diadepago, int porcentaje)
        {
            bool result = false;

            //Validamos el rango de los datos y cambiamos los valores en servicio
            if ((diadepago > 0 && diadepago < 8) || (porcentaje >= 0 && porcentaje <= 100))
            {
                result = _proveedorFManager.UpdateProveedorFactoraje(idProveedor, diadepago, porcentaje);
            }

            if (result)
                TempData["FlashSuccess"] = "Actualización realizada correctamente.";
            else
                TempData["FlashError"] = "Datos incorrectos";

            return RedirectToAction("Proveedores");
        }

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
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO")]
        public ActionResult Solicitudes()
        {
            //Obtenemos todas las solicitudes de pronto pago
            var solicitudesF = _solicitudFManager.GetSolicitudesFactoraje();

            //Int32.TryParse(CommonManager.GetConfiguraciones().Single(c => c.Clave == "prontopago.default.percent").Valor, out int p);
            //foreach(var item in solicitudesF)
            //{
            //    var index = solicitudesF.FindIndex(c => c.Id == item.Id);
            //    solicitudesF[index].Tasa = (item.Tasa == 0 ) ?  p : item.Tasa;                
            //}

            ViewBag.SolicitudesF = solicitudesF;
            
            return View();
        }

        public ActionResult SolicitudesEnviarPropuestas(string selectedlist)
        {
            var listpropuestas = new List<solicitudesfactoraje>();
            string[] split = selectedlist.Split(',');
            foreach (string element in split)
            {
                Int32.TryParse(element, out int id);
                var propuesta = _solicitudFManager.GetSolicitudById(id);
                listpropuestas.Add(propuesta);
            }

            TempData["FlashError"] = "Error al enviar solicitudes";
            TempData["FlashSuccess"] = "Solicitudes enviadas con exito";
            return RedirectToAction("Solicitudes");
        }

        public List<KeyValueCustom> GetSolicitudEstatus()
        {
            //Obtenemos una lista de los estatusfactoraje contabilizados 
            var lsolicitudesf = _solicitudFManager.GetSolicitudesFactoraje();
            var lestatus = _db.estatusfactoraje.ToList();
            var estatusSolicitudes = new List<KeyValueCustom>();

            foreach (var item in lestatus)
            {
                int count = 0;
                count = lsolicitudesf.Where(x => x.EstatusNombre == item.Nombre).Count();
                var element = new KeyValueCustom(item.Nombre, count);
                estatusSolicitudes.Add(element);
            }
            estatusSolicitudes.Add(new KeyValueCustom("Total", lsolicitudesf.Count()));
            var lse = estatusSolicitudes.OrderByDescending(x => x.Cantidad).ToList();
            return lse;
        }
        /*--------------END SOLICITUDES SECTION---------------*/

        /*-------------BEGIN SOLICITUD DETALLE SECTION--------------*/
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO")]
        public ActionResult SolicitudDetalle(int id)
        {
            //Obtenemos las facturas, descuentos por solicitud
            var solicitud = _solicitudFManager.GetSolicitudById(id);
            var proveedor = _proveedorManager.Find(solicitud.IdProveedor);
            var facturas = _facturaFManager.GetFacturasBySolicitud(id);
            var descuentos = _descuentoFManager.GetDescuentosBySolicitud(id);            

            //Se obtienen los calculos iniciales 
            TotalView totalView = new TotalView(id);
            //int dias = 0;
            ViewBag.MontoOriginal = totalView.MontoOriginal.ToString("C");
            ViewBag.DescuentosTotal = totalView.DescuentosTotal.ToString("C");
            ViewBag.DescuentoProntoPago = totalView.Interes.ToString("C");
            ViewBag.TotalSolicitado = totalView.TotalSolicitado.ToString("C");

            ViewBag.Solicitud = solicitud;
            ViewBag.Proveedor = proveedor;
            ViewBag.Facturas = facturas;
            ViewBag.Descuentos = descuentos;

            return View();
        }

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

        public ActionResult AprobarSolicitud(string facturas, string descuentos, int solicitudId, int estatus)
        {
            string[] facturasList = facturas.Split(',');
            string[] descuentosList = descuentos.Split(',');

            //ELIMINA FACTURAS Y DESCUENTOS SIN CHECK
            _facturaFManager.DeleteFacturas(facturasList, solicitudId);
            _descuentoFManager.DeleteDescuentos(descuentosList, solicitudId);

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
                _solicitudFManager.UpdateEstatusSolicitud(nuevaSolicitud.idSolicitudesFactoraje, estatus);
            }

            return RedirectToAction("Solicitudes");
        }

        public ActionResult RechazarSolicitud(int solicitudId)
        {
            //Cambia el estatus en solicitudes, facturas y descuentos por rechazada
            var result = _solicitudFManager.UpdateEstatusSolicitud(solicitudId, 3);

            return RedirectToAction("Solicitudes");
        }
        /*--------------END SOLICITUD DETALLE SECTION---------------*/

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO")]
        public ActionResult Configuraciones()
        {
            var configuraciones = _configuracionesFManager.GetConfiguraciones();
            ViewBag.Configuraciones = configuraciones;

            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO")]
        public ActionResult Logs()
        {
            return View();
        }              

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