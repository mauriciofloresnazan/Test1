using System.Collections.Generic;
using System.Linq;
using Ppgz.Repository;

namespace Ppgz.Services
{
    public class CuentaManager
    {
        private readonly PpgzEntities _db = new PpgzEntities();

        public void Add(cuenta cuenta)
        {
            _db.cuentas.Add(cuenta);

            _db.SaveChanges();
        }

        public List<cuenta> FinAll()
        {

            return _db.cuentas.ToList();

        }
    }
}
