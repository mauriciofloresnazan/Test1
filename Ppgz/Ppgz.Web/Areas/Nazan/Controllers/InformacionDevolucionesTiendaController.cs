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
using ScaleWrapper;
namespace Ppgz.Web.Areas.Nazan.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class InformacionDevolucionesTiendaController : Controller
    {
        private readonly Entities _db = new Entities();

        readonly ProveedorManager _proveedorManager = new ProveedorManager();
        readonly CuentaManager _cuentaManager = new CuentaManager();
        readonly CommonManager _commonManager = new CommonManager();
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


        [Authorize(Roles ="MAESTRO-NAZAN,NAZAN-VISTACITASCALIDAD")]
        public ActionResult Index()
        {
            var cuentas = _cuentaManager.FindAllWithUsuarioMaestro();

            ViewBag.Cuentas = cuentas;

            return View();
        }

        [Authorize(Roles ="MAESTRO-NAZAN,NAZAN-VISTACITASCALIDAD")]
        public ActionResult Seleccionar(int id)
        {
            var cuentaConUsuarioMaestro = _cuentaManager.FindWithUsuarioMaestro(id);
            if (cuentaConUsuarioMaestro == null)
            {
                TempData["FlashError"] = MensajesResource.ERROR_Cuenta_IdIncorrecto;
                return RedirectToAction("Index");
            }

            CuentaActiva = cuentaConUsuarioMaestro;


            ViewBag.Proveedores = _proveedorManager.FindByCuentaId(cuentaConUsuarioMaestro.Cuenta.Id);
            ViewBag.cuentaConUsuarioMaestro = cuentaConUsuarioMaestro;

            return View();
        }

        [Authorize(Roles ="MAESTRO-NAZAN,NAZAN-VISTACITASCALIDAD")]
        public ActionResult Inicio(int proveedorId, string sociedad)
        {
            SociedadActiva = sociedad;
            var proveedor = _proveedorManager.Find(proveedorId);

            ProveedorActivo = proveedor;
            var cuentaConUsuarioMaestro = CuentaActiva;

            ViewBag.Proveedor = ProveedorActivo;
            ViewBag.cuentaConUsuarioMaestro = cuentaConUsuarioMaestro;
            ViewBag.Sociedad = sociedad;
            if (cuentaConUsuarioMaestro == null)
            {
                TempData["FlashError"] = MensajesResource.ERROR_Cuenta_IdIncorrecto;
                return RedirectToAction("Index");
            }
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
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-VISTACITASCALIDAD")]
        public ActionResult Graficas(string numeroDocumento, string date = null)
        {
            var fecha = DateTime.Today;

            if (!string.IsNullOrWhiteSpace(date))
            {
                fecha = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            if (ProveedorActivo == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }

            var partidasManager = new PartidasManager();

            //var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
            var sociedad = SociedadActiva;
            var dsDevoluciones = partidasManager.GetDevoluciones(ProveedorActivo.NumeroProveedor, sociedad, fecha);
            
            ViewBag.Devoluciones = dsDevoluciones.Tables["T_DEVOLUCIONES"];

            ViewBag.DevolucionDetalles = dsDevoluciones.Tables["T_MAT_DEV"];
           
            ViewBag.Proveedor = ProveedorActivo;
            ViewBag.Fecha = fecha;
            return View();
        }
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-VISTACITASCALIDAD")]
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
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }
            var partidasManager = new PartidasManager();

            //var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
            var sociedad = SociedadActiva;
            var dsDevoluciones = partidasManager.GetDevoluciones(ProveedorActivo.NumeroProveedor, sociedad, fecha);
           
            ViewBag.Devoluciones = dsDevoluciones.Tables["T_DEVOLUCIONES"];

            ViewBag.DevolucionDetalles = dsDevoluciones.Tables["T_MAT_DEV"];
            
            ViewBag.Proveedor = ProveedorActivo;
            ViewBag.Fecha = fecha;
            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-VISTACITASCALIDAD")]
        public ActionResult DevolucionesDetalle(string numeroDocumento, string ndev, string date)
        {
            if (ProveedorActivo == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }
            var fecha = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var partidasManager = new PartidasManager();

            //var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
            var sociedad = SociedadActiva;
            var dsDevoluciones = partidasManager.GetDevoluciones(ProveedorActivo.NumeroProveedor, sociedad, fecha);
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

            ViewBag.Proveedor = ProveedorActivo;
            ViewBag.Fecha = fecha;
            return View();
        }


        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-VISTACITASCALIDAD")]
        public void DevolucionesDetalleDescargar(string numeroDocumento, string date)
        {
            if (ProveedorActivo == null)
            {
                return;
            }
            var fecha = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var partidasManager = new PartidasManager();

            //var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
            var sociedad = SociedadActiva;


            var dsDevoluciones = partidasManager.GetDevoluciones(ProveedorActivo.NumeroProveedor, sociedad, fecha);

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

            ws.Cell(3, "B").Value = string.Format("{0} {1} {2} {3}", ProveedorActivo.Nombre1, ProveedorActivo.Nombre2, ProveedorActivo.Nombre3, ProveedorActivo.Nombre4);
            ws.Cell(3, "D").Value = ProveedorActivo.Rfc;
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