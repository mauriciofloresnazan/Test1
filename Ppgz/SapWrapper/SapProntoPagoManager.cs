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

        public DataTable EnviarPropuesta(DateTime fechasolictud, string numeroProveedor, string[] facturasList, DateTime[] fechaList)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;

            try
            {
                var function = rfcRepository.CreateFunction("ZFM_PAGOS_PORTAL");

                //Parametros Fecha y Table
                function.SetValue("IM_FECHA_PAGO", fechasolictud);

                var tablep = function.GetTable("IT_DOCS");
                for (int i = 0; i < facturasList.Count(); i++)
                {
                    tablep.Append();
                    tablep.SetValue("IM_BELNR", facturasList[i]);
                    tablep.SetValue("IM_LIFNR", numeroProveedor);
                    tablep.SetValue("IM_BLDAT", fechaList[i]);
                }

                function.Invoke(rfcDestinationManager);

                var resultDocs = function.GetTable("IT_DOCS");
                var resultReturn = function.GetTable("ET_RETORNO");

                if (resultReturn.Count <= 0) return null;

                var dt = resultReturn.ToDataTable("ET_RETORNO");

                return dt;
            }
            catch 
            {
                return null;
            }
        }
    }
}
