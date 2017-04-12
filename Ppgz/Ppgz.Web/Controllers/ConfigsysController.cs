using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MySql.Data.MySqlClient;
using Ppgz.Repository;

namespace Ppgz.Web.Controllers
{
    public class ConfigsysController : Controller
    {
        //
        // GET: /Configsys/
        public ActionResult Index()
        {
            const string sql = @"SELECT id, Clave, Valor, Habilitado, Descripcion FROM configuraciones";

            var result = Db.GetDataTable(sql);

            ViewBag.Resultado = result;

            return View();
        }

        public class estConfig
        {
            public string id { get; set; }
            public string Clave { get; set; }
            public string Valor { get; set; }
            public string Descripcion { get; set; }
            public string Habilitado { get; set; }

        }

        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult editarConfig(estConfig Object)
        {

            


            int Res = 1;

            return Json(Res, JsonRequestBehavior.DenyGet);

        }
	}
}