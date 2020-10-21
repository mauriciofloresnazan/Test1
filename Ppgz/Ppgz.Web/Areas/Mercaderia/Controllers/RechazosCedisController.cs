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
    public class RechazosCedisController : Controller
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
            return RedirectToAction("GraficasMotivos");
        }
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult GraficasMotivos(string numeroDocumento, string date = null)
        {
            if (ProveedorCxp == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }
            DateTime dat = DateTime.Now.AddMonths(-10);
            DateTime myDate = DateTime.Now;

            var res = DbScaleGNZN.GetDataTable("select vsh.id_proveedor, sum(vsd.cantidad) prs, cr.descripcion from GNZN_vales_salida_header vsh (nolock) join GNZN_vales_salida_detail vsd(nolock) on vsh.id_reg = vsd.id_reg left join GNZN_vales_salida_codigos_razon cr(nolock) on cr.id_codigo_razon = vsd.id_codigo_razon where vsh.id_proveedor = " + ProveedorCxp.NumeroProveedor + " and vsh.Estatus = 'Activo' and vsh.area = 'Calidad' and vsh.fecha >= '" + dat.ToString("yyyy-MM-dd HH:mm:ss.fff") + "' and vsh.fecha <= '" + myDate.ToString("yyyy-MM-dd HH:mm:ss.fff") + "' group by vsh.id_proveedor, cr.descripcion order by prs asc");

            ViewBag.Res = res;
            ViewBag.Proveedor = ProveedorCxp;
            return View();
        }
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult Graficas(string numeroDocumento, string date = null)
        {
            if (ProveedorCxp == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }
            DateTime dat = DateTime.Now.AddMonths(-10);
            DateTime myDate = DateTime.Now;

            var r = DbScaleGNZN.GetDataTable("select suma_pares, mes, año from fn_gnzn_vales_total_meses(" + ProveedorCxp.NumeroProveedor + ", '" + dat.ToString("yyyy-MM-dd HH:mm:ss.fff") + "', '" + myDate.ToString("yyyy-MM-dd HH:mm:ss.fff") + "')order by mes");
            ViewBag.R = r;
            ViewBag.Proveedor = ProveedorCxp;
            return View();
        }
        
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult Devoluciones(string numeroDocumento, string date = null)
        {
           
            DateTime myDateTime = DateTime.Now.AddDays(-7);
            if (ProveedorCxp == null)
            {
                // TODO pasar a recurso
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }
            DateTime myDate = DateTime.Now;
            var result = DbScaleGNZN.GetDataTable("select vsh.id_vale_salida,vsh.nombre,vsh.id_proveedor, vsh.canal, vsh.fecha, sum(vsd.cantidad) prs from GNZN_vales_salida_header vsh (nolock) join GNZN_vales_salida_detail vsd(nolock) on vsh.id_reg = vsd.id_reg join GNZN_vales_salida_codigos_razon cr(nolock) on cr.id_codigo_razon = vsd.id_codigo_razon where vsh.id_proveedor = " + ProveedorCxp.NumeroProveedor + " and vsh.Estatus = 'Activo' and vsh.fecha >= '" + myDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + "' and vsh.fecha <= '" + myDate.ToString("yyyy-MM-dd HH:mm:ss.fff") + "' and vsh.area = 'Calidad' group by vsh.id_proveedor, vsh.nombre,vsh.canal, vsh.fecha, id_vale_salida");
            var res = DbScaleGNZN.GetDataTable("select vsh.id_proveedor, sum(vsd.cantidad) prs, cr.descripcion from GNZN_vales_salida_header vsh (nolock) join GNZN_vales_salida_detail vsd(nolock) on vsh.id_reg = vsd.id_reg left join GNZN_vales_salida_codigos_razon cr(nolock) on cr.id_codigo_razon = vsd.id_codigo_razon where vsh.id_proveedor = " + ProveedorCxp.NumeroProveedor + " and vsh.Estatus = 'Activo' and vsh.area = 'Calidad' and vsh.fecha >= '" + myDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + "' and vsh.fecha <='" + myDate.ToString("yyyy-MM-dd HH:mm:ss.fff") + "' group by vsh.id_proveedor, cr.descripcion order by prs desc");
            
            ViewBag.Resul = res;
            ViewBag.Resulatdo = result;
            ViewBag.Proveedor = ProveedorCxp;
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

            var result = DbScaleGNZN.GetDataTable("select vsh.id_vale_salida, vsh.id_proveedor, vsh.nombre, vsd.generico, vsd.descripcion, vsh.fecha, sum(vsd.cantidad) prs, cr.descripcion C_razon from GNZN_vales_salida_header vsh (nolock) join GNZN_vales_salida_detail vsd(nolock) on vsh.id_reg = vsd.id_reg join GNZN_vales_salida_codigos_razon cr(nolock) on cr.id_codigo_razon = vsd.id_codigo_razon where vsh.id_vale_salida = '" + numeroDocumento + "' and vsh.Estatus = 'Activo' and vsh.area = 'Calidad' group by vsh.id_vale_salida, vsh.id_proveedor, vsh.nombre, vsd.generico, vsd.descripcion, vsh.fecha, cr.descripcion");
            ViewBag.Resulatdo = result;

            ViewBag.Proveedor = ProveedorCxp;

            return View();
        }




    }
}