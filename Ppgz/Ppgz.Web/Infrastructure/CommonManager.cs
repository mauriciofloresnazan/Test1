using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using MySql.Data.MySqlClient;
using Ppgz.Repository;
using Ppgz.Web.Models;

namespace Ppgz.Web.Infrastructure
{
    public class CommonManager 
    {
        private readonly Entities _db = new Entities();
        private readonly UserManager<ApplicationUser> _applicationUserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
       

        public aspnetuser  GetUsuarioAutenticado()
        {
            var userName = HttpContext.Current.User.Identity.GetUserName();
            var usuarioAutenticado = _db.aspnetusers.Single(u => u.UserName == userName);
            return usuarioAutenticado;
        }



        public cuenta GetCuentaUsuarioAutenticado()
        {
            var usuarioAutenticado =  GetUsuarioAutenticado();

            return _db.Database.SqlQuery<cuenta>(@"
                SELECT  * 
                FROM    cuentas
                WHERE   id IN (SELECT cuenta_id 
                               FROM usuarios_cuentas_xref 
                               WHERE usuario_id = {0})", usuarioAutenticado.Id).FirstOrDefault();

            

            return usuarioAutenticado.cuentas.First();

        




        }

        public void UsuarioCuentaXrefAdd(string usuarioId, int cuentaId )
        {
            const string sql = @"
                        INSERT INTO  usuarios_cuentas_xref (usuario_id, cuenta_id)
                        VALUES ({0},{1})";
            _db.Database.ExecuteSqlCommand(sql, usuarioId, cuentaId);

            _db.SaveChanges();

        }

        public string HashPassword(string password)
        {
            return  _applicationUserManager.PasswordHasher.HashPassword(password);
        }



        public DataTable QueryToTable(string queryText, SqlParameter[] parametes = null)
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