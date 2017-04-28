using System;
using System.Data;
using SAP.Middleware.Connector;

namespace SapWrapper
{
    public class SapPartidaManager
    {
        private readonly CommonRfcConfigParam _rfc = new CommonRfcConfigParam();

        public DataSet GetPartidasAbiertas(string numeroProveedor, string sociedad, DateTime fecha)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZEXTRAE_PART_PROV");

            function.SetValue("LIFNR", numeroProveedor);

            function.SetValue("BUKRS", sociedad);

            function.SetValue("IDIOMA", "S");

            function.SetValue("FECHA_BASE", fecha.ToString("yyyyMMdd"));

            function.SetValue("TIPO", "A");

            function.Invoke(rfcDestinationManager);

            var dataSet = new DataSet();
            
            dataSet.Tables.Add(function.GetTable("T_PARTIDAS_ABIERTAS")
                .ToDataTable("T_PARTIDAS_ABIERTAS"));
            dataSet.Tables.Add(function.GetTable("T_DETALLE_AB")
                .ToDataTable("T_DETALLE_AB"));

            return dataSet;
        }

        public DataSet GetPagos(string numeroProveedor, string sociedad, DateTime fecha)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZEXTRAE_PART_PROV");

            function.SetValue("LIFNR", numeroProveedor);

            function.SetValue("BUKRS", sociedad);

            function.SetValue("IDIOMA", "S");

            function.SetValue("FECHA_BASE", fecha.ToString("yyyyMMdd"));

            function.SetValue("TIPO", "P");

            function.Invoke(rfcDestinationManager);

            var dataSet = new DataSet();
            
            dataSet.Tables.Add(function.GetTable("T_LISTA_PAGOS")
                .ToDataTable("T_LISTA_PAGOS"));
            dataSet.Tables.Add(function.GetTable("T_PAGOS")
                .ToDataTable("T_PAGOS"));
            
            return dataSet;

        }

        public DataSet GetDevoluciones(string numeroProveedor, string sociedad, DateTime fecha)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);

            var rfcRepository = rfcDestinationManager.Repository;

            var function = rfcRepository.CreateFunction("ZEXTRAE_PART_PROV");

            function.SetValue("LIFNR", numeroProveedor);

            function.SetValue("BUKRS", sociedad);
   
            function.SetValue("IDIOMA", "S");

            function.SetValue("FECHA_BASE", fecha.ToString("yyyyMMdd"));

            function.SetValue("TIPO", "D");

            function.Invoke(rfcDestinationManager);

            var dataSet = new DataSet();

            dataSet.Tables.Add(function.GetTable("T_DEVOLUCIONES")
                .ToDataTable("T_DEVOLUCIONES"));
            dataSet.Tables.Add(function.GetTable("T_MAT_DEV")
                .ToDataTable("T_MAT_DEV"));

            return dataSet;
        }
       
        
    }
}
