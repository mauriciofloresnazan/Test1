using System;
using System.Data;
using System.Reflection.Emit;
using SAP.Middleware.Connector;

namespace SapWrapper
{
    public class SapProveedorManager
    {
        private readonly CommonRfcConfigParam _rfc = new CommonRfcConfigParam();

        public DataTable GetProveedor(string numeroProveedor)
        {
            if (String.IsNullOrWhiteSpace(numeroProveedor.Trim()))
            {
                //TODO
                throw new Exception("Número de proveedor incorrecto");
            }

            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZEXTRAE_PROV");
            function.SetValue("LIFNR", numeroProveedor.PadLeft(10, '0'));
            function.Invoke(rfcDestinationManager);

            var result = function.GetTable("T_PROV");

            if (result.Count <= 0) return null;

            var dt = result.ToDataTable("T_PROV");
            return dt;
        }

        public double GetPrestamo(string numeroProveedor, string sociedad)
        {
            if (String.IsNullOrWhiteSpace(numeroProveedor.Trim()))
            {
                //TODO
                throw new Exception("Número de proveedor incorrecto");
            }

            double prestamo = -1;
                        
            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;

            var function = rfcRepository.CreateFunction("ZFM_PRESTAMOS_PORTAL");

            function.SetValue("IM_BUKRS", sociedad);
            function.SetValue("IM_ACREEDOR", numeroProveedor);
            function.Invoke(rfcDestinationManager);


            prestamo = Convert.ToDouble(function.GetValue("EX_TOTAL"));
            return prestamo;
        }

    }
}
