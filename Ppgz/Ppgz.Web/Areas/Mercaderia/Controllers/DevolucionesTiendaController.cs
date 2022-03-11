using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using ClosedXML.Excel;
using Newtonsoft.Json;
using Ppgz.Repository;
using Ppgz.Services;
using Ppgz.Web.Infrastructure;
using ScaleWrapper;
namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class DevolucionesTiendaController : Controller
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

        internal string SociedadActiva
        {
            get
            {
                if (System.Web.HttpContext.Current.Session["sociedadactiva"] != null)
                {
                    return (string)System.Web.HttpContext.Current.Session["sociedadactiva"];
                }
                return null;
            }
            set
            {
                System.Web.HttpContext.Current.Session["sociedadactiva"] = value;
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
        public ActionResult SeleccionarProveedor(int proveedorId, string sociedad)
        {

            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            var proveedor = _proveedorManager.Find(proveedorId, cuenta.Id);


            ProveedorCxp = proveedor;
            SociedadActiva = sociedad;
            //dynamic empObj = JsonConvert.DeserializeObject<dynamic>(proveedor.Sociedad);
            //ProveedorCxp.Sociedades = empObj;
            //var test = ProveedorCxp.Sociedades[0];
            return RedirectToAction("Graficas");
        }
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult Graficas(string numeroDocumento, string date = null)
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

            //var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
            var sociedad = SociedadActiva;
            var dsDevoluciones = partidasManager.GetDevoluciones(ProveedorCxp.NumeroProveedor, sociedad, fecha);
            
            ViewBag.Devoluciones = dsDevoluciones.Tables["T_DEVOLUCIONES"];

            ViewBag.DevolucionDetalles = dsDevoluciones.Tables["T_MAT_DEV"];
           
            ViewBag.Proveedor = ProveedorCxp;
            ViewBag.Fecha = fecha;
            return View();
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

            //var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
            var sociedad = SociedadActiva;
            var dsDevoluciones = partidasManager.GetDevoluciones(ProveedorCxp.NumeroProveedor, sociedad, fecha);
           
            ViewBag.Devoluciones = dsDevoluciones.Tables["T_DEVOLUCIONES"];

            ViewBag.DevolucionDetalles = dsDevoluciones.Tables["T_MAT_DEV"];
            
            ViewBag.Proveedor = ProveedorCxp;
            ViewBag.Fecha = fecha;
            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult DevolucionesDetalle(string numeroDocumento, string ndev, string date)
        {
            if (ProveedorCxp == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }
            var fecha = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var partidasManager = new PartidasManager();

            //var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
            var sociedad = SociedadActiva;
            var dsDevoluciones = partidasManager.GetDevoluciones(ProveedorCxp.NumeroProveedor, sociedad, fecha);
            var r = DbScaleGNZN.GetDataTable("select * from fn_gnzn_mtvo_dev('" + ndev + "')");
            ViewBag.devo = r;
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

            //var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
            var sociedad = SociedadActiva;


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

    }


}