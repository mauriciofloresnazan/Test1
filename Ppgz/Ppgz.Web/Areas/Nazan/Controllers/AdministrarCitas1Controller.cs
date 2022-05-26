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
using ScaleWrapper;
using Ppgz.Services;
namespace Ppgz.Web.Areas.Nazan.Controllers
{
    public class AdministrarCitas1Controller : Controller
    {
        readonly ProveedorManager _proveedorManager = new ProveedorManager();
        readonly CommonManager _commonManager = new CommonManager();
        internal void CrearRieles(DateTime date)
        {
            var parameters = new List<MySqlParameter>
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
        }
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS,NAZAN-VISTACITASCALIDAD")]
        public ActionResult Index(string fechaFrom, string fechaTo)
        {
            var db = new Entities();
            if (!String.IsNullOrEmpty(fechaFrom) && !String.IsNullOrEmpty(fechaTo))
            {
                var fechaf = DateTime.ParseExact(fechaFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                var fechat = DateTime.ParseExact(fechaTo, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                ViewBag.Citas = db.citas.Where(c => c.FechaCita >= fechaf && c.FechaCita <= fechat && c.TipoCita != "Cita Menor").ToList();
            }
            else
            {
                var fecha = DateTime.Today;
                ViewBag.Citas = db.citas.Where(c => c.FechaCita == fecha && c.TipoCita != "Cita Menor").ToList();
            }

            ViewBag.EstatusCita = db.estatuscitas.ToList();

            return View();
        }


        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS,NAZAN-VISTACITASCALIDAD")]
        public ActionResult IndexM(string fechaFrom, string fechaTo)
        {
            var db = new Entities();
            if (!String.IsNullOrEmpty(fechaFrom) && !String.IsNullOrEmpty(fechaTo))
            {
                var fechaf = DateTime.ParseExact(fechaFrom, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                var fechat = DateTime.ParseExact(fechaTo, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                ViewBag.Citas = db.citas.Where(c => c.FechaCita >= fechaf && c.FechaCita <= fechat && c.TipoCita == "Cita Menor").ToList();
            }
            else
            {
                var fecha = DateTime.Today;
                ViewBag.Citas = db.citas.Where(c => c.FechaCita == fecha && c.TipoCita == "Cita Menor").ToList();
            }

            ViewBag.EstatusCita = db.estatuscitas.ToList();

            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-VISTACITASCALIDAD")]
        public ActionResult CitaDetalle(int citaId)
        {


            var db = new Entities();
            var cita = db.citas.FirstOrDefault(c => c.Id == citaId);

            if (cita == null)
            {
                //TODO
                TempData["FlashError"] = "Cita incorrecta";
                return RedirectToAction("Citas");
            }

            ViewBag.Cita = cita;

            var resss = Db.GetDataTable(@"SELECT distinct (min(SUBSTRING_INDEX(NombreMaterial, ',', 1))) as NombreMaterial from Impuls_Portal.asn where CitaId='"+citaId+"'   group by NombreMaterial");


            ViewBag.Resss = resss;
            return View();
        }
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-VISTACITASCALIDAD")]
        public ActionResult Enroque1(string fecha = null)
        {
            if (fecha == null)
            {
                fecha = DateTime.Today.Date.ToString("dd/MM/yyyy");

            }

            var date = DateTime.ParseExact(fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            CrearRieles(date);

            var db = new Entities();

            var horarioRieles = db.horariorieles.Where(h => h.Fecha == date).ToList();

            var total = db.citas.Where(c => c.FechaCita == date).ToList();

            var sum = total.Sum(s => s.CantidadTotal);

            ViewBag.Total = sum;
            var capacidadRiel = CommonManager.GetConfiguraciones().Single(c => c.Clave == "warehouse.platform-rail.max-pair.30min");

            int rielcap = 0;
            int.TryParse(capacidadRiel.Valor, out rielcap);

            ViewBag.CapacidadRiel = rielcap;
            ViewBag.HorarioRieles = horarioRieles;
            ViewBag.Fecha = date.ToString("yyyy/MM/dd");

            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-VISTACITASCALIDAD")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> Enroque1(int horarioRielId1, int horarioRielId2)
        {
            var db = new Entities();

            var horarioRiel1 = db.horariorieles.Find(horarioRielId1);
            var citaId1 = horarioRiel1.CitaId;
            var disponibilidad1 = horarioRiel1.Disponibilidad;
            var comentario1 = horarioRiel1.ComentarioBloqueo;

            var horarioRiel2 = db.horariorieles.Find(horarioRielId2);
            var citaId2 = horarioRiel2.CitaId;
            var disponibilidad2 = horarioRiel2.Disponibilidad;
            var comentario2 = horarioRiel2.ComentarioBloqueo;


            horarioRiel1.CitaId = citaId2;
            horarioRiel1.Disponibilidad = disponibilidad2;
            horarioRiel1.ComentarioBloqueo = comentario2;
            db.Entry(horarioRiel1).State = EntityState.Modified;

            horarioRiel2.CitaId = citaId1;
            horarioRiel2.Disponibilidad = disponibilidad1;
            horarioRiel2.ComentarioBloqueo = comentario1;
            db.Entry(horarioRiel2).State = EntityState.Modified;

            try
            {
                db.SaveChanges();


                if (citaId1 != null)
                {
                    var cita = db.citas.Find(citaId1);

                    var horarioAnterior = string.Format("NUEVO: Anden {0} Riel {1} Horario {2} - {3}",
                        horarioRiel2.riele.andene.Anden,
                        horarioRiel2.riele.Riel,
                        horarioRiel2.horario.HoraDesde,
                        horarioRiel2.horario.HoraHasta);

                    var horarioNuevo = string.Format("ANTERIOR: Anden {0} Riel {1} Horario {2} - {3}",
                        horarioRiel1.riele.andene.Anden,
                        horarioRiel1.riele.Riel,
                        horarioRiel1.horario.HoraDesde,
                        horarioRiel1.horario.HoraHasta);

                    var correos = cita.proveedore.cuenta.AspNetUsers.Select(u => u.Email).ToArray();

                    var commonManager = new CommonManager();

                    foreach (var correo in correos)
                    {
                        await commonManager.SendHtmlMail(
                             "Modificación de la Cita #" + cita.Id,
                             "Se ha modificado Cita #" + cita.Id + " reservada para el día " + cita.FechaCita.ToString("dd/MM/yyyy")
                             + ".<br><br>" + horarioAnterior
                             + ".<br>" + horarioNuevo,
                             correo);
                    }
                }
                if (citaId2 != null)
                {
                    var cita = db.citas.Find(citaId2);

                    var horarioNuevo = string.Format("(ANTERIOR: Anden {0} Riel {1} Horario {2} - {3}",
                        horarioRiel2.riele.andene.Anden,
                        horarioRiel2.riele.Riel,
                        horarioRiel2.horario.HoraDesde,
                        horarioRiel2.horario.HoraHasta);

                    var horarioAnterior = string.Format("NUEVO: Anden {0} Riel {1} Horario {2} - {3}",
                        horarioRiel1.riele.andene.Anden,
                        horarioRiel1.riele.Riel,
                        horarioRiel1.horario.HoraDesde,
                        horarioRiel1.horario.HoraHasta);

                    var correos = cita.proveedore.cuenta.AspNetUsers.Select(u => u.Email).ToArray();

                    var commonManager = new CommonManager();

                    foreach (var correo in correos)
                    {
                        await commonManager.SendHtmlMail(
                             "Modificación de la Cita #" + cita.Id,
                             "Se ha modificado Cita #" + cita.Id + " reservada para el día " + cita.FechaCita.ToString("dd/MM/yyyy")
                             + ".<br><br>" + horarioAnterior
                             + ".<br>" + horarioNuevo,
                             correo);
                    }
                }



                TempData["FlashSuccess"] = "Enroque aplicado exitosamente";
                return RedirectToAction("Enroque", new { fecha = horarioRiel1.Fecha.ToString("dd/MM/yyyy"), Area = "Nazan" });
            }
            catch (Exception exception)
            {

                TempData["FlashError"] = exception.Message;
                return RedirectToAction("Enroque", new { fecha = horarioRiel1.Fecha.ToString("dd/MM/yyyy"), Area = "Nazan" });
            }
        }


    }
}