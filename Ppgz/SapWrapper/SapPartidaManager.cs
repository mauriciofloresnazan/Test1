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

        public DataTable GetPartidasAbiertasDetalle(string codigoproveedor)
        {

            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZEXTRAE_PART_PROV");

            function.SetValue("LIFNR", "0000000059");
            function.SetValue("BUKRS", "1001");
            function.SetValue("IDIOMA", "S");
            function.Invoke(rfcDestinationManager);

            var result = function.GetTable("T_PARTIDAS_ABIERTAS");
            var result2 = function.GetTable("T_DETALLE_AB");

            return result2.ToDataTable("T_DETALLES_AB");

        }

        public DataTable GetListasDePagos(string codigoproveedor)
        {
            
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZEXTRAE_PART_PROV");

            function.SetValue("LIFNR", "0000000059");
            function.SetValue("BUKRS", "1001");
            function.SetValue("IDIOMA", "S");
            function.SetValue("FECHA_BASE", "20160630");
            
            function.Invoke(rfcDestinationManager);

            var result = function.GetTable("T_LISTA_PAGOS");
            var result2 = function.GetTable("T_PAGOS");

            return result2.ToDataTable("T_PAGOS");

        }

        public DataTable GetDevoluciones(string codigoproveedor)
        {


            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZEXTRAE_PART_PROV");

            function.SetValue("LIFNR", "0000000004");
            function.SetValue("BUKRS", "1001");
            function.SetValue("IDIOMA", "S");



            function.Invoke(rfcDestinationManager);

            var result = function.GetTable("T_DEVOLUCIONES");
            var result2 = function.GetTable("T_MAT_DEV");

            return result2.ToDataTable("T_MAT_DEV");
        }

    }
}
