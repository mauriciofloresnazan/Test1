using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using SAP.Middleware.Connector;

namespace SapWrapper
{
    public class SapPenalizacionesManager
    {
        private readonly CommonRfcConfigParam _rfc = new CommonRfcConfigParam();

        public String[] aplicarPenalizacion(string numeroProveedor, string monto, string descrip)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_GENERAR_PENALIZACION");

            //Numero de proveedorc
            function.SetValue("PI_NEWKO", numeroProveedor);

            //Monto de la penalidad
            function.SetValue("PI_WRBTR", monto);

            //Descripcion
            function.SetValue("PI_SGTXT", descrip);

            function.Invoke(rfcDestinationManager);

            var temp = function.GetValue("PE_BELNR");

            var temp1 = function.GetValue("PE_ERROR");

            String[] resultaldo = { temp.ToString(), temp1.ToString() };

            return resultaldo;

        }
    }
}
