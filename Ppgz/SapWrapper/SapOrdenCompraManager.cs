using System.Data;
using SAP.Middleware.Connector;

namespace SapWrapper
{
    public class SapOrdenCompraManager
    {
        private readonly CommonRfcConfigParam _rfc = new CommonRfcConfigParam();

        public DataTable GetOrdenesDeCompraHeader(string numeroProveedor)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_EKKO_PO");
            function.SetValue("IM_LIFNR", numeroProveedor);
            // TODO CONFIGURAR CUANDO SEA DE SERVICIO
            function.SetValue("IM_EKORG", "OC01"); 
            function.Invoke(rfcDestinationManager);

            var result = function.GetTable("ET_HDR");
            return result.ToDataTable("ET_HDR");
        }
        public DataTable GetOrdenesDeCompraHeader(string numeroDocumento, string numeroProveedor)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_EKKO_PO");
            function.SetValue("IM_EBELN", numeroDocumento);
            function.SetValue("IM_LIFNR", numeroProveedor);
            function.Invoke(rfcDestinationManager);

            var result = function.GetTable("ET_HDR");
            return result.ToDataTable("ET_HDR");
        }
        public DataTable GetOrdenDeCompraDetalle(string documento)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_EKKO_PO");
            function.SetValue("IM_EBELN", documento);
            function.Invoke(rfcDestinationManager);

            //var result = function.GetTable("ET_HDR");
            var result = function.GetTable("ET_DET");
            return result.ToDataTable("ET_HDR");
        }
    }
}
