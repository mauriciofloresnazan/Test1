using System.Collections.Generic;
using System.Linq;
using Ppgz.Repository;

namespace Ppgz.Services
{
    public class TipoProveedorManager
    {
        private readonly PpgzEntities _db = new PpgzEntities();
        public List<tipos_proveedor> FindAll()
        {
            return _db.tipos_proveedor.ToList();

        }

        public tipos_proveedor GetByCodigo(string codigo)
        {
            return _db.tipos_proveedor.First(t => t.codigo == codigo);
        }
    }
}
