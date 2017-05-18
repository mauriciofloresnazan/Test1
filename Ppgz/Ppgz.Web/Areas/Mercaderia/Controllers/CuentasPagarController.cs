using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
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

            var dt = dsPagos.Tables["T_PAGOS"]
                .Select(string.Format("BELNR_PAGO = '{0}'", numeroDocumento)).CopyToDataTable();

            var columnsNames = new[]
            {        
                "XBLNR","DMBTR_COMPEN","WAERS_COMPEN",  "BLART_COMPEN"
            };

            foreach (DataRow dr in dt.Rows)
            {
                var tipo = "";
                switch (dr["BLART_COMPEN"].ToString())
                {
                    case "4":
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

                        if ((decimal)dr["DMBTR_COMPEN"] > 0)
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

                dr["BLART_COMPEN"] = tipo;
            }

            for (var i = dt.Columns.Count - 1; i >= 0; i--)
            {
                if (!columnsNames.Contains(dt.Columns[i].ColumnName))
                {
                    dt.Columns.RemoveAt(i);
                }
            }

            dt.Columns["XBLNR"].ColumnName = "Referencia";
            dt.Columns["DMBTR_COMPEN"].ColumnName = "Importe";
            dt.Columns["WAERS_COMPEN"].ColumnName = "Ml";
            dt.Columns["BLART_COMPEN"].ColumnName = "Tipo de Movimiento";

            FileManager.ExportExcel(dt,"RefdPago" + numeroDocumento, HttpContext);
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

            var dt = dsDevoluciones.Tables["T_MAT_DEV"]
                .Select(string.Format("BELNR = '{0}'", numeroDocumento)).CopyToDataTable();

            var columnsNames = new[]
            {        
                "EBELN","MATNR","MAKTX","DMBTR","MENGE"
            };

            for (var i = dt.Rows.Count - 1; i >= 0; i--)
            {
                if (dt.Rows[i]["BUZID"].ToString() == "T")
                {
                    dt.Rows.RemoveAt(i);
                }
            }

            for (var i = dt.Columns.Count - 1; i >= 0; i--)
            {
                if (!columnsNames.Contains(dt.Columns[i].ColumnName))
                {
                    dt.Columns.RemoveAt(i);
                }
            }

            dt.Columns["EBELN"].ColumnName = "Documento";
            dt.Columns["MATNR"].ColumnName = "Artículo";
            dt.Columns["MAKTX"].ColumnName = "Descripción";
            dt.Columns["DMBTR"].ColumnName = "Total";
            dt.Columns["MENGE"].ColumnName = "Cantidad";

            FileManager.ExportExcel(dt, numeroDocumento, HttpContext);
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

            var columnsNames = new[]
            {        
                "XBLNR","DMBTR","WAERS","BLART","FECHA_PAGO"
            };

            foreach (DataRow dr in dt.Rows)
            {
                dr["FECHA_PAGO"] = DateTime.ParseExact(dr["FECHA_PAGO"].ToString(), "yyyyMMdd",
                CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");

                var tipo = "";
                switch (dr["BLART"].ToString())
                {
                    case "4":
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

                dr["BLART"] = tipo;
            }

            for (var i = dt.Columns.Count - 1; i >= 0; i--)
            {
                if (!columnsNames.Contains(dt.Columns[i].ColumnName))
                {
                    dt.Columns.RemoveAt(i);
                }
            }

            dt.Columns["XBLNR"].ColumnName = "Referencia";
            dt.Columns["DMBTR"].ColumnName = "Importe";
            dt.Columns["WAERS"].ColumnName = "Ml";
            dt.Columns["BLART"].ColumnName = "Tipo de Movimiento";

           FileManager.ExportExcel(dt, "NroSAP" + ProveedorCxp.NumeroProveedor, HttpContext);
        }


    }
}