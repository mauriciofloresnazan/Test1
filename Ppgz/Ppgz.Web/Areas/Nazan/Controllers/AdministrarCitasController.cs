using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Ppgz.Repository;

namespace Ppgz.Web.Areas.Nazan.Controllers
{
    public class AdministrarCitasController : Controller
    {
        [Authorize(Roles = "MAESTRO-NAZAN")]
        public ActionResult Index()
        {
            var db = new Entities();

            var fecha = DateTime.Today.AddMonths(-3);

            ViewBag.Citas = db.citas.Where(c => c.FechaCita > fecha).ToList();

            ViewBag.EstatusCita = db.estatuscitas.ToList();

            return View();
        }


        [Authorize(Roles = "MAESTRO-NAZAN")]
        public ActionResult Enroque(string fecha = null)
        {
            if (fecha == null)
            {
                fecha = DateTime.Today.Date.ToString("dd/MM/yyyy");
            }
            var date = DateTime.ParseExact(fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            var db = new Entities();

            var horarioRieles = db.horariorieles.Where(h => h.Fecha == date).ToList();

            ViewBag.HorarioRieles = horarioRieles;

            ViewBag.Fecha = date.ToString("yyyy/MM/dd");

            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Enroque(int horarioRielId1, int horarioRielId2)
        {

            var db = new Entities();

            var horarioRiel1 = db.horariorieles.Find(horarioRielId1);
            var citaId1 = horarioRiel1.CitaId;
            var disponibilidad1 = horarioRiel1.Disponibilidad;



            var horarioRiel2 = db.horariorieles.Find(horarioRielId2);
            var citaId2 = horarioRiel2.CitaId;
            var disponibilidad2 = horarioRiel2.Disponibilidad;
            

            horarioRiel1.CitaId = citaId2;
            horarioRiel1.Disponibilidad = disponibilidad2;
            db.Entry(horarioRiel1).State = EntityState.Modified;

            horarioRiel2.CitaId = citaId1;
            horarioRiel2.Disponibilidad = disponibilidad1;
            db.Entry(horarioRiel2).State = EntityState.Modified;


            db.SaveChanges();


            TempData["FlashSuccess"] = "Enroque aplicado exitosamente";
            return RedirectToAction("Index");
        }
        
        [Authorize(Roles = "MAESTRO-NAZAN")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Penalizar(int citaId, int? estatusId = null)
        {
            var db = new Entities();


            var cita  = db.citas.FirstOrDefault(c => c.Id == citaId);

            if (cita == null)
            {
                TempData["FlashError"] = "Cita incorrecta";
                return RedirectToAction("Index"); 
            }
            var estatus = db.estatuscitas.FirstOrDefault(e=> e.Id == estatusId);

            if (estatus == null)
            {
                cita.EstatusCitaId = null;
            }
            else
            {
                cita.EstatusCitaId = estatus.Id;
            }

            db.Entry(cita).State = EntityState.Modified;
            db.SaveChanges();


            TempData["FlashSuccess"] = "Penalización aplicada exitosamente";
            return RedirectToAction("Index");
        }
    }
}