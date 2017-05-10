using MySql.Data.MySqlClient;
using Ppgz.CitaWrapper.Entities;
using Ppgz.Repository;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Ppgz.CitaWrapper
{

	public static class CitaManager
	{

        public static bool ValidarCita(PreCita precita)
		{
			try
			{
				return true;
			}
			catch (Exception e)
			{
				//TODO: Manejar la excepción.
				return false;
			}
		}



	



	    public static void RegistrarCita(PreCita precita)
	    {

            var db = new Repository.Entities();
 
            // Disponibildiad de Rieles
	        if (db.horariorieles.Any(hr => precita.HorarioRielesIds.Contains(hr.Id) && hr.Disponibilidad == false))
	        {
	            throw  new Exception("Uno o más rieles seleccionados ya no estan disponibles,  por favor verifique su selección.");
	        }

            if (!db.horariorieles.Any(hr => precita.HorarioRielesIds.Contains(hr.Id) && hr.Disponibilidad))
            {
                throw new Exception("Selección de rieles incorrecta.");
            }

            throw new Exception("Cita inválida.");

	        //TODO VALIDAR LA CITA

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

                    db = new Repository.Entities();
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

	  
	    }


	    public static void CancelarCita(int citaId)
	    {
            var db = new Repository.Entities();
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

