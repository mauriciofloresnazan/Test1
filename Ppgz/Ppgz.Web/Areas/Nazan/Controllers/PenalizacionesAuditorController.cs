using System;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Ppgz.Web.Infrastructure;
using SapWrapper;
using Ppgz.Repository;
using Ppgz.Services;
namespace Ppgz.Web.Areas.Nazan.Controllers
{

    public class PenalizacionesAuditorController : Controller
    {
        private readonly Entities _db = new Entities();
        readonly SapPenalizacionesManager _penalizacionesManager = new SapPenalizacionesManager();

        readonly CuentaManager _cuentaManager = new CuentaManager();
        readonly ProveedorManager _proveedorManager = new ProveedorManager();
        //
        // GET: /Nazan/AdministrarProveedores/
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PENALIZACIONES")]
        public ActionResult Index()
        {
            var db = new Entities();
            
            ViewBag.Proveedores = db.proveedores.ToList();
            ViewBag.auditores = db.auditores.ToList();
            return View();
        }
        // GET: Nazan/Penalizaciones
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PENALIZACIONES")]
        public ActionResult Penalizaciones(string fechaDesde = null, string fechaHasta = null)
        {

            var dateFechaDesde = DateTime.Today.AddMonths(-3);
            if (!string.IsNullOrWhiteSpace(fechaDesde))
            {
                dateFechaDesde = DateTime.ParseExact(fechaDesde, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            var dateFechaHasta = DateTime.Today;
            if (!string.IsNullOrWhiteSpace(fechaHasta))
            {
                dateFechaHasta = DateTime.ParseExact(fechaHasta, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            var db = new Entities();

            ///var fecha = DateTime.Today.AddMonths(-3);
            
            ViewBag.FechaDesde = dateFechaDesde;
            ViewBag.FechaHasta = dateFechaHasta;
            ViewBag.penalizacion = db.Penalizacionauditores.Where(c => c.FechaPenalizacion > dateFechaDesde & c.FechaPenalizacion < dateFechaHasta & c.procesado==false).OrderByDescending(c => c.FechaPenalizacion).ToList();
            return View();
        }
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PENALIZACIONES")]
        public ActionResult Registrar(int proveedorid,DateTime fecha, string proveedornumeroproveedor, string proveedornombre1, string proveedorcuenta, string Monto, string auditor, string proveedorcorreo)
        {
            var db = new Entities();
            var penalizacion = new Penalizacionauditor();
            penalizacion.FechaPenalizacion = fecha;
            penalizacion.NumeroProveedor = proveedornumeroproveedor;
            penalizacion.RazonSocial = proveedornombre1;
            penalizacion.Marca = proveedorcuenta;
            penalizacion.Total = Monto;
            penalizacion.auditor = auditor;
            penalizacion.Correo = proveedorcorreo;
            db.Penalizacionauditores.Add(penalizacion);

            db.SaveChanges();

            TempData["FlashSuccess"] = "penalizacion Agregada correctamente.";
            return RedirectToAction("Index");
        }
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PENALIZACIONES")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Penalizar(int[] penalizarId, string disponible)
        {
            var db = new Entities();
            foreach (var riel in penalizarId)
            {
                var pena = db.Penalizacionauditores.FirstOrDefault(c => c.id == riel);
                var result = _penalizacionesManager.aplicarPenalizacionAuditoria(pena.NumeroProveedor, pena.Total, " penalizacion con Id: " + pena.id);

                if (result[0] == "")
                {
                    TempData["FlashError"] = "Error al aplicar la penalizacion";
                    
                }
                else
                {
                    pena.procesado = true;
                    db.Entry(pena).State = EntityState.Modified;
                    db.SaveChanges();
                    var commonManager = new CommonManager();
                    try
                    {
                        commonManager.SendNotificacionP("Portal de Proveedores del Grupo Nazan - Auditoria penalizada", "La Auditoria con ID " + pena.id + " ha sido Penalizada por el motivo  auditoria en fabrica por el monto de " + pena.Total + " pesos, generando el documento contable N° " + result[0] + " ", pena.Correo);
                    }
                    catch (Exception ex)
                    {
                        TempData["FlashError"] = "Error enviando correo con la notificacion de la penalizacion";
                    }

                    TempData["FlashSuccess"] = "Penalización aplicada exitosamente";

                }

            }

            return RedirectToAction("penalizaciones");
        }
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PENALIZACIONES")]
        public ActionResult Reporte(string fechaDesde = null, string fechaHasta = null)
        {

            var dateFechaDesde = DateTime.Today.AddMonths(-3);
            if (!string.IsNullOrWhiteSpace(fechaDesde))
            {
                dateFechaDesde = DateTime.ParseExact(fechaDesde, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            var dateFechaHasta = DateTime.Today;
            if (!string.IsNullOrWhiteSpace(fechaHasta))
            {
                dateFechaHasta = DateTime.ParseExact(fechaHasta, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            var db = new Entities();

            ///var fecha = DateTime.Today.AddMonths(-3);

            ViewBag.penalizacion = db.Penalizacionauditores.Where(c => c.FechaPenalizacion > dateFechaDesde & c.FechaPenalizacion < dateFechaHasta & c.procesado ==true).OrderByDescending(c => c.FechaPenalizacion).ToList();

           
            ViewBag.FechaDesde = dateFechaDesde;

            ViewBag.FechaHasta = dateFechaHasta;

            return View();
        }
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PENALIZACIONES")]
        public ActionResult Editar()
        {
            var db = new Entities();

            ViewBag.EditarAuditor = db.auditores.ToList();

            return View();
        }
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PENALIZACIONES")]
        public ActionResult Eliminar(int audiid)
        {
            var db = new Entities();
            var auditor = db.auditores.Find(audiid);
            db.Entry(auditor).State = EntityState.Deleted;
            db.SaveChanges();


            TempData["FlashSuccess"] = "Auditor Eliminado Exitosamente";
            return RedirectToAction("Editar");
        }
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PENALIZACIONES")]
        public ActionResult Crear(string numero, string nombre)
        {
            var db = new Entities();


            var auditores = new audi();


            auditores.NumeroEmpleado = numero;
            auditores.Nombreauditor = nombre;
            

            db.auditores.Add(auditores);
            db.SaveChanges();


            TempData["FlashSuccess"] = "Auditor creado exitosamente";
            return RedirectToAction("Editar");
        }
    }

}




