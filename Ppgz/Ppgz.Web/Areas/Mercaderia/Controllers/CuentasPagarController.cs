using System.Web.Mvc;
using Ppgz.Web.Infrastructure;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class CuentasPagarController : Controller
    {

        //
        // GET: /Nazan/CuentasPagar/
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult Pagos()
        {
            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult PagosPendientes()
        {
            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult PagosPendientesDetalle()
        {
            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-CUENTASPAGAR")]
        public ActionResult Devoluciones()
        {
            return View();
        }
        
    }
}