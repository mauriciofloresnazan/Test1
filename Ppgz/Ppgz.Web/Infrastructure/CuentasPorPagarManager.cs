using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ppgz.Repository;

namespace Ppgz.Web.Infrastructure
{
    public class CuentasPorPagarManager
    {
    
        private readonly Entities _db = new Entities();
        public List<dynamic> FindPagosByProveedorId(int id)
        {
            return null;
        }

        public List<dynamic> FindPagoDetalleByNumeroCompensacion(string numeroCompensacion)
        {

            return null;

            //return _db.cuentasxpagars.Where(c => c.Referencia == numeroCompensacion).ToList();
        }

 
 

    }
}