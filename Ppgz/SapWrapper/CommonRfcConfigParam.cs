using SAP.Middleware.Connector;

namespace SapWrapper
{
    public class CommonRfcConfigParam : RfcConfigParameters
    {
        
        //TODO PASAR A LA TABLA DE CONFIGURACION
        public CommonRfcConfigParam()
        {
            Add(Name, "QIM");
            Add(AppServerHost, "172.18.3.21");
            Add(User, "USRPORTAL");
            Add(Password, "wspportalp");
            Add(Client, "300");
            Add(SystemNumber, "22");
            Add(Language, "EN");
            Add(PoolSize, "5");
            Add(PeakConnectionsLimit, "35");
            Add(IdleTimeout, "500");
        }
    }
}
