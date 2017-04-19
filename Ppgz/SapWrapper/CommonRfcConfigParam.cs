using System.Linq;
using Ppgz.Repository;
using SAP.Middleware.Connector;

namespace SapWrapper
{
    public class CommonRfcConfigParam : RfcConfigParameters
    {

        public CommonRfcConfigParam()
        {
            //TODO PASAR A UN COMPONENTE UNICO
            var db = new Entities();
            var configuraciones = db.configuraciones.ToList();

            Add(Name, configuraciones.Single(co => co.Clave == "rfc.main.name").Valor);
            Add(AppServerHost, configuraciones.Single(co => co.Clave == "rfc.main.appserverhost").Valor);
            Add(User, configuraciones.Single(co => co.Clave == "rfc.main.user").Valor);
            Add(Password, configuraciones.Single(co => co.Clave == "rfc.main.password").Valor);
            Add(Client, configuraciones.Single(co => co.Clave == "rfc.main.client").Valor);
            Add(SystemNumber, configuraciones.Single(co => co.Clave == "rfc.main.systemnumber").Valor);
            Add(Language, configuraciones.Single(co => co.Clave == "rfc.main.language").Valor);
            Add(PoolSize, configuraciones.Single(co => co.Clave == "rfc.main.poolsize").Valor);
            Add(PeakConnectionsLimit, configuraciones.Single(co => co.Clave == "rfc.main.peakconnectionslimit").Valor);
            Add(IdleTimeout, configuraciones.Single(co => co.Clave == "rfc.main.idletimeout").Valor);
        }
    }
}
