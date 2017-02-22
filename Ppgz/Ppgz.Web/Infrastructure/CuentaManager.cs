using System;
using System.Collections.Generic;
using System.Linq;

namespace Ppgz.Web.Infrastructure
{
    public class CuentaManager
    {
        private readonly Entities _db = new Entities();

        public void Add(cuenta cuenta)
        {
            cuenta.codigo_proveedor = Guid.NewGuid().ToString("D");

            _db.cuentas.Add(cuenta);

            _db.SaveChanges();
        }

        public List<cuenta> FinAll()
        {
            return _db.cuentas.ToList();
        }
        public cuenta Find(int id)
        {
            return _db.cuentas.Find(id);
        }
    }
}
