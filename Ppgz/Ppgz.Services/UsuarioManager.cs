using System.Linq;
using Ppgz.Repository;

namespace Ppgz.Services
{
    public class UsuarioManager
    {
        private readonly PpgzEntities _db = new PpgzEntities();
        public void Add(usuario usuario)
        {
            _db.usuarios.Add(usuario);

            _db.SaveChanges();
        }

        public usuario FindUsuarioByUserName(string username)
        {
            return _db.usuarios.FirstOrDefault(u => u.userName == username);
        }

 

    }
}
