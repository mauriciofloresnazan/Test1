using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Ppgz.Repository;

namespace Ppgz.Web.Infraestructure
{
    public class CommonManager 
    {
        private readonly PpgzEntities _db = new PpgzEntities();


        public usuario  GetUsuarioAutenticado()
        {
            var userName = HttpContext.Current.User.Identity.GetUserName();
            var usuarioAutenticado = _db.usuarios.Single(u => u.userName == userName);
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

        public void UsuarioCuentaXrefAdd(usuario usuario, int cuentaId )
        {
            const string sql = @"
                        INSERT INTO  usuarios_cuentas_xref (usuario_id, cuenta_id)
                        VALUES ({0},{1})";
            _db.Database.ExecuteSqlCommand(sql, usuario.Id, cuentaId);

            _db.SaveChanges();

        }

        public string HashPassword(string password)
        {
            var store = new UserStore<IdentityUser>(_db);
            var userManager = new UserManager<IdentityUser>(store);
            return userManager.PasswordHasher.HashPassword(password);
        }
    }
}