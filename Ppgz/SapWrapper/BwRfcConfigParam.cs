using SAP.Middleware.Connector;

namespace SapWrapper
{
    public class BwRfcConfigParam : RfcConfigParameters
    {
        
        //TODO PASAR A LA TABLA DE CONFIGURACION
        public BwRfcConfigParam()
        {
            Add(Name, "QIM");
            Add(AppServerHost, "172.18.3.16");
            Add(User, "USRPORTAL");
            Add(Password, "wspportalp");
            Add(Client, "600");
            Add(SystemNumber, "50");
            Add(Language, "EN");
            Add(PoolSize, "5");
            Add(PeakConnectionsLimit, "35");
            Add(IdleTimeout, "500");
        }
    }
}
