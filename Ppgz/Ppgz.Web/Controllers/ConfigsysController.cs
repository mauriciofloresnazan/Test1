using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MySql.Data.MySqlClient;
using Ppgz.Repository;

namespace Ppgz.Web.Controllers
{
    public class ConfigsysController : Controller
    {

        private readonly Entities _db = new Entities();

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

            int Res = 0;
            
            try{

                const string sql = @"UPDATE configuraciones SET Clave = {0}, Valor = {1}, Descripcion = {2}, Habilitado = {3} WHERE  id = {4}";
                _db.Database.ExecuteSqlCommand(sql, Object.Clave,Object.Valor,Object.Descripcion,Object.Habilitado, Object.id);
                _db.SaveChanges();

               Res = 1;

            } catch (Exception) {
            
                Res = 0;

            }

            return Json(Res, JsonRequestBehavior.DenyGet);

        }
	}
}