using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ppgz.Web.Areas.Nazan.Controllers
{
    public class TestController : Controller
    {
        //
        // GET: /Nazan/Test/
        public void Index()
        {
             Response.Write("PRUEBA DE AREA");
        }
	}
}