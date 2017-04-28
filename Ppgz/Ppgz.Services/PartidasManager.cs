using System;
using System.Data;
using SapWrapper;

namespace Ppgz.Services
{
    public class PartidasManager
    {
        public DataSet GetPartidasAbiertas(string numeroProveedor, string sociedad, DateTime fecha)
        {
            var sapPartidaManager = new SapPartidaManager();
            return sapPartidaManager.GetPartidasAbiertas(numeroProveedor, sociedad, fecha);
        }

        public DataSet GetPagos(string numeroProveedor, string sociedad, DateTime fecha)
        {
            var sapPartidaManager = new SapPartidaManager();
            return sapPartidaManager.GetPagos(numeroProveedor, sociedad, fecha);
        }

        public DataSet GetDevoluciones(string numeroProveedor, string sociedad, DateTime fecha)
        {
            var sapPartidaManager = new SapPartidaManager();
            return sapPartidaManager.GetDevoluciones(numeroProveedor, sociedad, fecha);
        }
    }
}
