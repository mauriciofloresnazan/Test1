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
    public class ReporteRechazosCedisController : Controller
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


        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-VISTACITASCALIDAD")]
        public ActionResult Index()
        {
            var cuentas = _cuentaManager.FindAllWithUsuarioMaestro();

            ViewBag.Cuentas = cuentas;

            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-VISTACITASCALIDAD")]
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

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-VISTACITASCALIDAD")]
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
        public ActionResult GraficasMotivos(string numeroDocumento, string date = null)
        {
            if (ProveedorActivo == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }
            DateTime dat = DateTime.Now.AddMonths(-10);
            DateTime myDate = DateTime.Now;

            var res = DbScaleGNZN.GetDataTable("select vsh.id_proveedor, sum(vsd.cantidad) prs, cr.descripcion from GNZN_vales_salida_header vsh (nolock) join GNZN_vales_salida_detail vsd(nolock) on vsh.id_reg = vsd.id_reg left join GNZN_vales_salida_codigos_razon cr(nolock) on cr.id_codigo_razon = vsd.id_codigo_razon where vsh.id_proveedor = " + ProveedorActivo.NumeroProveedor + " and vsh.Estatus = 'Activo' and vsh.area = 'Calidad' and vsh.fecha >= '" + dat.ToString("yyyy-MM-dd HH:mm:ss.fff") + "' and vsh.fecha <= '" + myDate.ToString("yyyy-MM-dd HH:mm:ss.fff") + "' group by vsh.id_proveedor, cr.descripcion order by prs asc");

            ViewBag.Res = res;
            ViewBag.Proveedor = ProveedorActivo;
            return View();
        }
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-VISTACITASCALIDAD")]
        public ActionResult Graficas(string numeroDocumento, string date = null)
        {

            if (ProveedorActivo == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }
            DateTime dat = DateTime.Now.AddMonths(-10);
            DateTime myDate = DateTime.Now;
            var r = DbScaleGNZN.GetDataTable("select * from fn_gnzn_vales_total_meses(" + ProveedorActivo.NumeroProveedor + ", '" + dat.ToString("yyyy-MM-dd HH:mm:ss.fff") + "', '" + myDate.ToString("yyyy-MM-dd HH:mm:ss.fff") + "')");
            ViewBag.R = r;
            ViewBag.Proveedor = ProveedorActivo;
            return View();
        }
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-VISTACITASCALIDAD")]
        public ActionResult Devoluciones(string numeroDocumento, string date = null)
        {

            DateTime myDateTime = DateTime.Now.AddDays(-7);
            if (ProveedorActivo == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }
            DateTime myDate = DateTime.Now;
            var result = DbScaleGNZN.GetDataTable("select vsh.id_vale_salida,vsh.nombre,vsh.id_proveedor, vsh.canal, vsh.fecha, sum(vsd.cantidad) prs from GNZN_vales_salida_header vsh (nolock) join GNZN_vales_salida_detail vsd(nolock) on vsh.id_reg = vsd.id_reg join GNZN_vales_salida_codigos_razon cr(nolock) on cr.id_codigo_razon = vsd.id_codigo_razon where vsh.id_proveedor = " + ProveedorActivo.NumeroProveedor + " and vsh.Estatus = 'Activo' and vsh.fecha >= '" + myDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + "' and vsh.fecha <= '" + myDate.ToString("yyyy-MM-dd HH:mm:ss.fff") + "' and vsh.area = 'Calidad' group by vsh.id_proveedor, vsh.nombre,vsh.canal, vsh.fecha, id_vale_salida");
            var res = DbScaleGNZN.GetDataTable("select vsh.id_proveedor, sum(vsd.cantidad) prs, cr.descripcion from GNZN_vales_salida_header vsh (nolock) join GNZN_vales_salida_detail vsd(nolock) on vsh.id_reg = vsd.id_reg left join GNZN_vales_salida_codigos_razon cr(nolock) on cr.id_codigo_razon = vsd.id_codigo_razon where vsh.id_proveedor = " + ProveedorActivo.NumeroProveedor + " and vsh.Estatus = 'Activo' and vsh.area = 'Calidad' and vsh.fecha >= '" + myDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + "' and vsh.fecha <='" + myDate.ToString("yyyy-MM-dd HH:mm:ss.fff") + "' group by vsh.id_proveedor, cr.descripcion");
            ViewBag.Resul = res;
            ViewBag.Resulatdo = result;
            ViewBag.Proveedor = ProveedorActivo;
            return View();
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-VISTACITASCALIDAD")]
        public ActionResult DevolucionesDetalle(string numeroDocumento, string date)
        {

            if (ProveedorActivo == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }

            var result = DbScaleGNZN.GetDataTable("select vsh.id_vale_salida, vsh.id_proveedor, vsh.nombre, vsd.generico, vsd.descripcion, vsh.fecha, sum(vsd.cantidad) prs, cr.descripcion C_razon from GNZN_vales_salida_header vsh (nolock) join GNZN_vales_salida_detail vsd(nolock) on vsh.id_reg = vsd.id_reg join GNZN_vales_salida_codigos_razon cr(nolock) on cr.id_codigo_razon = vsd.id_codigo_razon where vsh.id_vale_salida = '" + numeroDocumento + "' and vsh.Estatus = 'Activo' and vsh.area = 'Calidad' group by vsh.id_vale_salida, vsh.id_proveedor, vsh.nombre, vsd.generico, vsd.descripcion, vsh.fecha, cr.descripcion");
            ViewBag.Resulatdo = result;

            ViewBag.Proveedor = ProveedorActivo;

            return View();
        }
    }
}


       