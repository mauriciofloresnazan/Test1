using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ppgz.Repository;

namespace Ppgz.Web.Areas.Nazan.Controllers
{
    public class AdministrarCitasController : Controller
    {

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARMENSAJESINSTITUCIONALES-LISTAR,NAZAN-ADMINISTRARMENSAJESINSTITUCIONALES-MODIFICAR")]
        public ActionResult Index(string fecha = null)
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

        public ActionResult Enroque(int horarioRielId1, int horarioRielId2)
        {

            var db = new Entities();

            var horarioRiel1 = db.horariorieles.Find(horarioRielId1);
            var citaId1 = horarioRiel1.CitaId;
            var disponibilidad1 = horarioRiel1.Disponibilidad;



            var horarioRiel2 = db.horariorieles.Find(horarioRielId2);
            var citaId2 = horarioRiel2.CitaId;
            var disponibilidad2 = horarioRiel2.Disponibilidad;


            DateTime? fecha = horarioRiel1.cita != null ? horarioRiel1.cita.FechaCita : horarioRiel2.cita.FechaCita;

            /*horarioRiel1.CitaId = null;
            db.Entry(horarioRiel1).State = EntityState.Modified;
            horarioRiel2.CitaId = null;
            db.Entry(horarioRiel2).State = EntityState.Modified;
            db.SaveChanges();

            var horarioRiel1_1 = db.horariorieles.Find(horarioRielId1);
            var horarioRiel2_2 = db.horariorieles.Find(horarioRielId2);
            *
            horarioRiel1_1.CitaId = citaId2;*/

            horarioRiel1.CitaId = citaId2;
            horarioRiel1.Disponibilidad = disponibilidad2;
            db.Entry(horarioRiel1).State = EntityState.Modified;

            horarioRiel2.CitaId = citaId1;
            horarioRiel2.Disponibilidad = disponibilidad1;
            db.Entry(horarioRiel2).State = EntityState.Modified;


            db.SaveChanges();


            TempData["FlashSuccess"] = "Enroque aplicado exitosamente";
            return RedirectToAction("Index", new { fecha = ((DateTime)fecha).ToString("dd/MM/yyyy") });
        }
	}
}