using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ClosedXML.Excel;
using MySql.Data.MySqlClient;
using Ppgz.CitaWrapper;
using Ppgz.Repository;
using Ppgz.Services;
using Ppgz.Web.Infrastructure;
using System.Web.Script.Serialization;
using SapWrapper;
using System.Text;
using System.Collections;
using OfficeOpenXml;
using ExcelDataReader;
using log4net;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class ReciboPorAsnController : Controller
    {
        public readonly ILog ErrorAppLog = LogManager.GetLogger(@"ErrorAppLog");
        readonly ProveedorManager _proveedorManager = new ProveedorManager();
        readonly CommonManager _commonManager = new CommonManager();
        readonly SapReciboAsn DatosSap = new SapReciboAsn();
        private readonly EtiquetasManager _etiquetasManager = new EtiquetasManager();
        private readonly PlantillaManager _PlantillaManager = new PlantillaManager();

        private const string NombreVarSession = "controlCita";
        private const string NombreVarSession2 = "SociedadCita";
        internal void LimpiarCita()
        {
            if (System.Web.HttpContext.Current.Session[NombreVarSession] != null)
            {
                System.Web.HttpContext.Current.Session.Remove(NombreVarSession);
            }
            if (System.Web.HttpContext.Current.Session[NombreVarSession2] != null)
            {
                System.Web.HttpContext.Current.Session.Remove(NombreVarSession2);
            }
        }


        internal CurrentCita CurrentCita
        {
            get
            {
                if (System.Web.HttpContext.Current.Session[NombreVarSession] == null)
                {
                    return null;
                }
                return (CurrentCita)System.Web.HttpContext.Current.Session[NombreVarSession];
            }
            set
            {
                System.Web.HttpContext.Current.Session[NombreVarSession] = value;
            }
        }


        internal string SociedadCita
        {
            get
            {
                if (System.Web.HttpContext.Current.Session[NombreVarSession2] == null)
                {
                    return null;
                }
                return (string)System.Web.HttpContext.Current.Session[NombreVarSession2];
            }
            set
            {
                System.Web.HttpContext.Current.Session[NombreVarSession2] = value;
            }
        }
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        public ActionResult Index()
        {
            LimpiarCita();
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();
            var p = Convert.ToString(cuenta.NombreCuenta);
            var db = new Entities();
            var Borrar = db.RevisarDatos.Where(m => m.Cuenta == p && m.Edo_Rev == 0).ToList();
            var Borrar1 = db.RevisarPedidos.Where(m => m.Cuenta == p).ToList();
            var Borrar2 = db.MensajeResultado.Where(b3 => b3.Cuenta == p).ToList();
            db.RevisarDatos.RemoveRange(Borrar);
            db.SaveChanges();
            db.RevisarPedidos.RemoveRange(Borrar1);
            db.SaveChanges();
            db.MensajeResultado.RemoveRange(Borrar2);
            db.SaveChanges();
            ViewBag.proveedores = _proveedorManager.FindByCuentaId(cuenta.Id);

            ViewBag.Almacenes = CommonManager.GetConfiguraciones().Single(c => c.Clave == "warehouse.warehouses").Valor.Split(',');

            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        public ActionResult SeleccionarProveedor(int proveedorId, string centro, string sociedad)
        {
            if (CurrentCita != null)
            {
                LimpiarCita();
            }

            try
            {
                var cuenta = _commonManager.GetCuentaUsuarioAutenticado();
                CurrentCita = new CurrentCita(cuenta.Id, proveedorId, centro, sociedad);
                SociedadCita = sociedad;

                return RedirectToAction("CargarArchivo");
            }
            catch (Exception exception)
            {
                TempData["FlashError"] = exception.Message;
                return RedirectToAction("Index");
            }
        }
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        public ActionResult SeleccionarProveedores(int proveedorId, string centro, string sociedad)
        {
            if (CurrentCita != null)
            {
                LimpiarCita();
            }

            try
            {
                var cuenta = _commonManager.GetCuentaUsuarioAutenticado();
                CurrentCita = new CurrentCita(cuenta.Id, proveedorId, centro, sociedad);
                SociedadCita = sociedad;

                return RedirectToAction("BuscarOrden");
            }
            catch (Exception exception)
            {
                TempData["FlashError"] = exception.Message;
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        public ActionResult BuscarOrden(int proveedorId = 0)
        {

            try
            {
                if (CurrentCita == null)
                {
                    return RedirectToAction("Index");
                }

            }
            catch (BusinessException exception)
            {
                TempData["FlashError"] = exception.Message;
                return RedirectToAction("Index");
            }

            ViewBag.proveedor = CurrentCita.Proveedor;
            ViewBag.CurrentCita = CurrentCita;

            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Generar(int proveedorId, bool nazan, string ordenesy, bool zapato, string tipoimpresora)
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            var proveedor = _proveedorManager.Find(proveedorId, cuenta.Id);

            if (proveedor == null)
            {
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }
            TempData["nazan"] = nazan;
            TempData["zapato"] = zapato;
            TempData["impresora"] = tipoimpresora;

            if (nazan == false && zapato == false)
            {
                nazan = true;
            }

            var resultado = _PlantillaManager.GetArchivoCsv(proveedorId, cuenta.Id, nazan, ordenesy.Split(','), zapato);

            TempData["resultado"] = resultado;
            TempData["proveedor"] = proveedor;

            if (System.Web.HttpContext.Current.Session[NombreVarSession] != null)
            {
                System.Web.HttpContext.Current.Session.Remove(NombreVarSession);
            }

            var resultados = TempData["resultado"] as Hashtable;
            var proveedore = TempData["proveedor"] as proveedore;
            bool nazans = Convert.ToBoolean(TempData["nazan"]);
            bool zapatos = Convert.ToBoolean(TempData["zapato"]);

            if (resultado == null || proveedor == null)
            {
                return RedirectToAction("Index");
            }


            ViewBag.Proveedor = proveedor;
            ViewBag.Resultado = resultado["return"];

            ViewBag.PuedeDescargar = false;

            if (resultado["csv"] == null) return View();
            if (resultado["csv"].ToString() == "") return View();
            if (string.IsNullOrWhiteSpace(resultado["csv"].ToString())) return View();
            System.Web.HttpContext.Current.Session[NombreVarSession] = resultado["csv"];
            ViewBag.PuedeDescargar = true;
            //CSV to DataTable
            DataTable dt = new DataTable();
            var s = resultado["csv"].ToString();
            s = s.Replace("\"", "");
            string[] tableData = s.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var col = from cl in tableData[0].Split(",".ToCharArray())
                      select new DataColumn(cl);
            dt.Columns.AddRange(col.ToArray());

            foreach (var item in tableData.Skip(1))
            {
                dt.Rows.Add(item.Split(",".ToCharArray()));
            }

            //fin

            //string[] etiquetas;
            List<string> etiquetas = new List<string>();
            List<string> etiquet = new List<string>();
            string etiquetasPrint = "";

            foreach (string etiqueta in etiquetas)
            {
                etiquetasPrint = etiquetasPrint + etiqueta;
            }

            ViewBag.etiquetas = etiquetas;
            ViewBag.etiq = etiquet;
            ViewBag.etiquetasPrint = etiquetasPrint;
            TempData["texto"] = etiquetasPrint;
            return RedirectToAction("Descargar");
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        public FileContentResult Descargar()
        {
            var csv = System.Web.HttpContext.Current.Session["controlCita"].ToString();

            return File(new UTF8Encoding().GetBytes(csv), "text/csv", "ASN.csv");


        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        public ActionResult CargarArchivo(int proveedorId = 0)
        {
            try
            {
                if (CurrentCita == null)
                {
                    return RedirectToAction("Index");
                }

            }
            catch (BusinessException exception)
            {
                TempData["FlashError"] = exception.Message;
                return RedirectToAction("Index");
            }
            ViewBag.proveedor = CurrentCita.Proveedor;
            ViewBag.CurrentCita = CurrentCita;
            var proveedor1 = CurrentCita.Proveedor;
            var p = Convert.ToInt32(proveedor1.NumeroProveedor);
            var db = new Entities();
            ViewBag.Resultado = db.MensajeResultado.Where(m => m.IdProveedor == p).ToList();

            return View();
        }
        [HttpPost]
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        public ActionResult ValidarArchivo(FormCollection collection)
        {
            var file = Request.Files[0];
            if (file != null && file.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                TempData["FlashError"] = "Archivo incorrecto";
                return RedirectToAction("CargarArchivo");
            }
            if (file == null)
            {
                TempData["FlashError"] = "No Ha Seleccionado Ningun Archivo";
                return RedirectToAction("CargarArchivo");
            }
            var fileName = Path.GetFileName(file.FileName);
            if (fileName == null)
            {
                TempData["FlashError"] = "Archivo incorrecto";
                return RedirectToAction("CargarArchivo");
            }
            //var wb = new XLWorkbook(file.InputStream);
            //var ws = wb.Worksheet(1);

            //if (ws == null)
            //{
            //    TempData["FlashError"] = "Archivo incorrecto";
            //    return RedirectToAction("CargarArchivo");
            //}

            //if (ws.RowsUsed().ToList().Count == 0)
            //{
            //    TempData["FlashError"] = "Archivo Nulo";
            //    return RedirectToAction("CargarArchivo");
            //}

            //FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read);
            FileInfo existingFile = new FileInfo(file.FileName);
            var dbb = new Entities();
            //using (FileStream stream = existingFile.Open(FileMode.Open, FileAccess.Read))
            var cuentaaa = _commonManager.GetCuentaUsuarioAutenticado();
            using (Stream stream = file.InputStream)
            //using (FileStream stream = file.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                using (IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream))
                {
                    if(excelReader == null)
                    {
                        TempData["FlashError"] = "Archivo incorrecto";
                        return RedirectToAction("CargarArchivo");
                    }
                    DataSet response = excelReader.AsDataSet();
                    if(response.Tables[0].Rows.Count == 0)
                    {
                        TempData["FlashError"] = "Archivo Nulo";
                        return RedirectToAction("CargarArchivo");
                    }
                    DataSet response2 = excelReader.AsDataSet();
                    List<string[]> articulos =GetDistinctArticulo(response2.Tables[0]);
                    bool rowsband = false;
                    
                    foreach (DataRow dr in response.Tables[0].Rows)
                    {
                        if (!rowsband) { rowsband = true; }
                        else
                        {
                            //Do stuff
                            if (!validadatos(dr[0].ToString(),
                            dr[1].ToString(),
                            dr[2].ToString(),
                            dr[3].ToString(),
                            dr[4].ToString(),
                            dr[5].ToString(),
                            dr[6].ToString()
                            ))
                            {
                                TempData["FlashError"] = "Archivo Con filas nulas o Archivo Con filas con valor en cero";
                                return RedirectToAction("CargarArchivo");
                            }

                            var Carga = dr[0].ToString();
                            var Caja = dr[1].ToString();
                            var Factura = dr[2].ToString();
                            var Pedido = dr[3].ToString();
                            var Tienda = dr[4].ToString();
                            var Ean = dr[5].ToString();
                            var pares = int.Parse(dr[6].ToString());
                            //var result = DatosSap.DatoEan(Ean);
                            //var db = new Entities();
                            var proveedor = CurrentCita.Proveedor;
                            //var cuenta = _commonManager.GetCuentaUsuarioAutenticado();
                            var archivo = new RevisarDatos();
                            archivo.Carga = Carga;
                            archivo.Caja = Caja;
                            archivo.Factura = Factura;
                            archivo.Pedido = Pedido;
                            archivo.Tienda = Tienda;
                            archivo.EAN = Ean;
                            archivo.Pares = pares;
                            archivo.Id_Proveedor = Convert.ToInt32(proveedor.NumeroProveedor);
                            archivo.Cuenta = Convert.ToString(cuentaaa.NombreCuenta);

                            //archivo.Material = result[0];
                            ////archivo.Material = "000000000016957701";
                            //archivo.Estilo = result[3];
                            ////archivo.Estilo = "PT527";
                            //archivo.Color = result[4];
                            ////archivo.Color = "NARANJA";

                            //for(int f=0;f< articulos.Count; f++)
                            ////foreach(string[] reg in articulos)
                            //{
                            //    //if (Ean.Equals(reg[2]))
                            //    if (Ean.Equals(articulos[f][2].ToString()))
                            //    {
                            //        archivo.Material = articulos[f][0].ToString();
                            //        archivo.Estilo = articulos[f][3].ToString();
                            //        archivo.Color = articulos[f][4].ToString();
                            //        f = articulos.Count+1;
                            //        //archivo.Material = reg[0];
                            //        //archivo.Estilo = reg[3];
                            //        //archivo.Color = reg[4];
                            //    }
                            //}
                            //IEnumerable<string> result = articulos.Where(a => a.Contains(Ean)).Select(a => a[0]);
                            try
                            {
                                var reg = articulos.FirstOrDefault(item => item.Contains(Ean));
                                if (reg != null)
                                {
                                    archivo.Material = reg[0];
                                    archivo.Estilo = reg[3];
                                    archivo.Color = reg[4];
                                }
                                else
                                {
                                    archivo.Material = "";
                                    archivo.Estilo = "";
                                    archivo.Color = "";
                                }
                            }
                            catch (Exception ed)
                            {
                                string MsgSelectArticulo=ed.Message;
                                TempData["FlashError"] = MsgSelectArticulo;
                                return RedirectToAction("CargarArchivo");
                            }

                            //Agrega el registro en tabla gnzn_tmp_asn_revision_datos de Mysql
                            dbb.RevisarDatos.Add(archivo);
                            //db.SaveChanges();
                            var orden = CurrentCita._ordenesActivas.FirstOrDefault(o => o.NumeroDocumento == Pedido);
                            if (orden == null)
                            {

                            }
                            else
                            {
                                if (CurrentCita._ordenes.Any(o => o.NumeroDocumento == Pedido))
                                {
                                }
                                else
                                {
                                    CurrentCita.AddPreAsnRecibo(Pedido);

                                    var preAsn = CurrentCita.GetPreAsn(Pedido);
                                    foreach (var preAsnDetail in preAsn.Detalles.Where(preAsnDetail => preAsnDetail.Cantidad > 0))
                                    {
                                        if (preAsnDetail.CantidadPermitida > 0)
                                        {
                                            //var db1 = new Entities();
                                            var sap = new RevisarPedidos();
                                            sap.Pedido = preAsn.NumeroDocumento;
                                            sap.Material = preAsnDetail.NumeroMaterial;
                                            sap.Cantidad = preAsnDetail.Cantidad;
                                            sap.Id_Proveedor = Convert.ToInt32(proveedor.NumeroProveedor);
                                            sap.Numero_linea = preAsnDetail.NumeroPosicion;
                                            //sap.Cuenta = Convert.ToString(cuenta1.NombreCuenta);
                                            sap.Cuenta = Convert.ToString(cuentaaa.NombreCuenta);
                                            //// hace referencia a gnzn_asn_pedidos_disponibles_sap de mysql
                                            //db1.RevisarPedidos.Add(sap);
                                            //db1.SaveChanges();
                                            dbb.RevisarPedidos.Add(sap);


                                        }
                                    }
                                }


                            }
                        }
                    }
                    //REvisar que error si es por la cantidada de registros
                    try
                    {
                        dbb.SaveChanges();
                    }
                    catch(Exception e)
                    {
                        string MsgError = e.Message;
                        TempData["FlashError"] = MsgError;
                        return RedirectToAction("CargarArchivo");
                    }
                }
            }
            
            /*
            //Solucion 1
            string filePath = string.Empty;
            string path = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/");
            string idreg = DateTime.Now.ToString("MMddHHmmss");
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            filePath = path + Path.GetFileName(cuenta.NombreCuenta + "_" + idreg + "_" + file.FileName);
            file.SaveAs(filePath);

            //FileInfo existingFile = new FileInfo(filePath);
            using (ExcelPackage package = new ExcelPackage(existingFile)) {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                int colCount = worksheet.Dimension.End.Column;
                //int rowCount = worksheet.Dimension.End.Row;                
                int rowCount = worksheet.Cells.Where(cell => !string.IsNullOrEmpty(cell.Value?.ToString() ?? string.Empty)).LastOrDefault().End.Row;
                //int lastRow = worksheet.Cells.Where(cell => !cell.Value.ToString().Equals("")).Last().End.Row;
                for (int row = 2; row <= rowCount; row++)
                {
                    if (!validadatos(worksheet.Cells[row, 1].Value?.ToString().Trim(), 
                        worksheet.Cells[row, 2].Value?.ToString().Trim(), 
                        worksheet.Cells[row, 3].Value?.ToString().Trim(), 
                        worksheet.Cells[row, 4].Value?.ToString().Trim(), 
                        worksheet.Cells[row, 5].Value?.ToString().Trim(),
                        worksheet.Cells[row, 6].Value?.ToString().Trim(), 
                        worksheet.Cells[row, 7].Value?.ToString().Trim() 
                        ))
                    {
                        TempData["FlashError"] = "Archivo Con filas nulas o Archivo Con filas con valor en cero";
                        return RedirectToAction("CargarArchivo");
                    }

                    var Carga   = worksheet.Cells[row, 1].Value?.ToString().Trim();
                    var Caja    = worksheet.Cells[row, 2].Value?.ToString().Trim();
                    var Factura = worksheet.Cells[row, 3].Value?.ToString().Trim();
                    var Pedido  = worksheet.Cells[row, 4].Value?.ToString().Trim();
                    var Tienda  = worksheet.Cells[row, 5].Value?.ToString().Trim();
                    var Ean     = worksheet.Cells[row, 6].Value?.ToString().Trim();
                    var pares   = int.Parse(worksheet.Cells[row, 7].Value?.ToString().Trim());
                    //var result  = DatosSap.DatoEan(Ean);
                    var db = new Entities();
                    var proveedor = CurrentCita.Proveedor;
                    //var cuenta = _commonManager.GetCuentaUsuarioAutenticado();
                    var archivo = new RevisarDatos();
                    archivo.Carga = Carga;
                    archivo.Caja = Caja;
                    archivo.Factura = Factura;
                    archivo.Pedido = Pedido;
                    archivo.Tienda = Tienda;
                    archivo.EAN = Ean;
                    archivo.Pares = pares;
                    //archivo.Material = result[0];
                    archivo.Material = "000000000016957701";
                    archivo.Id_Proveedor = Convert.ToInt32(proveedor.NumeroProveedor);
                    //archivo.Estilo = result[3];
                    archivo.Estilo = "PT527";
                    //archivo.Color = result[4];
                    archivo.Color = "NARANJA";
                    archivo.Cuenta = Convert.ToString(cuenta.NombreCuenta);
                    db.RevisarDatos.Add(archivo);
                    db.SaveChanges();
                    var orden = CurrentCita._ordenesActivas.FirstOrDefault(o => o.NumeroDocumento == Pedido);
                    if (orden == null)
                    {

                    }
                    else
                    {
                        if (CurrentCita._ordenes.Any(o => o.NumeroDocumento == Pedido))
                        {
                        }
                        else
                        {
                            CurrentCita.AddPreAsnRecibo(Pedido);
                        }


                    }

                }
            }
            */
            //BACK
            /*
            for (var i = 2; i < ws.RowsUsed().ToList().Count + 1; i++)
            {


                if (ws.Row(i).Cell(1).Value.ToString() == "" || ws.Row(i).Cell(2).Value.ToString() == "" ||
                    ws.Row(i).Cell(3).Value.ToString() == "" || ws.Row(i).Cell(4).Value.ToString() == "" ||
                    ws.Row(i).Cell(5).Value.ToString() == "" || ws.Row(i).Cell(6).Value.ToString() == "" ||
                    ws.Row(i).Cell(7).Value.ToString() == "")
                {

                    TempData["FlashError"] = "Archivo Con filas nulas";
                    return RedirectToAction("CargarArchivo");
                }

                if (ws.Row(i).Cell(1).Value.ToString() == "0" || ws.Row(i).Cell(2).Value.ToString() == "0" ||
                ws.Row(i).Cell(3).Value.ToString() == "0" || ws.Row(i).Cell(4).Value.ToString() == "0" ||
                ws.Row(i).Cell(5).Value.ToString() == "0" || ws.Row(i).Cell(6).Value.ToString() == "0" ||
                ws.Row(i).Cell(7).Value.ToString() == "0")
                {

                    TempData["FlashError"] = "Archivo Con filas con valor en cero";
                    return RedirectToAction("CargarArchivo");
                }

                var Carga = ws.Row(i).Cell(1).Value.ToString();
                var Caja = ws.Row(i).Cell(2).Value.ToString();
                var Factura = ws.Row(i).Cell(3).Value.ToString();
                var Pedido = ws.Row(i).Cell(4).Value.ToString();
                var Tienda = ws.Row(i).Cell(5).Value.ToString();
                var Ean = ws.Row(i).Cell(6).Value.ToString();
                var pares = int.Parse(ws.Row(i).Cell(7).Value.ToString());
                var result = DatosSap.DatoEan(Ean);
                var db = new Entities();
                var proveedor = CurrentCita.Proveedor;
                //var cuenta = _commonManager.GetCuentaUsuarioAutenticado();
                var archivo = new RevisarDatos();
                archivo.Carga = Carga;
                archivo.Caja = Caja;
                archivo.Factura = Factura;
                archivo.Pedido = Pedido;
                archivo.Tienda = Tienda;
                archivo.EAN = Ean;
                archivo.Pares = pares;
                archivo.Material = result[0];
                archivo.Id_Proveedor = Convert.ToInt32(proveedor.NumeroProveedor);
                archivo.Estilo = result[3];
                archivo.Color = result[4];
                archivo.Cuenta = Convert.ToString(cuenta.NombreCuenta);                
                db.RevisarDatos.Add(archivo);
                db.SaveChanges();
                var orden = CurrentCita._ordenesActivas.FirstOrDefault(o => o.NumeroDocumento == Pedido);
                if (orden == null)
                {

                }
                else
                {
                    if (CurrentCita._ordenes.Any(o => o.NumeroDocumento == Pedido))
                    {
                    }
                    else
                    {
                        CurrentCita.AddPreAsnRecibo(Pedido);
                    }


                }
            }
            */
            ////var db2 = new Entities();
            //var proveedor2 = CurrentCita.Proveedor;
            ////var cuenta1 = _commonManager.GetCuentaUsuarioAutenticado();
            //foreach (var preAsn in CurrentCita.GetPreAsns())
            //{
            //    foreach (var preAsnDetail in preAsn.Detalles.Where(preAsnDetail => preAsnDetail.Cantidad > 0))
            //    {
            //        if (preAsnDetail.CantidadPermitida > 0)
            //        {
            //            //var db1 = new Entities();
            //            var sap = new RevisarPedidos();
            //            sap.Pedido = preAsn.NumeroDocumento;
            //            sap.Material = preAsnDetail.NumeroMaterial;
            //            sap.Cantidad = preAsnDetail.Cantidad;
            //            sap.Id_Proveedor = Convert.ToInt32(proveedor2.NumeroProveedor);
            //            sap.Numero_linea = preAsnDetail.NumeroPosicion;
            //            //sap.Cuenta = Convert.ToString(cuenta1.NombreCuenta);
            //            sap.Cuenta = Convert.ToString(cuentaaa.NombreCuenta);
            //            //// hace referencia a gnzn_asn_pedidos_disponibles_sap de mysql
            //            //db1.RevisarPedidos.Add(sap);
            //            //db1.SaveChanges();
            //            dbb.RevisarPedidos.Add(sap);


            //        }
            //    }
            //    dbb.SaveChanges();
            //}
            
            ViewBag.NumeroDocumento = CurrentCita._ordenes;
            //var cuentas = _commonManager.GetCuentaUsuarioAutenticado();
            var proveedor1 = CurrentCita.Proveedor;
            //var prov = Convert.ToInt32(proveedor2.NumeroProveedor);
            var prov = Convert.ToInt32(proveedor1.NumeroProveedor);
            //var carga = db2.RevisarDatos.Where(crg => crg.Id_Proveedor == prov && crg.Edo_Rev == 0).FirstOrDefault();
            var carga = dbb.RevisarDatos.Where(crg => crg.Id_Proveedor == prov && crg.Edo_Rev == 0).FirstOrDefault();
            var parameters = new List<MySqlParameter>()
                {
                  new MySqlParameter("IDProv",proveedor1.NumeroProveedor ),
                  new MySqlParameter("Cuenta",cuentaaa.NombreCuenta),
                  new MySqlParameter("Car",carga.Carga),
                };

            Db.ExecuteProcedureOut(parameters, "sp_valida_archivoASN");
            //var db3 = new Entities();
            var proveedorval = CurrentCita.Proveedor;
            var pov = Convert.ToInt32(proveedor1.NumeroProveedor);
            //MensajeResultado hace referencia a la tabla gnzn_asn_resultado_de_sprev_asn de Mysql
            //var validarped = db3.MensajeResultado.Where(m => m.IdProveedor == pov && m.MsjError == "Pedido No Valido en SAP").GroupBy(x => x.DatoConDetalle).Select(x => x.FirstOrDefault());
            var validarped = dbb.MensajeResultado.Where(m => m.IdProveedor == pov && m.MsjError == "Pedido No Valido en SAP").GroupBy(x => x.DatoConDetalle).Select(x => x.FirstOrDefault());
            foreach (var csp in validarped)
            {
                // *******************************
                //valida el error de MYSQL con SAP para ver el status de la orden
                // *******************************
                var result = DatosSap.MensajeError(csp.DatoConDetalle);
                // variable result se llena con los errores de SAP. 
                //11 Total de errores que puede regresar
                if (result[0] == "" && result[1] == "" && result[2] == "" && result[3] == "" && result[4] == "" && result[5] == "" && result[6] == ""
                    && result[7] == "" && result[8] == "" && result[9] == "" && result[10] == "")
                {

                }
                else
                {
                    //var cuenta = _commonManager.GetCuentaUsuarioAutenticado();
                    //var db1 = new Entities();
                    csp.MsjError = Convert.ToString(csp.DatoConDetalle);
                    csp.DatoConDetalle = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}", result[0], result[1], result[2], result[3], result[4], result[5], result[6], result[7], result[8], result[9], result[10], "/", "Favor de comunicarse con su comprador");
                    //csp.Cuenta = Convert.ToString(cuenta1.NombreCuenta);
                    csp.Cuenta = Convert.ToString(cuentaaa.NombreCuenta);
                    //db1.Entry(csp).State = EntityState.Modified;
                    //db1.SaveChanges();
                    dbb.Entry(csp).State = EntityState.Modified;
                    dbb.SaveChanges();
                }


            }
            //var archivo1 = db2.MensajeResultado.FirstOrDefault(val => val.IdProveedor == pov);
            var archivo1 = dbb.MensajeResultado.FirstOrDefault(val => val.IdProveedor == pov);
            if (archivo1 == null)
            {
                TempData["FlashSuccess"] = "Archivo cargado exitosamente";
                return RedirectToAction("FechaCita");
            }
            else
            {
                //var db = new Entities();
                var proveedorc = CurrentCita.Proveedor;
                var p = Convert.ToInt32(proveedor1.NumeroProveedor);
                //var Borrar = db.RevisarDatos.Where(m => m.Id_Proveedor == p && m.Edo_Rev == 0).ToList();
                //var Borrarpedidos = db.RevisarPedidos.Where(m => m.Id_Proveedor == p).ToList();
                //db.RevisarDatos.RemoveRange(Borrar);
                //db.SaveChanges();
                //db.RevisarPedidos.RemoveRange(Borrarpedidos);
                //db.SaveChanges();
                var Borrar = dbb.RevisarDatos.Where(m => m.Id_Proveedor == p && m.Edo_Rev == 0).ToList();
                var Borrarpedidos = dbb.RevisarPedidos.Where(m => m.Id_Proveedor == p).ToList();
                dbb.RevisarDatos.RemoveRange(Borrar);
                dbb.SaveChanges();
                dbb.RevisarPedidos.RemoveRange(Borrarpedidos);
                dbb.SaveChanges();

                TempData["FlashError"] = "Favor de checar sus errores en el archivo ";
            }
            return RedirectToAction("CargarArchivo");
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        public ActionResult FechaCita()
        {

            if (CurrentCita == null)
            {
                return RedirectToAction("Index");
            }

            if (CurrentCita.Fecha != null)
            {
                // TODO redireccionar a la seccion de busqueda o al checkout
                return RedirectToAction("Index");
            }
            var db5 = new Entities();
            var prov = CurrentCita.Proveedor;
            var pro = Convert.ToInt32(prov.NumeroProveedor);
            var archivos = db5.RevisarDatos.Where(prove => prove.Id_Proveedor == pro && prove.Edo_Rev == 0).GroupBy(x => x.Pedido).Select(x => x.FirstOrDefault());
            db5.Database.CommandTimeout = 0;
            foreach (var ordencompras in archivos)
            {
                var numeroDocumento = ordencompras.Pedido;
                CurrentCita.RemovePreAsnRecibo(numeroDocumento);
            }

            var proveedor = CurrentCita.Proveedor;
            var proveedor1 = CurrentCita.Proveedor;
            var p = Convert.ToInt32(proveedor1.NumeroProveedor);
            var db = new Entities();
            var Borrar = db.MensajeResultado.Where(m => m.IdProveedor == p).ToList();
            db.MensajeResultado.RemoveRange(Borrar);
            db.SaveChanges();
            var archivo1 = db.RevisarDatos.Where(provee => provee.Id_Proveedor == p && provee.Edo_Rev == 0).GroupBy(x => x.Pedido).Select(x => x.FirstOrDefault());
            db.Database.CommandTimeout = 0;
            foreach (var ordencompra in archivo1)
            {
                var numeroDocumento = ordencompra.Pedido;
                var orden = CurrentCita.GetOrdenActivaDisponible(numeroDocumento);

                if (orden == null)
                {
                    LimpiarCita();
                    TempData["FlashError"] = "Número de documento incorrecto";
                    return RedirectToAction("CargarArchivo");

                }


                ViewBag.Fechas = orden.FechasPermitidas;

                ViewBag.NumeroDocumento = numeroDocumento;
                ViewBag.proveedor = proveedor;

                ViewBag.CurrentCita = CurrentCita;
            }
            return View();
        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        public ActionResult VisualizarDisponibilidad(string numeroDocumento, string fecha, string fechaspermitidas)
        {
            var date = DateTime.ParseExact(fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var FechasPermitidas = new JavaScriptSerializer().Deserialize<List<DateTime>>(fechaspermitidas);

            try
            {
                var parameters = new List<MySqlParameter>()
                {
                    new MySqlParameter
                    {
                        ParameterName = "pTotal",
                        Direction = ParameterDirection.Output,
                        MySqlDbType = MySqlDbType.VarChar
                    },
                    new MySqlParameter("pFecha", date)
                };

                Db.ExecuteProcedureOut(parameters, "config_appointment");
            }
            catch (Exception exception)
            {
                TempData["FlashError"] = exception.Message;
                return RedirectToAction("CargarArchivo");
            }
            var db = new Entities();
            var horarioRieles = db.horariorieles.Where(h => h.Fecha == date.Date).ToList();
            ViewBag.HorarioRieles = horarioRieles;
            ViewBag.date = date;
            ViewBag.numeroDocumento = numeroDocumento;
            ViewBag.CurrentCita = CurrentCita;
            ViewBag.Fechas = FechasPermitidas;
            //fin
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        public ActionResult AgregarPrimeraOrden(string numeroDocumento, string fecha)
        {
            string[] ordenes = numeroDocumento.Split(',');
            foreach (var ordencompra in ordenes)
            {
                if (CurrentCita == null)
                {
                    return RedirectToAction("Index");
                }
                var proveedor = CurrentCita.Proveedor;
                try
                {
                    var db = new Entities();
                    var proveedor1 = CurrentCita.Proveedor;
                    var p = Convert.ToInt32(proveedor1.NumeroProveedor);
                    var archivo1 = db.RevisarDatos.Where(pro => pro.Id_Proveedor == p && pro.Edo_Rev == 0).GroupBy(x => x.Pedido).Select(x => x.FirstOrDefault());
                    db.Database.CommandTimeout = 0;
                    foreach (var orden in archivo1)
                    {
                        var numero = orden.Pedido;
                        CurrentCita.SetFechaAsn(DateTime.ParseExact(fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture), ordencompra);
                        CurrentCita.AddPreAsnRecibo(numero);
                    }

                }
                catch (CurrentCita.OrdenDuplicadaException)
                {
                    TempData["FlashError"] = "El número de documento ya se encuentra en la lista";
                    return RedirectToAction("CargarArchivo", new { proveedor.Id });
                }
                catch (CurrentCita.OrdenSinDetalleException)
                {
                    TempData["FlashError"] = "La orden no contiene items para entregar, por favor seleccione otra orden";
                    return RedirectToAction("CargarArchivo", new { proveedor.Id });
                }
                catch (CurrentCita.NumeroDocumentoException)
                {
                    TempData["FlashError"] = "Número de documento incorrecto";
                    return RedirectToAction("CargarArchivo", new { proveedor.Id });
                }
                catch (BusinessException exception)
                {
                    TempData["FlashError"] = exception.Message;
                    return RedirectToAction("CargarArchivo", new { proveedor.Id });
                }
                catch (CurrentCita.FechaException)
                {
                    TempData["FlashError"] = "La orden no puede ser entregada en la fecha de la cita";
                    return RedirectToAction("CargarArchivo", new { proveedor.Id });
                }
                catch (CurrentCita.OrdenCentroException)
                {
                    TempData["FlashError"] = "La orden no contiene items para el Almacén seleccionado";
                    return RedirectToAction("CargarArchivo", new { proveedor.Id });
                }
            }
            var db2 = new Entities();
            var proveedor6 = CurrentCita.Proveedor;
            var pvr = Convert.ToInt32(proveedor6.NumeroProveedor);
            var revisar = db2.RevisarDatos.Where(prr => prr.Id_Proveedor == pvr && prr.Edo_Rev == 0).ToList();
            var pedidos = db2.RevisarPedidos.Where(rp => rp.Id_Proveedor == pvr).ToList();
            (from p in db2.RevisarPedidos
             where p.Id_Proveedor == pvr
             select p).ToList()
           .ForEach(x => x.Cantidad = 0);
            db2.SaveChanges();

            foreach (var o in revisar)
            {
                var preAsn = pedidos.FirstOrDefault(or => or.Material == o.Material && or.Pedido == o.Pedido);
                var dato = revisar.Where(x => x.Material == preAsn.Material && x.Pedido == preAsn.Pedido).Sum(x => x.Pares);
                if (preAsn == null)
                {
                    return RedirectToAction("FechaCita");

                }
                if (preAsn.Material == o.Material && preAsn.Pedido == o.Pedido)
                {
                    preAsn.Cantidad = dato;
                    db2.Entry(preAsn).State = EntityState.Modified;
                    db2.SaveChanges();

                }
                else
                {
                    preAsn.Cantidad = 0;
                    db2.Entry(preAsn).State = EntityState.Modified;
                    db2.SaveChanges();
                }

            }
            var archivo2 = db2.RevisarPedidos.Where(rp => rp.Id_Proveedor == pvr).ToList();
            foreach (var orden in archivo2)
            {
                var numero = orden.Pedido;
                var numero1 = orden.Material;
                var numero2 = orden.Cantidad;

                CurrentCita.UpdateDetailAsn(numero, numero1, numero2);

            }
            return RedirectToAction("SeleccionarRieles", new { numeroDocumento });
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        public ActionResult SeleccionarRieles()
        {
            var result = CommonManager.GetConfiguraciones().Single(c => c.Clave == "warehouse.min-pairs.per-meet");
            if (String.IsNullOrEmpty(result.Valor))
            {
                ViewBag.CantidadMinimaCita = 270;
            }
            else
            {
                ViewBag.CantidadMinimaCita = Convert.ToInt32(result.Valor);
            }

            if (CurrentCita == null)
            {
                return RedirectToAction("Index");
            }

            if (CurrentCita.Fecha == null)
            {
                return RedirectToAction("CargarArchivo");
            }

            if (CurrentCita.Cantidad < 1)
            {
                TempData["FlashError"] = "Debe incluir al menos un (1) PAR para poder agendar la Cita";
                return RedirectToAction("CargarArchivo");
            }

            var date = ((DateTime)CurrentCita.Fecha).Date;

            try
            {
                var parameters = new List<MySqlParameter>()
                {
                    new MySqlParameter
                    {
                        ParameterName = "pTotal",
                        Direction = ParameterDirection.Output,
                        MySqlDbType = MySqlDbType.VarChar
                    },
                    new MySqlParameter("pFecha", date)
                };

                Db.ExecuteProcedureOut(parameters, "config_appointment");
            }
            catch (Exception exception)
            {
                TempData["FlashError"] = exception.Message;
                return RedirectToAction("CargarArchivo");
            }

            ViewBag.CurrentCita = CurrentCita;

            var db = new Entities();

            var horarioRieles = db.horariorieles.Where(h => h.Fecha == date.Date).ToList();

            ViewBag.HorarioRieles = horarioRieles;
            return View();
        }

        public JsonResult VerificarRieles(string fecha)
        {
            var date = DateTime.ParseExact(fecha, "ddMMyyyy", CultureInfo.InvariantCulture);

            var db = new Entities();
            var horarioRieles = db.horariorieles.Where(hr => hr.Fecha == date).ToList();


            return Json(
                horarioRieles.Select(hr => new
                {
                    hr.Id,
                    hr.Disponibilidad
                }));
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Agendar(int[] rielesIds, DateTime FechaCreacion)
        {

            if (CurrentCita.Fecha == null)
            {
                // Todo mejorar
                return RedirectToAction("Citas");
            }
            var preCita = new PreCita()
            {
                Cantidad = CurrentCita.Cantidad,
                Centro = CurrentCita.Centro,
                Fecha = (DateTime)CurrentCita.Fecha,
                ProveedorId = CurrentCita.Proveedor.Id,
                UsuarioId = _commonManager.GetUsuarioAutenticado().Id,
                Asns = new List<Asn>(),
                HorarioRielesIds = rielesIds.ToList(),
                FechaCreacion = (DateTime)FechaCreacion,
                Sociedad = SociedadCita,
                TipoCita = "Cita Por Asn"
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
                        NumeroSurtido = preAsn.NumeroOrdenSurtido,
                        NumeroMaterial2 = preAsnDetail.NumeroMaterial2,

                        Centro = preAsn.Centro
                    });
                }
            }

            try
            {
                // podemos pasar las ordenes activar del sap en CurrentCita._ordenesActivas
                CitaManager.RegistrarCitaAsn(preCita,CurrentCita._ordenesActivas);
            }
            catch (ScaleException exception)
            {
                ErrorAppLog.Error(exception.Message, exception);
               LimpiarCita();
                TempData["FlashError"] = exception.Message;
                return RedirectToAction("Citas");
            }
            catch (Exception exception)
            {
                ErrorAppLog.Error(exception.Message, exception);
                //TODO
                TempData["FlashError"] = exception.Message;
                return RedirectToAction("SeleccionarRieles");
            }
            //TODO
            var db = new Entities();
            var proveedorc = CurrentCita.Proveedor;
            var p = Convert.ToInt32(proveedorc.NumeroProveedor);

            var borrarPedi = db.RevisarPedidos.Where(m => m.Id_Proveedor == p).ToList();
            db.RevisarPedidos.RemoveRange(borrarPedi);
            db.SaveChanges();
            LimpiarCita();
            //PARA QUE PUSO EL SLEEP
            //System.Threading.Thread.Sleep(210000);
            TempData["FlashSuccess"] = "Ha terminado de configurar su cita exitosamente";
            return RedirectToAction("Citas");
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        public ActionResult Citas()
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();
            var proveedoresIds = _proveedorManager.FindByCuentaId(cuenta.Id).Select(p => p.Id).ToList();
            var db = new Entities();
            var fecha = DateTime.Today.Date;
            var citas = db.citas.Where(c => proveedoresIds.Contains(c.ProveedorId) && c.FechaCita >= fecha && c.TipoCita == "Cita Por Asn").ToList();
            //MySqlDataReader rd = Db.GetDataReader("call sp_GetInsertToScale("+ citas[0].Id.ToString() +");");
            //DataSet rd = Db.GetDataReader("call sp_GetInsertToScale(" + citas[0].Id.ToString() + ");");
            ViewBag.Citas = citas;
            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        public ActionResult CitaDetalle(int citaId)
        {
            TempData["FlashError"] = "Las Citas Con ASn No Se Pueden Modificar";
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();
            var proveedoresIds = _proveedorManager.FindByCuentaId(cuenta.Id).Select(p => p.Id).ToList();
            var db = new Entities();
            var cita = db.citas.FirstOrDefault(c => c.Id == citaId && proveedoresIds.Contains(c.ProveedorId));
            if (cita == null)
            {
                //TODO
                TempData["FlashError"] = "Cita incorrecta";
                return RedirectToAction("Citas");
            }
            ViewBag.Cita = cita;
            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        public ActionResult CancelarCita(int citaId)
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();
            var proveedoresIds = _proveedorManager.FindByCuentaId(cuenta.Id).Select(p => p.Id).ToList();
            var db = new Entities();
            var cita = db.citas.FirstOrDefault(c => proveedoresIds.Contains(c.ProveedorId) && c.Id == citaId);
            var proveedor = cita.proveedore;
            var pvr = Convert.ToInt32(proveedor.NumeroProveedor);
            var borrardatos = db.RevisarDatos.Where(m => m.CitaId == citaId && m.Id_Proveedor == pvr).ToList();
            var borrardatos0 = db.RevisarDatos.Where(m => m.Edo_Rev == 0 && m.Id_Proveedor == pvr).ToList();
            var borrareti = db.EnviarEtiquetas.Where(m => m.cita == citaId && m.Id_Proveedor == pvr).ToList();
            var borrarCon = db.EnviarDatos.Where(m => m.CitaId == citaId && m.Id_Proveedor == pvr).ToList();
            db.RevisarDatos.RemoveRange(borrardatos);
            db.RevisarDatos.RemoveRange(borrardatos0);
            db.EnviarEtiquetas.RemoveRange(borrareti);
            db.EnviarDatos.RemoveRange(borrarCon);
            db.SaveChanges();
            if (cita == null)
            {
                //TODO
                TempData["FlashError"] = "Cita incorrecta";
                return RedirectToAction("Citas");
            }
            if (!RulesManager.PuedeCancelarCita(cita.FechaCita))
            {
                //TODO
                TempData["FlashError"] = "La cita no puede ser Cancelada";
                return RedirectToAction("Citas");
            }
            CitaManager.CancelarCitaAsn(citaId);
            //TODO
            TempData["FlashSuccess"] = "Cita cancelada exitosamente";
            return RedirectToAction("Citas");
        }


        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        public ActionResult Resultado(int citaId)
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();
            var proveedoresIds = _proveedorManager.FindByCuentaId(cuenta.Id).Select(p => p.Id).ToList();
            var db2 = new Entities();
            var cita = db2.citas.FirstOrDefault(c => c.Id == citaId && proveedoresIds.Contains(c.ProveedorId));
            if (!RulesManager.PuedeImprimir(cita.FechaCreacion))
            {

                TempData["FlashError"] = "Espere un momento se estan procesando las etiquetas";
                return RedirectToAction("Citas");
            }
            var proveedor = TempData["proveedor"] as proveedore;
            ViewBag.Proveedor = proveedor;
            ViewBag.PuedeDescargar = false;
            ViewBag.PuedeDescargar = true;
            List<string> etiquetas = new List<string>();
            string etiquetasPrint = "";

            var db = new Entities();
            var archivo1 = db.EnviarEtiquetas.Where(et => et.cita == citaId).ToList();
            foreach (var et in archivo1)
            {

                etiquetas.Add(@"^XA
^JZN
^PR9			
^PQ1
^FO0,10^A0N ,45,25^FD" + et.carga + @"^FS
^FO0,65^A0N ,150,70^FD" + et.cita + @"^FS
^FO230,5^BY,2.5^BAN,170Y^FD" + et.recibo + @"^FS
^FO5,195^A0N ,45,25^FDPares:^FS
^FO60,190^A0N ,55,55^FD " + et.pares + @"^FS
^FO230,218^A0N ,30,25^FDTienda:^FS
^FO300,218^A0N ,30,25^FD " + et.tienda + @"^FS
^FO400,218^A0N ,30,25^FDIDProv:^FS
^FO500,218^A0N ,30,25^FD" + et.Id_Proveedor + @"^FS
^FO200,250^BY,2.1^BAN,122Y^FD" + et.caja + @"^FS
^FO0,240^A0N ,25,25^FD Color:^FS
^FO0,265^A0N ,35,35^FD" + et.color + @"^FS
^FO0,310^A0N ,25,25^FDEstilo:^FS
^FO80,305^A0N ,35,35^FD" + et.estilo + @"^FS
^FO0,340^A0N ,25,25^FDFactura:^FS
^FO0,370^A0N ,35,35^FD" + et.factura + @"^FS
^XZ");
            }


            foreach (string etiqueta in etiquetas)
            {
                etiquetasPrint = etiquetasPrint + etiqueta;
            }

            ViewBag.etiquetas = etiquetas;
            ViewBag.etiquetasPrint = etiquetasPrint;
            TempData["texto"] = etiquetasPrint;
            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-IMPRESIONETIQUETAS")]
        public FileContentResult ImpresionEtiquetas()
        {
            var texto = TempData["texto"].ToString();

            return File(TextToByte(texto), System.Net.Mime.MediaTypeNames.Application.Octet, "Impresion Etiquetas.txt");
        }
        public static byte[] TextToByte(string texto)
        {
            // convert string to stream
            byte[] byteArray = Encoding.ASCII.GetBytes(texto);
            return byteArray;
        }


        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        public ActionResult Individual(int citaId)
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            var proveedoresIds = _proveedorManager.FindByCuentaId(cuenta.Id).Select(p => p.Id).ToList();

            var db = new Entities();
            var cita = db.citas.FirstOrDefault(c => c.Id == citaId && proveedoresIds.Contains(c.ProveedorId));
            if (cita == null)
            {
                //TODO
                TempData["FlashError"] = "Cita incorrecta";
                return RedirectToAction("Citas");
            }
            if (!RulesManager.PuedeImprimir(cita.FechaCreacion))
            {

                TempData["FlashError"] = "Espere un momento se estan procesando las etiquetas";
                return RedirectToAction("Citas");
            }
            var db2 = new Entities();

            ViewBag.etiq = db2.EnviarEtiquetas.Where(csa => csa.cita == citaId).ToList();

            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CONTROLCITAS")]
        public ActionResult Resultadoind(string cantidades, string materiales)
        {
            string[] ordenes = cantidades.Split(',');
            var proveedor = TempData["proveedor"] as proveedore;
            ViewBag.Proveedor = proveedor;
            ViewBag.PuedeDescargar = false;
            ViewBag.PuedeDescargar = true;
            List<string> etiquetas = new List<string>();
            string etiquetasPrint = "";
            foreach (var ordencompra in ordenes)
            {

                var db = new Entities();
                var archivo1 = db.EnviarEtiquetas.Where(et => et.caja == ordencompra).FirstOrDefault();


                etiquetas.Add(@"^XA
^JZN
^PR9			
^PQ1
^FO0,10^A0N ,45,25^FD" + archivo1.carga + @"^FS
^FO0,65^A0N ,150,70^FD" + archivo1.cita + @"^FS
^FO230,5^BY,2.5^BAN,170Y^FD" + archivo1.recibo + @"^FS
^FO5,195^A0N ,45,25^FDPares:^FS
^FO60,190^A0N ,55,55^FD" + archivo1.pares + @"^FS
^FO230,218^A0N ,30,25^FDTienda:^FS
^FO300,218^A0N ,30,25^FD " + archivo1.tienda + @"^FS
^FO400,218^A0N ,30,25^FDIDProv:^FS
^FO500,218^A0N ,30,25^FD" + archivo1.Id_Proveedor + @"^FS
^FO200,250^BY,2.1^BAN,122Y^FD" + ordencompra + @"^FS
^FO0,240^A0N ,25,25^FD Color:^FS
^FO0,265^A0N ,35,35^FD" + archivo1.color + @"^FS
^FO0,310^A0N ,25,25^FDEstilo:^FS
^FO80,305^A0N ,35,35^FD" + archivo1.estilo + @"^FS
^FO0,340^A0N ,25,25^FDFactura:^FS
^FO0,370^A0N ,35,35^FD" + archivo1.factura + @"^FS
^XZ");

            }

            foreach (string etiqueta in etiquetas)
            {
                etiquetasPrint = etiquetasPrint + etiqueta;
            }

            ViewBag.etiquetas = etiquetas;
            ViewBag.etiquetasPrint = etiquetasPrint;
            TempData["texto"] = etiquetasPrint;


            return View();
        }

        private bool validadatos(
            string CARGA    ,string CAJA 
            ,string FACTURA ,string PEDIDO
            ,string TIENDA  ,string EAN 
            ,string PARES)
        {
            if (string.IsNullOrEmpty(CARGA)     || 
                string.IsNullOrEmpty(CAJA)      ||
                string.IsNullOrEmpty(FACTURA)   ||
                string.IsNullOrEmpty(PEDIDO)    ||
                string.IsNullOrEmpty(TIENDA)    ||
                string.IsNullOrEmpty(EAN)       ||
                string.IsNullOrEmpty(PARES)
                )
                return false;

            if (CARGA.Equals("0")   ||
                CAJA.Equals("0")    ||
                FACTURA.Equals("0") ||
                PEDIDO.Equals("0")  ||
                TIENDA.Equals("0")  ||
                EAN.Equals("0")     ||
                PARES.Equals("0")
                )
                return false;
            
            return true;
        }

        private List<string[]> GetDistinctArticulo(DataTable Lst)
        {
            List<string[]> regreso = new List<string[]>();
            //List<string> dtsindupl = new List<string>();
            //bool band = false;
            //foreach (DataRow dr in Lst.Rows)
            //{
            //    if (!band) band = true;
            //    else
            //        if (!dtsindupl.Contains(dr[5].ToString()))
            //            dtsindupl.Add(dr[5].ToString());
            //}
            Lst.Columns.Remove("Column0");
            Lst.Columns.Remove("Column1");
            Lst.Columns.Remove("Column2");
            Lst.Columns.Remove("Column3");
            Lst.Columns.Remove("Column4");
            Lst.Columns.Remove("Column6");
            DataView vista = new DataView(Lst);
            DataTable dtsindupl = vista.ToTable(true);
            regreso=SapLoadDatosArticilos(dtsindupl);
            return regreso;

        }

        //private List<string[]> SapLoadDatosArticilos(List<string> dtsindupl)
        private List<string[]> SapLoadDatosArticilos(DataTable dtsindupl)
        {
            List<string[]> regreso = new List<string[]>();
            //foreach (string dr in dtsindupl)
            bool band = false;
            foreach (DataRow dr in dtsindupl.Rows)
            {
                if (!band) band = true;
                else
                //string articulo = dr[0].ToString();
                    regreso.Add(DatosSap.DatoEan(dr[0].ToString()));
                //regreso.Add(DatosSap.DatoEan(dr));
            }
            return regreso;
            
        }
    }
}