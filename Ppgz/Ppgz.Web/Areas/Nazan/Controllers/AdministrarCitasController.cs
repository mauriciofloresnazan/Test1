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

namespace Ppgz.Web.Areas.Nazan.Controllers
{
    public class AdministrarCitasController : Controller
    {

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

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS")]
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


        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS")]
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

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS")]
        public ActionResult Enroque(string fecha = null)
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

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> Enroque(int horarioRielId1, int horarioRielId2)
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
                    CitaManager.ActualizarFechaScaleEnroque(Convert.ToInt32(citaId1));
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
                    CitaManager.ActualizarFechaScaleEnroque(Convert.ToInt32(citaId2));
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

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS")]
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


            /*var parameters = new List<MySqlParameter>()
			{
				new MySqlParameter
				{
					ParameterName = "pTotal",
					Direction = ParameterDirection.Output,
					MySqlDbType = MySqlDbType.VarChar
				},
				new MySqlParameter("pFecha", date)
			};

            Db.ExecuteProcedureOut(parameters, "config_appointment");*/


            CrearRieles(date);

            var horarioRieles = db.horariorieles.Where(h => h.Fecha == date.Date).ToList();

            ViewBag.HorarioRieles = horarioRieles;

            return View();
        }


        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> CambiarFecha(int citaId, string fecha, DateTime FechaCreacion, int[] horarioRielesIds)
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
                    return RedirectToAction("CambiarFecha", new { citaId, fecha });
                }
            }


            foreach (var horarioRiel in cita.horariorieles.ToList())
            {
                horarioRiel.CitaId = null;
                horarioRiel.Disponibilidad = true;
                horarioRiel.CantidadTotal = 0;
                db.Entry(horarioRiel).State = EntityState.Modified;
            }



            foreach (var horarioRileId in horarioRielesIds)
            {
                var horarioRiel = db.horariorieles.FirstOrDefault(hr => hr.Id == horarioRileId);
                var cant = cita.CantidadTotal;
                horarioRiel.CitaId = citaId;
                horarioRiel.Disponibilidad = false;
                horarioRiel.CantidadTotal = cant;
                db.Entry(horarioRiel).State = EntityState.Modified;
            }

            cita.FechaCita = date;
            cita.MovimientoCita = FechaCreacion;
            db.Entry(cita).State = EntityState.Modified;
            db.SaveChanges();

            CitaManager.ActualizarFechaScale(citaId);

            var correos = cita.proveedore.cuenta.AspNetUsers.Select(u => u.Email).ToArray();

            var commonManager = new CommonManager();

            foreach (var correo in correos)
            {
                await commonManager.SendHtmlMail(
                     "Modificación de la Cita #" + cita.Id,
                     "La cita #." + cita.Id + " se ha modificado para la fecha " + cita.FechaCita.ToString("dd/MM/yyyy"),
                     correo);
            }



            TempData["FlashSuccess"] = "Cambio de fecha aplicado exitosamente";
            return RedirectToAction("Index");
        }



        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS")]
        public ActionResult CambiarFechaMenor(int citaId, string fecha)
        {

            var db = new Entities();
            var date = DateTime.ParseExact(fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture);

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


            /*var parameters = new List<MySqlParameter>()
			{
				new MySqlParameter
				{
					ParameterName = "pTotal",
					Direction = ParameterDirection.Output,
					MySqlDbType = MySqlDbType.VarChar
				},
				new MySqlParameter("pFecha", date)
			};

            Db.ExecuteProcedureOut(parameters, "config_appointment");*/


            CrearRieles(date);

            var horarioRieles = db.horariorieles.Where(h => h.Fecha == date.Date).ToList();

            ViewBag.HorarioRieles = horarioRieles;


            var hora = db.horariorieles.Where(h => h.Fecha == date.Date && h.TipoCita == "Cita Menor").FirstOrDefault();
            var horaa = db.horariorieles.Where(h => h.Fecha == date.Date && h.TipoCita == "Cita Menor").ToList();
            foreach (var riel in horaa)
            {

                ViewBag.HorarioR = hora;
                var res = CommonManager.GetConfiguraciones().Single(c => c.Clave == "warehouse.platform-rail.max-pair.30min");
                var pa = Convert.ToInt32(res.Valor);
                var ca = db.horariorieles.Where(cit => cit.Fecha == date.Date && cit.TipoCita == "Cita Menor" && cit.Id == riel.Id).FirstOrDefault();
                var an = ca.CantidadTotal + cita.CantidadTotal;
                var blo = horarioRieles.FirstOrDefault(ri => ri.CantidadTotal <= pa && an <= pa && ri.TipoCita == "Cita Menor" && ri.Disponibilidad == false);
                if (blo != null)
                {
                    ViewBag.Ho = blo;
                    return View();
                }
                ViewBag.Ho = blo;
            }


            return View();
        }


        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> CambiarFechaMenor(int citaId, string fecha, DateTime FechaCreacion, int[] horarioRielesIds)
        {


            string connectionString = "server = server=172.22.10.21;user id=impuls_portal;password=7emporal@S;database=impuls_portal; SslMode = none";
            var date = DateTime.ParseExact(fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            var db = new Entities();

            var cita = db.citas.FirstOrDefault(c => c.FechaCita >= DateTime.Today && c.Id == citaId);

            if (cita == null)
            {
                TempData["FlashError"] = "Cita incorrecta";
                return RedirectToAction("Index");
            }



            if (date.Date < DateTime.Today.Date)
            {
                TempData["FlashError"] = "Fecha incorrecta para el cambio";
                return RedirectToAction("IndexM");
            }

            foreach (var horarioRileId in horarioRielesIds)
            {
                var horarioRiel = db.horariorieles.FirstOrDefault(hr => hr.Id == horarioRileId);

                if (horarioRiel == null)
                {
                    TempData["FlashError"] = "Rieles incorrectos";
                    return RedirectToAction("IndexM");
                }

            }

            var citas = db.citas.Where(c => c.Id == citaId).ToList();

            foreach (var pr in citas)
            {
                var conv = Convert.ToInt64(pr.IdRiel);
                var hr = db.horariorieles.Where(ho => ho.Id == conv).FirstOrDefault();
                var c = hr.CantidadTotal; // trae la cantidadTotal de horariorieles
                var horarioRiel = db.citas.Find(pr.Id);
                var pro = horarioRiel.proveedore.Nombre1;
                var ca = horarioRiel.CantidadTotal; // trae la cantidadTotal de citas


                c -= ca;

                if (c == 0)
                {
                    hr.Disponibilidad = true;
                    hr.Citas = null;
                    hr.ComentarioBloqueo = null;
                    hr.CantidadPorCita = null;
                    hr.TipoCita = null;
                    hr.CitaId = null;
                    db.Entry(hr).State = EntityState.Modified;
                    db.SaveChanges();
                }
                hr.CantidadTotal = c;
                db.Entry(hr).State = EntityState.Modified;
                db.SaveChanges();

                MySqlConnection connectione = new MySqlConnection(@connectionString);
                string querys = @"UPDATE horariorieles SET ComentarioBloqueo = REPLACE(ComentarioBloqueo,'" + pro + " ,' ,'')where Id='" + conv + "'; ";
                MySqlCommand commandos = new MySqlCommand(querys, connectione);
                try
                {
                    connectione.Close();
                    connectione.Open();
                    commandos.ExecuteNonQuery();

                }
                catch 
                {

                }
                finally
                {
                    connectione.Close();
                }



                MySqlConnection connection = new MySqlConnection(@connectionString);
                string quer = @"UPDATE horariorieles SET CantidadPorCita = REPLACE(CantidadPorCita,'" + ca + " ,','')where Id='" + conv + "'; ";
                MySqlCommand comman = new MySqlCommand(quer, connection);
                try
                {
                    connection.Close();
                    connection.Open();
                    comman.ExecuteNonQuery();

                }
                catch 
                {

                }
                finally
                {
                    connection.Close();
                }
            }


            MySqlConnection connectiones = new MySqlConnection(@connectionString);
            string query = @"UPDATE horariorieles SET Citas = REPLACE(Citas,'" + citaId + " ,',''); ";
            MySqlCommand commando = new MySqlCommand(query, connectiones);
            try
            {
                connectiones.Close();
                connectiones.Open();
                commando.ExecuteNonQuery();

            }
            catch 
            {

            }
            finally
            {
                connectiones.Close();
            }

            foreach (var horarioRileId in horarioRielesIds)
            {
                var horarioRiel = db.horariorieles.FirstOrDefault(hr => hr.Id == horarioRileId);
                var pro = horarioRiel.ComentarioBloqueo;
                var proveedor = string.Format("{0} {1}{2}",
                                                pro, ",",
                                                cita.proveedore.Nombre1
                                                );

                var canxcita = horarioRiel.CantidadPorCita;
                var cacita = string.Format("{0} {1}{2}",
                                                canxcita, ",",
                                               cita.CantidadTotal);
                var c = horarioRiel.Citas;
                var cit = string.Format("{0} {1}{2}",
                                                c, ",",
                                               cita.Id
                                                );
                var ct = cita.CantidadTotal;
                var ctc = horarioRiel.CantidadTotal;
                ct += ctc;
                horarioRiel.CantidadTotal = ct;
                horarioRiel.CantidadPorCita = cacita;
                horarioRiel.CantidadTotal = ct;
                horarioRiel.Citas = cit;
                horarioRiel.TipoCita = "Cita Menor";
                horarioRiel.ComentarioBloqueo = proveedor;
                horarioRiel.CitaId = citaId;
                horarioRiel.Disponibilidad = false;
                db.Entry(horarioRiel).State = EntityState.Modified;
                var riel = horarioRiel.Id;
                var r = Convert.ToString(riel);
                var hora = horarioRiel.horario.HoraDesde;
                cita.IdRiel = r;
                cita.HoraInicio = hora;
                cita.FechaCita = date;
                cita.MovimientoCita = FechaCreacion;
                db.Entry(cita).State = EntityState.Modified;
                db.SaveChanges();
            }


            CitaManager.ActualizarFechaScale(citaId);

            var correos = cita.proveedore.cuenta.AspNetUsers.Select(u => u.Email).ToArray();

            var commonManager = new CommonManager();

            foreach (var correo in correos)
            {
                await commonManager.SendHtmlMail(
                     "Modificación de la Cita #" + cita.Id,
                     "La cita #." + cita.Id + " se ha modificado para la fecha " + cita.FechaCita.ToString("dd/MM/yyyy"),
                     correo);
            }

            TempData["FlashSuccess"] = "Cambio de fecha aplicado exitosamente";
            return RedirectToAction("IndexM");
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<ActionResult> CambiarFechaMenor_Corregido_20230102(int citaId, string fecha, DateTime FechaCreacion, int[] horarioRielesIds)
        {
            var date = DateTime.ParseExact(fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            var db = new Entities();

            var cita = db.citas.FirstOrDefault(c => c.FechaCita >= DateTime.Today && c.Id == citaId);

            if (cita == null)
            {
                TempData["FlashError"] = "Cita incorrecta";
                return RedirectToAction("Index");
            }

            if (date.Date < DateTime.Today.Date)
            {
                TempData["FlashError"] = "Fecha incorrecta para el cambio";
                return RedirectToAction("IndexM");
            }

            foreach (var horarioRileId in horarioRielesIds)
            {
                var horarioRiel = db.horariorieles.FirstOrDefault(hr => hr.Id == horarioRileId);

                if (horarioRiel == null)
                {
                    TempData["FlashError"] = "Rieles incorrectos";
                    return RedirectToAction("IndexM");
                }

            }

            var citas = db.citas.Where(c => c.Id == citaId).ToList();

            string msg = "";
            foreach (var horarioRileId in horarioRielesIds)
            {
                msg = CitaManager.ActualizaFechaCitaLocal(citaId, horarioRileId, date);
            }
            
            if (msg.Equals("Cita actualizada"))
            {
                CitaManager.ActualizarFechaScale(citaId);
                var correos = cita.proveedore.cuenta.AspNetUsers.Select(u => u.Email).ToArray();

                var commonManager = new CommonManager();

                foreach (var correo in correos)
                {
                    await commonManager.SendHtmlMail(
                         "Modificación de la Cita #" + cita.Id,
                         "La cita #." + cita.Id + " se ha modificado para la fecha " + cita.FechaCita.ToString("dd/MM/yyyy"),
                         correo);
                }

                TempData["FlashSuccess"] = "Cambio de fecha aplicado exitosamente";
                return RedirectToAction("IndexM");
            }
            else
            {
                TempData["FlashError"] = "Error en el cambio - " + msg;
                return RedirectToAction("IndexM");
            }
        }

        public JsonResult VerificarRieles(string fecha)
        {
            var date = DateTime.ParseExact(fecha, "ddMMyyyy", CultureInfo.InvariantCulture);

            var db = new Entities();
            var horarioRieles = db.horariorieles.Where(hr => hr.Fecha == date).ToList();


            return Json(
                horarioRieles.Select(hr => new
                {
                    hr.Id,
                    hr.Disponibilidad
                }));
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

            var citas = db.citas.Where(c => c.EstatusCitaId != null && (dateFechaDesde <= c.FechaCita && dateFechaHasta >= c.FechaCita))
                .ToList();



            ViewBag.FechaDesde = dateFechaDesde;

            ViewBag.FechaHasta = dateFechaHasta;
            ViewBag.Citas = citas;


            return View();
        }


        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS")]
        public void PenalizacionesExportar(string fechaDesde, string fechaHasta)
        {
            var dateFechaDesde = DateTime.ParseExact(fechaDesde, "dd/MM/yyyy", CultureInfo.InvariantCulture);


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
            dt.Columns.Add("Pronto Pago");
            dt.Columns.Add("Penalización");



            foreach (var cita in citas)
            {
                dt.Rows.Add(
                    cita.Id,
                    cita.FechaCita.ToString("dd/M/yyyy"),
                    cita.RielesOcupados,
                    cita.CantidadTotal,
                    cita.proveedore.Rfc,
                    cita.proveedore.Nombre1 + " " + cita.proveedore.Nombre2 + " " + cita.proveedore.Nombre3 + " " + cita.proveedore.Nombre4,
                    cita.proveedore.cuenta.EsEspecial ? "SI" : "NO",
                    cita.estatuscita.Nombre);
            }

            FileManager.ExportExcel(dt, "Pen_" + dateFechaDesde.ToString("ddMMyyyy") + "_" + dateFechaHasta.ToString("ddMMyyyy"), HttpContext);
        }


        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS")]
        public ActionResult DisponibilidadRieles(string fecha = null)
        {
            if (fecha == null)
            {
                fecha = DateTime.Today.Date.ToString("dd/MM/yyyy");
            }
            var date = DateTime.ParseExact(fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            CrearRieles(date);

            var db = new Entities();

            var horarioRieles = db.horariorieles.Where(h => h.Fecha == date).ToList();

            ViewBag.HorarioRieles = horarioRieles;

            ViewBag.Fecha = date.ToString("yyyy/MM/dd");

            return View();
        }


        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult DisponibilidadRieles(int[] horarioRielId, string disponible, string comentario = null)
        {


            var db = new Entities();

            foreach (var riel in horarioRielId)
            {
                var horario = db.horariorieles.FirstOrDefault(hr => hr.Id == riel);
                horario.Disponibilidad = false;
                horario.ComentarioBloqueo = comentario;
                db.Entry(horario).State = EntityState.Modified;
            }

            db.SaveChanges();



            TempData["FlashSuccess"] = "Rieles Bloqueados Correctamente";
            return RedirectToAction("DisponibilidadRieles");

        }



        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS")]
        public ActionResult DisponibilidadRieles1(string fecha = null)
        {
            if (fecha == null)
            {
                fecha = DateTime.Today.Date.ToString("dd/MM/yyyy");
            }
            var date = DateTime.ParseExact(fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            CrearRieles(date);

            var db = new Entities();

            var horarioRieles = db.horariorieles.Where(h => h.Fecha == date).ToList();

            ViewBag.HorarioRieles = horarioRieles;

            ViewBag.Fecha = date.ToString("yyyy/MM/dd");

            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult DisponibilidadRieles1(int[] horarioRielId, string disponible, string comentario = null)
        {
            var db = new Entities();

            foreach (var riel in horarioRielId)
            {
                var horario = db.horariorieles.FirstOrDefault(hr => hr.Id == riel);
                horario.Disponibilidad = true;
                horario.ComentarioBloqueo = null;
                db.Entry(horario).State = EntityState.Modified;
                db.SaveChanges();


            }


            TempData["FlashSuccess"] = "Rieles Desbloqueados Correctamente";
            return RedirectToAction("DisponibilidadRieles1");

        }
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult ReEnvioASN(int idCita)
        {

            var scaleManager = new ScaleManager();

            var db = new Entities();

            var cita = db.citas.Find(idCita);
            scaleManager.Registrar(cita);

            TempData["FlashSuccess"] = "Reenvio de ASN exitoso";
            return RedirectToAction("Index");
        }
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult ReEnvioASNCita(int idCita)
        {

            var scaleManager = new ScaleManagerAsn();

            var db = new Entities();

            var cita = db.citas.Find(idCita);
            //scaleManager.Registrar(cita);
            scaleManager.RegistrarMINE(cita);

            TempData["FlashSuccess"] = "Reenvio de ASN exitoso";
            return RedirectToAction("Index");
        }


    }
}