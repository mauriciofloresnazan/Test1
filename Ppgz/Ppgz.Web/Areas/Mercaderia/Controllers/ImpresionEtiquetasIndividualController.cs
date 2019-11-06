using System;
using System.Linq;
using System.Web.Mvc;
using System.Net;
using Ppgz.Repository;
using Ppgz.Services;
using Ppgz.Web.Infrastructure;
using System.Collections;
using System.Text;
using System.Data;
using System.Collections.Generic;
namespace Ppgz.Web.Areas.Mercaderia.Controllers
{

    [Authorize]
    [TerminosCondiciones]
    public class ImpresionEtiquetasIndividualController : Controller
    {
       readonly ProveedorManager _proveedorManager = new ProveedorManager();
		readonly CommonManager _commonManager = new CommonManager();
        private readonly EtiquetasManager _etiquetasManager = new EtiquetasManager();
        private readonly OrdenCompraManager _ordenesCompraManager = new OrdenCompraManager();

        private const string NombreVarSession = "controlCita";
        private const string NombreVarSession1 = "etiqueta_csv";



        internal CurrentCitaEtiqueta CurrentCitaEtiqueta
		{
			get
			{
				if (System.Web.HttpContext.Current.Session[NombreVarSession] == null)
				{
					return null;
				}
				return (CurrentCitaEtiqueta)System.Web.HttpContext.Current.Session[NombreVarSession];
			}
			set
			{
				System.Web.HttpContext.Current.Session[NombreVarSession] = value;
			}
		}


       
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-ImpresionEtiquetaIndividual")]
		public ActionResult Index()
		{
			
			
			var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            ViewBag.proveedores = _proveedorManager.FindByCuentaId(cuenta.Id);

            ViewBag.Almacenes = CommonManager.GetConfiguraciones().Single(c => c.Clave == "warehouse.warehouses").Valor.Split(',');
			
			return View();
		}
		
		[Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-ImpresionEtiquetaIndividual")]
		public ActionResult SeleccionarProveedor(int proveedorId, string centro, string sociedad)
		{
			

			try
			{
				var cuenta = _commonManager.GetCuentaUsuarioAutenticado();
				CurrentCitaEtiqueta = new CurrentCitaEtiqueta(cuenta.Id, proveedorId, centro, sociedad);
                

                return RedirectToAction("BuscarOrden");
			}
			catch (Exception exception)
			{
				TempData["FlashError"] = exception.Message;
				return RedirectToAction("Index"); 
			}
		}
		
		[Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-ImpresionEtiquetaIndividual")]
		public ActionResult BuscarOrden(int proveedorId = 0)
		{
			try
			{
				if (CurrentCitaEtiqueta == null)
				{
					return RedirectToAction("Index");
				}

			}
			catch (BusinessException exception)
			{
				TempData["FlashError"] = exception.Message;
				return RedirectToAction("Index");
			}

			ViewBag.proveedor = CurrentCitaEtiqueta.Proveedor; 
			ViewBag.CurrentCitaEtiqueta = CurrentCitaEtiqueta;
			
			return View();
		}

        [ValidateAntiForgeryToken]
		[HttpPost]
		[Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-ImpresionEtiquetaIndividual")]
		public ActionResult AgregarPrimeraOrden(string numeroDocumento, string fecha)
		{

			if (CurrentCitaEtiqueta == null)
			{
				return RedirectToAction("Index");
			}
			
			
			var proveedor = CurrentCitaEtiqueta.Proveedor;

			try
			{
				

				CurrentCitaEtiqueta.AddPreAsn(numeroDocumento);

				return RedirectToAction("Generar", new {numeroDocumento});

			}
			catch (CurrentCitaEtiqueta.OrdenDuplicadaException)
			{
				TempData["FlashError"] = "El número de documento ya se encuentra en la lista";
				return RedirectToAction("BuscarOrden", new {proveedor.Id});
			}
			catch (CurrentCitaEtiqueta.OrdenSinDetalleException)
			{
				TempData["FlashError"] = "La orden no contiene items para entregar, por favor seleccione otra orden";
				return RedirectToAction("BuscarOrden", new {proveedor.Id});
			}
			catch (CurrentCitaEtiqueta.NumeroDocumentoException)
			{
				TempData["FlashError"] = "Número de documento incorrecto";
				return RedirectToAction("BuscarOrden", new {proveedor.Id});
			}
			catch (BusinessException exception)
			{
				TempData["FlashError"] = exception.Message;
				return RedirectToAction("BuscarOrden", new {proveedor.Id});
			}
			catch (CurrentCitaEtiqueta.FechaException)
			{
				TempData["FlashError"] = "La orden no puede ser entregada en la fecha de la cita";
				return RedirectToAction("BuscarOrden", new {proveedor.Id});
			}
			catch (CurrentCitaEtiqueta.OrdenCentroException)
			{
				TempData["FlashError"] = "La orden no contiene items para el Almacén seleccionado";
				return RedirectToAction("BuscarOrden", new { proveedor.Id });
			}
		}

		[Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-ImpresionEtiquetaIndividual")]
		public ActionResult Generar(string numeroDocumento)
		{

			if (CurrentCitaEtiqueta == null)
			{
				return RedirectToAction("Index");
			}


			if(!CurrentCitaEtiqueta.HasPreAsn(numeroDocumento))
			{
				return RedirectToAction("ListaDeOrdenes");
			}

			ViewBag.OrdenCompra = CurrentCitaEtiqueta.GetPreAsn(numeroDocumento);
			ViewBag.CurrentCitaEtiqueta = CurrentCitaEtiqueta;
			return View();
		
		}


        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-ImpresionEtiquetaIndividual")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Generar(bool nazan, string ordenesy, bool zapato, string tipoimpresora, string cantidades, string materiales)
        // Agregar 2 parametros adicionales materiales y cantidades [000000342343,00000002342342]    [3,20]
        // matener el parametro de orden de compra
        {
            var proveedorId = CurrentCitaEtiqueta.Proveedor.Id;

            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            var proveedor = _proveedorManager.Find(proveedorId, cuenta.Id);

            if (proveedor == null)
            {
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }
            TempData["nazan"] = nazan;
            TempData["zapato"] = zapato;
            TempData["impresora"] = tipoimpresora;

            if (nazan == false && zapato == false)
            {
                nazan = true;
            }

            var resultado = _etiquetasManager.GetArchivoCsv(proveedorId, cuenta.Id, nazan, ordenesy.Split(','), zapato);



            TempData["resultado"] = resultado;
            TempData["proveedor"] = proveedor;
            TempData["cantidades"] = cantidades.Split(',');
            TempData["materiales"] = materiales.Split(',');

            return RedirectToAction("Resultado");
        }


        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-ImpresionEtiquetaIndividual")]
        public ActionResult Resultado()
        {
            if (System.Web.HttpContext.Current.Session[NombreVarSession1] != null)
            {
                System.Web.HttpContext.Current.Session.Remove(NombreVarSession1);
            }

            var resultado = TempData["resultado"] as Hashtable;
            var proveedor = TempData["proveedor"] as proveedore;
            var cantidadesS = TempData["cantidades"] as string[];
            int[] cantidades = Array.ConvertAll(cantidadesS, int.Parse);
            var materiales = TempData["materiales"] as string[];
            bool nazan = Convert.ToBoolean(TempData["nazan"]);
            bool zapato = Convert.ToBoolean(TempData["zapato"]);

            if (resultado == null || proveedor == null)
            {
                return RedirectToAction("Index");
            }


            ViewBag.Proveedor = proveedor;
            ViewBag.Resultado = resultado["return"];

            ViewBag.PuedeDescargar = false;

            if (resultado["csv"] == null) return View();
            if (resultado["csv"].ToString() == "") return View();
            if (string.IsNullOrWhiteSpace(resultado["csv"].ToString())) return View();

            System.Web.HttpContext.Current.Session[NombreVarSession1] = resultado["csv"];
            ViewBag.PuedeDescargar = true;




            //CSV to DataTable
            DataTable dt = new DataTable();
            var s = resultado["csv"].ToString();
            s = s.Replace("\"", "");
            string[] tableData = s.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var col = from cl in tableData[0].Split(",".ToCharArray())
                      select new DataColumn(cl);
            dt.Columns.AddRange(col.ToArray());

            foreach (var item in tableData.Skip(1))
            {
                dt.Rows.Add(item.Split(",".ToCharArray()));
            }

            //fin

            //string[] etiquetas;
            List<string> etiquetas = new List<string>();
            string etiquetasPrint = "";

            //int i = 0;

            var totalEtiquetas = cantidades.Sum();

            for (int i = 0; i < materiales.Count(); i++)
            {
                
            

            if (nazan == true)
            {

                foreach (DataRow row in dt.Rows)
                {

                    if (materiales[i] == row["ITEM"].ToString().Trim())
                    {

                    

                    etiquetas.Add(@"^XA
^SZ2^JMA
^MCY^PMN
^PW580^MTT
^MNW
~JSN
^MD2
^PR3,4,4
^JZY
^LH0,0^LRN
^XZ
^XA^JZN^
^FO5.00,60.50
^GB85.50P0,30.00,45.00,,3^FS
^FO52.50,15.00^A0N,22.50,22.50^FD^FS
^FO5.50,35.00^A0N,27.00,17.00^FD" + row["Fecha"].ToString().Trim() + @" " + row["Hora"].ToString().Trim() + @"^FS
^FO5.50,65.00^A0N,50.00,40.00^FR^FD" + row["Centro"].ToString().Trim() + @"^FS
^FO160.50,194.00^AFN,5.00,12.50^FDCorrida:^FS
^FO160.50,258.50^AFN,5.00,12.50^FDItem:^FS
^FO290.00,194.50^AFN,4.00,7.00^FD" + row["Corrida_real"].ToString().Trim() + @"^FS
^FO240.50,262.00^A0N,25.50,25.00^FD" + row["ITEM"].ToString().Trim() + @"^FS
^FO5.50,328.00^A0N,25.00,25.00^FD" + row["Desc_familia"].ToString().Trim() + @"^FS
^FO160.50,10.00^AFN,5.00,12.50^FDMarca:^FS
^FO160.50,48.50^AFN,5.00,12.50^FDCodigo:^FS
^FO160.50,88.50^AFN,5.00,12.50^FDEstilo:^FS
^FO160.50,126.50^AFN,5.00,12.50^FDColor:^FS
^FO160.50,162.50^AFN,5.50,12.00^FDTalla:^FS
^FO160.50,229.00^AFN,5.00,12.50^FDAcabado:^FS
^FO260.00,4.00^A0N,35.00,28.50^FD" + row["Marca"].ToString().Trim() + @"^FS
^FO275.00,45.50^A0N,30.00,32.50^FD" + row["Codigo_Uni"].ToString().Trim() + @"^FS
^FO275.00,82.50^A0N,35.00,35.50^FD" + row["estilo"].ToString().Trim() + @"^FS
^FO262.00,124.50^AFN,3.00,8.50^FD" + row["color"].ToString().Trim() + @"^FS
^FO290.50,227.00^AFN,2.00,4.50^FD" + row["acabado"].ToString().Trim() + @"^FS
^FO5.50,298.50^A0N,26.50,24.50^FD" + row["Agrupador_familia"].ToString().Trim() + @"^FS
^FO5.50,237.50^A0N,20.40,25.00^FD" + row["Pedido"].ToString().Trim() + @"^FS
^FO5.50,262.50^A0N,20.15,26.50^FD" + row["No_Prov"].ToString().Trim() + @"^FS
^FO510.00,348.00^A0N,20.35,42.00^FD" + row["IR"].ToString().Trim() + @"^FS
^ISLABEL001,N,^FS
^XZ
^XA^JZN^PR9
^PQ" + cantidades[i].ToString() + @",0,1,Y
^ILLABEL001^FS
^FO262.00,161.50^AFN,15.00,22.50^FD" + row["talla"].ToString().Trim() + @"^FS
^FO180.00,295.00^BY2.00,.8^BAN,47.00,N,N,N^FD" + row["Ean_Nazan"].ToString().Trim() + @"^FS
^FO212.00,348.50^A0N,24.30,40.00^FD" + row["Ean_Nazan"].ToString().Trim() + @"^FS
^XZ
^XZ
^EG
^XZ");
                        //i++;  }
                    }
                }


            }
            else
            {
                if (zapato)
                {
                    //Etiquetas Colgantes
                    foreach (DataRow row in dt.Rows)
                    {

                        if (materiales[i] == row["ITEM"].ToString().Trim())
                        {
                            /**
                             *
                             *Se selecciona entre plantilla para Toshiba o Zebra
                             */
                            if (TempData["impresora"].ToString() == "Toshiba")
                            {
                                etiquetas.Add(@"^XA
^SZ2^JMA
^MCY^PMN
^PW220^MTT
^MNM
~JSN
^MD2
^PR3,4,4
^JZY
^LH0,0^LRN
^XZ
^XA^JZN^
^FO0.50,263.00^AFN,5.00,12.50^FDSKU:^FS
^FO59.00,263.00^A0N,27.00,32.00^FD" + row["Sku_cadena"].ToString().Trim() + @"^FS
^FO.50,185.50^AFN,5.50,12.00^FDTalla:^FS
^FO5.00,32.00^A0N,35.00,30.50^FD" + row["Marca"].ToString().Trim() + @"^FS
^FO0.50,80.50^A0N,38.00,45.50^FD" + row["estilo"].ToString().Trim() + @"^FS
^FO0.50,125.50^AFN,3.00,8.50^FD" + row["color"].ToString().Trim() + @"^FS
^FO0.50,160.50^A0N,20.50,30.00^FD" + row["Desc_familia"].ToString().Trim() + @"^FS
^FO0.50,220.50^A0N,35.50,40.00^FD$^FS 
^FO40.50,220.50^A0N,40.60,35.00^FD" + row["Entero_prec"].ToString().Trim() + @"^FS
^FO100.50,220.50^A0N,22.30,25.00^FD" + row["Dec_prec"].ToString().Trim() + @"^FS
^ISLABEL001,N,^FS
^XZ
^XA^JZN^PR9
^PQ" + cantidades[i].ToString() + @",0,1,Y
^ILLABEL001^FS
^FO95.00,185.50^AFN,10.00,18.50^FD" + row["talla"].ToString().Trim() + @"^FS
^FO10.00,295.00^BY2,.12^BEN,55.00,Y,N^FD" + row["Ean_cadena"].ToString().Trim() + @"^FS
^XZ
^XZ
^EG 
^XZ");
                            }
                            else
                            {
                                etiquetas.Add(@"^XA^JZN^
^FO60.50,287.50^AFN,5.00,12.50^FDSKU:^FS
^FO125.50,288.00^A0N,25.50,25.00^FD" + row["Sku_cadena"].ToString().Trim() + @"^FS
^FO50.50,210.50^AFN,5.50,12.00^FDTalla:^FS
^FO55.00,70.00^A0N,35.00,30.50^FD" + row["Marca"].ToString().Trim() + @"^FS
^FO100.00,110.50^A0N,35.00,35.50^FD" + row["estilo"].ToString().Trim() + @"^FS
^FO98.00,150.50^AFN,3.00,8.50^FD" + row["color"].ToString().Trim() + @"^FS
^FO77.50,185.50^A0N,20.50,30.00^FD" + row["Desc_familia"].ToString().Trim() + @"^FS
^FO60.50,245.50^A0N,35.50,40.00^FD$^FS 
^FO105.50,245.50^A0N,40.60,35.00^FD" + row["Entero_prec"].ToString().Trim() + @"^FS
^FO160.50,240.50^A0N,19.30,29.00^FD" + row["Dec_prec"].ToString().Trim() + @"^FS
^ISLABEL001,N,^FS
^XZ
^XA^JZN^PR9
^PQ" + cantidades[i].ToString() + @",0,1,Y
^ILLABEL001^FS
^FO145.00,210.50^AFN,10.00,18.50^FD" + row["talla"].ToString().Trim() + @"^FS
^FO40.00,315.00^BY2,.10^BEN,60.00,Y,N^FD" + row["Ean_cadena"].ToString().Trim() + @"^FS
^XZ
^XZ
^EG 
^XZ");
                            }

                            //Fin Seleccion
                        }

                    }

                }
                else
                {
                    //Etiquetas De Precio cadena
                    foreach (DataRow row in dt.Rows)
                    {
                        if (materiales[i] == row["ITEM"].ToString().Trim())
                        {
                            var barras = "";
                            if (row["Centro"].ToString().Trim() != "CD04")
                            {
                                barras = @"^FO30,283.50^A0N,40.50,50.00^FD$^FS
^FO82.50,267.50^A0N,77.60,72.00^FD" + row["Entero_prec"].ToString().Trim() + @"^FS
^FO190.50,267.50^A0N,35.60,34.00^FD" + row["Dec_prec"].ToString().Trim() + @"^FS
^FO300.00,296.00^BY2.0.20,.20^BEN,48.00,Y,N^FD" + row["Ean_cadena"].ToString().Trim() + @"^FS";

                            }

                            etiquetas.Add(@"^XA
^SZ2^JMA
^MCY^PMN
^PW580^MTT
^MNW
~JSN
^MD2
^PR3,4,4
^JZY
^LH0,0^LRN
^XZ
^XA^JZN^
^FO35.00,44.55^GB0.0P0,30.00,45.00,,3^FS
^FO52.50,5.00^A0N,22.50,22.50^FD^FS
^FO475.50,3.00^A0N,22.00,20.00^FD" + row["Fecha"].ToString().Trim() + @" " + row["Hora"].ToString().Trim() + @"^FS
^FO30.00,45.00^A0N,50.00,40.00^FR^FD" + row["Centro"].ToString().Trim() + @"^FS
^FO188.50,192.00^AFN,5.00,12.00^FDCorrida:^FS
^FO238.50,265.50^AFN,5.00,12.50^FDItem:^FS
^FO318.00,192.50^AFN,4.00,7.00^FD" + row["Corrida_real"].ToString().Trim() + @"^FS
^FO320.50,265.00^A0N,27.47,25.00^FD" + row["ITEM"].ToString().Trim() + @"^FS
^FO170.50,37.00^AFN,5.00,12.50^FDMarca:^FS
^FO208.50,81.50^AFN,5.00,12.50^FDEstilo:^FS
^FO218.50,119.50^AFN,5.00,12.50^FDColor:^FS
^FO218.50,157.50^AFN,5.50,12.00^FDTalla:^FS
^FO188.50,227.00^AFN,5.00,12.50^FDAcabado:^FS
^FO264.00,24.50^A0N,45.00,40.50^FD" + row["Marca"].ToString().Trim() + @"^FS
^FO318.00,77.50^A0N,35.00,42.50^FD" + row["estilo"].ToString().Trim() + @"^FS
^FO318.00,119.50^AFN,3.00,8.50^FD" + row["color"].ToString().Trim() + @"^FS
^FO318.50,226.00^AFN,2.00,4.50^FD" + row["acabado"].ToString().Trim() + @"^FS
^FO30.50,235.50^A0N,20.50,25.00^FD" + row["Pedido"].ToString().Trim() + @"^FS
^FO30.00,342.50^A0N,30.20,30.50^FD" + row["No_Prov"].ToString().Trim() + @"^FS
^FO30.50,400.00^A0N,10.50,15.00^FDWeb 1^FS
^ISLABEL001,N,^FS
^XZ
^XA^JZN^PR9
^PQ" + cantidades[i].ToString() + @",0,1,Y
^ILLABEL001^FS
^FO318.00,157.50^AFN,7.00,32.50^FD" + row["talla"].ToString().Trim() + @"^FS
" + barras + @"
^XZ
^XA
^JZN");
                            //i++;
                            //}

                        }

                    }
                }
            }
            }

            foreach (string etiqueta in etiquetas)
            {
                etiquetasPrint = etiquetasPrint + etiqueta;
            }

            ViewBag.etiquetas = etiquetas;
            ViewBag.etiquetasPrint = etiquetasPrint;
            TempData["texto"] = etiquetasPrint;
            ViewBag.totalEtiquetas = totalEtiquetas;
            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-ImpresionEtiquetaIndividual")]
        public FileContentResult Descargar()
        {
            var csv = System.Web.HttpContext.Current.Session["etiqueta_csv"].ToString();

            return File(new UTF8Encoding().GetBytes(csv), "text/csv", "etiquetas.csv");
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-ImpresionEtiquetaIndividual")]
        public FileContentResult ImpresionEtiquetas()
        {
            var texto = TempData["texto"].ToString();

            return File(TextToByte(texto), System.Net.Mime.MediaTypeNames.Application.Octet, "Impresion Etiquetas.txt");
        }

        public static byte[] TextToByte(string texto)
        {
            // convert string to stream
            byte[] byteArray = Encoding.ASCII.GetBytes(texto);
            return byteArray;
        }




		[HttpPost]
		[Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-ImpresionEtiquetaIndividual")]
		public JsonResult AsnActualizarDetalle(string numeroDocumento, string numeroPosicion,  string numeroMaterial, int cantidad)
		{
			try
			{
				CurrentCitaEtiqueta.UpdateDetail(numeroDocumento, numeroPosicion, numeroMaterial, cantidad);
			}
			catch (Exception exception)
			{
				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return Json(exception.Message);
			}
			return Json("Actualizado correctamente");
		}




    }
}