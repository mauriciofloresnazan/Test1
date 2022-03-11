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
    public class ReenvioAsnController : Controller
    {


    

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARCITAS")]
        public ActionResult Index()
        {
            var db2 = new Entities();
            var f = DateTime.Today;
            var Borrar = db2.citasscal.Where(m => m.FechaCarga <=f).ToList();
            db2.citasscal.RemoveRange(Borrar);
            var Borrarsc = db2.reenvio.Where(m => m.FechaCarga <=f).ToList();
            db2.reenvio.RemoveRange(Borrarsc);
            db2.SaveChanges();
            
                
                var citsca = db2.citasscal.Where(csca=> csca.FechaCarga==f).FirstOrDefault();
                var reensca = db2.reenvio.Where(ren=> ren.FechaCarga==f).FirstOrDefault();
                
                var cit = DbScaleGNZN.GetDataTable(" SELECT * FROM GNZN_citas_abiertas_view");
            foreach (DataRow devolucion in cit.Rows)
            {


                var re = devolucion.ItemArray[0];
                var par = devolucion.ItemArray[1];
                var fe = devolucion.ItemArray[1];
                if (citsca == null)
                {

                    var archivo = new citasscal();
                    archivo.IdCita = Convert.ToInt32(devolucion.ItemArray[0]);
                    archivo.Pares = Convert.ToInt32(devolucion.ItemArray[1]);
                    archivo.FechaCita = Convert.ToDateTime(devolucion.ItemArray[2]);
                    archivo.FechaCarga = f;
                    db2.citasscal.Add(archivo);
                    db2.SaveChanges();


                }
            }
            var citas = db2.citas.Where(rp => rp.FechaCita > f).ToList();
                var ci = db2.citasscal.Where(ce => ce.FechaCita > f).ToList();
                var result1 = citas.Where(p => !ci.Any(p2 => p2.IdCita == p.Id));
                foreach (var Reenvio in result1)
                {

                    if (reensca==null)
                    {
                        var ciscale = new reenvio();
                        ciscale.IdCita = Reenvio.Id;
                        ciscale.Pares = Reenvio.CantidadTotal;
                        ciscale.FechaCita = Reenvio.FechaCita;
                        ciscale.FechaCita = Reenvio.FechaCita;
                        ciscale.Almacen = Reenvio.Almacen;
                        ciscale.FechaCarga = f;

                        db2.reenvio.Add(ciscale);
                        db2.SaveChanges();
                    }

                }
            
            ViewBag.CitasScal = db2.reenvio.Where(csa=>csa.Almacen!= "Sin ASN"&&csa.FechaCarga==f).ToList();

            return View();
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
    }
}