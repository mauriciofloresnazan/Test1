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

            function.SetValue("IM_AEDAT", "20150101");

            try
            {
                function.Invoke(rfcDestinationManager);
                var result = function.GetTable("ET_HDR");
                return result.ToDataTable("ET_HDR");
            }
            catch
            {
                return new DataTable();
            }
        }

        public int GetCantidadValidacionSAP(string ano, string sociedad, string referencia)
        {
            //var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            //var rfcRepository = rfcDestinationManager.Repository;
            //var function = rfcRepository.CreateFunction("ZMF_PARES_ACUMULADO");
            //function.SetValue("PI_MJAHR", ano);
            //function.SetValue("PI_KOKRS", sociedad);
            //function.SetValue("PI_XBLNR", referencia);


            //function.Invoke(rfcDestinationManager);
            //var result = function.GetTable("PE_ERFMG");
            //result.ToDataTable("PE_ERFMG");
            //var noe= result.ToDataTable("PE_ERFMG");
            return 3;
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
            function.SetValue("IM_AEDAT", "20150101");


            try
            {
                function.Invoke(rfcDestinationManager);
                var result = function.GetTable("ET_HDR");
                return result.ToDataTable("ET_HDR");
            }
            catch
            {
                return new DataTable();
            }
        }

        public DataTable GetOrdenDeCompraDetalle(string documento, string numeroProveedor, string organizacionCompras)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_EKKO_PO");
            function.SetValue("IM_LIFNR", numeroProveedor);
            function.SetValue("IM_AEDAT", "20150101");
            function.SetValue("IM_EKORG", organizacionCompras);
            function.SetValue("IM_LOEKZ", "A");
            function.SetValue("IM_EBELN", documento);
            function.SetValue("IM_GET_DET", "X");

            try
            {
                function.Invoke(rfcDestinationManager);
                var result = function.GetTable("ET_HDR");
                return result.ToDataTable("ET_HDR");
            }
            catch
            {
                return new DataTable();
            }
        }

        public List<SapOrdenCompra> GetActivasConDetalle(string numeroProveedor, string organizacionCompras)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_EKKO_PO");

            function.SetValue("IM_LIFNR", numeroProveedor);
            function.SetValue("IM_EKORG", organizacionCompras);
            function.SetValue("IM_LOEKZ", "A");
            function.SetValue("IM_AEDAT", "20150101");
            function.SetValue("IM_GET_DET", "X");
            var dtH = new DataTable();
            var dtD = new DataTable();
            try
            {
                function.Invoke(rfcDestinationManager);
                var resultH = function.GetTable("ET_HDR");
                dtH = resultH.ToDataTable("ET_HDR");

                var resultD = function.GetTable("ET_DET");
                dtD = resultD.ToDataTable("ET_DET");

            }
            catch
            {

            }



            var result = new List<SapOrdenCompra>();
            foreach (DataRow dr in dtH.Rows)
            {
                var sapOrdenCompra = new SapOrdenCompra(dr);

                // Relacion del encabezado del orden con el detalle a traves del número de orden de compra
                var drsDetalle = dtD.Select(string.Format("EBELN = '{0}'", dr["EBELN"]));

                var detalles = drsDetalle.Select(drDetalle => new SapOrdenCompraDetalle(drDetalle)).ToList();

                // Se devuelven las ordenes tienen detalles que no estan marcadas como entrega completa
                sapOrdenCompra.Detalles = detalles.Where(de => de.EntregaCompleta != "X").ToList();

                if (sapOrdenCompra.Detalles.Any())
                {
                    result.Add(sapOrdenCompra);
                }
            }

            return result;
        }

        public SapOrdenCompra GetOrdenConDetalle(string numeroProveedor, string organizacionCompras, string numeroDocumento)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_EKKO_PO");

            function.SetValue("IM_LIFNR", numeroProveedor);
            function.SetValue("IM_EKORG", organizacionCompras);
            function.SetValue("IM_EBELN", numeroDocumento);
            function.SetValue("IM_LOEKZ", "A");
            function.SetValue("IM_AEDAT", "20150101");
            function.SetValue("IM_GET_DET", "X");

            var dtH = new DataTable();
            var dtD = new DataTable();
            try
            {
                function.Invoke(rfcDestinationManager);
                var resultH = function.GetTable("ET_HDR");
                dtH = resultH.ToDataTable("ET_HDR");

                var resultD = function.GetTable("ET_DET");
                dtD = resultD.ToDataTable("ET_DET");

            }
            catch
            {

            }

            var result = new List<SapOrdenCompra>();
            foreach (DataRow dr in dtH.Rows)
            {
                var sapOrdenCompra = new SapOrdenCompra(dr);

                // Relacion del encabezado del orden con el detalle a traves del número de orden de compra
                var drsDetalle = dtD.Select(string.Format("EBELN = '{0}'", dr["EBELN"]));

                var detalles = drsDetalle.Select(drDetalle => new SapOrdenCompraDetalle(drDetalle)).ToList();

                sapOrdenCompra.Detalles = detalles;

                if (sapOrdenCompra.Detalles.Any())
                {
                    result.Add(sapOrdenCompra);
                }
            }
            return result.Any() ? result[0] : null;
        }

    }
}
