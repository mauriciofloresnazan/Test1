using Ppgz.Repository;
using Ppgz.Services;
using Ppgz.Web.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
	[Authorize]
	[TerminosCondiciones]
	public class ProntoPagoController : Controller
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

		//**********CU002-Seleccion razon social*****************
		
		[Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
		public ActionResult VerSolicitudes(string date = null)
		{
			return View();
		}

		[Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
		public ActionResult Index()
		{
			var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

			ViewBag.proveedores = _proveedorManager.FindByCuentaId(cuenta.Id);

			return View();
		}
		//**********Termina CU002-Seleccion razon social*****************


		//**********CU003-Facturas pendientes de pago*****************
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
		public ActionResult Pagos()
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

			//Armamos la lista
			List<Web.Models.ProntoPago.FacturaView> _list = new List<Web.Models.ProntoPago.FacturaView>();
			for (int i = 0; i < dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows.Count; i++)
			{
				Web.Models.ProntoPago.FacturaView item = new Web.Models.ProntoPago.FacturaView()
				{
					referencia = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["XBLNR"].ToString(),
					importe = Convert.ToDouble(dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["DMBTR"].ToString()),
					ml = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["WAERS"].ToString(),
					vencimiento = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["FECHA_PAGO"].ToString(),
					tipoMovimiento = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["BLART"].ToString(),
					fechaDocumento = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["BLDAT"].ToString(),
					descripcion = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["SGTXT"].ToString(),
					pagar = false,
					idProveedor = ProveedorCxp.Id.ToString(),
					numeroDocumento = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["BELNR"].ToString()
				};
				if (item.importe < 0)
				{
					item.pagar = true;
				}
				_list.Add(item);
			}

			return View(_list);
		}
		[Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
		public ActionResult RegistraSolicitud(List<Web.Models.ProntoPago.FacturaView> model) {
			foreach(var item in model)
			{
				if (item.importe < 0)
				{
					item.pagar = true;
				}
			}
			return RedirectToAction("Pagos");
		}
		//**********Termina CU003-Facturas pendientes de pago*****************
	}
}