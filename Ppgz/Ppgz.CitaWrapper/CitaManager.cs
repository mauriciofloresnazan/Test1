using MySql.Data.MySqlClient;
using Ppgz.CitaWrapper.Entities;
using Ppgz.Repository;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Ppgz.CitaWrapper
{
	/// <summary>Clase gestora de citas.</summary>
	public static class CitaManager
	{
		/// <summary>Método que aplica las reglas de negocio para las citas.</summary>
		/// <param name="cita"></param>
		public static bool ValidarCita(Citation cita)
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

		/// <summary>Método que agrega un registro en la tabla cita.</summary>
		/// <param name="value">Objeto cita.</param>
		public static bool InsertCita(Citation value)
		{
			try
			{
				string queryCitas = @"INSERT INTO citas (FechaCita, Tienda, CantidadTotal, ProveedorId, UsuarioIdTx) VALUES (@param1, @param2, @param3, @param4, @param5); ";
				List<MySqlParameter> valuesCitas = new List<MySqlParameter>
				{
					new MySqlParameter {
						 ParameterName = @"param1",
						Value = Convert.ToDateTime(value.fechaCita).ToString(@"yyyy-MM-dd")
					},
					new MySqlParameter {
						 ParameterName = @"param2",
						Value = value.tienda
					},
					new MySqlParameter {
						 ParameterName = @"param3",
						Value = value.cantidadTotal
					},
					new MySqlParameter {
						 ParameterName = @"param4",
						Value = value.proveedorId
					},
					new MySqlParameter {
						 ParameterName = @"param5",
						Value = value.usuarioId
					}
				};
				Db.Insert(queryCitas, valuesCitas);
				return true;
			}
			catch (Exception e)
			{
				//TODO: Manejar la excepción.
				return false;
			}
		}

		/// <summary>Método que agrega un registro en la tabla asn.</summary>
		/// <param name="items">Objeto Asn de la cita.</param>
		public static bool InsertAsn(List<Asn> items)
		{
			try
			{
				string queryAsn = @"INSERT INTO asn (OrdenNumeroDocumento, NumeroMaterial, NombreMaterial, Cantidad, NumeroPosicion) VALUES (@param1, @param2, @param3, @param4, @param5);";
				foreach (Asn item in items)
				{
					List<MySqlParameter> itemAsn = new List<MySqlParameter> {
						new MySqlParameter {
								ParameterName = @"param1",
							Value = item.OrdenNumeroDocumento
						},
						new MySqlParameter {
								ParameterName = @"param2",
							Value = item.NumeroMaterial
						},
						new MySqlParameter {
								ParameterName = @"param3",
							Value = item.NombreMaterial
						},
						new MySqlParameter {
								ParameterName = @"param4",
							Value = item.Cantidad
						},
						new MySqlParameter {
								ParameterName = @"param5",
							Value = item.NumeroPosicion
						}
					};
					Db.Insert(queryAsn, itemAsn);
				}
				return true;
			}
			catch (Exception e)
			{
				//TODO: Manejar la excepción
				return false;
			}
		}




	    public static void RegistrarCita(PreCita precita)
	    {
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
                var db = new Repository.Entities();
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


            db.Entry(cita).State = EntityState.Deleted;
            db.SaveChanges();


	    }

    
    }
}

