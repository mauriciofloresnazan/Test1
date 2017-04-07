using System.Data;
using SapWrapper;

namespace Ppgz.Services
{
    public class PartidasManager
    {
        public DataSet GetPartidas1001(string numeroProveedor)
        {
            var sapPartidaManager =  new SapPartidaManager();

            return sapPartidaManager.GetPartidas1001(numeroProveedor);
        }
        public DataSet GetPartidas1001SinFecha(string numeroProveedor)
        {
            var sapPartidaManager = new SapPartidaManager();

            return sapPartidaManager.GetPartidas1001SinFecha(numeroProveedor);
        }
        

        public DataSet GetPartidasAbiertas(string numeroProveedor)
        {
            var sapPartidaManager = new SapPartidaManager();
            return sapPartidaManager.GetPartidasAbiertas(numeroProveedor);
        }

        public DataSet GetPagos(string numeroProveedor)
        {
            var sapPartidaManager = new SapPartidaManager();
            return sapPartidaManager.GetPagos(numeroProveedor);
        }

        public DataSet GetDevoluciones(string numeroProveedor)
        {
            var sapPartidaManager = new SapPartidaManager();
            return sapPartidaManager.GetDevoluciones(numeroProveedor);
        }
    }
}
