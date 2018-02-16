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
using Ppgz.Repository;
using Ppgz.Web.Infrastructure;
using SapWrapper;

namespace Ppgz.Web.Areas.Nazan.Controllers
{
    public class PenalizacionesController : Controller
    {
        
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

        //[Authorize(Roles = "MAESTRO-NAZAN,NAZAN-PENALIZACIONES")]
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

            _penalizacionesManager.aplicarPenalizacion(cita.proveedore.NumeroProveedor, estatus.Monto, estatus.Nombre);



            cita.EstatusCitaId = estatus.Id;
           

            db.Entry(cita).State = EntityState.Modified;
            db.SaveChanges();


            TempData["FlashSuccess"] = "Penalización aplicada exitosamente";
            return RedirectToAction("Index");
        }
    }
}