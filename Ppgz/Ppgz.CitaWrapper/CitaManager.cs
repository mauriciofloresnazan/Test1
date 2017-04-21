using MySql.Data.MySqlClient;
using Ppgz.CitaWrapper.Entities;
using Ppgz.Repository;
using System;
using System.Collections.Generic;

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
							Value = item.ordenNumeroDocumento
						},
						new MySqlParameter {
								ParameterName = @"param2",
							Value = item.numeroMaterial
						},
						new MySqlParameter {
								ParameterName = @"param3",
							Value = item.nombreMaterial
						},
						new MySqlParameter {
								ParameterName = @"param4",
							Value = item.cantidad
						},
						new MySqlParameter {
								ParameterName = @"param5",
							Value = item.numeroPosicion
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
	}
}

