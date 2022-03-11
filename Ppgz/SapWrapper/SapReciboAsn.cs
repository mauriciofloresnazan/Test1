using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using SAP.Middleware.Connector;

namespace SapWrapper
{
    public class SapReciboAsn
    {
        private readonly CommonRfcConfigParam _rfc = new CommonRfcConfigParam();

        public String[] DatoEan(string Ean)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_CONVIERTE_EAN_MATERIAL");
            //Numero de Ean
            function.SetValue("EAN", Ean);
            function.Invoke(rfcDestinationManager);
            var temp = function.GetValue("material");
            var temp2 = function.GetValue("Ean11");
            var temp1 = function.GetValue("error");
            var temp3 = function.GetValue("ESTILO");
            var temp4 = function.GetValue("COLOR");
            String[] resultaldo = { temp.ToString(), temp1.ToString(),temp2.ToString(), temp3.ToString(), temp4.ToString() };
            return resultaldo;

        }

        public String[] MensajeError(string Pedido)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_VALIDA_STS_ORDEN_COMPRA");
            //Numero de Pedido
            function.SetValue("EBELN", Pedido);
            function.Invoke(rfcDestinationManager);
            var temp3 = function.GetValue("ERROR1");
            var temp4 = function.GetValue("ERROR2");
            var temp5 = function.GetValue("ERROR3");
            var temp6 = function.GetValue("ERROR4");
            var temp7 = function.GetValue("ERROR5");
            var temp8 = function.GetValue("ERROR6");
            var temp9 = function.GetValue("ERROR7");
            var temp10 = function.GetValue("ERROR8");
            var temp11 = function.GetValue("ERROR9");
            var temp12 = function.GetValue("ERROR10");
            var temp13= function.GetValue("ERROR11");
            String[] resultaldo = {temp3.ToString(), temp4.ToString(), temp5.ToString()
            , temp6.ToString(), temp7.ToString(), temp8.ToString(), temp9.ToString(), temp10.ToString(), temp11.ToString()
            , temp12.ToString(), temp13.ToString()};
            return resultaldo;

        }

    }
}
