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

        public DataSet GetPartidas(string numeroProveedor, string sociedad)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZEXTRAE_PART_PROV");
   
            function.SetValue("LIFNR", numeroProveedor);
            function.SetValue("BUKRS", sociedad);
            //TODO ELIMINAR PARA PRODUCCION
            function.SetValue("IDIOMA", "S");
            function.SetValue("FECHA_BASE", "20160630");
            
            function.Invoke(rfcDestinationManager);

            var dataSet = new DataSet();
            
            dataSet.Tables.Add(function.GetTable("T_DEVOLUCIONES")
                .ToDataTable("T_DEVOLUCIONES"));
            dataSet.Tables.Add(function.GetTable("T_MAT_DEV")
                .ToDataTable("T_MAT_DEV"));

            dataSet.Tables.Add(function.GetTable("T_LISTA_PAGOS")
                .ToDataTable("T_LISTA_PAGOS"));
            dataSet.Tables.Add(function.GetTable("T_PAGOS")
                .ToDataTable("T_PAGOS"));

            dataSet.Tables.Add(function.GetTable("T_PARTIDAS_ABIERTAS")
                .ToDataTable("T_PARTIDAS_ABIERTAS"));
            dataSet.Tables.Add(function.GetTable("T_DETALLE_AB")
                .ToDataTable("T_DETALLE_AB"));

            return dataSet;
        }

        //TODO  HAY QUE PASARLE LA SOCIEDAD Y SOLO DEVUELVE DEVOLUCIONES
        public DataSet GetPartidas1001(string numeroProveedor)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZEXTRAE_PART_PROV");

            function.SetValue("LIFNR", numeroProveedor);
            function.SetValue("BUKRS", "1001");
            //TODO ELIMINAR PARA PRODUCCION
            function.SetValue("IDIOMA", "S");
            function.SetValue("FECHA_BASE", "20160630");

            function.Invoke(rfcDestinationManager);

            var dataSet = new DataSet();

            dataSet.Tables.Add(function.GetTable("T_DEVOLUCIONES")
                .ToDataTable("T_DEVOLUCIONES"));
            dataSet.Tables.Add(function.GetTable("T_MAT_DEV")
                .ToDataTable("T_MAT_DEV"));

            dataSet.Tables.Add(function.GetTable("T_LISTA_PAGOS")
                .ToDataTable("T_LISTA_PAGOS"));
            dataSet.Tables.Add(function.GetTable("T_PAGOS")
                .ToDataTable("T_PAGOS"));

            dataSet.Tables.Add(function.GetTable("T_PARTIDAS_ABIERTAS")
                .ToDataTable("T_PARTIDAS_ABIERTAS"));
            dataSet.Tables.Add(function.GetTable("T_DETALLE_AB")
                .ToDataTable("T_DETALLE_AB"));

            return dataSet;
        }
    }
}
