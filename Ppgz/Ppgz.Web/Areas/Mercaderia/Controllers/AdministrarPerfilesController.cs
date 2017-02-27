using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Ppgz.Repository;
using Ppgz.Web.Infrastructure;
using Ppgz.Web.Infrastructure.Nazan;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
	public class AdministrarPerfilesController : Controller
	{
		private readonly PerfilProveedorManager _perfilProveedorManager = new  PerfilProveedorManager();
        private readonly CommonManager _commonManager = new CommonManager();
		

		//
		// GET: /Mercaderia/AdministrarPerfiles/
		public ActionResult Index()
		{
		   
		    var perfiles = _perfilProveedorManager
                .FindByCuentaId(_commonManager.GetCuentaUsuarioAutenticado().Id);

		     perfiles .Add(_perfilProveedorManager.GetMaestroByUsuarioTipo(CuentaManager.Tipo.MERCADERIA));

            ViewBag.Perfiles = perfiles;
			return View();
		}
	}
}