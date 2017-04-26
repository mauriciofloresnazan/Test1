using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
            function.SetValue("IM_LOEKZ", "A");
            function.SetValue("IM_LIFNR", numeroProveedor);
            // TODO CONFIGURAR CUANDO SEA DE SERVICIO
            function.SetValue("IM_EKORG", "OC01");
            function.SetValue("IM_GET_DET", " ");
            function.Invoke(rfcDestinationManager);

            var result = function.GetTable("ET_HDR");
            return result.ToDataTable("ET_HDR");
        }
        public DataTable GetOrdenesDeCompraHeader(string numeroDocumento, string numeroProveedor)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_EKKO_PO");
            function.SetValue("IM_EKORG", "OC01");
            function.SetValue("IM_LOEKZ", "A");
            function.SetValue("IM_GET_DET", " ");
            function.SetValue("IM_EBELN", numeroDocumento);
            function.SetValue("IM_LIFNR", numeroProveedor);
            function.Invoke(rfcDestinationManager);

            var result = function.GetTable("ET_HDR");
            return result.ToDataTable("ET_HDR");
        }
        public DataTable GetOrdenDeCompraDetalle(string documento, string numeroProveedor)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_EKKO_PO");
            function.SetValue("IM_LIFNR", numeroProveedor);
            function.SetValue("IM_EKORG", "OC01");
            function.SetValue("IM_LOEKZ", "A");
            function.SetValue("IM_EBELN", documento);
            function.SetValue("IM_GET_DET", "X");

            function.Invoke(rfcDestinationManager);
            var result = function.GetTable("ET_DET");
            return result.ToDataTable("ET_DET");
        }

        public List<SapOrdenCompra> GetActivasSinDetalle(string numeroProveedor, string organizacionCompras)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_EKKO_PO");

            function.SetValue("IM_LIFNR", numeroProveedor);
            function.SetValue("IM_EKORG", organizacionCompras);
            function.SetValue("IM_LOEKZ", "A");
            function.SetValue("IM_GET_DET", " ");

            function.Invoke(rfcDestinationManager);

            var result = function.GetTable("ET_HDR");
            var dt = result.ToDataTable("ET_HDR");

            return (from DataRow dr in dt.Rows select new SapOrdenCompra(dr)).ToList();
        }

        public SapOrdenCompra GetActivaSinDetalle(string numeroProveedor, string organizacionCompras, string numeroDocumento)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_EKKO_PO");

            function.SetValue("IM_LIFNR", numeroProveedor);
            function.SetValue("IM_EKORG", organizacionCompras);
            function.SetValue("IM_LOEKZ", "A");
            function.SetValue("IM_EBELN", numeroDocumento);
            function.SetValue("IM_GET_DET", " ");

            function.Invoke(rfcDestinationManager);

            var result = function.GetTable("ET_HDR");
            var dt = result.ToDataTable("ET_HDR");

            if (dt.Rows.Count == 0)
            {
                return null;
            }
            if (dt.Rows.Count > 1)
            {
                throw new Exception("Ordenes repetidas con el mismo número de documento");
            }
           
            return new SapOrdenCompra(dt.Rows[0]);
        }


        public List<SapOrdenCompraDetalle> GetDetalle(string numeroProveedor, string organizacionCompras, string numeroDocumento)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_EKKO_PO");

            function.SetValue("IM_LIFNR", numeroProveedor);
            function.SetValue("IM_EKORG", organizacionCompras);
            function.SetValue("IM_LOEKZ", "A");
            function.SetValue("IM_EBELN", numeroDocumento);
            function.SetValue("IM_GET_DET", "X");

            function.Invoke(rfcDestinationManager);

            var result = function.GetTable("ET_DET");
            var dt = result.ToDataTable("ET_DET");

            return (from DataRow dr in dt.Rows select new SapOrdenCompraDetalle(dr)).ToList();
        } 
    
    }
}
