using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using log4net;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;
using MySql.Data.MySqlClient;
using Ppgz.Repository;
using Ppgz.Web.Models;

namespace Ppgz.Web.Infrastructure
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
		/// <summary> Log para almacenar errores de negocio.</summary>
		public static readonly ILog BusinessLog = LogManager.GetLogger(@"BusinessLog");

		/// <summary> Log para almacenar errores de aplicación.</summary>
		public static readonly ILog ErrorAppLog = LogManager.GetLogger(@"ErrorAppLog");

		/// <summary> Log para ser visualizado en el navegador.</summary>
		public static readonly ILog TraceView = LogManager.GetLogger(@"TraceView");

		private readonly UserManager<ApplicationUser> _applicationUserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
		private readonly Entities _db = new Entities();

		/// <summary> Etiqueta un mensaje de traza de log.</summary>
		/// <param name="tipoMensaje">Indica el tipo de mensaje a etiquetar. (Informativo, Advertencia y Error)</param>
		/// <param name="controller">Nombre del controlador.</param>
		/// <param name="action"> Nombre de la acción llamada.</param>
		/// <param name="parametros"> Petición realizada.</param>
		/// <param name="mensaje">Mensaje previo a etiquetar. </param>
		/// <returns>Mensaje de error etiquetado.</returns>
		public static string BuildMessageLog(TipoMensaje tipoMensaje, string controller, string action, string mensaje, HttpRequestBase parametros = null)
		{
			StringBuilder respuesta = new StringBuilder();
			StringBuilder valores = new StringBuilder();
			//Se construye el mensaje de la traza.
			respuesta.Append(string.Empty + Environment.NewLine);
			respuesta.Append(@"Nombre del Controlador = " + controller + Environment.NewLine);
			respuesta.Append(@"Nombre del método o acción solicitada = " + action + Environment.NewLine);
			if (parametros != null)
			{
				//Detectar donde fué enviado los valores.
				if (parametros.Form.Count > 0)
				{
					for (int i = 1; i < parametros.Form.Count; i++)
					{
						if (i == parametros.Form.Count - 1)
						{
							valores.Append(parametros.Form.Keys[i] + @" = " + parametros.Form[i]);
							break;
						}
						valores.Append(parametros.Form.Keys[i] + @" = " + parametros.Form[i] + @", ");
					}
				}
				if (parametros.QueryString.Count > 0)
				{
					for (int i = 1; i < parametros.QueryString.Count; i++)
					{
						if (i == parametros.Form.Count - 1)
						{
							valores.Append(parametros.QueryString.Keys[i] + @" = " + parametros.QueryString[i]);
							break;
						}
						valores.Append(parametros.QueryString.Keys[i] + @" = " + parametros.QueryString[i] + @", ");
					}
				}
				respuesta.Append(@"Conjunto de parámetros enviados = " + valores.ToString() + Environment.NewLine);
			}
			switch (tipoMensaje)
			{
				case TipoMensaje.Informativo:
					respuesta.Append(@"Tipo de mensaje = Informativo." + Environment.NewLine);
					break;
				case TipoMensaje.Advertencia:
					respuesta.Append(@"Tipo de mensaje = Advertencia." + Environment.NewLine);
					break;
				case TipoMensaje.Error:
					respuesta.Append(@"Tipo de mensaje = Error." + Environment.NewLine);
					break;
			}
			return respuesta.Append(@"Traza detallada: " + Environment.NewLine + mensaje).ToString();
		}

		/// <summary> Escribe en el log de error de la aplicación y en el visor web. </summary>
		/// <param name="mensaje">Mensaje completo del error.</param>
		public static void WriteAppLog(string mensaje, TipoMensaje tipo)
		{
			switch (tipo)
			{
				case TipoMensaje.Informativo:
					ErrorAppLog.Info(mensaje);
					TraceView.Info(mensaje);
					break;
				case TipoMensaje.Advertencia:
					ErrorAppLog.Warn(mensaje);
					TraceView.Warn(mensaje);
					break;
				case TipoMensaje.Error:
					ErrorAppLog.Error(mensaje);
					TraceView.Error(mensaje);
					break;
			}
		}

		/// <summary> Escribe en el log de errores de negocio y en el visor web. </summary>
		/// <param name="mensaje">Mensaje completo del error.</param>
		public static void WriteBusinessLog(string mensaje, TipoMensaje tipo)
		{
			switch (tipo)
			{
				case TipoMensaje.Informativo:
					BusinessLog.Info(mensaje);
					TraceView.Info(mensaje);
					break;
				case TipoMensaje.Advertencia:
					BusinessLog.Warn(mensaje);
					TraceView.Warn(mensaje);
					break;
				case TipoMensaje.Error:
					BusinessLog.Error(mensaje);
					TraceView.Error(mensaje);
					break;
			}
		}

		public cuenta GetCuentaUsuarioAutenticado()
		{
			var usuarioAutenticado = GetUsuarioAutenticado();
			// TODO PASAR A STORE O RECURSO
			return _db.Database.SqlQuery<cuenta>(@"SELECT * FROM cuentas WHERE   id IN (SELECT CuentaId FROM cuentasusuarios WHERE UsuarioId = {0})", usuarioAutenticado.Id).FirstOrDefault();

		}

        public AspNetUser GetUsuarioAutenticado()
		{
			var userName = HttpContext.Current.User.Identity.GetUserName();
            var usuarioAutenticado = _db.AspNetUsers.Single(u => u.UserName == userName);
			return usuarioAutenticado;
		}
		public string HashPassword(string password)
		{
			return _applicationUserManager.PasswordHasher.HashPassword(password);
		}

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


        public void SendHtmlMail(string subject, string body, string toEmail)
        {
           /* var senderMailAddress = new MailAddress(ConfigurationManager.AppSettings["Mail"]);
            var senderMailPassword = ConfigurationManager.AppSettings["MailPass"];
            var senderSmtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpPort"]);
            var senderSmtpServer = ConfigurationManager.AppSettings["Smtp"];
            */

            /*var senderMailAddress = new MailAddress("impuls.ppgz@gmail.com");
            var senderMailPassword = "Venezuela2017";
            var senderSmtpPort = 587;
            var senderSmtpServer = "smtp.gmail.com";
            */
            var senderMailAddress = new MailAddress("impuls@servicioshorizonte.com.ve");
            var senderMailPassword = "impmsh123$$";
            var senderSmtpPort = 25;
            var senderSmtpServer = "mail.servicioshorizonte.com.ve";

            var mailMessage = new MailMessage(
                senderMailAddress.Address,
                toEmail,
                subject,
                body)
            {
                Sender = senderMailAddress,
                IsBodyHtml = true
            };
            
            var smtp = new SmtpClient(senderSmtpServer)
            {
                Credentials = new NetworkCredential(
                    senderMailAddress.Address,
                    senderMailPassword),
                EnableSsl = false,
                Port = senderSmtpPort
            };
            smtp.Send(mailMessage);

            
        }

	}
}