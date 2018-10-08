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
            ViewBag.SolicitudesF = _solicitudFManager.GetSolicitudesFactoraje();
            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO")]
        public ActionResult Proveedores()
        {
            ViewBag.ProveedoresF = _proveedorFManager.GetProveedoresFactoraje(); 
            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PRONTOPAGO")]
        public ActionResult Configuraciones()
        {
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
            //var solicitud = _solicitudFManager.

            return View();
        }

        public ActionResult ActualizarProveedor(int id, int diadepago, int porcentaje)
        {
            bool result = false;

            if ((diadepago >0 && diadepago < 8) || (porcentaje >= 0 && porcentaje <=100))
            {
                result = _proveedorFManager.UpdateProveedorFactoraje(id, diadepago, porcentaje);
            }

            TempData["FlashSuccess"] = result ? "Actualización realizada correctamente." : "Datos incorrectos";

            return RedirectToAction("Proveedores");
        }

        public ActionResult EliminarProveedor(int id)
        {
            bool result = false;
            result = _proveedorFManager.DeleteProveedorFactoraje(id);

            TempData["FlashSuccess"] = result ? "Eliminado correctamente." : "Ocurrio un error al eliminar";
            return RedirectToAction("Proveedores");
        }        
    }    
}