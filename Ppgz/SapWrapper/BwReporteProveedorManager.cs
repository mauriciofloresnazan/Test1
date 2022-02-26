using System;
using System.Data;
using System.Reflection.Emit;
using SAP.Middleware.Connector;

namespace SapWrapper
{
    public class BwReporteProveedorManager
    {
        private readonly BwRfcConfigParam _rfc = new BwRfcConfigParam();

        public DataTable GetReporteProveedor(string codigoProveedor)
        {

            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZRP_REPORTE_PROVEEDORES");
            function.SetValue("IM_VENDOR", codigoProveedor);
            function.Invoke(rfcDestinationManager);

            var result = function.GetTable("ET_DET");

            return result.ToDataTable("ET_DET");
     
        }
    }
}
