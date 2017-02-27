using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ppgz.Web.Infrastructure.Nazan;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
	public class AdministrarPerfilesController : Controller
	{
		private readonly PerfilProveedorManager _perfilProveedorManager = new  PerfilProveedorManager();
		
		//
		// GET: /Mercaderia/AdministrarPerfiles/
		public ActionResult Index()
		{
			//var perfiles = _perfilProveedorManager.FindByCuentaId()
			ViewBag.Perfiles = _perfilProveedorManager.GetMaestroByUsuarioTipo(CuentaManager.Tipo.MERCADERIA);

			return View();
		}
	}
}