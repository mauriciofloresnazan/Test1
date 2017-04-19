using System.Linq;
using Ppgz.Repository;
using SAP.Middleware.Connector;

namespace SapWrapper
{
    public class BwRfcConfigParam : RfcConfigParameters
    {
        

        public BwRfcConfigParam()
        {
            //TODO PASAR A UN COMPONENTE UNICO
            var db = new Entities();
            var configuraciones = db.configuraciones.ToList();

            Add(Name, configuraciones.Single(co => co.Clave == "rfc.bw.name").Valor);
            Add(AppServerHost, configuraciones.Single(co => co.Clave == "rfc.bw.appserverhost").Valor);
            Add(User, configuraciones.Single(co => co.Clave == "rfc.bw.user").Valor);
            Add(Password, configuraciones.Single(co => co.Clave == "rfc.bw.password").Valor);
            Add(Client, configuraciones.Single(co => co.Clave == "rfc.bw.client").Valor);
            Add(SystemNumber, configuraciones.Single(co => co.Clave == "rfc.bw.systemnumber").Valor);
            Add(Language, configuraciones.Single(co => co.Clave == "rfc.bw.language").Valor);
            Add(PoolSize, configuraciones.Single(co => co.Clave == "rfc.bw.poolsize").Valor);
            Add(PeakConnectionsLimit, configuraciones.Single(co => co.Clave == "rfc.bw.peakconnectionslimit").Valor);
            Add(IdleTimeout, configuraciones.Single(co => co.Clave == "rfc.bw.idletimeout").Valor);
        }
    }
}
