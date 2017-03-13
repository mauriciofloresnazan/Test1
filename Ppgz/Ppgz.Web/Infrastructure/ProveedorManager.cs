using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Ppgz;
using Ppgz.Repository;

namespace Ppgz.Web.Infrastructure
{
    public class ProveedorManager
    {
        private readonly Entities _db = new Entities();

        public List<proveedore> FindByCuentaId(int cuentaId)
        {
            return _db.proveedores.Where(p => p.CuentaId == cuentaId).ToList();
        }

        public proveedore Find(int id)
        {
            return _db.proveedores.Find(id);
        }

        public proveedore FindByCodigoProveedor(string codigoProveedor)
        {
            return _db.proveedores.SingleOrDefault(a => a.CodigoProveedor == codigoProveedor);
        }

    }
}