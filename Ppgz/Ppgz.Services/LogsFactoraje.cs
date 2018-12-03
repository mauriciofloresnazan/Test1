using Ppgz.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ppgz.Services
{
    public class LogsFactoraje
    {
        private readonly Entities _db = new Entities();

        public void InsertLog(string usuario, string operacion, int objeto, string descripcion)
        {
            logfactoraje lf = new logfactoraje()
            {
                Fecha = System.DateTime.Now,
                Usuario = usuario,
                Operacion = operacion,
                IdObjeto = objeto,
                Descripcion = descripcion
            };

            _db.logfactoraje.Add(lf);
            _db.SaveChanges();
        }
    }
}
