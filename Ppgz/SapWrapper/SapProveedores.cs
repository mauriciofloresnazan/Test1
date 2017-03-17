using System.Data;
using SAP.Middleware.Connector;

namespace SapWrapper
{
    public class SapProveedores
    {
        private readonly RfcConfigParameters _rfc = new RfcConfigParameters();

        public SapProveedores()
        {
            // TODO MOVER A LA TABLA DE CONFIGURACIÓN EN LA BASE DE DATOS
            
           /* _rfc.Add(RfcConfigParameters.Name, "QIM");

            _rfc.Add(RfcConfigParameters.AppServerHost, "172.18.3.21");

            _rfc.Add(RfcConfigParameters.User, "USRPORTAL");

            _rfc.Add(RfcConfigParameters.Password, "wspportalp");

            _rfc.Add(RfcConfigParameters.Client, "300");

            _rfc.Add(RfcConfigParameters.SystemNumber, "22");

            _rfc.Add(RfcConfigParameters.Language, "EN");

            _rfc.Add(RfcConfigParameters.PoolSize, "5");

            _rfc.Add(RfcConfigParameters.PeakConnectionsLimit, "35");

            _rfc.Add(RfcConfigParameters.IdleTimeout, "500");*/

            _rfc.Add(RfcConfigParameters.Name, "DIM");

            _rfc.Add(RfcConfigParameters.AppServerHost, "172.18.3.21");

            _rfc.Add(RfcConfigParameters.User, "ABAP_INDRA");

            _rfc.Add(RfcConfigParameters.Password, "extnazan");

            _rfc.Add(RfcConfigParameters.Client, "100");

            _rfc.Add(RfcConfigParameters.SystemNumber, "11");

            _rfc.Add(RfcConfigParameters.Language, "EN");

            _rfc.Add(RfcConfigParameters.PoolSize, "5");

            _rfc.Add(RfcConfigParameters.PeakConnectionsLimit, "35");

            _rfc.Add(RfcConfigParameters.IdleTimeout, "500");
        }

        public DataTable GetProveedores()
        {
            // TODO VALIDAR LOS PARAMETROS 
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;

            // Asignación de la Funcion
            var function = rfcRepository.CreateFunction("ZEXTRAE_PROV");

            // Sociedad de Compras
            //function.SetValue("BUKRS", "1001");
            function.Invoke(rfcDestinationManager);

            var result = function.GetTable("T_PROV");

            return result.ToDataTable("T_PROV");
        }
    }
}
