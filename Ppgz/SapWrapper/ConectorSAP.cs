using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAP.Middleware.Connector;

namespace SapWrapper
{
    class ConectorSAP
    {
        public RfcConfigParameters rfc = new RfcConfigParameters();



        public void Conectar()
        {

            //  QAS

            //rfc.Add(RfcConfigParameters.Name, "DIM");

            //rfc.Add(RfcConfigParameters.AppServerHost, "172.18.3.21");

            //rfc.Add(RfcConfigParameters.User, "ABAP_INDRA");

            //rfc.Add(RfcConfigParameters.Password, "extnazan");

            //rfc.Add(RfcConfigParameters.Client, "100");

            //rfc.Add(RfcConfigParameters.SystemNumber, "11");

            //rfc.Add(RfcConfigParameters.Language, "EN");

            //rfc.Add(RfcConfigParameters.PoolSize, "5");

            //rfc.Add(RfcConfigParameters.PeakConnectionsLimit, "35");

            //rfc.Add(RfcConfigParameters.IdleTimeout, "500");
            rfc.Add(RfcConfigParameters.Name, "QIM");

            rfc.Add(RfcConfigParameters.AppServerHost, "172.18.3.21");

            rfc.Add(RfcConfigParameters.User, "USRPORTAL");

            rfc.Add(RfcConfigParameters.Password, "wspportalp");

            rfc.Add(RfcConfigParameters.Client, "300");

            rfc.Add(RfcConfigParameters.SystemNumber, "22");

            rfc.Add(RfcConfigParameters.Language, "EN");

            rfc.Add(RfcConfigParameters.PoolSize, "5");

            rfc.Add(RfcConfigParameters.PeakConnectionsLimit, "35");

            rfc.Add(RfcConfigParameters.IdleTimeout, "500");

        }
    }
}
