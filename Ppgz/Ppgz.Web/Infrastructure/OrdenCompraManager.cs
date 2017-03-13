using System.Collections.Generic;
using System.Linq;
using Ppgz.Repository;

namespace Ppgz.Web.Infrastructure
{
    public class OrdenCompraManager
    {
        private readonly Entities _db = new Entities();
        public List<ordencompra> FindByProveedorId(int id )
        {
            return _db.ordencompras.Where(o => o.ProveedoresId == id).ToList();

        } 
    }
}