using System;
using System.Data;
using SAP.Middleware.Connector;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {

        
            ConectorSAP conectaSAP = new ConectorSAP();

            conectaSAP.Conectar();

            RfcDestination rfcDest = RfcDestinationManager.GetDestination(conectaSAP.rfc);

            RfcRepository rfcRep = rfcDest.Repository;

            IRfcFunction function;

            function = rfcRep.CreateFunction("ZEXTRAE_PROV");

       

            function.Invoke(rfcDest);





            var result = function.GetTable("T_PROV");
            
            var datatable = result.ToDataTable("test");

          



        }
    }

    class ConectorSAP

    {

        public RfcConfigParameters rfc = new RfcConfigParameters();



        public void Conectar()

        {

            //  QAS

            rfc.Add(RfcConfigParameters.Name, "DIM");

            rfc.Add(RfcConfigParameters.AppServerHost, "172.18.3.21");

            rfc.Add(RfcConfigParameters.User, "ABAP_INDRA");

            rfc.Add(RfcConfigParameters.Password, "extnazan");

            rfc.Add(RfcConfigParameters.Client, "100");

            rfc.Add(RfcConfigParameters.SystemNumber, "11");

            rfc.Add(RfcConfigParameters.Language, "EN");

            rfc.Add(RfcConfigParameters.PoolSize, "5");

            rfc.Add(RfcConfigParameters.PeakConnectionsLimit, "35");

            rfc.Add(RfcConfigParameters.IdleTimeout, "500");

        }

    }

    public static class IRfcTableExtentions
    {
        /// <summary>
        /// Converts SAP table to .NET DataTable table
        /// </summary>
        /// <param name="sapTable">The SAP table to convert.</param>
        /// <returns></returns>
        public static DataTable ToDataTable(this IRfcTable sapTable, string name)
        {
            DataTable adoTable = new DataTable(name);
            //... Create ADO.Net table.
            for (int liElement = 0; liElement < sapTable.ElementCount; liElement++)
            {
                RfcElementMetadata metadata = sapTable.GetElementMetadata(liElement);
                adoTable.Columns.Add(metadata.Name, GetDataType(metadata.DataType));
            }

            //Transfer rows from SAP Table ADO.Net table.
            foreach (IRfcStructure row in sapTable)
            {
                DataRow ldr = adoTable.NewRow();
                for (int liElement = 0; liElement < sapTable.ElementCount; liElement++)
                {
                    RfcElementMetadata metadata = sapTable.GetElementMetadata(liElement);

                    switch (metadata.DataType)
                    {
                        case RfcDataType.DATE:
                            ldr[metadata.Name] = row.GetString(metadata.Name).Substring(0, 4) + row.GetString(metadata.Name).Substring(5, 2) + row.GetString(metadata.Name).Substring(8, 2);
                            break;
                        case RfcDataType.BCD:
                            ldr[metadata.Name] = row.GetDecimal(metadata.Name);
                            break;
                        case RfcDataType.CHAR:
                            ldr[metadata.Name] = row.GetString(metadata.Name);
                            break;
                        case RfcDataType.STRING:
                            ldr[metadata.Name] = row.GetString(metadata.Name);
                            break;
                        case RfcDataType.INT2:
                            ldr[metadata.Name] = row.GetInt(metadata.Name);
                            break;
                        case RfcDataType.INT4:
                            ldr[metadata.Name] = row.GetInt(metadata.Name);
                            break;
                        case RfcDataType.FLOAT:
                            ldr[metadata.Name] = row.GetDouble(metadata.Name);
                            break;
                        default:
                            ldr[metadata.Name] = row.GetString(metadata.Name);
                            break;
                    }
                }
                adoTable.Rows.Add(ldr);
            }
            return adoTable;
        }

        private static Type GetDataType(RfcDataType rfcDataType)
        {
            switch (rfcDataType)
            {
                case RfcDataType.DATE:
                    return typeof(string);
                case RfcDataType.CHAR:
                    return typeof(string);
                case RfcDataType.STRING:
                    return typeof(string);
                case RfcDataType.BCD:
                    return typeof(decimal);
                case RfcDataType.INT2:
                    return typeof(int);
                case RfcDataType.INT4:
                    return typeof(int);
                case RfcDataType.FLOAT:
                    return typeof(double);
                default:
                    return typeof(string);
            }
        }
    }
}
