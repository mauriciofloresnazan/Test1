using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using MySql.Data.MySqlClient;
using Ppgz.CitaWrapper;
using Ppgz.Web.Infrastructure;
using SapWrapper;
using Ppgz.Repository;

namespace Ppgz.Web.Areas.Nazan.Controllers
{

    public class PenalizacionesController : Controller
    {
        private readonly Entities _db = new Entities();
        readonly SapPenalizacionesManager _penalizacionesManager = new SapPenalizacionesManager();

        // GET: Nazan/Penalizaciones
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PENALIZACIONES")]
        public ActionResult Index()
        {
            var db = new Entities();

            var fecha = DateTime.Today.AddMonths(-3);

            ViewBag.Citas = db.citas.Where(c => c.FechaCita > fecha & c.FechaCita < DateTime.Today).OrderByDescending(c => c.FechaCita).ToList();

            ViewBag.EstatusCita = db.estatuscitas.ToList();

            return View();
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

            ViewBag.Citas = db.citas.Where(c => c.FechaCita > dateFechaDesde & c.FechaCita < dateFechaHasta & c.estatuscita != null).OrderByDescending(c => c.FechaCita).ToList();

            ViewBag.EstatusCita = db.estatuscitas.ToList();
            ViewBag.FechaDesde = dateFechaDesde;

            ViewBag.FechaHasta = dateFechaHasta;

            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PENALIZACIONES")]
        public ActionResult Editar()
        {
            var db = new Entities();

            ViewBag.EstatusCita = db.estatuscitas.ToList();

            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PENALIZACIONES")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Penalizar(int citaId, int? estatusId = null)
        {
            var db = new Entities();


            var cita = db.citas.FirstOrDefault(c => c.Id == citaId);

            if (cita == null)
            {
                TempData["FlashError"] = "Cita incorrecta";
                return RedirectToAction("Index");
            }
            var estatus = db.estatuscitas.FirstOrDefault(e => e.Id == estatusId);

            var result = _penalizacionesManager.aplicarPenalizacion(cita.proveedore.NumeroProveedor, estatus.Monto, estatus.Nombre);

            if (result[0] == "")
            {
                TempData["FlashError"] = "Error al aplicar la penalizacion";
                return RedirectToAction("Index");
            }
            else
            {
                cita.EstatusCitaId = estatus.Id;


                db.Entry(cita).State = EntityState.Modified;
                db.SaveChanges();


                var commonManager = new CommonManager();
                try
                {
                    commonManager.SendNotificacionP("Portal de Proveedores del Grupo Nazan - Cita penalizada", "La Cita con ID "+ cita.Id +" ha sido Penalizada con el motivo "+ estatus.Nombre+" por el monto de "+estatus.Monto+" pesos, generando el documento contable N° "+ result[0] + " ", cita.proveedore.Correo);
                }
                catch (Exception ex)
                {
                    TempData["FlashError"] = "Error enviando correo con la notificacion de la penalizacion";
                }

                TempData["FlashSuccess"] = "Penalización aplicada exitosamente, documento generado N° "+ result[0];
                return RedirectToAction("Index");
            }

        }

        



        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PENALIZACIONES")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Modificar(int Id, string Codigo, string Nombre, string Monto)
        {
            var db = new Entities();


            var penalizacion = db.estatuscitas.FirstOrDefault(c => c.Id == Id);

            if (penalizacion == null)
            {
                TempData["FlashError"] = "Penalizacion incorrecta";
                return RedirectToAction("Editar");
            }

            penalizacion.Codigo = Codigo;
            penalizacion.Nombre = Nombre;
            penalizacion.Monto = Monto;

            db.Entry(penalizacion).State = EntityState.Modified;
            db.SaveChanges();


            TempData["FlashSuccess"] = "Penalización modificada exitosamente";
            return RedirectToAction("Editar");
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PENALIZACIONES")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Crear(string Codigo, string Nombre, string Monto)
        {
            var db = new Entities();


            var penalizacion = new estatuscita();

            if (penalizacion == null)
            {
                TempData["FlashError"] = "Penalizacion incorrecta";
                return RedirectToAction("Editar");
            }

            penalizacion.Codigo = Codigo;
            penalizacion.Nombre = Nombre;
            penalizacion.Monto = Monto;

            db.estatuscitas.Add(penalizacion);
            db.SaveChanges();


            TempData["FlashSuccess"] = "Penalización creada exitosamente";
            return RedirectToAction("Editar");
        }
    }
}