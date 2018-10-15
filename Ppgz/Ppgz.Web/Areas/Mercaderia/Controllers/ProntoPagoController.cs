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
		public ActionResult ObtenerTotalFactoraje(string facturas, int proveedorId)
		{
			string[] facturasList = facturas.Split(',');

			var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
			var partidasManager = new PartidasManager();
			var dsPagosPendientes = partidasManager.GetPartidasAbiertas(ProveedorCxp.NumeroProveedor, sociedad, DateTime.Today);

			var proveedorFManager = new ProveedorFManager();
			ProveedorFManager.localPF provedorFactoraje = proveedorFManager.GetProveedoresFactoraje().Where(x => x.IdProveedor == proveedorId).FirstOrDefault();
			int porcentaje = 0;
			if (provedorFactoraje != null)
			{
				porcentaje = provedorFactoraje.Porcentaje;
			}
			else
			{
				porcentaje = Convert.ToInt32(CommonManager.GetConfiguraciones().Single(c => c.Clave == "prontopago.default.percent").Valor);
			}

			double MontoOriginal = 0;
			double Descuentos = 0;
			double DescuentoProntoPago = 0;
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
					numeroDocumento = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["BELNR"].ToString(),
					porcentaje = porcentaje
				};

				//Acumulamos el monto original


				
				if(item.importe < 0)
					Descuentos = Descuentos + item.importe;

				//si la factura esta en la lista
				if (facturasList.Contains(dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["BELNR"].ToString()))
				{
					//Acumulamos el monto original y los descuentos
					if (item.importe > 0)
					{
						MontoOriginal = MontoOriginal + item.importe;
						DescuentoProntoPago = DescuentoProntoPago + (item.importe * (Convert.ToDouble(item.porcentaje) / 100));
					}
				}
			}
			Descuentos = Descuentos * -1;
			ViewBag.MontoOriginal = MontoOriginal;
			ViewBag.Descuentos = Descuentos;
			ViewBag.DescuentoProntoPago = DescuentoProntoPago;
			ViewBag.TotalSolicitado = MontoOriginal - Descuentos - DescuentoProntoPago;

			return PartialView("_totalProntoPago");
		}

		

		[Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
		public ActionResult NuevaSolicitud(int proveedorId)
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
			

			//Traemos los pagos pendientes
			var partidasManager = new PartidasManager();
			var proveedorFManager = new ProveedorFManager();
			
			/*
			 ViewBag.DiaDePago = CommonManager.GetConfiguraciones().Single(c => c.Clave == "prontopago.default.day").Valor;
            ViewBag.Porcentaje = CommonManager.GetConfiguraciones().Single(c => c.Clave == "prontopago.default.percent").Valor;
			 */

			var sociedad = CommonManager.GetConfiguraciones().Single(c => c.Clave == "rfc.common.function.param.bukrs.mercaderia").Valor;
			var dsPagosPendientes = partidasManager.GetPartidasAbiertas(ProveedorCxp.NumeroProveedor, sociedad, DateTime.Today);

			//Se obtiene porcentaje de factoraje por proveedor.
			ProveedorFManager.localPF provedorFactoraje = proveedorFManager.GetProveedoresFactoraje().Where(x => x.IdProveedor == proveedorId).FirstOrDefault();
			int porcentaje = 0;
			if(provedorFactoraje != null)
			{
				porcentaje = provedorFactoraje.Porcentaje;
			}
			else
			{
				porcentaje =Convert.ToInt32(CommonManager.GetConfiguraciones().Single(c => c.Clave == "prontopago.default.percent").Valor);
			}

			//.Porcentaje ?? CommonManager.GetConfiguraciones().Single(c => c.Clave == "prontopago.default.percent").Valor				;



			//Armamos la lista con las cuentas por pagas es decir mayores a cero 
			List<Web.Models.ProntoPago.FacturaView> _list = new List<Web.Models.ProntoPago.FacturaView>();
			List<Web.Models.ProntoPago.FacturaView> _listDescuentos = new List<Web.Models.ProntoPago.FacturaView>();

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
					numeroDocumento = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["BELNR"].ToString(),
					porcentaje = porcentaje
				};
				if (item.importe < 0)
				{
					item.pagar = true;
				}
				if(item.importe > 0)
					_list.Add(item);
				else
					_listDescuentos.Add(item);
			}

			ViewBag.PagosPendientes = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"];
			ViewBag.Proveedor = proveedor;
			ViewBag.Title = "Nueva solicitud";
			ViewBag.SubTitle = "En esta sección podrán crear una solicitud.";
			ViewBag.Facturas = _list;
			ViewBag.Descuentos = _listDescuentos;

			return View("Solicitud");
		}

		[Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
		public ActionResult VerSolicitudes(int proveedorId)
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
			ViewBag.Proveedor = proveedor;
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