using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Ppgz.Repository;
using SapWrapper;
using ScaleWrapper;

namespace Ppgz.CitaWrapper
{
    public class ScaleException : Exception
    {
        public ScaleException(string mensaje)
            : base(mensaje)
        {

        }
    }

    public static class CitaManager
    {

        public static bool ValidarCita(PreCita precita)
        {
            var db = new Entities();
            var organizacionCompras = db.configuraciones.Single(c => c.Clave == "rfc.common.function.param.ekorg.mercaderia").Valor;
            var sapOrdenCompraManager = new SapOrdenCompraManager();

            var proveedor = db.proveedores.Find(precita.ProveedorId);

            var ordenes = sapOrdenCompraManager.GetActivasConDetalle(proveedor.NumeroProveedor, organizacionCompras).ToList();


            var asnFuturos = db.asns.Where(asnf => asnf.cita.FechaCita > DateTime.Today && asnf.cita.ProveedorId == precita.ProveedorId).ToList();

            var documentos = precita.Asns.Select(d => d.OrdenNumeroDocumento).Distinct().ToArray();

            var cantidadDiariaLimite = Convert.ToInt32(db.configuraciones.Single(c => c.Clave == "warehouse.max-pairs.per-day").Valor);
            var cantidadSemanalLimite = Convert.ToInt32(db.configuraciones.Single(c => c.Clave == "warehouse.max-pairs.per-week").Valor);

            if (precita.Centro.ToUpper() == "CROSS DOCK")
            {
                cantidadDiariaLimite = Convert.ToInt32(db.configuraciones.Single(c => c.Clave == "warehouse.crossdock-max-pairs.per-day").Valor);
            }
            var cantidadDelDiaEnAlmacen = 0;
                
            var citasDelDiaEnAlpmacen = db.citas.Where(c => c.Almacen == precita.Centro && c.FechaCita == precita.Fecha.Date).ToList();

            if (citasDelDiaEnAlpmacen.Any())
            {
                cantidadDelDiaEnAlmacen = citasDelDiaEnAlpmacen.Sum(c => c.asns.Sum(asn => asn.Cantidad));
            }

            var fechaDesde = new DateTime();
            var fechaHasta = new DateTime();

            switch (precita.Fecha.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    fechaDesde = precita.Fecha.Date;
                    fechaHasta = precita.Fecha.AddDays(6).Date;
                    break;
                case DayOfWeek.Tuesday:
                    fechaDesde = precita.Fecha.AddDays(-1).Date;
                    fechaHasta = precita.Fecha.AddDays(5).Date;
                    break;
                case DayOfWeek.Wednesday:
                    fechaDesde = precita.Fecha.AddDays(-2).Date;
                    fechaHasta = precita.Fecha.AddDays(4).Date;
                    break;
                case DayOfWeek.Thursday:
                    fechaDesde = precita.Fecha.AddDays(-3).Date;
                    fechaHasta = precita.Fecha.AddDays(3).Date;
                    break;
                case DayOfWeek.Friday:
                    fechaDesde = precita.Fecha.AddDays(-4).Date;
                    fechaHasta = precita.Fecha.AddDays(2).Date;
                    break;
                case DayOfWeek.Saturday:
                    fechaDesde = precita.Fecha.AddDays(-5).Date;
                    fechaHasta = precita.Fecha.AddDays(1).Date;
                    break;
                case DayOfWeek.Sunday:
                    fechaDesde = precita.Fecha.AddDays(-6).Date;
                    fechaHasta = precita.Fecha.Date;
                    break;
            }
            var cantidadDeLaSemanaEnAlmacen = 0;

            var citasDeLaSemanaEnAlmacen = db.citas
                .Where(c => c.Almacen == precita.Centro)
                .Where(c => c.FechaCita >= fechaDesde && c.FechaCita <= fechaHasta)
                .ToList();

            if (citasDeLaSemanaEnAlmacen.Any())
            {
                cantidadDeLaSemanaEnAlmacen = citasDeLaSemanaEnAlmacen.Sum(c => c.asns.Sum(asn => asn.Cantidad));
            }

            
            #region ReglasGenerales
            if (!RulesManager.Regla2(precita.Fecha))
            {
                throw new Exception(string.Format("No puede agendar una cita para mañana a esta hora"));
            }

            if (!RulesManager.Regla4(precita.Fecha))
            {
                throw new Exception(string.Format("No puede agendar una cita para esta fecha"));
            }

            if (!RulesManager.Regla5())
                throw new Exception(string.Format(""));
            if (!RulesManager.Regla6())
                throw new Exception(string.Format(""));
            if (!RulesManager.Regla7())
                throw new Exception(string.Format(""));

            if (!RulesManager.Regla8(precita.Cantidad, precita.HorarioRielesIds.Count))
            {
                throw new Exception(string.Format("Selección incorrecta de rieles"));
            }

            if (!RulesManager.Regla9())
                throw new Exception(string.Format(""));
            if (!RulesManager.Regla10())
                throw new Exception(string.Format(""));
            if (!RulesManager.Regla11())
                throw new Exception(string.Format(""));



            if (!RulesManager.Regla13(precita.Centro, Convert.ToInt32(cantidadDelDiaEnAlmacen), precita.Cantidad))
            {
                var cantidadDisponible = cantidadDiariaLimite - cantidadDelDiaEnAlmacen;

                throw new Exception(string.Format("El almacén {0} puede recibir {1} pares para esta fecha, pero la cita tiene {2} pares.", precita.Centro, cantidadDisponible, precita.Cantidad));
            }


            if (!RulesManager.Regla14(precita.Centro, Convert.ToInt32(cantidadDeLaSemanaEnAlmacen), precita.Cantidad))
            {
                var cantidadDisponible = cantidadSemanalLimite - cantidadDeLaSemanaEnAlmacen;

                throw new Exception(string.Format("El almacén {0} puede recibir {1} pares para esta semana, pero la cita tiene {2} pares.", precita.Centro, cantidadDisponible, precita.Cantidad));
            }


            #endregion ReglasGenerales



            foreach (var documento in documentos)
            {
                var orden = ordenes.FirstOrDefault(o => o.NumeroDocumento == documento);

                if (orden == null)
                {
                    throw new Exception(string.Format("Orden {0} incorrecta", documento));
                }

                if (!RulesManager.Regla3(precita.Fecha, orden.FechaEntrega))
                {
                    throw new Exception(string.Format("Fecha incorrecta para la orden {0}", documento));
                }

                // Para evitar un comportamiento erroneo en el compilador
                var documento1 = documento;
                var materiales = precita.Asns.Where(i => i.OrdenNumeroDocumento == documento1).Select(i => i.NumeroMaterial).ToArray();

                foreach (var material in materiales)
                {

                    var cantidad =
                        precita.Asns.Where(p => p.OrdenNumeroDocumento == documento1 && p.NumeroMaterial == material)
                            .Sum(s => s.Cantidad);

                    var cantidadPermitida =
                        orden.Detalles.Where(d => d.NumeroMaterial == material).Sum(s => s.CantidadPorEntregar);

                    var cantidadFutura =
                        asnFuturos.Where(af => af.OrdenNumeroDocumento == documento1 && af.NumeroMaterial == material)
                            .Sum(s => s.Cantidad);

                    if (!RulesManager.Regla12(cantidad, cantidadPermitida, cantidadFutura))
                    {
                        throw new Exception(string.Format("Candidad incorrecta para la Orden {0}, material {1}", documento1, material));
                    }
                }
            }

            return true;
        }
        
        public static void RegistrarCita(PreCita precita)
        {

            var db = new Entities();

            // Disponibildiad de Rieles
            if (db.horariorieles.Any(hr => precita.HorarioRielesIds.Contains(hr.Id) && hr.Disponibilidad == false))
            {
                throw new Exception("Uno o más rieles seleccionados ya no estan disponibles,  por favor verifique su selección.");
            }

            if (!db.horariorieles.Any(hr => precita.HorarioRielesIds.Contains(hr.Id) && hr.Disponibilidad))
            {
                throw new Exception("Selección de rieles incorrecta.");
            }


            ValidarCita(precita);
            db = new Entities();

            if (db.horariorieles.Any(hr => precita.HorarioRielesIds.Contains(hr.Id) && hr.Disponibilidad == false))
            {
                throw new Exception("Uno o más rieles seleccionados ya no estan disponibles,  por favor verifique su selección.");
            }

            if (!db.horariorieles.Any(hr => precita.HorarioRielesIds.Contains(hr.Id) && hr.Disponibilidad))
            {
                throw new Exception("Selección de rieles incorrecta.");
            }

            var cita = new cita
            {
                FechaCita = precita.Fecha,
                UsuarioIdTx = precita.UsuarioId,
                CantidadTotal = precita.Cantidad,
                ProveedorId = precita.ProveedorId,
                Almacen = precita.Centro,
                RielesOcupados = (sbyte)precita.HorarioRielesIds.Count,
                OperacionTx = "CREATE"
            };

            foreach (var asn in precita.Asns)
            {
                cita.asns.Add(new asn
                {
                    Cantidad = asn.Cantidad,
                    NombreMaterial = asn.NombreMaterial,
                    NumeroMaterial = asn.NumeroMaterial,
                    NumeroPosicion = asn.NumeroPosicion,
                    OrdenNumeroDocumento = asn.OrdenNumeroDocumento,
                    Tienda = asn.Tienda,

                    TiendaOrigen = asn.TiendaOrigen,
                    Precio = asn.Precio,
                    UnidadMedida = asn.UnidadMedida,
                    CantidadPedidoSap = asn.CantidadSolicitada,
                    InOut = asn.InOut,
                    NumeroOrdenSurtido = asn.NumeroSurtido,
                    NumeroMaterial2 = asn.NumeroMaterial2,
                    Centro = asn.Centro
                });
            }


            db.citas.Add(cita);
            db.SaveChanges();

            foreach (var horarioRielId in precita.HorarioRielesIds)
            {

                db = new Entities();
                var horarioRiel = db.horariorieles.Find(horarioRielId);
                horarioRiel.Disponibilidad = false;
                horarioRiel.CitaId = cita.Id;
                db.Entry(horarioRiel).State = EntityState.Modified;
                db.SaveChanges();
            }

            Task.Factory.StartNew(() =>
            {
                var scaleManager = new ScaleManager();

                cita = db.citas.Find(cita.Id);
                scaleManager.Registrar(cita);
              
            });
        }


        public static void RegistrarCitaSinASN(PreCita precita)
        {

            var db = new Entities();

            // Disponibildiad de Rieles
            if (db.horariorieles.Any(hr => precita.HorarioRielesIds.Contains(hr.Id) && hr.Disponibilidad == false))
            {
                throw new Exception("Uno o más rieles seleccionados ya no estan disponibles,  por favor verifique su selección.");
            }

            if (!db.horariorieles.Any(hr => precita.HorarioRielesIds.Contains(hr.Id) && hr.Disponibilidad))
            {
                throw new Exception("Selección de rieles incorrecta.");
            }


            //ValidarCita(precita);
            db = new Entities();

            if (db.horariorieles.Any(hr => precita.HorarioRielesIds.Contains(hr.Id) && hr.Disponibilidad == false))
            {
                throw new Exception("Uno o más rieles seleccionados ya no estan disponibles,  por favor verifique su selección.");
            }

            if (!db.horariorieles.Any(hr => precita.HorarioRielesIds.Contains(hr.Id) && hr.Disponibilidad))
            {
                throw new Exception("Selección de rieles incorrecta.");
            }

            var cita = new cita
            {
                FechaCita = precita.Fecha,
                UsuarioIdTx = precita.UsuarioId,
                CantidadTotal = precita.Cantidad,
                ProveedorId = precita.ProveedorId,
                Almacen = precita.Centro,
                RielesOcupados = (sbyte)precita.HorarioRielesIds.Count,
                OperacionTx = "CREATE"
            };

            //foreach (var asn in precita.Asns)
            //{
            //    cita.asns.Add(new asn
            //    {
            //        Cantidad = asn.Cantidad,
            //        NombreMaterial = asn.NombreMaterial,
            //        NumeroMaterial = asn.NumeroMaterial,
            //        NumeroPosicion = asn.NumeroPosicion,
            //        OrdenNumeroDocumento = asn.OrdenNumeroDocumento,
            //        Tienda = asn.Tienda,

            //        TiendaOrigen = asn.TiendaOrigen,
            //        Precio = asn.Precio,
            //        UnidadMedida = asn.UnidadMedida,
            //        CantidadPedidoSap = asn.CantidadSolicitada,
            //        InOut = asn.InOut,
            //        NumeroOrdenSurtido = asn.NumeroSurtido,
            //        NumeroMaterial2 = asn.NumeroMaterial2,
            //        Centro = asn.Centro
            //    });
            //}


            db.citas.Add(cita);
            db.SaveChanges();

            foreach (var horarioRielId in precita.HorarioRielesIds)
            {

                db = new Entities();
                var horarioRiel = db.horariorieles.Find(horarioRielId);
                horarioRiel.Disponibilidad = false;
                horarioRiel.CitaId = cita.Id;
                db.Entry(horarioRiel).State = EntityState.Modified;
                db.SaveChanges();
            }

            //Task.Factory.StartNew(() =>
            //{
            //    var scaleManager = new ScaleManager();

            //    cita = db.citas.Find(cita.Id);
            //    scaleManager.Registrar(cita);

            //});
        }


        public static void CancelarCita(int citaId)
        {
            var db = new Entities();
            var cita = db.citas.Find(citaId);

            if (!RulesManager.PuedeCancelarCita(cita.FechaCita)) return;

            foreach (var horarioRiel in cita.horariorieles.ToList())
            {
                horarioRiel.CitaId = null;
                horarioRiel.Disponibilidad = true;
                db.Entry(horarioRiel).State = EntityState.Modified;
            }

            cita.asns.ToList().ForEach(asn => db.asns.Remove(asn));

            cita.crs.ToList().ForEach(cr => db.crs.Remove(cr));

            db.Entry(cita).State = EntityState.Deleted;
            db.SaveChanges();

            Task.Factory.StartNew(() =>
            {
                var scaleManager = new ScaleManager();
                scaleManager.Cancelar(citaId);
            });

        }

        public static void ActualizarFechaScale(int citaId)
        {
            Task.Factory.StartNew(() =>
            {
                var scaleManager = new ScaleManager();
                scaleManager.ActualizarFecha(citaId);
            });
        }
        
        public static void ActualizarCantidadScale(int[] asnIds)
        {
            Task.Factory.StartNew(() =>
            {
                var scaleManager = new ScaleManager();
                scaleManager.ActualizarCantidad(asnIds);
            });
        }
    }
}

