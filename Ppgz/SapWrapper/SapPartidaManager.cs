using System.Data;
using SAP.Middleware.Connector;

namespace SapWrapper
{
    public class SapPartidaManager
    {
        private readonly CommonRfcConfigParam _rfc = new CommonRfcConfigParam();

        public DataSet GetPartidasAbiertas(string numeroProveedor)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZEXTRAE_PART_PROV");

            function.SetValue("LIFNR", numeroProveedor);

            // TODO SOCIEDAD INCORRECTA
            function.SetValue("BUKRS", "1001");
            //TODO PASAR A LA TABLA DE CONFIGURACIONES
            function.SetValue("IDIOMA", "S");
            //TODO SOLO ES PARA  LA PRESENTACION AL CLIENTE
            function.SetValue("FECHA_BASE", "20160630");

            function.SetValue("TIPO", "A");

            function.Invoke(rfcDestinationManager);

            var dataSet = new DataSet();
            
            dataSet.Tables.Add(function.GetTable("T_PARTIDAS_ABIERTAS")
                .ToDataTable("T_PARTIDAS_ABIERTAS"));
            dataSet.Tables.Add(function.GetTable("T_DETALLE_AB")
                .ToDataTable("T_DETALLE_AB"));

            return dataSet;
        }

        public DataSet GetPagos(string numeroProveedor)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZEXTRAE_PART_PROV");

            function.SetValue("LIFNR", numeroProveedor);

            // TODO SOCIEDAD INCORRECTA
            function.SetValue("BUKRS", "1001");
            //TODO PASAR A LA TABLA DE CONFIGURACIONES
            function.SetValue("IDIOMA", "S");
            //TODO SOLO ES PARA  LA PRESENTACION AL CLIENTE
            function.SetValue("FECHA_BASE", "20160630");

            function.SetValue("TIPO", "P");

            function.Invoke(rfcDestinationManager);

            var dataSet = new DataSet();
            
            dataSet.Tables.Add(function.GetTable("T_LISTA_PAGOS")
                .ToDataTable("T_LISTA_PAGOS"));
            dataSet.Tables.Add(function.GetTable("T_PAGOS")
                .ToDataTable("T_PAGOS"));
            
            return dataSet;

        }

        public DataSet GetDevoluciones(string numeroProveedor)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZEXTRAE_PART_PROV");

            function.SetValue("LIFNR", numeroProveedor);

            // TODO SOCIEDAD INCORRECTA
            function.SetValue("BUKRS", "1001");
            //TODO PASAR A LA TABLA DE CONFIGURACIONES
            function.SetValue("IDIOMA", "S");
            //TODO SOLO ES PARA  LA PRESENTACION AL CLIENTE
            function.SetValue("FECHA_BASE", "20160630");

            function.SetValue("TIPO", "D");

            function.Invoke(rfcDestinationManager);

            var dataSet = new DataSet();

            dataSet.Tables.Add(function.GetTable("T_DEVOLUCIONES")
                .ToDataTable("T_DEVOLUCIONES"));
            dataSet.Tables.Add(function.GetTable("T_MAT_DEV")
                .ToDataTable("T_MAT_DEV"));

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

        public DataSet GetPartidas1001SinFecha(string numeroProveedor)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZEXTRAE_PART_PROV");

            function.SetValue("LIFNR", numeroProveedor);
            function.SetValue("BUKRS", "1001");
            //TODO ELIMINAR PARA PRODUCCION
            function.SetValue("IDIOMA", "S");


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
