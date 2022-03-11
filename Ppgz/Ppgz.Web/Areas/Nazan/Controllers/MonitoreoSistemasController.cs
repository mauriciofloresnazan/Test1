using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Ppgz.Repository;
using Ppgz.Services;
using ScaleWrapper;
namespace Ppgz.Web.Areas.Nazan.Controllers
{
    public class MonitoreoSistemasController : Controller
    {
        

        public ActionResult Monitoreo()
        {
            var id = string.Format("{0}", DateTime.Now.ToString("yyyy-MM-dd"));
            var resss = DbScaleGNZN.GetDataTable(@"Select Hora_Reporte, Sistema,
       case when CLIENTE  >  0 then 'OK' else 'ERR' end as CLIENTE,
       case when SERVIDOR >  0 then 'OK' else 'ERR' end as SERVIDOR

  from (
             SELECT top 100 percent Hora_Reporte, Sistema, Origen, Estatus
                    FROM GNZN_Estatus_Aplicaciones
                    where Hora_Reporte = ( Select MAX(Hora_reporte) 
                                                            from GNZN_Estatus_Aplicaciones  
                                                            where Fecha_Registro > Convert(Varchar(10),GETDATE(),112 ) + ' 00:00:00'
                                                      )
                  AND Fecha_Registro > Convert(Varchar(10),GETDATE(),112 ) + ' 00:00:00'
                    AND Estatus = 'OK' 
                    order by Sistema, origen
             ) as Datos
PIVOT (
          Count(Estatus)
          FOR Origen in ([CLIENTE], [SERVIDOR])
       ) as PVT");
            ViewBag.Resss = resss;

            return View();
        }

        
    }
}