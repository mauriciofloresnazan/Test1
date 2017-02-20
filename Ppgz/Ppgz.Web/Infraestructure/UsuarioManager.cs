using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Ppgz.Repository;

namespace Ppgz.Web.Infraestructure
{
    public class UsuarioManager
    {
        private readonly PpgzEntities _db = new PpgzEntities();
        public void Add(usuario usuario, string password)
        {

            // Encriptacion del password
            var store = new UserStore<IdentityUser>(_db);
            var userManager = new UserManager<IdentityUser>(store);
            var passwordHash = userManager.PasswordHasher.HashPassword(password);
            usuario.PasswordHash = passwordHash;

            // Normalizacion del Nombre de usuario y email
            usuario.userName = usuario.userName.ToLower().Trim();
            usuario.email = usuario.email.ToLower().Trim();

            // TODO SECURITY STAMP
            usuario.SecurityStamp = string.Empty;
            
            _db.usuarios.Add(usuario);
            _db.SaveChanges();
        }

        public usuario FindUsuarioByUserName(string username)
        {
            return _db.usuarios.FirstOrDefault(u => u.userName == username);
        }

 

    }
}
