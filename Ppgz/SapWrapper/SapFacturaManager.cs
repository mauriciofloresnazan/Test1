using System;
using System.Data;
using SAP.Middleware.Connector;

namespace SapWrapper
{
    public class SapFacturaManager
    {
        public class Resultado
        {
            public DataTable ErrorTable;

            public string FacturaNumero;

            public string Estatus;
        }
        
        private readonly CommonRfcConfigParam _rfc = new CommonRfcConfigParam();

        public Resultado CrearFactura(string numeroProveedor, string numeroDocumentoReferencia, DateTime fechaFactura, 
            string importe, string importeConTaxas, string cantidad, string uuid, string rfc)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_MM_CREATE_INVOICE");
            function.SetValue("I_PROVEEDOR", numeroProveedor);
            function.SetValue("I_REF_DOC", numeroDocumentoReferencia);
            function.SetValue("I_FECHA_FACT", fechaFactura.ToString("yyyyMMdd"));
            function.SetValue("I_IMPORTE", importe);
            function.SetValue("I_IMPORTE_TAX", importeConTaxas);

            function.SetValue("I_CANTIDAD", cantidad);

            function.SetValue("I_UUID", uuid);
            function.SetValue("I_RFC", rfc);
            
            function.Invoke(rfcDestinationManager);


            function.GetValue("E_STATUS");
            
            var result = function.GetTable("T_RETURN");
            			

            var resultado = new Resultado
            {
                ErrorTable = result.ToDataTable("T_RETURN"),
                Estatus = function.GetValue("E_STATUS").ToString(),
                FacturaNumero = function.GetValue("E_INVOICE_NUM").ToString()
            };


            return resultado;

        }

		public Resultado CrearNotaCredito(
			string numeroProveedor, 
			DateTime fechaFactura,
			string importe, 
			string cabecera, 
			string posicion, 
			string folio //Serie+Folio
			)
		{
			var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
			var rfcRepository = rfcDestinationManager.Repository;
			var function = rfcRepository.CreateFunction("ZFM_NOTA_CRED");
			function.SetValue("IM_ACREEDOR", numeroProveedor);
			function.SetValue("IM_FECHA", fechaFactura.ToString("yyyyMMdd"));
			function.SetValue("IM_REF", folio);
			function.SetValue("IM_IMPORTE", importe);

			function.SetValue("IM_TEXTO_H", cabecera);
			function.SetValue("IM_TEXTO_P", posicion);
			
			function.Invoke(rfcDestinationManager);


			//function.GetValue("E_STATUS");

			var result = function.GetTable("ET_RETORNO");

			Resultado resultado = new Resultado();
			try
			{
				resultado = new Resultado
				{
					ErrorTable = result.ToDataTable("T_RETURN"),
					Estatus = "1",
					FacturaNumero = function.GetValue("EX_OBJ_KEY").ToString().Substring(0, 10)
				};
			}
			catch(Exception ioe)
			{
				resultado = new Resultado
				{
					ErrorTable = result.ToDataTable("T_RETURN"),
					Estatus = "0",
					FacturaNumero = ""
				};
			}


			return resultado;

		}

		public Resultado CrearFacturaServicio(string numeroProveedor, string numeroDocumentoReferencia, DateTime fechaFactura,
            string importe, string importeConTaxas, string cantidad, string uuid, string rfc)
        {
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_MM_CREATE_INVOICE_SERV");
            function.SetValue("I_PROVEEDOR", numeroProveedor);
            function.SetValue("I_REF_DOC", numeroDocumentoReferencia);
            function.SetValue("I_FECHA_FACT", fechaFactura.ToString("yyyyMMdd"));
            function.SetValue("I_IMPORTE", importe);
            function.SetValue("I_IMPORTE_TAX", importeConTaxas);

            function.SetValue("I_CANTIDAD", cantidad);

            function.SetValue("I_UUID", uuid);
            function.SetValue("I_RFC", rfc);

            function.Invoke(rfcDestinationManager);


            function.GetValue("E_STATUS");

            var result = function.GetTable("T_RETURN");


            var resultado = new Resultado
            {
                ErrorTable = result.ToDataTable("T_RETURN"),
                Estatus = function.GetValue("E_STATUS").ToString(),
                FacturaNumero = function.GetValue("E_INVOICE_NUM").ToString()
            };


            return resultado;

        }
    }
}
