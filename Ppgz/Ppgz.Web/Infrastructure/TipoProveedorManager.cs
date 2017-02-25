using System.Collections.Generic;
using System.Linq;
using Ppgz.Repository;

namespace Ppgz.Web.Infrastructure
{
    public class TipoProveedorManager
    {
        private readonly Entities _db = new Entities();
        public List<tipos_proveedor> FindAll()
        {
            return _db.tipos_proveedor.ToList();

        }
        public tipos_proveedor Find(int id)
        {
            return _db.tipos_proveedor.Find(id);

        }

        public tipos_proveedor GetByCodigo(string codigo)
        {
            return _db.tipos_proveedor.First(t => t.codigo == codigo);
        }
    }
}
