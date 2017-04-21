using Ppgz.CitaWrapper.Entities;
using Ppgz.Repository;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;

namespace Ppgz.CitaWrapper
{
	/// <summary>Clase gestora de citas.</summary>
	public class CitaManager
	{
		/// <summary>Propiedad de contexto de base de datos.</summary>
		private Repository.Entities db;

		/// <summary>Constructor de la clase.</summary>
		public CitaManager()
		{
			db = new Repository.Entities();
		}

		/// <summary>Método que aplica las reglas de negocio para las citas.</summary>
		/// <param name="cita"></param>
		public void ValidarCita(Citation cita)
		{
		}

		/// <summary>Método que agrega un registro en la tabla cita.</summary>
		/// <param name="value">Objeto cita.</param>
		private void InsertCita(Citation value)
		{
            try
            {

            	ValidarCita(value);
			}
			catch (Exception)
			{
				//TODO: Manejar la excepción.
			}

			cita objCita = new cita
			{
				FechaCita = value.fechaCita,
				Tienda = value.tienda,
				CantidadTotal = value.cantidadTotal,
				ProveedorId = value.proveedorId,
				UsuarioIdTx = value.usuarioId
			};
			db.citas.Add(objCita);
			db.SaveChanges();


            InsertAsn(value.asnItems);
		}

		/// <summary>Método que agrega un registro en la tabla asn.</summary>
		/// <param name="items">Objeto Asn de la cita.</param>
		private void InsertAsn(List<Asn> items)
		{
			foreach (Asn item in items)
			{
				try
				{
					asn objAsn = new asn
					{
						OrdenNumeroDocumento = item.ordenNumeroDocumento,
						NumeroMaterial = item.nombreMaterial,
						Cantidad = item.cantidad,
						NombreMaterial = item.nombreMaterial,
						NumeroPosicion = item.numeroPosicion
					};
					db.asns.Add(objAsn);
					db.SaveChanges();
				}
				catch 
				{
				}
			}
		}
	}
}
