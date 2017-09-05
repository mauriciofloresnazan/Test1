using System;
using System.Web.Mvc;
using System.Globalization;
using Ppgz.Repository;
using Ppgz.Services;
using Ppgz.Web.Areas.Nazan.Models;
using Ppgz.Web.Infrastructure;
using System.Linq;

namespace Ppgz.Web.Areas.Nazan.Controllers
{
    public class VistaProveedorController : Controller
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
                TempData["FlashError"] = "Proveedor incorrecto";
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
                TempData["FlashError"] = "Proveedor incorrecto";
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
                TempData["FlashError"] = "Proveedor incorrecto";
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
                TempData["FlashError"] = "Proveedor incorrecto";
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
    }
}