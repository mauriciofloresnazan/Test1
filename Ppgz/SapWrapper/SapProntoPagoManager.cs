using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAP.Middleware.Connector;
using System.Data;

namespace SapWrapper
{
    public class SapProntoPagoManager
    {
        private readonly CommonRfcConfigParam _rfc = new CommonRfcConfigParam();

        public DataTable[] EnviarPropuesta(DateTime fechasolictud, string[] numerosProveedor, string[] facturasList, DateTime[] fechaList, string sociedad)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;

            try
            {
                var function = rfcRepository.CreateFunction("ZFM_PAGOS_PORTAL");

                //Parametros Fecha y Table
                function.SetValue("IM_BUKRS", sociedad);
                function.SetValue("IM_FECHA_PAGO", fechasolictud.ToString("yyyyMMdd"));

                var tablep = function.GetTable("IT_DOCS");
                for (int i = 0; i < facturasList.Count(); i++)
                {
                    tablep.Append();
                    tablep.SetValue("IM_BELNR", facturasList[i]);
                    tablep.SetValue("IM_LIFNR", numerosProveedor[i]);
                    tablep.SetValue("IM_BLDAT", fechaList[i].ToString("yyyyMMdd"));
                }

                function.Invoke(rfcDestinationManager);

                var resultDocs = function.GetTable("IT_DOCS");
                var resultReturn = function.GetTable("ET_RETORNO");

                DataTable [] dt = { resultReturn.ToDataTable("ET_RETORNO"), resultDocs.ToDataTable("IT_DOCS") };
                

                return dt;
            }
            catch(Exception ex)
            {
                DataTable[] dt = { new DataTable("ET_RETORNO"), new DataTable("IT_DOCS") };
                dt[0].Rows[0][3] = "Catch Ex" + ex.Message.ToString();
                return dt;
            }
        }
    }
}
