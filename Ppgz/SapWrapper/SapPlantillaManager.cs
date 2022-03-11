using System;
using System.Collections;
using System.IO;
using System.Linq;
using SAP.Middleware.Connector;

namespace SapWrapper
{
    public class SapPlantillaManager
    {
        private readonly CommonRfcConfigParam _rfc = new CommonRfcConfigParam();

        /// <summary> Método para obtener el contenido del csv de etiquetas.
        ///     Retorna un Hashtable con los siguientes objetos
        ///     csv: Contiene el contenido del archivo en string
        ///     return: Contiene un datatable con el resultado del rfc
        /// </summary>
        public Hashtable GetContenidoCsv(string numeroProveedor, bool etiquetaNazan, string[] ordenes)
        {
            if (String.IsNullOrWhiteSpace(numeroProveedor.Trim()))
            {
                throw new Exception("Número de proveedor incorrecto");
            }

            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_ENVIO_TAB_PLAN");
            function.SetValue("PE_PROVEEDOR", numeroProveedor.PadLeft(10, '0'));

            function.SetValue("PE_NAZAN", etiquetaNazan ? "X" : " ");

            var table = function.GetTable("T_ORDENES");

            ordenes = ordenes.Distinct().ToArray();

            foreach (var orden in ordenes)
            {
                table.Append();
                table.SetValue("EBELN", orden);
            }

            function.Invoke(rfcDestinationManager);

            var rfcOutTetiqueta = function.GetTable("T_ETIQUETA");
            var rfcOutreturn = function.GetTable("RETURN");

            var resultado = new Hashtable();

            var csvStringWriter = new StringWriter();

            foreach (var tableRow in rfcOutTetiqueta)
            {
                csvStringWriter.WriteLine(tableRow.GetValue("VALOR"));
            }

            resultado.Add("csv", csvStringWriter.ToString());

            resultado.Add("return", rfcOutreturn.ToDataTable("return"));
            
            return resultado;
        }
    }
}
