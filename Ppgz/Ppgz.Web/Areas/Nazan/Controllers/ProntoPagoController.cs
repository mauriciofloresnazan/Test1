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

namespace Ppgz.Web.Areas.Nazan.Controllers
{
    public class ProntoPagoController : Controller
    {
        readonly Entities _db = new Entities();
        
        readonly CuentaManager _cuentaManager = new CuentaManager();
        readonly ConfiguracionesFManager _configuracionesFManager = new ConfiguracionesFManager();
        readonly FacturaFManager _facturaFManager = new FacturaFManager();
        readonly ProveedorManager _proveedorManager = new ProveedorManager();
        readonly ProveedorFManager _proveedorFManager = new ProveedorFManager();
        readonly SolicitudFManager _solicitudFManager = new SolicitudFManager();

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO")]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO")]
        public ActionResult Solicitudes()
        {
            var solicitudesF = _solicitudFManager.GetSolicitudesFactoraje();

            Int32.TryParse(CommonManager.GetConfiguraciones().Single(c => c.Clave == "prontopago.default.percent").Valor, out int p);
            foreach(var item in solicitudesF)
            {
                var index = solicitudesF.FindIndex(c => c.Id == item.Id);
                solicitudesF[index].Tasa = (item.Tasa == 0 ) ?  p : item.Tasa;                
            }

            ViewBag.SolicitudesF = solicitudesF;
            
            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO")]
        public ActionResult Proveedores()
        {
            var cuentas = _cuentaManager.FindAllFactoraje();
            List<localPF> lproveedores = new List<localPF>();

            foreach(var cuenta in cuentas)
            {
                foreach(var proveedor in cuenta.proveedores)
                {
                    localPF p = new localPF{
                        CuentaId = cuenta.Id,
                        IdProveedor = proveedor.Id,
                        Nombre = proveedor.Nombre1,
                        Numero = proveedor.NumeroProveedor,
                        Rfc = proveedor.Rfc
                    };

                    lproveedores.Add(p);
                }
            }

            foreach(var item in lproveedores)
            {
                proveedorfactoraje _pf = _proveedorFManager.GetProveedorById(item.IdProveedor);
                if(_pf != null)
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

            ViewBag.DiaDePago = CommonManager.GetConfiguraciones().Single(c => c.Clave == "prontopago.default.day").Valor; 
            ViewBag.Porcentaje = CommonManager.GetConfiguraciones().Single(c => c.Clave == "prontopago.default.percent").Valor;
            ViewBag.ProveedoresF = lproveedores;
            return View();
        }

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

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO")]
        public ActionResult SolicitudDetalle(int id)
        {
            var solicitud = _solicitudFManager.GetSolicitudById(id);
            var proveedor = _proveedorManager.Find(solicitud.IdProveedor);
            var facturas = _facturaFManager.GetFacturasBySolicitud(id);

            ViewBag.Proveedor = proveedor;
            ViewBag.Facturas = facturas;

            return View();
        }

        public ActionResult ActualizarProveedor(int idCuenta, int idProveedor, int diadepago, int porcentaje)
        {
            bool result = false;

            if ((diadepago >0 && diadepago < 8) || (porcentaje >= 0 && porcentaje <=100))
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
            result = _proveedorFManager.DeleteProveedorFactoraje(idProveedor);

            if (result)
                TempData["FlashSuccess"] = "Eliminado correctamente.";
            else
                TempData["FlashError"] = "Ocurrio un error al eliminar";

            return RedirectToAction("Proveedores");
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
    }    
}