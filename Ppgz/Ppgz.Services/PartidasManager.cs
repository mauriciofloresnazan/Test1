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
    }
}
