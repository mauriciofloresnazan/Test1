using System;
using System.Data.Entity;
using System.Linq;
using Ppgz.Repository;
using SapWrapper;
using ScaleWrapper;

namespace Ppgz.CitaWrapper
{

	public static class CitaManager
	{

        public static bool ValidarCita(PreCita precita)
        {
            var db = new Entities();
            var organizacionCompras = db.configuraciones.Single(c => c.Clave == "rfc.common.function.param.ekorg.mercaderia").Valor;
                        var sapOrdenCompraManager = new SapOrdenCompraManager();

            var proveedor = db.proveedores.Find(precita.ProveedorId);

            var ordenes  = sapOrdenCompraManager.GetActivasConDetalle(proveedor.NumeroProveedor, organizacionCompras).ToList();
            

            var asnFuturos = db.asns.Where(asnf=> asnf.cita.FechaCita > DateTime.Today && asnf.cita.ProveedorId == precita.ProveedorId).ToList();

            var documentos = precita.Asns.Select(d => d.OrdenNumeroDocumento).Distinct().ToArray();

            var cantidadDiariaLimite = Convert.ToInt32(db.configuraciones.Single(c => c.Clave == "warehouse.max-pairs.per-day").Valor);

            var cantidadAlmacen =
                db.citas.Where(c => c.Almacen == precita.Centro && c.FechaCita == precita.Fecha.Date)
                .Sum(c => c.CantidadTotal);

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



            if (!RulesManager.Regla13(precita.Centro, Convert.ToInt32(cantidadAlmacen), precita.Cantidad))
            {
                var cantidadDisponible = cantidadDiariaLimite - cantidadAlmacen;

                throw new Exception(string.Format("El almacén {0} puede recibir {1} pares para esta fecha, pero la cita tiene {2} pares.", precita.Centro, cantidadDisponible, precita.Cantidad));
            }
            
            //TODO CONTINUAR LA REGLA 14

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

                    var cantidadPedido =
                        orden.Detalles.Where(d => d.NumeroMaterial == material).Sum(s => s.CantidadPedido);

                    var cantidadEntregada =
                        orden.Detalles.Where(d => d.NumeroMaterial == material).Sum(s => s.CantidadEntregada);

                    var cantidadFutura =
                        asnFuturos.Where(af => af.OrdenNumeroDocumento == documento1 && af.NumeroMaterial == material)
                            .Sum(s => s.Cantidad);

                    if (!RulesManager.Regla12(cantidad, cantidadPedido, cantidadEntregada, cantidadFutura))
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
	            throw  new Exception("Uno o más rieles seleccionados ya no estan disponibles,  por favor verifique su selección.");
	        }

            if (!db.horariorieles.Any(hr => precita.HorarioRielesIds.Contains(hr.Id) && hr.Disponibilidad))
            {
                throw new Exception("Selección de rieles incorrecta.");
            }


	        ValidarCita(precita);

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
                RielesOcupados = (sbyte) precita.HorarioRielesIds.Count,
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
                    OrdenNumeroDocumento = asn.OrdenNumeroDocumento
	            });
	        }

	        try
	        {
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

                

	        }
	        catch (Exception exception)
	        {
	            
	            throw new Exception(exception.Message);
	        }
	        try
	        {
                var scaleManager = new ScaleManager();
                scaleManager.Registrar(cita);
	        }
	        catch (Exception exception)
	        {
	            var error = string.Format("Cita registrada exitosamente, con errores al cargar en SCALE:{0}{1}",Environment.NewLine, exception.Message);

	            throw new Exception(error);
	        }

	  
	    }


	    public static void CancelarCita(int citaId)
	    {
            var db = new Entities();
	        var cita = db.citas.Find(citaId);

	        if(!RulesManager.PuedeCancelarCita(cita.FechaCita)) return;

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


	    }

    
    }
}

