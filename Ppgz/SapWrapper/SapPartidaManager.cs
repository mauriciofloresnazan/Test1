using System;
using System.Data;
using SAP.Middleware.Connector;

namespace SapWrapper
{
    public class SapPartidaManager
    {
        private readonly CommonRfcConfigParam _rfc = new CommonRfcConfigParam();

        public DataTable GetPatidasAbiertas(string codigoProveedor)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZEXTRAE_PART_PROV");
            function.SetValue("LIFNR", codigoProveedor);
            function.SetValue("BUKRS", "100");
            function.SetValue("FECHA_INICIO", "20160101");
            function.Invoke(rfcDestinationManager);

            var result = function.GetTable("T_PARTIDAS_ABIERTAS");
            
            return  result.ToDataTable("T_PARTIDAS_ABIERTAS");
          
        }

    }
}
