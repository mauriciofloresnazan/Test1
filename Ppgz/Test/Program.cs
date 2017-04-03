using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Ppgz.Repository;
using Ppgz.Services;
using SAP.Middleware.Connector;

namespace Test
{
    class Program
    {

        
        static void Main(string[] args)
        {
            new TestCitas();
            return;

            
            var ordenCompraManager = new OrdenCompraManager();

            ordenCompraManager.FindOrdenCompraWithAvailableDates("4500916565", 3);




            return;
            
            var result = ordenCompraManager.FindOrdenesDecompraActivas(3);

            var testRfc = new TestRfc();
            //testRfc.TestPartidas("0000001727");
            testRfc.TestOrdenesDeCompraHeader("0000001725");

          /*  Entities db = new Entities();
            var usuario = db.AspNetUsers.FirstOrDefault(u => u.UserName == "superusuario");
            Console.WriteLine(usuario.Id);
            Console.ReadLine();

            return;
            */
            //
            //testRfc.TestProveedores();
            //testRfc.TestConsultarDetalleDeOrdenCompra("4500916565");
            //testRfc.BuscarCodigosProveedores();
            //Test();
            //CrearUsuarioProveedor();
            //TestOrdenCompra();
            //TestCuentaManager();


        }
        
        static void TestOrdenCompra()
        {
            var ordenCompraManager= new OrdenCompraManager();
            var orden = ordenCompraManager.FindActivaByIdAndUsuarioId(5, "");
        
            Console.WriteLine( JsonConvert.SerializeObject(orden));
        }

        static void CrearUsuarioNazan()
        {
            var usuarioManager = new UsuarioManager();
            var usuarioNazan = usuarioManager.CrearNazan("unazan_x1", "JUAN", "GODOY",
                "G.JUANCH14@GMAIL.COM", "04169113665", "GERENTE", true, PerfilManager.MaestroNazan.Id, "123456");

            usuarioManager.Eliminar(usuarioNazan.Id);
        }


        static void CrearUsuarioProveedor()
        {
            var usuarioManager = new UsuarioManager();
            var usuarioNazan = usuarioManager.CrearProveedor("unazan_x1", "JUAN", "GODOY",
                "G.JUANCH14@GMAIL.COM", "04169113665", "GERENTE", true, PerfilManager.MaestroNazan.Id, "123456", 2);

            usuarioManager.Eliminar(usuarioNazan.Id);
        }

    



        static void TestCuentaManager()
        {
            var cuentaManager = new CuentaManager();
            try
            {
                cuentaManager.Crear(
                    CuentaManager.Tipo.Mercaderia,
                    "Nombre del Proveedor",
                    "test_x1", "Nombre del Responsable", "Apellido del Responsable",
                    "Responsable Cargo", "juan.godoy@test.com", "04145555555",
                    "123456");

            }
            catch (Exception ex)
            {
                
                Console.WriteLine(ex.Message);
            }
        }

        static void Test()
        {
            CuentaManager cuentaManager = new CuentaManager();
            var result =  cuentaManager.FindAllWithUsuarioMaestro();
            Console.WriteLine(JsonConvert.SerializeObject(result));

            
            var entities  = new Entities();

            var query = (from g in entities.ordencompras
            join u in entities.proveedores on g.ProveedorId equals u.Id
            select new { g, u, }).Take(2);


            var json = JsonConvert.SerializeObject(query);
           // Console.Write(json);
            //Console.ReadLine();


            var quer2 = (from c in entities.cuentas
                from u in c.AspNetUsers
                where u.Tipo == UsuarioManager.Tipo.MaestroProveedor
                select  new {c, u}).ToList();

//            var sql = ((System.Data.Objects.ObjectQuery)query).ToTraceString();

            Console.WriteLine(quer2.ToString());
 
            json = JsonConvert.SerializeObject(quer2.ToList());
            Console.Write(json);
            Console.ReadLine();



        }

        static void TestDao(string id)
        {
            var i = 10000;
            while (i > 0)
            {
                var table = Db.GetDataTable("SELECT * FROM cuentasxpagar WHERE Id = @id", new List<MySqlParameter>()
                {
                    new MySqlParameter("id", id)
                });

                if (table.Rows.Count > 0)
                {
                    Console.WriteLine("IMPORTE: " + table.Rows[0]["importe"] );
                }
                else
                {
                    Console.WriteLine("ID INCORRECTO");
                }

                i--;
            }
        }


        void TestSap()
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
