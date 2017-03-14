using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using MySql.Data.MySqlClient;
using Ppgz.Repository;

namespace Ppgz.Services
{
	/// <summary> Indica los tipos de mensajes manejados para la aplicación.</summary>
	public enum TipoMensaje
	{
		Informativo,
		Advertencia,
		Error
	}

	/// <summary>
	/// Manejadores comunes de la aplicacion que no estan separados en areas de negocio 
	/// </summary>
	public class CommonManager
	{
	

		private readonly Entities _db = new Entities();



	




		internal void ActualizarPermisosByPefilId(int perfilId)
		{
			const string sql = @"

						DELETE FROM AspNetUserRoles 
						WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE PerfilId = {0});
						INSERT INTO AspNetUserRoles 
							(UserId, RoleId)
						SELECT u.Id, r.RoleId
						FROM   AspNetUsers u
						JOIN   perfilesroles r ON r.PerfilId = {0}
						WHERE  u.PerfIlId = {0};";

			_db.Database.ExecuteSqlCommand(sql, perfilId);
			_db.SaveChanges();

		}

		public DataTable QueryToTable(string queryText, MySqlParameter[] parametes = null)
		{
			using (DbDataAdapter adapter = new MySqlDataAdapter())
			{
				adapter.SelectCommand = _db.Database.Connection.CreateCommand();
				adapter.SelectCommand.CommandText = queryText;
				if (parametes != null)
					adapter.SelectCommand.Parameters.AddRange(parametes);
				var table = new DataTable();
				adapter.Fill(table);
				return table;
			}
		}

	}
}