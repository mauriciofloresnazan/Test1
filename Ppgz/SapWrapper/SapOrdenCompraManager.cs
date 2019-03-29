using System.Collections.Generic;
using System.Data;
using System.Linq;
using SAP.Middleware.Connector;
using Ppgz.Repository;

namespace SapWrapper
{
    public class SapOrdenCompraManager
    {
        private readonly CommonRfcConfigParam _rfc = new CommonRfcConfigParam();

        public DataTable GetOrdenesDeCompraHeader(string numeroProveedor, string organizacionCompras, string sociedad)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_EKKO_PO");
            function.SetValue("IM_LOEKZ", "A");
            function.SetValue("IM_LIFNR", numeroProveedor);
            function.SetValue("IM_BUKRS", sociedad);
            function.SetValue("IM_EKORG", organizacionCompras);
            function.SetValue("IM_GET_DET", " ");

            function.SetValue("IM_AEDAT", "20150101");

          //  try
            //{
                function.Invoke(rfcDestinationManager);
                var result = function.GetTable("ET_HDR");
                return result.ToDataTable("ET_HDR");
            /*}
            catch
            {
                return new DataTable();
            }*/
        }

        public DataTable GetOrdenesDeCompraEtiquetas(string numeroProveedor, string organizacionCompras, string sociedad)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_EKKO_PO");
            function.SetValue("IM_LOEKZ", "A");
            function.SetValue("IM_LIFNR", numeroProveedor);
            function.SetValue("IM_BUKRS", sociedad);
            function.SetValue("IM_EKORG", organizacionCompras);
            function.SetValue("IM_GET_DET", " ");
            function.SetValue("IM_ETIQ", "X");

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

        public int GetCantidadValidacionSAP(string ano, string sociedad, string referencia, string numeroProveedor)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZMF_PARES_ACUMULADO");
            function.SetValue("PI_MJAHR", ano);
            function.SetValue("PI_BUKRS",  sociedad);
            function.SetValue("PI_XBLNR", referencia);
            function.SetValue("PI_LIFNR", numeroProveedor);


            function.Invoke(rfcDestinationManager);
            try
            {
                var result = function.GetValue("PE_ERFMG");
                return int.Parse(result.ToString());
            }
            catch
            {
                return 0;
            }
            
        }

        public DataTable GetOrdenesDeCompraHeader(string numeroDocumento, string numeroProveedor, string organizacionCompras, string sociedad)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_EKKO_PO");
            function.SetValue("IM_EKORG", organizacionCompras);
            function.SetValue("IM_LOEKZ", "A");
            function.SetValue("IM_BUKRS", sociedad);
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

        public DataTable GetOrdenesDeCompraHeaderSociedad(string numeroProveedor, string organizacionCompras, string sociedad)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_EKKO_PO");
            function.SetValue("IM_EKORG", organizacionCompras);
            function.SetValue("IM_LOEKZ", "A");
            function.SetValue("IM_GET_DET", " ");
            function.SetValue("IM_BUKRS", sociedad);
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

        public DataTable GetOrdenDeCompraDetalle(string documento, string numeroProveedor, string organizacionCompras, string sociedad)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_EKKO_PO");
            function.SetValue("IM_LIFNR", numeroProveedor);
            function.SetValue("IM_AEDAT", "20150101");
            function.SetValue("IM_EKORG", organizacionCompras);
            function.SetValue("IM_BUKRS", sociedad);
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

        public List<SapOrdenCompra> GetActivasConDetalle(string numeroProveedor, string organizacionCompras, string sociedad)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_EKKO_PO");

            function.SetValue("IM_LIFNR", numeroProveedor);
            function.SetValue("IM_EKORG", organizacionCompras);
            function.SetValue("IM_LOEKZ", "A");
            function.SetValue("IM_BUKRS", sociedad);
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

        public SapOrdenCompra GetOrdenConDetalle(string numeroProveedor, string organizacionCompras, string numeroDocumento, string sociedad)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_EKKO_PO");

            function.SetValue("IM_LIFNR", numeroProveedor);
            function.SetValue("IM_EKORG", organizacionCompras);
            function.SetValue("IM_EBELN", numeroDocumento);
            function.SetValue("IM_BUKRS", sociedad);
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


        public DataTable SetOrdenesDeCompraCita(ICollection<asn> asns)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_EKPO_CITAS");


            IRfcTable IM_CITAS = function.GetTable("IM_CITAS");

            //Add select option values to MATNRSELECTION table
            foreach ( asn asn in asns)
            {

                RfcStructureMetadata am = rfcRepository.GetStructureMetadata("ZTY_EKPO_CITAS");
                IRfcStructure articol = am.CreateStructure();

               //Populate current MATNRSELECTION row with data from list
                articol.SetValue("EBELN", asn.OrdenNumeroDocumento);
                articol.SetValue("EBELP", asn.NumeroPosicion);
                articol.SetValue("ZZCITAS", "X");
                IM_CITAS.Append(articol);

            }

            function.SetValue("IM_CITAS", IM_CITAS);


            try
            {
                function.Invoke(rfcDestinationManager);
                var result = function.GetTable("ET_RETORNO");
                var nose= result.ToDataTable("ET_RETORNO");
                return result.ToDataTable("ET_RETORNO");
            }
            catch
            {
                return new DataTable();
            }
        }

        public DataTable UnsetOrdenesDeCompraCita(ICollection<asn> asns)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_EKPO_CITAS");


            IRfcTable IM_CITAS = function.GetTable("IM_CITAS");

            //Add select option values to MATNRSELECTION table
            foreach (asn asn in asns)
            {

                RfcStructureMetadata am = rfcRepository.GetStructureMetadata("ZTY_EKPO_CITAS");
                IRfcStructure articol = am.CreateStructure();

                //Populate current MATNRSELECTION row with data from list
                articol.SetValue("EBELN", asn.OrdenNumeroDocumento);
                articol.SetValue("EBELP", asn.NumeroPosicion);
                articol.SetValue("ZZCITAS", " ");
                IM_CITAS.Append(articol);

            }

            function.SetValue("IM_CITAS", IM_CITAS);


            try
            {
                function.Invoke(rfcDestinationManager);
                var result = function.GetTable("ET_RETORNO");
                var nose = result.ToDataTable("ET_RETORNO");
                return result.ToDataTable("ET_RETORNO");
            }
            catch
            {
                return new DataTable();
            }
        }


        
    }
}
