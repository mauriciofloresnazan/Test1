using System;
using System.Web.Mvc;
using Ppgz.Repository;

namespace Ppgz.Web.Areas.Nazan.Controllers
{
    public class ConfignegController : Controller
    {

        private readonly Entities _db = new Entities();


        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-CONFIGNEG")]
        public ActionResult Index()
        { 
            const string sql = @"SELECT id, Clave, Valor, Habilitado, Descripcion, Negocio FROM configuraciones";

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
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-CONFIGNEG")]
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