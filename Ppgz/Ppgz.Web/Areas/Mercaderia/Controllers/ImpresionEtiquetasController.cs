using System.Collections;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Ppgz.Repository;
using Ppgz.Services;
using Ppgz.Web.Infrastructure;
using System;
using System.Data;
using System.Collections.Generic;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class ImpresionEtiquetasController : Controller
    {

        private readonly CommonManager _commonManager = new CommonManager();
        private readonly ProveedorManager _proveedorManager = new ProveedorManager();
        private readonly EtiquetasManager _etiquetasManager = new EtiquetasManager();
        private readonly OrdenCompraManager _ordenesCompraManager = new OrdenCompraManager();

        private const string NombreVarSession = "etiqueta_csv";

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-IMPRESIONETIQUETAS")]
        public ActionResult Index()
        {
            /*var commonManager = new CommonManager();

            var cuenta = commonManager.GetCuentaUsuarioAutenticado();
        
            var proveedorManager = new ProveedorManager();
            var proveedores = proveedorManager.FindByCuentaId(cuenta.Id).ToList();

            var proveedoresIds = proveedores.Select(p => p.Id).ToArray();

            var db = new Entities();
            

            ViewBag.Etiquetas = db.etiquetas.Where(e=> proveedoresIds.Contains(e.ProveedorId)).ToList();*/

            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            ViewBag.proveedores = _proveedorManager.FindByCuentaId(cuenta.Id);

 

            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-IMPRESIONETIQUETAS")]
        public ActionResult Generar(int proveedorId)
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();
            var proveedor = _proveedorManager.Find(proveedorId, cuenta.Id);
            if (proveedor == null)
            {
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index"); 
            }

            ViewBag.Proveedor = proveedor;
            ViewBag.Ordenes = _ordenesCompraManager.FindOrdenesDecompraImprimir(proveedor.NumeroProveedor);
            return View();
        }

/*
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-IMPRESIONETIQUETAS")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Generar(int proveedorId, bool nazan, string [] ordenes)
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            var proveedor = _proveedorManager.Find(proveedorId, cuenta.Id);

            if (proveedor == null)
            {
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }

            var resultado = _etiquetasManager.GetArchivoCsv(proveedorId, cuenta.Id, nazan, ordenes);

            TempData["resultado"] = resultado;
            TempData["proveedor"] = proveedor;

            return RedirectToAction("Resultado"); 
        }*/

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-IMPRESIONETIQUETAS")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Generar(int proveedorId, bool nazan, string ordenesy)
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            var proveedor = _proveedorManager.Find(proveedorId, cuenta.Id);

            if (proveedor == null)
            {
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }
            

            var resultado = _etiquetasManager.GetArchivoCsv(proveedorId, cuenta.Id, nazan, ordenesy.Split(','));

            TempData["resultado"] = resultado;
            TempData["proveedor"] = proveedor;
            TempData["nazan"] = nazan;

            return RedirectToAction("Resultado");
        }


        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-IMPRESIONETIQUETAS")]
        public ActionResult Resultado()
        {
            if (System.Web.HttpContext.Current.Session[NombreVarSession] != null)
            {
                System.Web.HttpContext.Current.Session.Remove(NombreVarSession);
            }

            var resultado = TempData["resultado"] as Hashtable;
            var proveedor = TempData["proveedor"] as proveedore;
            var nazan = TempData["nazan"];

            if (resultado == null || proveedor == null)
            {
                return RedirectToAction("Index");
            }


            ViewBag.Proveedor = proveedor;
            ViewBag.Resultado = resultado["return"];

            ViewBag.PuedeDescargar = false;

            if (resultado["csv"] == null) return View();
            if (string.IsNullOrWhiteSpace(resultado["csv"].ToString())) return View();

            System.Web.HttpContext.Current.Session[NombreVarSession] = resultado["csv"];
            ViewBag.PuedeDescargar = true;

           
            

            //CSV to DataTable
            DataTable dt = new DataTable();
            var s = resultado["csv"].ToString();
            s = s.Replace("\"", "");
            string[] tableData = s.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var col = from cl in tableData[0].Split(",".ToCharArray())
                      select new DataColumn(cl);
            dt.Columns.AddRange(col.ToArray());

            foreach (var item in tableData.Skip(1)) {
                dt.Rows.Add(item.Split(",".ToCharArray()));
            }
            
            //fin

           //string[] etiquetas;
            List<string> etiquetas = new List<string>();
            string etiquetasPrint="";

            int i = 1;
            foreach (DataRow row in dt.Rows )
            {
               etiquetas.Add(@"^XA^JZN^
                                ^FO5.00,60.50
                                ^GB85.50P0,30.00,45.00,,3^FS
                                ^FO52.50,15.00^A0N,22.50,22.50^FD^FS
                                ^FO5.50,35.00^A0N,27.00,17.00^FD" + row["Fecha"] + @" " + row["Hora"] + @"^FS
                                ^FO5.00,65.00^A0N,50.00,40.00^FR^FD" + row["Centro"] + @"^FS
                                ^FO200.50,202.00^AFN,5.00,12.00^FDCorrida:^FS
                                ^FO200.50,257.50^AFN,5.00,12.50^FDItem:^FS
                                ^FO340.00,202.50^AFN,4.00,7.00^FD" + row["Corrida_real"] + @"^FS
                                ^FO310.50,261.00^A0N,25.50,25.00^FD" + row["ITEM"] + @"^FS
                                ^FO52.50,525.00^A0N,42.00,42.00^FD" + row["Desc_familia"] + @"^FS
                                ^FO160.50,10.00^AFN,5.00,12.50^FDMarca:^FS
                                ^FO200.50,60.50^AFN,5.00,12.50^FDCodigo:^FS
                                ^FO200.50,112.50^AFN,5.00,12.50^FDEstilo:^FS
                                ^FO200.50,142.50^AFN,5.00,12.50^FDColor:^FS
                                ^FO200.50,172.50^AFN,5.50,12.00^FDTalla:^FS
                                ^FO200.50,232.00^AFN,5.00,12.50^FDAcabado:^FS
                                ^FO250.00,12.00^A0N,35.00,30.50^FD" + row["Marca"] + @"^FS
                                ^FO310.00,60.50^A0N,35.00,35.50^FD" + row["Codigo_Uni"] + @"^FS
                                ^FO310.00,108.50^A0N,35.00,35.50^FD" + row["estilo"] + @"^FS
                                ^FO310.00,142.50^AFN,3.00,8.50^FD" + row["color"] + @"^FS
                                ^FO330.50,232.00^AFN,2.00,4.50^FD" + row["acabado"] + @"^FS
                                ^FO5.50,330.50^A0N,26.50,26.50^FD" + row["Agrupador_familia"] + @"^FS
                                ^FO5.50,250.50^A0N,20.50,25.00^FD" + row["Pedido"] + @"^FS
                                ^FO5.50,360.50^A0N,20.15,26.50^FD" + row["No_Prov"] + @"^FS
                                ^FO515.00,385.00^A0N,20.35,42.00^FD" + row["IR"] + @"^FS
                                ^FO5.00,400.00^A0N,10.50,15.00^FDVP 5^FS
                                ^FO15.50,10.00^A0N,13.50,25.00^FD" + i + @" / " + dt.Rows.Count + @"^FS
                                ^ISLABEL001,N,^FS
                                ^XZ
                                ^XA^JZN^PR9
                                ^PQ1,0,1,Y
                                ^ILLABEL001^FS
                                ^FO310.00,172.50^AFN,15.00,22.50^FD" + row["talla"] + @"^FS
                                ^FO122.00,285.00^BY3.00,.10^BAN,70.00,N,N,N^FD" + row["Ean_Nazan"] + @"^FS
                                ^FO230.00,360.50^A0N,25.50,40.00^FD" + row["Ean_Nazan"] + @"^FS
                                ^XZ
                                ^XZ
                                ^EG
                                ^XZ");

                etiquetasPrint = etiquetasPrint + @"^XA^JZN^
                                ^FO5.00,60.50
                                ^GB85.50P0,30.00,45.00,,3^FS
                                ^FO52.50,15.00^A0N,22.50,22.50^FD^FS
                                ^FO5.50,35.00^A0N,27.00,17.00^FD" + row["Fecha"] + @" " + row["Hora"] + @"^FS
                                ^FO5.00,65.00^A0N,50.00,40.00^FR^FD" + row["Centro"] + @"^FS
                                ^FO200.50,202.00^AFN,5.00,12.00^FDCorrida:^FS
                                ^FO200.50,257.50^AFN,5.00,12.50^FDItem:^FS
                                ^FO340.00,202.50^AFN,4.00,7.00^FD" + row["Corrida_real"] + @"^FS
                                ^FO310.50,261.00^A0N,25.50,25.00^FD" + row["ITEM"] + @"^FS
                                ^FO52.50,525.00^A0N,42.00,42.00^FD" + row["Desc_familia"] + @"^FS
                                ^FO160.50,10.00^AFN,5.00,12.50^FDMarca:^FS
                                ^FO200.50,60.50^AFN,5.00,12.50^FDCodigo:^FS
                                ^FO200.50,112.50^AFN,5.00,12.50^FDEstilo:^FS
                                ^FO200.50,142.50^AFN,5.00,12.50^FDColor:^FS
                                ^FO200.50,172.50^AFN,5.50,12.00^FDTalla:^FS
                                ^FO200.50,232.00^AFN,5.00,12.50^FDAcabado:^FS
                                ^FO250.00,12.00^A0N,35.00,30.50^FD" + row["Marca"] + @"^FS
                                ^FO310.00,60.50^A0N,35.00,35.50^FD" + row["Codigo_Uni"] + @"^FS
                                ^FO310.00,108.50^A0N,35.00,35.50^FD" + row["estilo"] + @"^FS
                                ^FO310.00,142.50^AFN,3.00,8.50^FD" + row["color"] + @"^FS
                                ^FO330.50,232.00^AFN,2.00,4.50^FD" + row["acabado"] + @"^FS
                                ^FO5.50,330.50^A0N,26.50,26.50^FD" + row["Agrupador_familia"] + @"^FS
                                ^FO5.50,250.50^A0N,20.50,25.00^FD" + row["Pedido"] + @"^FS
                                ^FO5.50,360.50^A0N,20.15,26.50^FD" + row["No_Prov"] + @"^FS
                                ^FO515.00,385.00^A0N,20.35,42.00^FD" + row["IR"] + @"^FS
                                ^FO5.00,400.00^A0N,10.50,15.00^FDVP 5^FS
                                ^FO15.50,10.00^A0N,13.50,25.00^FD" + i + @" / " + dt.Rows.Count + @"^FS
                                ^ISLABEL001,N,^FS
                                ^XZ
                                ^XA^JZN^PR9
                                ^PQ1,0,1,Y
                                ^ILLABEL001^FS
                                ^FO310.00,172.50^AFN,15.00,22.50^FD" + row["talla"] + @"^FS
                                ^FO122.00,285.00^BY3.00,.10^BAN,70.00,N,N,N^FD" + row["Ean_Nazan"] + @"^FS
                                ^FO230.00,360.50^A0N,25.50,40.00^FD" + row["Ean_Nazan"] + @"^FS
                                ^XZ
                                ^XZ
                                ^EG
                                ^XZ";

                i++;
            }


            ViewBag.etiquetas = etiquetas;
            ViewBag.etiquetasPrint = etiquetasPrint;
            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-IMPRESIONETIQUETAS")]
        public FileContentResult Descargar()
        {
            var csv = System.Web.HttpContext.Current.Session["etiqueta_csv"].ToString(); 

            return File(new UTF8Encoding().GetBytes(csv), "text/csv", "etiquetas.csv");
        }

    }
}