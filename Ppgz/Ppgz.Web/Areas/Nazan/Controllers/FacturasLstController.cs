using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using Ppgz.Repository;

namespace Ppgz.Web.Areas.Nazan.Controllers
{
    public class FacturasLstController : Controller
    {
        // GET: Nazan/FacturasLst
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-LISTARFACTURAS")]
        public ActionResult Index()
        {
            ViewBag.Proveedores = null;
            //JArray lstProveedores = new JArray();
            try
            {
                List<proveedore> lstProveedores = new List<proveedore>();
                DataSet dsProveedores = ConsultasDB.GetProveedores();
                if (dsProveedores.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in dsProveedores.Tables[0].Rows)
                    {
                        //dynamic Proveedor = new JObject();
                        proveedore Proveedor = new proveedore();
                        Proveedor.Id = Convert.ToInt32(dr["Id"].ToString());
                        Proveedor.Nombre1 = dr["Nombre"].ToString();
                        Proveedor.Rfc = dr["Rfc"].ToString();
                        lstProveedores.Add(Proveedor);
                    }
                    //ViewBag.Proveedores = lstProveedores.ToString();
                    ViewBag.Proveedores = lstProveedores;
                }
            }
            catch
            {

            }
            return View();
        }

        [HttpPost]
        public ActionResult GetFacturasxProveedor(string IdProveedor,string status,string fechaFin, string fecha = null)
        {
            var jsonresult = new JsonResult();
            jsonresult.MaxJsonLength = int.MaxValue;
            JArray lstFacturas = new JArray();
            DateTime FechaInicio;
            DateTime FechaFin;
            try
            {
                FechaInicio = DateTime.Parse(fecha);
                FechaFin = DateTime.Parse(fechaFin).AddDays(1);
                DataSet dsFacturas = ConsultasDB.GetFacturasxFecha(FechaInicio.ToString("yyyy-MM-dd"), FechaFin.ToString("yyyy-MM-dd"), IdProveedor, status);
                if (dsFacturas.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in dsFacturas.Tables[0].Rows)
                    {
                        dynamic Factura = new JObject();
                        Factura.rfc = dr["rfc"].ToString();
                        Factura.Nombre = dr["Nombre"].ToString();
                        Factura.Serie = dr["Serie"].ToString();
                        Factura.Folio = dr["Folio"].ToString();
                        Factura.EstatusOriginal = dr["EstatusOriginal"].ToString();
                        Factura.NumeroGenerado = dr["NumeroGenerado"].ToString();
                        Factura.Uuid = dr["Uuid"].ToString();
                        Factura.Fecha = dr["Fecha"].ToString();
                        Factura.FechaPortal = dr["FechaPortal"].ToString();
                        Factura.Total = dr["Total"].ToString();
                        Factura.Estatus = dr["Estatus"].ToString();
                        Factura.Comentario = dr["Comentario"].ToString();
                        Factura.RFCReceptor = dr["RFCReceptor"].ToString();
                        Factura.Procesado = dr["Procesado"].ToString();
                        Factura.XmlRuta = dr["XmlRuta"].ToString();
                        Factura.PdfRuta = dr["PdfRuta"].ToString();
                        if(FileCheckExist(dr["XmlRuta"].ToString()))
                            if (FileCheckExist(dr["PdfRuta"].ToString()))
                                lstFacturas.Add(Factura);
                    }
                    jsonresult = Json(new { status = "success", Datos = lstFacturas.ToString(), msg = "Consulta Correcta" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    jsonresult = Json(new { status = "success", Datos = "", msg = "Sin Facturas a Mostrar" }, JsonRequestBehavior.AllowGet);
                }
                
            }
            catch (Exception ex)
            {
                jsonresult = Json(new { status = "error", Datos = ex.Message, msg = "Error en la Consulta" }, JsonRequestBehavior.AllowGet);
            }
            return jsonresult;
        }

        private bool FileCheckExist(string filePath)
        {
            bool check = false;
            FileInfo existingFile = new FileInfo(filePath);
            try
            {    
                if (existingFile.Exists)
                    check = true;
                return check;
            }
            catch
            {
                return check;
            }
        }

        [HttpPost]
        public ActionResult CopyFile(string FileName, string FilePath)
        {
            var jsonresult = new JsonResult();
            string ServerPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Uploads/");
            string FileNameDestino = "View_" + DateTime.Today.ToString("yyyyMMdd") + "_Tmp_" + Path.GetFileName(FilePath);
            try
            {
                CleanFilesTemp();
            }
            catch
            {

            }
            try
            {
                if (!FileCheckExist(ServerPath + FileNameDestino))
                    System.IO.File.Copy(FilePath, ServerPath + FileNameDestino);
                jsonresult = Json(new { status = "success", Datos = FileNameDestino, msg = "Consulta Correcta" }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                jsonresult = Json(new { status = "error", Datos = ex.Message, msg = "Error en la Consulta" }, JsonRequestBehavior.AllowGet);
            }

            return jsonresult;

        }

        public void CleanFilesTemp()
        {
            string coincidencias = @"View_" + DateTime.Today.AddDays(-3).ToString("yyyyMMdd") + "_Tmp_*.pdf";
            string[] fileList = System.IO.Directory.GetFiles(System.Web.Hosting.HostingEnvironment.MapPath("~/Uploads/"), coincidencias);
            foreach (string file in fileList)
            {
                System.IO.File.Delete(file);
            }
        }
    }
}