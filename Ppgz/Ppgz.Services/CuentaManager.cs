using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ppgz.Repository;

namespace Ppgz.Services
{
    public class CuentaManager
    {
        public IEnumerable<cuenta> GetAll()
        {

            var repository = new PpgzEntities();

            return repository.cuentas.ToList();

        } 
    }
}
