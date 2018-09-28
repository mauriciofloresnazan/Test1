using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;
using ClosedXML.Excel;
using MySql.Data.MySqlClient;
using MySql.Data.MySqlClient.Authentication;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Ppgz.CitaWrapper;
using Ppgz.Repository;
using Ppgz.Services;
using RestSharp;
using SapWrapper;
using SatWrapper;
using SAP.Middleware.Connector;
using ScaleWrapper;

namespace Test
{
    class Program
    {
        static void TestRule()
        {
            Console.WriteLine(RulesManager.Regla2(DateTime.Today.AddDays(1)));
            Console.ReadLine();
        }

        static void TestEtiquetas()
        {
            var ordenes = new[]
            {
                "4700000080", "0000500000",
                "4700000081", "4700000082", "4700000083", "4700000084", "4700000085", "4700000086",
                "4700000087", 
                "4700000088", "4700000089","4700000080","4700000080","4700000080","4700000080",
            };

            var sapEtiquetasManager = new SapEtiquetasManager();
            var resultado = sapEtiquetasManager.GetContenidoCsv("0000500000", true, ordenes);

            Console.WriteLine(string.Join(",", (string[])resultado["s_orders"]));
            Console.WriteLine(string.Join(",", (string[])resultado["e_orders"]));
            Console.WriteLine(resultado["csv"]);
            Console.ReadLine();

        }

        static void TestExcel()
        {
            var fileName = Guid.NewGuid() + ".xlsx";
            var template = new XLWorkbook(@"c:\temp\borrar\plantillaoc.xlsx");
            template.SaveAs(fileName);

            var workbook = new XLWorkbook(fileName);
            var ws = workbook.Worksheet(1);

            var row = 6;
            while (row < 20)
            {
                var col = 1;
                while (col < 10)
                {
                    ws.Cell(row, col).Value = string.Format("Row{0} x Col {1}", row, col);
                    col++;
                }
        
                row++;
            }
            workbook.Save();


        }
        static void TestCita()
        {
            Console.WriteLine("Inicio");
            Console.WriteLine(DateTime.Now);
            var  preAsnManager = new PreAsnManager();
            var ordenes = preAsnManager.GetOrdenesActivasConDetalle(69);

            Console.WriteLine("Fin");
            Console.WriteLine(DateTime.Now);

            Console.ReadLine();
            /*
            var preCita = new PreCita()
            {
                Cantidad = 2,
                Centro = CurrentCita.Centro,
                Fecha = (DateTime)CurrentCita.Fecha,
                ProveedorId = CurrentCita.Proveedor.Id,
                UsuarioId = _commonManager.GetUsuarioAutenticado().Id,
                Asns = new List<Asn>(),
                HorarioRielesIds = rielesIds.ToList()
            };

            foreach (var preAsn in CurrentCita.GetPreAsns())
            {
                foreach (var preAsnDetail in preAsn.Detalles.Where(preAsnDetail => preAsnDetail.Cantidad > 0))
                {
                    preCita.Asns.Add(new Asn
                    {
                        Cantidad = preAsnDetail.Cantidad,
                        NombreMaterial = preAsnDetail.DescripcionMaterial,
                        NumeroMaterial = preAsnDetail.NumeroMaterial,
                        NumeroPosicion = preAsnDetail.NumeroPosicion,
                        OrdenNumeroDocumento = preAsn.NumeroDocumento,
                        Tienda = preAsn.Tienda,

                        TiendaOrigen = preAsn.TiendaOrigen,
                        CantidadSolicitada = preAsnDetail.CantidadPedido,
                        InOut = preAsn.InOut,
                        Precio = preAsnDetail.Precio,
                        UnidadMedida = preAsnDetail.UnidadMedida,
                        NumeroSurtido = preAsn.NumeroOrdenSurtido
                    });
                }
            }
            try
            {
                
                CitaManager.RegistrarCita(preCita);

            }
            catch (ScaleException exception)
            {
                LimpiarCita();
                TempData["FlashError"] = exception.Message;
                return RedirectToAction("Citas");
            }
            catch (Exception exception)
            {

                //TODO
                TempData["FlashError"] = exception.Message;
                return RedirectToAction("SeleccionarRieles");
            }
            */

        }
        
        static void ValidarSat()
        {

            var serializer = new XmlSerializer(typeof(Comprobante));

            //var factura = File.ReadAllText(@"C:\temp\borrar\miro\4EEC-A....xml");
            //var factura = File.ReadAllText(@"C:\Users\Juan\Downloads\fwdrvfacturasutilizadasenpruebas\-2017-04-07-FOJG690223SE5_i_9378_09.41.06.20170407_2298C412-8116-46AD-B55C-A3A3684FAAF9.pdf");
            //var factura = File.ReadAllText(@"C:\Users\Juan\Downloads\fwdrvfacturasutilizadasenpruebas\F0000004597.xml");
            var factura = File.ReadAllText(@"C:\Users\Juan\Downloads\fwdrvfacturasutilizadasenpruebas\-2017-04-07-RILG7812305Q9_i_6388_10.02.56.20170407_18ECB762-C296-4EAF-8D33-E12AFE1962FF.pdf");
            
            var archivoXml = new StreamReader(@"C:\Users\Juan\Downloads\fwdrvfacturasutilizadasenpruebas\F0000004597_1_mod.xml");


            //var doc = XDocument.Load(archivoXml);
            /*var test = from folios in doc.Root.Elements("folio")
                            //where address.Element("firstName").Value.Contains("er")
                       select folios;*/

            //var comprobante = (Comprobante)serializer.Deserialize(archivoXml);

            try
            {
                var consulta = CfdiServiceConsulta.Validar(factura, "tokSY9Db304Kx", "pswNfFPAaQhJ2", "usraLYhIjXOCJ", "ctaduPw4Xeh0w", "NCC1011058I0");
                Console.WriteLine(consulta);
            }
            catch (Exception exception)
            {

                Console.WriteLine(exception.Message);
            }
            Console.ReadKey();
        }

        static void TestScale()
        {
            var db = new Entities();
            var cita = db.citas.Find(79);
            var scaleManager = new ScaleManager();
            try
            {

                scaleManager.Registrar(cita);
            }
            catch (Exception exception)
            {
                
                Console.WriteLine(exception.Message);
            }
        }

        static void TestCalcularRieles()
        {
            Console.WriteLine(RulesManager.GetCantidadRieles(701));
            Console.ReadLine();
        }

        static void T1()
        {
            var db = new Entities();

            string email = null;

            if (db.AspNetUsers.FirstOrDefault(u => u.Email == email) != null)
            {
                Console.WriteLine("La Dirección de correo ya ha sido utilizada por otro usuario");
            }
            Console.ReadLine();
        }

        static void Main(string[] args)
        {
            TestRule();
            return;

            /*ValidarSat();
            return;
            */
            // solo el folio
            var serializer = new XmlSerializer(typeof(Comprobante));
            //var archivoXml = new FileStream(@"C:\temp\borrar\miro\factura.xml", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            //var archivoXml = new FileStream(@"C:\temp\borrar\miro\4EEC-A....xml", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            //var archivoXml = new FileStream(@"C:\temp\borrar\miro\4939-A....xml", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var archivoXml = new FileStream(@"C:\temp\borrar\miro\error.xml", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var comprobante = (Comprobante)serializer.Deserialize(archivoXml);
            var sapFacturaManager = new SapFacturaManager();


            decimal cantidad = 0;
            foreach (var concepto in comprobante.Conceptos.Concepto)
            {
                cantidad = cantidad + Convert.ToDecimal(concepto.Cantidad);
            }

            var factura = sapFacturaManager.CrearFactura(
                "0000500000",
                //"0000000004",
                /*comprobante.Serie + */comprobante.Folio,
                DateTime.ParseExact(comprobante.Fecha, "yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture),
                
                //DateTime.Parse(comprobante.Fecha, null, DateTimeStyles.RoundtripKind),
                comprobante.SubTotal,
                comprobante.Total,
                cantidad.ToString(),
                 comprobante.Complemento.TimbreFiscalDigital.UUID,
                 comprobante.Emisor.Rfc);

            Console.WriteLine( JsonConvert.SerializeObject(factura));
            Console.ReadKey();
            return;
            var preCita = new PreCita();
            preCita.Fecha =DateTime.Today;
            preCita.HorarioRielesIds = new List<int> {850};

            var url = "http://localhost:14766/CitationControlService.svc/rest/AddCitation";
            var client = new RestClient(url);
            var request = new RestRequest(string.Empty, Method.POST);

            var microsoftDateFormatSettings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
            };

            var json = JsonConvert.SerializeObject(preCita, microsoftDateFormatSettings);

            request.AddParameter("application/json", json, ParameterType.RequestBody);

            var restResponse = (RestResponse)client.Execute(request);


            Console.WriteLine(restResponse.Content);
            Console.ReadLine();
            return;
            
            var preasnManager = new PreAsnManager();

            var test = preasnManager.GetOrdenesActivasConDetalle(33);

            var query = test.Where(o => o.Detalles.Any(de => de.Centro.ToUpper() == "CD01")).ToList();
            Console.WriteLine(JsonConvert.SerializeObject(query));
            Console.ReadKey();
            return;
           
            Console.WriteLine(RulesManager.GetCantidadRieles(1716));
            Console.ReadKey();
            return;


            const string sql = @"INSERT INTO citas (FechaCita, Tienda, CantidadTotal, ProveedorId, UsuarioIdTx)
                        VALUES(@FechaCita, @Tienda, @CantidadTotal, @ProveedorId, @UsuarioIdTx)";

                var parameters =new List<MySqlParameter>
                {
                    new MySqlParameter("FechaCita", DateTime.Today.Date),
                    new MySqlParameter("Tienda", "TD01"),
                    new MySqlParameter("CantidadTotal", 50),
                    new MySqlParameter("ProveedorId", "33"),
                    new MySqlParameter("UsuarioIdTx", "15e814e8-0967-46e1-9a9d-fdfb7a1f2d4b"),              

                };

                Db.Insert(sql, parameters);


            Console.WriteLine(DateTime.Now.Hour);
            Console.ReadLine();
            return;


            var sapFechaEntrega = DateTime.Today;
            
            var semana = CultureInfo
                .GetCultureInfo("es-MX")
                .Calendar
                .GetWeekOfYear(sapFechaEntrega, CalendarWeekRule.FirstDay, sapFechaEntrega.DayOfWeek);
            
            Console.WriteLine(semana);

            var day = sapFechaEntrega.AddDays(-30);

            while (day < sapFechaEntrega.AddDays(30))
            {
                var semana2 = CultureInfo
                    .GetCultureInfo("es-MX")
                    .Calendar
                    .GetWeekOfYear(day, CalendarWeekRule.FirstDay, day.DayOfWeek);

                if (semana2 >= semana - 2 && semana2 <= semana + 2)
                    Console.WriteLine(day.ToString("dd/MM/yyyy"));
                day = day.AddDays(1);
            }



            Console.ReadLine();
            return;

            var db = new Entities();

            var proveedor = db.proveedores.Find(41);
            var testPartida = new SapPartidaManager();

            return;

            
            var ordenCompraManager = new OrdenCompraManager();

  




            return;
            
         

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
                //cuentaManager.Crear(
                //    CuentaManager.Tipo.Mercaderia,
                //    "Nombre del Proveedor",
                //    "test_x1", "Nombre del Responsable", "Apellido del Responsable",
                //    "Responsable Cargo", "juan.godoy@test.com", "04145555555",
                //    "123456", true);

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
