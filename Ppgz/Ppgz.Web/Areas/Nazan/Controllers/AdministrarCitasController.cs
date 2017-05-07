using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using MySql.Data.MySqlClient;
using Ppgz.Repository;
using Ppgz.Web.Infrastructure;

namespace Ppgz.Web.Areas.Nazan.Controllers
{
    public class AdministrarCitasController : Controller
    {
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS")]
        public ActionResult Index()
        {
            var db = new Entities();

            var fecha = DateTime.Today.AddMonths(-3);

            ViewBag.Citas = db.citas.Where(c => c.FechaCita > fecha).ToList();

            ViewBag.EstatusCita = db.estatuscitas.ToList();

            return View();
        }


        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS")]
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

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS")]
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

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS")]
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


        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS")]
        public ActionResult CambiarFecha(int citaId, string fecha)
        {

            var date = DateTime.ParseExact(fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            var db = new Entities();

            var cita = db.citas.FirstOrDefault(c => c.FechaCita >= DateTime.Today && c.Id == citaId);

            if (cita == null)
            {
                TempData["FlashError"] = "Cita incorrecta";
                return RedirectToAction("Index"); 
            }

            if (date.Date == cita.FechaCita.Date)
            {
                TempData["FlashError"] = "Fecha incorrecta para el cambio";
                return RedirectToAction("Index");
            }

            if (date.Date < DateTime.Today.Date)
            {
                TempData["FlashError"] = "Fecha incorrecta para el cambio";
                return RedirectToAction("Index");
            }

            ViewBag.Fecha = date;
            ViewBag.Cita = cita;


            var parameters = new List<MySqlParameter>()
			{
				new MySqlParameter
				{
					ParameterName = "pTotal",
					Direction = ParameterDirection.Output,
					MySqlDbType = MySqlDbType.VarChar
				},
				new MySqlParameter("pFecha", date)
			};

            Db.ExecuteProcedureOut(parameters, "config_appointment");

            var horarioRieles = db.horariorieles.Where(h => h.Fecha == date.Date).ToList();

            ViewBag.HorarioRieles = horarioRieles;

            return View();
        }


        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult CambiarFecha(int citaId, string fecha, int[] horarioRielesIds)
        {
            var date = DateTime.ParseExact(fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            var db = new Entities();

            var cita = db.citas.FirstOrDefault(c => c.FechaCita >= DateTime.Today && c.Id == citaId);

            if (cita == null)
            {
                TempData["FlashError"] = "Cita incorrecta";
                return RedirectToAction("Index");
            }

            if (date.Date == cita.FechaCita.Date)
            {
                TempData["FlashError"] = "Fecha incorrecta para el cambio";
                return RedirectToAction("Index");
            }

            if (date.Date < DateTime.Today.Date)
            {
                TempData["FlashError"] = "Fecha incorrecta para el cambio";
                return RedirectToAction("Index");
            }

            foreach (var horarioRileId in horarioRielesIds)
            {
                var horarioRiel = db.horariorieles.FirstOrDefault(hr => hr.Id == horarioRileId);

                if (horarioRiel == null)
                {
                    TempData["FlashError"] = "Rieles incorrectos";
                    return RedirectToAction("Index");
                }
                if (horarioRiel.CitaId != null)
                {
                    TempData["FlashError"] = "Selección incorrecta de Rieles";
                    return RedirectToAction("CambiarFecha", new {citaId, fecha });
                }
            }
            

            foreach (var horarioRiel in cita.horariorieles.ToList())
            {
                horarioRiel.CitaId = null;
                horarioRiel.Disponibilidad = true;
                db.Entry(horarioRiel).State = EntityState.Modified;
            }



            foreach (var horarioRileId in horarioRielesIds)
            {
                var horarioRiel = db.horariorieles.FirstOrDefault(hr => hr.Id == horarioRileId);
                horarioRiel.CitaId = citaId;
                horarioRiel.Disponibilidad = false;
                db.Entry(horarioRiel).State = EntityState.Modified;
            }
            cita.FechaCita = date;
            db.Entry(cita).State = EntityState.Modified;
            db.SaveChanges();

            TempData["FlashSuccess"] = "Cambio de fecha aplicado exitosamente";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS")]
        public ActionResult Penalizaciones(string fechaDesde = null, string fechaHasta = null)
        {

            var dateFechaDesde = DateTime.Today.AddDays(-5);
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

            var citas = db.citas.Where(c => c.EstatusCitaId != null && (dateFechaDesde <= c.FechaCita && dateFechaHasta >=  c.FechaCita))
                .ToList();



            ViewBag.FechaDesde = dateFechaDesde;

            ViewBag.FechaHasta = dateFechaHasta;
            ViewBag.Citas = citas;
         

            return View();
        }


        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS")]
        public void PenalizacionesExportar(string fechaDesde, string fechaHasta)
        {
            var dateFechaDesde  = DateTime.ParseExact(fechaDesde, "dd/MM/yyyy", CultureInfo.InvariantCulture);
      

            var dateFechaHasta = DateTime.ParseExact(fechaHasta, "dd/MM/yyyy", CultureInfo.InvariantCulture);
      

            var db = new Entities();

            var citas = db.citas.Where(c => c.EstatusCitaId != null && (dateFechaDesde <= c.FechaCita && dateFechaHasta >= c.FechaCita))
                .ToList();


            var dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("Fecha");
            dt.Columns.Add("Rieles");
            dt.Columns.Add("Cantidad");
            dt.Columns.Add("RFC Proveedor");
            dt.Columns.Add("Nombre Proveedor");
            dt.Columns.Add("Es Especial");
            dt.Columns.Add("Penalización");



            foreach (var cita in citas)
            {
                dt.Rows.Add(
                    cita.Id,
                    cita.FechaCita.ToString("dd/M/yyyy"),
                    cita.RielesOcupados,
                    cita.CantidadTotal,
                    cita.proveedore.Rfc,
                    cita.proveedore.Nombre1 + " " + cita.proveedore.Nombre2+ " " + cita.proveedore.Nombre3+ " " + cita.proveedore.Nombre4,
                    cita.proveedore.cuenta.EsEspecial ? "SI" : "NO",
                    cita.estatuscita.Nombre);
            }

            FileManager.ExportExcel(dt, "Pen_" + dateFechaDesde.ToString("ddMMyyyy") + "_" + dateFechaHasta.ToString("ddMMyyyy"), HttpContext);
        }
    }
}