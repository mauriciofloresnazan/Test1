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

        public DataTable GetOrdenesDeCompraHeader(string numeroProveedor, string organizacionCompras)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_EKKO_PO");
            function.SetValue("IM_LOEKZ", "A");
            function.SetValue("IM_LIFNR", numeroProveedor);

            function.SetValue("IM_EKORG", organizacionCompras);
            function.SetValue("IM_GET_DET", " ");
            function.Invoke(rfcDestinationManager);

            var result = function.GetTable("ET_HDR");
            return result.ToDataTable("ET_HDR");
        }
        public DataTable GetOrdenesDeCompraHeader(string numeroDocumento, string numeroProveedor, string organizacionCompras)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_EKKO_PO");
            function.SetValue("IM_EKORG", organizacionCompras);
            function.SetValue("IM_LOEKZ", "A");
            function.SetValue("IM_GET_DET", " ");
            function.SetValue("IM_EBELN", numeroDocumento);
            function.SetValue("IM_LIFNR", numeroProveedor);
            function.Invoke(rfcDestinationManager);

            var result = function.GetTable("ET_HDR");
            return result.ToDataTable("ET_HDR");
        }
        public DataTable GetOrdenDeCompraDetalle(string documento, string numeroProveedor, string organizacionCompras)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_EKKO_PO");
            function.SetValue("IM_LIFNR", numeroProveedor);
            function.SetValue("IM_EKORG", organizacionCompras);
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


        public List<SapOrdenCompra> GetActivasConDetalle(string numeroProveedor, string organizacionCompras)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_EKKO_PO");

            function.SetValue("IM_LIFNR", numeroProveedor);
            function.SetValue("IM_EKORG", organizacionCompras);
            function.SetValue("IM_LOEKZ", "A");
            function.SetValue("IM_GET_DET", "X");

            function.Invoke(rfcDestinationManager);

            var resultH = function.GetTable("ET_HDR");
            var dtH = resultH.ToDataTable("ET_HDR");

            var resultD = function.GetTable("ET_DET");
            var dtD = resultD.ToDataTable("ET_DET");

            var result = new List<SapOrdenCompra>();
            foreach (DataRow dr in dtH.Rows)
            {
                var sapOrdenCompra = new SapOrdenCompra(dr);

                // Relacion del encabezado del orden con el detalle a traves del número de orden de compra
                var drsDetalle = dtD.Select(string.Format("EBELN = '{0}'", dr["EBELN"]));

                var detalles = drsDetalle.Select(drDetalle => new SapOrdenCompraDetalle(drDetalle)).ToList();

                // Se devuelven las ordenes tienen detalles que no estan marcadas como entrega completa
                sapOrdenCompra.Detalles = detalles.Where(de=> de.EntregaCompleta != "X").ToList();

                if (sapOrdenCompra.Detalles.Any())
                {
                    result.Add(sapOrdenCompra);
                }
            }

            return result;
        }


    }
}
