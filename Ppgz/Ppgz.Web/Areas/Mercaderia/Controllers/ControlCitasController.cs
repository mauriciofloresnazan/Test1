using System.Linq;
using System.Web.Mvc;
using MySql.Data.MySqlClient;
using Ppgz.Repository;
using Ppgz.Web.Infrastructure;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
    public class ControlCitasController : Controller
    {


        readonly ProveedorManager _proveedorManager = new ProveedorManager();
        readonly CommonManager _commonManager = new CommonManager();
        readonly CuentasPorPagarManager _cuentasPorPagarManager = new CuentasPorPagarManager();

        //
        // GET: /Mercaderia/ControlCitas/
        public ActionResult Index()
        {

            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            ViewBag.proveedores = _proveedorManager.FindByCuentaId(cuenta.Id);
            
            return View();
        }


        public ActionResult OrdenDeCompra(int proveedorId)
        {
            
            return View();
        }


	}
}