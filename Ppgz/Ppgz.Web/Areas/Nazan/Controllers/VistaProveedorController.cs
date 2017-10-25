using System;
using System.Web.Mvc;
using System.Globalization;
using Ppgz.Repository;
using Ppgz.Services;
using Ppgz.Web.Areas.Nazan.Models;
using Ppgz.Web.Infrastructure;
using System.Linq;
using ClosedXML.Excel;
using System.Collections;
using System.Data;
using System.Collections.Generic;
using System.Text;

namespace Ppgz.Web.Areas.Nazan.Controllers
{
    public class VistaProveedorController : Controller
    {

        private readonly Entities _db = new Entities();

        readonly ProveedorManager _proveedorManager = new ProveedorManager();
        readonly CuentaManager _cuentaManager = new CuentaManager();
        readonly CommonManager _commonManager = new CommonManager();
        readonly OrdenCompraManager _ordenCompraManager = new OrdenCompraManager();
        private readonly EtiquetasManager _etiquetasManager = new EtiquetasManager();
        readonly ReporteProveedorManager _reporteProveedorManager = new ReporteProveedorManager();

        private const string NombreVarSession = "etiqueta_csv";

        internal proveedore ProveedorActivo
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["proveedoractivo"] != null)
                {
                    return (proveedore)System.Web.HttpContext.Current.Session["proveedoractivo"];
                }
                return null;
            }
            set
            {
                System.Web.HttpContext.Current.Session["proveedoractivo"] = value;
            }
        }

        internal CuentaConUsuarioMaestro CuentaActiva
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["cuentaactiva"] != null)
                {
                    return (CuentaConUsuarioMaestro)System.Web.HttpContext.Current.Session["cuentaactiva"];
                }
                return null;
            }
            set
            {
                System.Web.HttpContext.Current.Session["cuentaactiva"] = value;
            }
        }


        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-VISTAPROVEEDOR")]
        public ActionResult Index()
        {
            var cuentas = _cuentaManager.FindAllWithUsuarioMaestro();

            ViewBag.Cuentas = cuentas;
             
            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-VISTAPROVEEDOR")]
        public ActionResult Seleccionar(int id)
        {
            var cuentaConUsuarioMaestro = _cuentaManager.FindWithUsuarioMaestro(id);
            if (cuentaConUsuarioMaestro == null)
            {
                TempData["FlashError"] = MensajesResource.ERROR_Cuenta_IdIncorrecto;
                return RedirectToAction("Index");
            }

            CuentaActiva = cuentaConUsuarioMaestro;

            ViewBag.cuentaConUsuarioMaestro = cuentaConUsuarioMaestro;

            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-VISTAPROVEEDOR")]
        public ActionResult Inicio(int proveedorId)
        {
            var proveedor = _proveedorManager.Find(proveedorId);

            ProveedorActivo = proveedor;
            var cuentaConUsuarioMaestro = CuentaActiva;

            ViewBag.Proveedor = ProveedorActivo;
            ViewBag.cuentaConUsuarioMaestro = cuentaConUsuarioMaestro;

            var model = new CuentaViewModel
            {
                UserName = cuentaConUsuarioMaestro.UsuarioMaestro.UserName,
                ResponsableNombre = cuentaConUsuarioMaestro.UsuarioMaestro.Nombre,
                ResponsableApellido = cuentaConUsuarioMaestro.UsuarioMaestro.Apellido,
                ResponsableCargo = cuentaConUsuarioMaestro.UsuarioMaestro.Cargo,
                ResponsableTelefono = cuentaConUsuarioMaestro.UsuarioMaestro.PhoneNumber,
                ResponsableEmail = cuentaConUsuarioMaestro.UsuarioMaestro.Email

            };

            return View(model);
        }
        //////////////////////////
        /////////////////////////
        //Cuentas por pagar
        //////////////////////////
        /////////////////////////
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-CUENTASPAGAR")]
        public ActionResult Pagos(string date = null)
        {

            var fecha = DateTime.Today;

            if (!string.IsNullOrWhiteSpace(date))
            {
                fecha = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            if (ProveedorActivo == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Primero seleccione un proveedor";
                return RedirectToAction("Index");
            }

            var partidasManager = new PartidasManager();


            var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
            var dsPagos = partidasManager.GetPagos(ProveedorActivo.NumeroProveedor, sociedad, fecha);

            ViewBag.Pagos = dsPagos.Tables["T_LISTA_PAGOS"];

            ViewBag.Proveedor = ProveedorActivo;

            ViewBag.Fecha = fecha;

            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-CUENTASPAGAR")]
        public ActionResult PagosDetalle(string numeroDocumento, string date)
        {
            if (ProveedorActivo == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Primero seleccione un proveedor";
                return RedirectToAction("Index");
            }

            var fecha = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            var partidasManager = new PartidasManager();

            var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
            var dsPagos = partidasManager.GetPagos(ProveedorActivo.NumeroProveedor, sociedad, fecha);

            ViewBag.Pago = dsPagos.Tables["T_LISTA_PAGOS"]
                .Select(string.Format("BELNR = '{0}'", numeroDocumento))[0];

            ViewBag.PagoDetalles = dsPagos.Tables["T_PAGOS"]
                .Select(string.Format("BELNR_PAGO = '{0}'", numeroDocumento));

            ViewBag.Proveedor = ProveedorActivo;

            ViewBag.Fecha = fecha;

            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-CUENTASPAGAR")]
        public ActionResult PagosPendientes()
        {
            if (ProveedorActivo == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Primero seleccione un proveedor";
                return RedirectToAction("Index");
            }

            var partidasManager = new PartidasManager();

            var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
            var dsPagosPendientes = partidasManager.GetPartidasAbiertas(ProveedorActivo.NumeroProveedor, sociedad, DateTime.Today);

            ViewBag.PagosPendientes = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"];

            ViewBag.Proveedor = ProveedorActivo;

            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-CUENTASPAGAR")]
        public ActionResult PagosPendientesDetalle(int proveedorId)
        {
            if (ProveedorActivo == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Primero seleccione un proveedor";
                return RedirectToAction("Index");
            }

            var partidasManager = new PartidasManager();

            var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
            var dsPagosPendientes = partidasManager.GetPartidasAbiertas(ProveedorActivo.NumeroProveedor, sociedad, DateTime.Today);

            ViewBag.PagosPendientes = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"];

            ViewBag.Proveedor = ProveedorActivo;

            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-CUENTASPAGAR")]
        public ActionResult Devoluciones(string numeroDocumento, string date = null)
        {
            var fecha = DateTime.Today;

            if (!string.IsNullOrWhiteSpace(date))
            {
                fecha = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            if (ProveedorActivo == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Primero seleccione un proveedor";
                return RedirectToAction("Index");
            }

            var partidasManager = new PartidasManager();

            var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
            var dsDevoluciones = partidasManager.GetDevoluciones(ProveedorActivo.NumeroProveedor, sociedad, fecha);

            ViewBag.Devoluciones = dsDevoluciones.Tables["T_DEVOLUCIONES"];

            ViewBag.Proveedor = ProveedorActivo;
            ViewBag.Fecha = fecha;
            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-CUENTASPAGAR")]
        public ActionResult DevolucionesDetalle(string numeroDocumento, string date)
        {
            if (ProveedorActivo == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Primero seleccione un proveedor";
                return RedirectToAction("Index");
            }
            var fecha = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var partidasManager = new PartidasManager();

            var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
            var dsDevoluciones = partidasManager.GetDevoluciones(ProveedorActivo.NumeroProveedor, sociedad, fecha);

            ViewBag.Devolucion = dsDevoluciones.Tables["T_DEVOLUCIONES"]
                .Select(string.Format("BELNR = '{0}'", numeroDocumento))[0];

            ViewBag.DevolucionDetalles = dsDevoluciones.Tables["T_MAT_DEV"]
                .Select(string.Format("BELNR = '{0}' AND  BUZID = 'W'", numeroDocumento));

            var drsImpuesto = dsDevoluciones.Tables["T_MAT_DEV"]
                .Select(string.Format("BELNR = '{0}' AND  BUZID = 'T'", numeroDocumento));

            Decimal impuesto = 0;
            if (drsImpuesto.Any())
            {
                impuesto = drsImpuesto.Aggregate(
                    impuesto, (current, dr) => current + decimal.Parse(dr["DMBTR"].ToString()));
            }
            ViewBag.Impuesto = impuesto;

            ViewBag.Proveedor = ProveedorActivo;
            ViewBag.Fecha = fecha;
            return View();
        }

        //////////////////////////
        /////////////////////////
        //Fin cuentas por pagar
        //////////////////////////
        /////////////////////////


        //////////////////////////
        /////////////////////////
        //Ordenes de Compra
        //////////////////////////
        /////////////////////////
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ORDENESCOMPRA")]
        public ActionResult OrdenesCompra()
        {

            if (ProveedorActivo == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Primero seleccione un proveedor";
                return RedirectToAction("Index");
            }

            var cuenta = CuentaActiva.Cuenta;
            ViewBag.data = _ordenCompraManager.FindOrdenesDecompraActivasByCuenta(cuenta.Id);

            return View();
        }


        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ORDENESCOMPRA")]
        public void DescargarOrdenes(string numeroDocumento, int proveedorId)
        {
            var orden = _ordenCompraManager.FindOrdenConDetallesSN(proveedorId, numeroDocumento);


            var workbook = new XLWorkbook(Server.MapPath(@"~/App_Data/plantillaoc.xlsx"));
            var ws = workbook.Worksheet(1);

            var proveedorManager = new ProveedorManager();
            var proveedor = proveedorManager.Find(proveedorId);

            ws.Cell(3, "B").Value = string.Format("{0} {1} {2} {3}", proveedor.Nombre1, proveedor.Nombre2, proveedor.Nombre3, proveedor.Nombre4);
            ws.Cell(3, "D").Value = proveedor.Rfc;
            ws.Cell(5, "B").Value = numeroDocumento;
            ws.Cell(6, "B").Value = orden.FechaEntrega.ToString("dd/MM/yyyy");

            var row = 9;
            foreach (var detalle in orden.Detalles)
            {
                ws.Cell(row, "A").Value = detalle.NumeroMaterial;
                ws.Cell(row, "B").Value = detalle.Descripcion;
                ws.Cell(row, "C").Value = orden.CrossD == "X" ? orden.TiDest : detalle.Centro;
                ws.Cell(row, "D").Value = detalle.CantidadPedido;
                //ws.Cell(row, "E").Value = detalle.PrecioNeto;

                row++;
            }

            FileManager.ExportExcel(workbook, "ORDEN" + numeroDocumento, HttpContext);
        }
        //////////////////////////
        /////////////////////////
        //Fin Ordenes de Compra
        //////////////////////////
        /////////////////////////

        //////////////////////////
        /////////////////////////
        //Comprobantes de Recibo 
        //////////////////////////
        /////////////////////////
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-COMPROBANTESRECIBO")]
        public ActionResult ComprobantesDeRecibo()
        {
            var commonManager = new CommonManager();

            if (ProveedorActivo == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Primero seleccione un proveedor";
                return RedirectToAction("Index");
            }

            var cuenta = CuentaActiva.Cuenta;

            var proveedorManager = new ProveedorManager();
            var proveedores = proveedorManager.FindByCuentaId(cuenta.Id);

            var proveedoresIds = proveedores.Select(p => p.Id).ToArray();


            var db = new Entities();

            ViewBag.Crs = db.crs.Where(cr => proveedoresIds.Contains(cr.cita.ProveedorId)).ToList();
            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-COMPROBANTESRECIBO")]
        public FileResult DescargarComprobantesDeRecibo(int id)
        {
            var db = new Entities();
            var cr = db.crs.Find(id);
            if (cr == null)
            {
                // TODO
                throw new Exception("CR Incorrecto");
            }
            var fileBytes = System.IO.File.ReadAllBytes(cr.ArchivoCR);

            var fileName = string.Format("CR_{0}_{1}.pdf", cr.cita.Id, ((DateTime)cr.Fecha).ToString("dd/MM/yyyy"));

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, fileName);


        }
        //////////////////////////
        /////////////////////////
        //Fin Comprobantes de Recibo
        //////////////////////////
        /////////////////////////

        //////////////////////////
        /////////////////////////
        //Impresion de Etiquetas 
        //////////////////////////
        /////////////////////////
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-IMPRESIONETIQUETAS")]
        public ActionResult ImpresionDeEtiquetas()
        {

            if (ProveedorActivo == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Primero seleccione un proveedor";
                return RedirectToAction("Index");
            }

            var cuenta = CuentaActiva.Cuenta;

            ViewBag.proveedores = _proveedorManager.FindByCuentaId(cuenta.Id);



            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-IMPRESIONETIQUETAS")]
        public ActionResult GenerarEtiquetas(int proveedorId)
        {
            if (ProveedorActivo == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Primero seleccione un proveedor";
                return RedirectToAction("Index");
            }

            var cuenta = CuentaActiva.Cuenta;

            var proveedor = _proveedorManager.Find(proveedorId, cuenta.Id);
            if (proveedor == null)
            {
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }

            ViewBag.Proveedor = proveedor;
            ViewBag.Ordenes = _ordenCompraManager.FindOrdenesDecompraImprimir(proveedor.NumeroProveedor);
            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-IMPRESIONETIQUETAS")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerarEtiquetas(int proveedorId, bool nazan, string ordenesy, bool zapato)
        {
            if (ProveedorActivo == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Primero seleccione un proveedor";
                return RedirectToAction("Index");
            }

            var cuenta = CuentaActiva.Cuenta;

            var proveedor = _proveedorManager.Find(proveedorId, cuenta.Id);

            if (proveedor == null)
            {
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }
            TempData["nazan"] = nazan;
            TempData["zapato"] = zapato;

            if (nazan == false && zapato == false)
            {
                nazan = true;
            }

            var resultado = _etiquetasManager.GetArchivoCsv(proveedorId, cuenta.Id, nazan, ordenesy.Split(','), zapato);

            TempData["resultado"] = resultado;
            TempData["proveedor"] = proveedor;

            return RedirectToAction("ResultadoEtiquetas");
        }


        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-IMPRESIONETIQUETAS")]
        public ActionResult ResultadoEtiquetas()
        {
            if (System.Web.HttpContext.Current.Session[NombreVarSession] != null)
            {
                System.Web.HttpContext.Current.Session.Remove(NombreVarSession);
            }

            var resultado = TempData["resultado"] as Hashtable;
            var proveedor = TempData["proveedor"] as proveedore;
            bool nazan = Convert.ToBoolean(TempData["nazan"]);
            bool zapato = Convert.ToBoolean(TempData["zapato"]);

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
            string etiquetasPrint = "";

            int i = 1;
            int totaletiquetas = 0;

            foreach (DataRow row in dt.Rows)
            {
                totaletiquetas = totaletiquetas + Int32.Parse(row["pares"].ToString());
            }


            if (nazan == true)
            {

                foreach (DataRow row in dt.Rows)
                {

                    for (int xx = 0; xx < Int32.Parse(row["pares"].ToString()); xx++)
                    {



                        etiquetas.Add(@"^XA
^SZ2^JMA
^MCY^PMN
^PW580^MTT
^MNW
~JSN
^MD2
^PR3,4,4
^JZY
^LH0,0^LRN
^XZ
^XA^JZN^
^FO5.00,60.50
^GB85.50P0,30.00,45.00,,3^FS
^FO52.50,15.00^A0N,22.50,22.50^FD^FS
^FO5.50,35.00^A0N,27.00,17.00^FD" + row["Fecha"].ToString().Trim() + @" " + row["Hora"].ToString().Trim() + @"^FS
^FO5.50,65.00^A0N,50.00,40.00^FR^FD" + row["Centro"].ToString().Trim() + @"^FS
^FO160.50,194.00^AFN,5.00,12.50^FDCorrida:^FS
^FO160.50,258.50^AFN,5.00,12.50^FDItem:^FS
^FO290.00,194.50^AFN,4.00,7.00^FD" + row["Corrida_real"].ToString().Trim() + @"^FS
^FO240.50,262.00^A0N,25.50,25.00^FD" + row["ITEM"].ToString().Trim() + @"^FS
^FO5.50,328.00^A0N,25.00,25.00^FD" + row["Desc_familia"].ToString().Trim() + @"^FS
^FO160.50,10.00^AFN,5.00,12.50^FDMarca:^FS
^FO160.50,48.50^AFN,5.00,12.50^FDCodigo:^FS
^FO160.50,88.50^AFN,5.00,12.50^FDEstilo:^FS
^FO160.50,126.50^AFN,5.00,12.50^FDColor:^FS
^FO160.50,162.50^AFN,5.50,12.00^FDTalla:^FS
^FO160.50,229.00^AFN,5.00,12.50^FDAcabado:^FS
^FO260.00,4.00^A0N,35.00,28.50^FD" + row["Marca"].ToString().Trim() + @"^FS
^FO275.00,45.50^A0N,30.00,32.50^FD" + row["Codigo_Uni"].ToString().Trim() + @"^FS
^FO275.00,82.50^A0N,35.00,35.50^FD" + row["estilo"].ToString().Trim() + @"^FS
^FO262.00,124.50^AFN,3.00,8.50^FD" + row["color"].ToString().Trim() + @"^FS
^FO290.50,227.00^AFN,2.00,4.50^FD" + row["acabado"].ToString().Trim() + @"^FS
^FO5.50,298.50^A0N,26.50,24.50^FD" + row["Agrupador_familia"].ToString().Trim() + @"^FS
^FO5.50,237.50^A0N,20.40,25.00^FD" + row["Pedido"].ToString().Trim() + @"^FS
^FO5.50,262.50^A0N,20.15,26.50^FD" + row["No_Prov"].ToString().Trim() + @"^FS
^FO510.00,348.00^A0N,20.35,42.00^FD" + row["IR"].ToString().Trim() + @"^FS
^FO15.50,10.00^A0N,13.50,25.00^FD" + i + @" / " + totaletiquetas + @"^FS
^ISLABEL001,N,^FS
^XZ
^XA^JZN^PR9
^PQ1,0,1,Y
^ILLABEL001^FS
^FO262.00,161.50^AFN,15.00,22.50^FD" + row["talla"].ToString().Trim() + @"^FS
^FO180.00,295.00^BY2.00,.8^BAN,47.00,N,N,N^FD" + row["Ean_Nazan"].ToString().Trim() + @"^FS
^FO212.00,348.50^A0N,24.30,40.00^FD" + row["Ean_Nazan"].ToString().Trim() + @"^FS
^XZ
^XZ
^EG
^XZ");
                        i++;
                    }

                }


            }
            else
            {
                if (zapato)
                {
                    //Etiquetas Colgantes
                    foreach (DataRow row in dt.Rows)
                    {
                        for (int xx = 0; xx < Int32.Parse(row["pares"].ToString()); xx++)
                        {
                            etiquetas.Add(@"^XA
^SZ2^JMA
^MCY^PMN
^PW220^MTT
^MNM
~JSN
^MD2
^PR3,4,4
^JZY
^LH0,0^LRN
^XZ
^XA^JZN^
^FO0.50,263.00^AFN,5.00,12.50^FDSKU:^FS
^FO59.00,263.00^A0N,27.00,32.00^FD" + row["Sku_cadena"].ToString().Trim() + @"^FS
^FO.50,185.50^AFN,5.50,12.00^FDTalla:^FS
^FO5.00,32.00^A0N,35.00,30.50^FD" + row["Marca"].ToString().Trim() + @"^FS
^FO0.50,80.50^A0N,38.00,45.50^FD" + row["estilo"].ToString().Trim() + @"^FS
^FO0.50,125.50^AFN,3.00,8.50^FD" + row["color"].ToString().Trim() + @"^FS
^FO0.50,160.50^A0N,20.50,30.00^FD" + row["Desc_familia"].ToString().Trim() + @"^FS
^FO0.50,220.50^A0N,35.50,40.00^FD$^FS 
^FO40.50,220.50^A0N,40.60,35.00^FD" + row["Entero_prec"].ToString().Trim() + @"^FS
^FO100.50,220.50^A0N,22.30,25.00^FD" + row["Dec_prec"].ToString().Trim() + @"^FS
^ISLABEL001,N,^FS
^XZ
^XA^JZN^PR9
^PQ1,0,1,Y
^ILLABEL001^FS
^FO95.00,185.50^AFN,10.00,18.50^FD" + row["talla"].ToString().Trim() + @"^FS
^FO10.00,295.00^BY2,.12^BEN,55.00,Y,N^FD" + row["Ean_cadena"].ToString().Trim() + @"^FS
^XZ
^XZ
^EG 
^XZ");
                            i++;
                        }

                    }



                }
                else
                {
                    //Etiquetas De Precio cadena
                    foreach (DataRow row in dt.Rows)
                    {
                        for (int xx = 0; xx < Int32.Parse(row["pares"].ToString()); xx++)
                        {
                            etiquetas.Add(@"^XA
^SZ2^JMA
^MCY^PMN
^PW580^MTT
^MNW
~JSN
^MD2
^PR3,4,4
^JZY
^LH0,0^LRN
^XZ
^XA^JZN^
^FO35.00,44.55^GB0.0P0,30.00,45.00,,3^FS
^FO52.50,5.00^A0N,22.50,22.50^FD^FS
^FO475.50,3.00^A0N,22.00,20.00^FD" + row["Fecha"].ToString().Trim() + @" " + row["Hora"].ToString().Trim() + @"^FS
^FO30.00,45.00^A0N,50.00,40.00^FR^FD" + row["Centro"].ToString().Trim() + @"^FS
^FO188.50,192.00^AFN,5.00,12.00^FDCorrida:^FS
^FO238.50,265.50^AFN,5.00,12.50^FDItem:^FS
^FO318.00,192.50^AFN,4.00,7.00^FD" + row["Corrida_real"].ToString().Trim() + @"^FS
^FO320.50,265.00^A0N,27.47,25.00^FD" + row["ITEM"].ToString().Trim() + @"^FS
^FO170.50,37.00^AFN,5.00,12.50^FDMarca:^FS
^FO208.50,81.50^AFN,5.00,12.50^FDEstilo:^FS
^FO218.50,119.50^AFN,5.00,12.50^FDColor:^FS
^FO218.50,157.50^AFN,5.50,12.00^FDTalla:^FS
^FO188.50,227.00^AFN,5.00,12.50^FDAcabado:^FS
^FO264.00,24.50^A0N,45.00,40.50^FD" + row["Marca"].ToString().Trim() + @"^FS
^FO318.00,77.50^A0N,35.00,42.50^FD" + row["estilo"].ToString().Trim() + @"^FS
^FO318.00,119.50^AFN,3.00,8.50^FD" + row["color"].ToString().Trim() + @"^FS
^FO318.50,226.00^AFN,2.00,4.50^FD" + row["acabado"].ToString().Trim() + @"^FS
^FO30.50,235.50^A0N,20.50,25.00^FD" + row["Pedido"].ToString().Trim() + @"^FS
^FO30,283.50^A0N,40.50,50.00^FD$^FS
^FO82.50,267.50^A0N,77.60,72.00^FD" + row["Entero_prec"].ToString().Trim() + @"^FS
^FO190.50,267.50^A0N,35.60,34.00^FD" + row["Dec_prec"].ToString().Trim() + @"^FS
^FO30.00,342.50^A0N,30.20,30.50^FD" + row["No_Prov"].ToString().Trim() + @"^FS
^FO30.50,400.00^A0N,10.50,15.00^FDWeb 1^FS
^FO30.50,9.00^A0N,18.50,30.00^FD" + i + @" / " + dt.Rows.Count + @"^FS
^ISLABEL001,N,^FS
^XZ
^XA^JZN^PR9
^PQ1,0,1,Y
^ILLABEL001^FS
^FO318.00,157.50^AFN,7.00,32.50^FD" + row["talla"].ToString().Trim() + @"^FS
^FO300.00,296.00^BY2.0.20,.20^BEN,48.00,Y,N^FD" + row["Ean_cadena"].ToString().Trim() + @"^FS
^XZ
^XA
^JZN");
                            i++;
                        }

                    }

                }

            }


            foreach (string etiqueta in etiquetas)
            {
                etiquetasPrint = etiquetasPrint + etiqueta;
            }




            ViewBag.etiquetas = etiquetas;
            ViewBag.etiquetasPrint = etiquetasPrint;
            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-IMPRESIONETIQUETAS")]
        public FileContentResult DescargarEtiquetas()
        {
            var csv = System.Web.HttpContext.Current.Session["etiqueta_csv"].ToString();

            return File(new UTF8Encoding().GetBytes(csv), "text/csv", "etiquetas.csv");
        }
        //////////////////////////
        /////////////////////////
        //Fin Impresion de Etiquetas
        //////////////////////////
        /////////////////////////


        //////////////////////////
        /////////////////////////
        //Reporte de proveedores
        //////////////////////////
        /////////////////////////
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-REPORTESPROVEEDORES")]
        public ActionResult ReporteProveedores()
        {

            if (ProveedorActivo == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Primero seleccione un proveedor";
                return RedirectToAction("Index");
            }

            var cuenta = CuentaActiva.Cuenta;

            ViewBag.proveedores = _proveedorManager.FindByCuentaId(cuenta.Id);



            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-REPORTESPROVEEDORES")]
        public ActionResult Reportes(int proveedorId = 0)
        {
            int id = proveedorId > 0 ? proveedorId : Convert.ToInt32(System.Web.HttpContext.Current.Session["proveedorId"]);

            if (id == 0)
                return RedirectToAction("Index");

            var proveedor = _proveedorManager.Find(id);
            ViewBag.proveedor = proveedor;
            try
            {
                ViewBag.reportes = _reporteProveedorManager.FindReporteProveedor(proveedor.NumeroProveedor);

                ViewBag.nivelservicio = _reporteProveedorManager.FindNivelSerNiveleseervicio(proveedor.Id);

                /*if (ViewBag.nivelservicio == null)
                {
                    TempData["FlashError"] = "No hay datos para este proveedor";
                    return RedirectToAction("Index");
                }*/

            }
            catch (Exception ex)
            {
                if (ex.Message == "NOT_DATA_FOUND")
                {
                    TempData["FlashError"] = "No hay datos para este proveedor";
                    return RedirectToAction("ReporteProveedores");
                }


                throw;
            }

            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-REPORTESPROVEEDORES")]
        public void DescargarReporte(string numeroProveedor)
        {
            var detalles = _reporteProveedorManager.FindReporteProveedor(numeroProveedor);

            var proveedor = _proveedorManager.FindByNumeroProveedor(numeroProveedor);

            var nivelServicio = _reporteProveedorManager.FindNivelSerNiveleseervicio(proveedor.Id);


            var workbook = new XLWorkbook(Server.MapPath(@"~/App_Data/plantillareporte.xlsx"));
            var ws = workbook.Worksheet(1);

            ws.Cell(4, "C").Value = string.Format("{0} {1} {2} {3}", proveedor.Nombre1, proveedor.Nombre2, proveedor.Nombre3, proveedor.Nombre4);
            ws.Cell(5, "C").Value = proveedor.Rfc;

            if (nivelServicio != null)
            {
                ws.Cell(9, "C").Value = nivelServicio.UltimoMes / 100;
                ws.Cell(10, "C").Value = nivelServicio.TemporadaActual / 100;
                ws.Cell(11, "C").Value = nivelServicio.AcumuladoAnual / 100;

                ws.Cell(9, "F").Value = nivelServicio.PedidoAtrasado;
                ws.Cell(10, "F").Value = nivelServicio.PedidoEnTiempo;
                ws.Cell(11, "F").Value = nivelServicio.PedidoTotal;
            }

            ws.Cell(4, "M").Value = detalles[0].FechaProceso.ToString("dd/MM/yyyy");

            ws.Cell(14, "D").Value = string.Format("Ventas ({0})", DateTime.Today.AddMonths(-2).ToString("MM/yyyy"));
            ws.Cell(14, "E").Value = string.Format("Ventas ({0})", DateTime.Today.AddMonths(-1).ToString("MM/yyyy"));
            ws.Cell(14, "F").Value = string.Format("Ventas ({0})", DateTime.Today.ToString("MM/yyyy"));

            var row = 15;

            foreach (var detalle in detalles)
            {


                ws.Cell(row, "B").Value = detalle.Material.TrimStart('0');
                ws.Cell(row, "C").Value = detalle.NombreMaterial;
                ws.Cell(row, "D").Value = detalle.CantidadVentas2;
                ws.Cell(row, "E").Value = detalle.CantidadVentas1;
                ws.Cell(row, "F").Value = detalle.CantidadVentas;
                ws.Cell(row, "G").Value = detalle.CantidadTotal;
                ws.Cell(row, "H").Value = detalle.CalculoTotal;
                ws.Cell(row, "I").Value = detalle.InvTienda;
                ws.Cell(row, "J").Value = detalle.InvTransito;
                ws.Cell(row, "K").Value = detalle.InvCedis;
                ws.Cell(row, "L").Value = detalle.PedidosPendiente;
                ws.Cell(row, "M").Value = detalle.EstadoMaterial;
                row++;
            }

            FileManager.ExportExcel(workbook, "REP_" + proveedor.Rfc + "_" + detalles[0].FechaProceso.ToString("ddMMyyyy"), HttpContext);

        }
        //////////////////////////
        /////////////////////////
        //Fin Reporte de proveedores
        //////////////////////////
        /////////////////////////
    }
}