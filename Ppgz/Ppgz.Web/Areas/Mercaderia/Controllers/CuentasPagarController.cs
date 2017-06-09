using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using ClosedXML.Excel;
using Ppgz.Repository;
using Ppgz.Services;
using Ppgz.Web.Infrastructure;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class CuentasPagarController : Controller
    {

        readonly CommonManager _commonManager = new CommonManager();
        readonly ProveedorManager _proveedorManager = new ProveedorManager();

        internal proveedore ProveedorCxp
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["proveedorcxp"] != null)
                {
                    return (proveedore)System.Web.HttpContext.Current.Session["proveedorcxp"];
                }
                return null;
            }
            set
            {
                System.Web.HttpContext.Current.Session["proveedorcxp"] = value;
            }
        }

        // GET: /Mercaderia/CuentasPagar/
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult Index()
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            ViewBag.proveedores = _proveedorManager.FindByCuentaId(cuenta.Id);

            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult SeleccionarProveedor(int proveedorId)
        {

            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            var proveedor = _proveedorManager.Find(proveedorId, cuenta.Id);

            if (proveedor == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }

            ProveedorCxp = proveedor;
            return RedirectToAction("Pagos");
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult Pagos(string date = null)
        {

            var fecha = DateTime.Today;

            if (!string.IsNullOrWhiteSpace(date))
            {
                fecha = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            if (ProveedorCxp == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }

            var partidasManager = new PartidasManager();


            var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
            var dsPagos = partidasManager.GetPagos(ProveedorCxp.NumeroProveedor, sociedad, fecha);

            ViewBag.Pagos = dsPagos.Tables["T_LISTA_PAGOS"];

            ViewBag.Proveedor = ProveedorCxp;

            ViewBag.Fecha = fecha;

            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult PagosDetalle(string numeroDocumento, string date)
        {
            if (ProveedorCxp == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }

            var fecha = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            var partidasManager = new PartidasManager();

            var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
            var dsPagos = partidasManager.GetPagos(ProveedorCxp.NumeroProveedor, sociedad, fecha);

            ViewBag.Pago = dsPagos.Tables["T_LISTA_PAGOS"]
                .Select(string.Format("BELNR = '{0}'", numeroDocumento))[0];

            ViewBag.PagoDetalles = dsPagos.Tables["T_PAGOS"]
                .Select(string.Format("BELNR_PAGO = '{0}'", numeroDocumento));

            ViewBag.Proveedor = ProveedorCxp;

            ViewBag.Fecha = fecha;

            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public void PagoDetallesDescargar(string numeroDocumento, string date)
        {
            if (ProveedorCxp == null)
            {
                return;
            }
            var fecha = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            var partidasManager = new PartidasManager();

            var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
            var dsPagos = partidasManager.GetPagos(ProveedorCxp.NumeroProveedor, sociedad, fecha);


            var pago = dsPagos.Tables["T_LISTA_PAGOS"]
                .Select(string.Format("BELNR = '{0}'", numeroDocumento))[0];

            var detalles = dsPagos.Tables["T_PAGOS"]
                .Select(string.Format("BELNR_PAGO = '{0}'", numeroDocumento));


            var workbook = new XLWorkbook(Server.MapPath(@"~/App_Data/plantillapagos.xlsx"));
            var ws = workbook.Worksheet(1);

            ws.Cell(3, "B").Value = string.Format("{0} {1} {2} {3}", ProveedorCxp.Nombre1, ProveedorCxp.Nombre2, ProveedorCxp.Nombre3, ProveedorCxp.Nombre4);
            ws.Cell(3, "F").Value = ProveedorCxp.Rfc;
            ws.Cell(5, "B").Value = pago["BELNR"].ToString();
            ws.Cell(6, "B").Value = pago["DMBTR"].ToString();

            var row = 9;

            foreach (var drPago in detalles)
            {
                var tipo = "";
                switch (drPago["BLART_COMPEN"].ToString())
                {
                    case "04":
                        tipo = "Cargo a proveedor";
                        break;
                    case "10":
                        tipo = "Factura a proveedor";
                        break;
                    case "21":
                        tipo = "Factura a proveedor";
                        break;
                    case "23":
                        tipo = "Nota de Cargo";
                        break;
                    case "AB":
                        tipo = "Cargo a proveedor";
                        break;
                    case "DA":
                        tipo = "Cargo a proveedor";
                        break;
                    case "DG":
                        tipo = "Cargo a proveedor";
                        break;
                    case "DZ":
                        tipo = "Pago de proveedor";
                        break;
                    case "KA":
                        tipo = "Cargo a proveedor";
                        break;
                    case "KG":
                        tipo = "Cargo a proveedor";
                        break;
                    case "KR":
                        tipo = "Devolución";
                        break;
                    case "":
                        tipo = "Cargo a proveedor";
                        break;
                    case "RE":

                        if ((decimal)drPago["DMBTR_COMPEN"] > 0)
                        {
                            tipo = "Factura de mercancia";
                            break;

                        }
                        tipo = "Devolucion de mercancia";
                        break;

                    case "RV":
                        tipo = "Cargo a proveedor";
                        break;
                    case "SA":
                        tipo = "Cargo a proveedor";
                        break;
                    case "ZN":
                        tipo = "Anulación de documento";
                        break;
                    case "ZP":
                        tipo = "Pago";
                        break;
                }

                ws.Cell(row, "A").Value = drPago["XBLNR"].ToString();
                ws.Cell(row, "B").Value =  drPago["DMBTR_COMPEN"].ToString();
                ws.Cell(row, "C").Value = drPago["WAERS_COMPEN"].ToString();
                ws.Cell(row, "D").Value = tipo;

                ws.Cell(row, "E").Value = DateTime.ParseExact(
                        drPago["FECHA_DOC"].ToString(),
                        "yyyyMMdd",
                        CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");
                ws.Cell(row, "F").Value = drPago["SGTXT"].ToString();

                row++;

            }

            FileManager.ExportExcel(workbook, "PAG_" + pago["BELNR"], HttpContext);
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult Devoluciones(string numeroDocumento, string date = null)
        {
            var fecha = DateTime.Today;

            if (!string.IsNullOrWhiteSpace(date))
            {
                fecha = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            if (ProveedorCxp == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }

            var partidasManager = new PartidasManager();

            var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
            var dsDevoluciones = partidasManager.GetDevoluciones(ProveedorCxp.NumeroProveedor, sociedad, fecha);

            ViewBag.Devoluciones = dsDevoluciones.Tables["T_DEVOLUCIONES"];

            ViewBag.Proveedor = ProveedorCxp;
            ViewBag.Fecha = fecha;
            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult DevolucionesDetalle(string numeroDocumento, string date)
        {
            if (ProveedorCxp == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }
            var fecha = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var partidasManager = new PartidasManager();

            var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
            var dsDevoluciones = partidasManager.GetDevoluciones(ProveedorCxp.NumeroProveedor, sociedad, fecha);

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

            ViewBag.Proveedor = ProveedorCxp;
            ViewBag.Fecha = fecha;
            return View();
        }


        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public void DevolucionesDetalleDescargar(string numeroDocumento, string date)
        {
            if (ProveedorCxp == null)
            {
                return;
            }
            var fecha = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var partidasManager = new PartidasManager();

            var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;


            var dsDevoluciones = partidasManager.GetDevoluciones(ProveedorCxp.NumeroProveedor, sociedad, fecha);

            var drDevolucion = dsDevoluciones.Tables["T_DEVOLUCIONES"]
                .Select(string.Format("BELNR = '{0}'", numeroDocumento))[0];

            var drDevolucionDetalles = dsDevoluciones.Tables["T_MAT_DEV"]
                .Select(string.Format("BELNR = '{0}' AND  BUZID = 'W'", numeroDocumento));

            var drsImpuesto = dsDevoluciones.Tables["T_MAT_DEV"]
                .Select(string.Format("BELNR = '{0}' AND  BUZID = 'T'", numeroDocumento));

            Decimal impuesto = 0;
            if (drsImpuesto.Any())
            {
                impuesto = drsImpuesto.Aggregate(
                    impuesto, (current, dr) => current + decimal.Parse(dr["DMBTR"].ToString()));
            }

            var subTotal = drDevolucionDetalles.Aggregate<DataRow, decimal>(0, (current, dr) => current + decimal.Parse(dr["DMBTR"].ToString()));

            var total = decimal.Parse(drDevolucion["DMBTR"].ToString()) * -1;

            var cantidadTotal = drDevolucionDetalles.Aggregate<DataRow, decimal>(0, (current, dr) => current + decimal.Parse(dr["MENGE"].ToString()));

            var workbook = new XLWorkbook(Server.MapPath(@"~/App_Data/plantilladevoluciones.xlsx"));
            var ws = workbook.Worksheet(1);

            ws.Cell(3, "B").Value = string.Format("{0} {1} {2} {3}", ProveedorCxp.Nombre1, ProveedorCxp.Nombre2, ProveedorCxp.Nombre3, ProveedorCxp.Nombre4);
            ws.Cell(3, "D").Value = ProveedorCxp.Rfc;
            ws.Cell(5, "B").Value = drDevolucion["XBLNR"].ToString();
            ws.Cell(6, "B").Value = DateTime.ParseExact(
                        drDevolucion["BLDAT"].ToString(),
                        "yyyyMMdd",
                        CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");
            ws.Cell(7, "B").Value = cantidadTotal;
            ws.Cell(5, "D").Value = subTotal;
            ws.Cell(6, "D").Value = impuesto;
            ws.Cell(7, "D").Value = total;
            var row = 10;

            foreach (var dr in drDevolucionDetalles)
            {
                ws.Cell(row, "A").Value = dr["MATNR"].ToString();
                ws.Cell(row, "B").Value = dr["MAKTX"].ToString();
                ws.Cell(row, "C").Value = string.Format("{0:N}", dr["DMBTR"]);
                ws.Cell(row, "D").Value = dr["MENGE"].ToString();
                row++;
            }

            FileManager.ExportExcel(workbook, "DEV_" + drDevolucion["XBLNR"], HttpContext);
        }


        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult PagosPendientes()
        {
            if (ProveedorCxp == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }

            var partidasManager = new PartidasManager();

            var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
            var dsPagosPendientes = partidasManager.GetPartidasAbiertas(ProveedorCxp.NumeroProveedor, sociedad, DateTime.Today);

            ViewBag.PagosPendientes = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"];

            ViewBag.Proveedor = ProveedorCxp;

            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult PagosPendientesDetalle(int proveedorId)
        {
            if (ProveedorCxp == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }

            var partidasManager = new PartidasManager();

            var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
            var dsPagosPendientes = partidasManager.GetPartidasAbiertas(ProveedorCxp.NumeroProveedor, sociedad, DateTime.Today);

            ViewBag.PagosPendientes = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"];

            ViewBag.Proveedor = ProveedorCxp;

            return View();
        }


        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public void PagosPendientesDescargar()
        {
            if (ProveedorCxp == null)
            {
                return;
            }

            var partidasManager = new PartidasManager();

            var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
            var dsPagosPendientes = partidasManager.GetPartidasAbiertas(ProveedorCxp.NumeroProveedor, sociedad, DateTime.Today);

            var dt = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"];

            var workbook = new XLWorkbook(Server.MapPath(@"~/App_Data/plantillapagospendientes.xlsx"));
            var ws = workbook.Worksheet(1);

            ws.Cell(3, "B").Value = string.Format("{0} {1} {2} {3}", ProveedorCxp.Nombre1, ProveedorCxp.Nombre2, ProveedorCxp.Nombre3, ProveedorCxp.Nombre4);
            ws.Cell(3, "G").Value = ProveedorCxp.Rfc;
            ws.Cell(5, "B").Value = DateTime.Today.ToString("dd/MM/yyyy");
            ws.Cell(6, "B").Value = dt.Rows.Cast<DataRow>()
                .Aggregate<DataRow, decimal>(0, (current, pagoPendiente) => current + decimal.Parse(pagoPendiente["DMBTR"].ToString()));

            var row = 9;

            foreach (DataRow dr in dt.Rows)
            {
                ws.Cell(row, "A").Value = dr["XBLNR"].ToString();
                ws.Cell(row, "B").Value = dr["DMBTR"].ToString();
                ws.Cell(row, "C").Value = dr["WAERS"].ToString();
                ws.Cell(row, "D").Value = DateTime.ParseExact(dr["FECHA_PAGO"].ToString(), "yyyyMMdd",
                CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");
                var tipo = "";
                switch (dr["BLART"].ToString())
                {
                    case "04":
                        tipo = "Cargo a proveedor";
                        break;
                    case "10":
                        tipo = "Factura a proveedor";
                        break;
                    case "21":
                        tipo = "Factura a proveedor";
                        break;
                    case "23":
                        tipo = "Nota de Cargo";
                        break;
                    case "AB":
                        tipo = "Cargo a proveedor";
                        break;
                    case "DA":
                        tipo = "Cargo a proveedor";
                        break;
                    case "DG":
                        tipo = "Cargo a proveedor";
                        break;
                    case "DZ":
                        tipo = "Pago de proveedor";
                        break;
                    case "KA":
                        tipo = "Cargo a proveedor";
                        break;
                    case "KG":
                        tipo = "Cargo a proveedor";
                        break;
                    case "KR":
                        tipo = "Devolución";
                        break;
                    case "":
                        tipo = "Cargo a proveedor";
                        break;
                    case "RE":

                        if ((decimal)dr["DMBTR"] > 0)
                        {
                            tipo = "Factura de mercancia";
                            break;
                        }
                        tipo = "Devolucion de mercancia";
                        break;
                    case "RV":
                        tipo = "Cargo a proveedor";
                        break;
                    case "SA":
                        tipo = "Cargo a proveedor";
                        break;
                    case "ZN":
                        tipo = "Anulación de documento";
                        break;
                    case "ZP":
                        tipo = "Pago";
                        break;
                }

                ws.Cell(row, "E").Value = tipo;


                ws.Cell(row, "F").Value = DateTime.ParseExact(dr["BLDAT"].ToString(), "yyyyMMdd",
                CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");
                ws.Cell(row, "G").Value = dr["SGTXT"].ToString();

                row++;
            }
            FileManager.ExportExcel(workbook, "CXP_" + ProveedorCxp.Rfc, HttpContext);
        }
    }


}