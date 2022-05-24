using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using SAP.Middleware.Connector;

namespace SapWrapper
{
    public class SapBuho
    {
        private readonly CommonRfcConfigParam _rfc = new CommonRfcConfigParam();

        public DataTable GetApi(string Api)
        {

            var rfcDestinationManager = RfcDestinationManager.GetDestination(_rfc);
            var rfcRepository = rfcDestinationManager.Repository;
            var function = rfcRepository.CreateFunction("ZFM_BHPRODUCTOS");
            function.SetValue("IM_TEST", Api);
            function.Invoke(rfcDestinationManager);

            var result = function.GetTable("ST_PRODUCTOS");

            if (result.Count <= 0) return null;

            var dt = result.ToDataTable("ST_PRODUCTOS");
            return dt;
        }

    }
}
