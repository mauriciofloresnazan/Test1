using System;
using System.Data;
using SAP.Middleware.Connector;

namespace SapWrapper
{
    public class SapProveedorManager
    {
        private readonly CommonRfcConfigParam _rfc = new CommonRfcConfigParam();

        public DataRow GetProveedor(string codigoProveedor)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZEXTRAE_PROV");
            function.SetValue("LIFNR", codigoProveedor);
            function.Invoke(rfcDestinationManager);

            var result = function.GetTable("T_PROV");

            if (result.Count == 1)
            {
                var dt = result.ToDataTable("T_PROV");
                return dt.Rows[0];
            }
            if (result.Count > 1)
            {
                //TODO 
                throw new Exception("Multiples Registros con el mismo codigo de proveedor en SAP");
            }

            return null;
        }

    }
}
