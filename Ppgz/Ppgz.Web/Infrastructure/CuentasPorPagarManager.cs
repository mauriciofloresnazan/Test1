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
        public List<cuentasxpagar> FindPagosByProveedorId(int id)
        {
            return _db.cuentasxpagars.Where(c => c.ProveedoresId == id && c.Referencia == null).ToList();
        }

        public List<cuentasxpagar> FindPagoDetalleByNumeroCompensacion(string numeroCompensacion)
        {

            return _db.cuentasxpagars.Where(c => c.Referencia == numeroCompensacion).ToList();

            //return _db.cuentasxpagars.Where(c => c.Referencia == numeroCompensacion).ToList();
        }

 
 

    }
}