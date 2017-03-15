using System.Linq;
using Ppgz.Repository;

namespace Ppgz.Services
{
    public class OrdenCompraManager
    {
        private readonly Entities _db = new Entities();

        public ordencompra FindActivaByIdAndUsuarioId(int id, string usuarioId)
        {
            return _db.ordencompras.FirstOrDefault(
                o => o.Id == id);
        }
    }
}
