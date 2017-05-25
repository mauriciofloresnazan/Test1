using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Ppgz.Services;
using Ppgz.Web.Infrastructure;

namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class FacturasController : Controller
    {
        private readonly CommonManager _commonManager = new CommonManager();
        private readonly ProveedorManager _proveedorManager = new ProveedorManager();
        private readonly FacturaManager _facturaManager = new FacturaManager();


        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-FACTURAS")]
        public ActionResult Index()
        {

            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            ViewBag.proveedores = _proveedorManager.FindByCuentaId(cuenta.Id);

            return View();
        }
            
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-FACTURAS")]
        public ActionResult Facturas(int proveedorId)
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            var proveedor = _proveedorManager.Find(proveedorId, cuenta.Id);
            
            if (proveedor ==  null)
            {
                // TODO
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }

            ViewBag.Facturas = _facturaManager.GetFacturas(proveedorId);

            ViewBag.Proveedor = proveedor;

            return View();

        }
        
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-FACTURAS")]
        public ActionResult CargarFactura(int proveedorId)
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            var proveedor = _proveedorManager.Find(proveedorId, cuenta.Id);

            if (proveedor == null)
            {
                // TODO
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }


            ViewBag.Proveedor = proveedor;

            return View();

        }


        internal void ValidarPdf(HttpPostedFileBase file)
        {
            if (file != null && file.ContentType != "application/pdf")
            {
                throw new BusinessException("Pdf Invalido");
            }

            if (file == null)
            {
                throw new BusinessException("Pdf Invalido");
            }
            /*
            var fileName = Path.GetFileName(file.FileName);

            if (fileName == null)
            {
                throw new BusinessException(MensajesResource.ERROR_MensajesInstitucionales_PdfInvalido);
            }

            var path = Path.Combine(Server.MapPath("~/Uploads/"), fileName);

            file.SaveAs(path);

            return "~/Uploads/" + fileName;*/
        }
        internal void ValidarXml(HttpPostedFileBase file)
        {
            if (file != null && file.ContentType != "text/xml")
            {
                throw new BusinessException("Xml Invalido");
            }

            if (file == null)
            {
                throw new BusinessException("Xml Invalido");
            }

            _facturaManager.ValidarFactura(file.InputStream);

            /*
            var fileName = Path.GetFileName(file.FileName);

            if (fileName == null)
            {
                throw new BusinessException(MensajesResource.ERROR_MensajesInstitucionales_PdfInvalido);
            }

            var path = Path.Combine(Server.MapPath("~/Uploads/"), fileName);

            file.SaveAs(path);

            return "~/Uploads/" + fileName;*/
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-FACTURAS")]
        public ActionResult CargarFactura(FacturaViewModel model)
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();

            var proveedor = _proveedorManager.Find(model.ProveedorId, cuenta.Id);

            if (proveedor == null)
            {
   
                TempData["FlashError"] = "Proveedor incorrecto";
                return RedirectToAction("Index");
            }

            ViewBag.Proveedor = proveedor;


            if (!ModelState.IsValid) return View(model);

            try
            {
                
                if (Request.Files.Count == 0)
                {
                    ModelState.AddModelError(string.Empty, "Archivos incorrectos");
                    return View(model);
                }

                if (Request.Files["xml"] == null)
                {
                    ModelState.AddModelError(string.Empty, "Xml Incorrecto");

                    return View(model);
                    
                }
                if (Request.Files["pdf"] == null)
                {
                    ModelState.AddModelError(string.Empty, "Pdf incorrecto.");

                    return View(model);
                    
                }

                var xmlFile = Request.Files["xml"];
                ValidarXml(xmlFile);
                var tempXmlPath = Path.Combine(Server.MapPath("~/Uploads/"), "temp-" + Guid.NewGuid() + ".xml");
                xmlFile.SaveAs(tempXmlPath);
                
                var pdfFile = Request.Files["pdf"];
                ValidarPdf(pdfFile);
                var tempPdfPath = Path.Combine(Server.MapPath("~/Uploads/"), "temp-" + Guid.NewGuid() + ".pdf");
                pdfFile.SaveAs(tempPdfPath);


                _facturaManager.CargarFactura(proveedor.Id, cuenta.Id, tempXmlPath, tempPdfPath);
                

                TempData["FlashSuccess"] = "Factura registrada satisfactoriamente";
                return RedirectToAction("Facturas", new { proveedorId  = proveedor.Id });
            }
            catch (BusinessException businessEx)
            {
                ModelState.AddModelError(string.Empty, businessEx.Message);

                return View(model);
            }
            catch (Exception e)
            {
                var log = CommonManager.BuildMessageLog(
                    TipoMensaje.Error,
                    ControllerContext.Controller.ValueProvider.GetValue("controller").RawValue.ToString(),
                    ControllerContext.Controller.ValueProvider.GetValue("action").RawValue.ToString(),
                    e.ToString(), Request);

                CommonManager.WriteAppLog(log, TipoMensaje.Error);

                ModelState.AddModelError(string.Empty, e.Message);
                return View(model);
            }

        }

                
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-FACTURAS")]
        public FileResult DescargarXml(int facturaId, int proveedorId)
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();
            var proveedor = _proveedorManager.Find(proveedorId, cuenta.Id);
            if (proveedor == null)
            {
                throw new Exception("Proveedor incorrecto");
            }

           var factura = _facturaManager.Find(facturaId, proveedorId);

            if (factura == null)
            {
                throw new Exception("Factura incorrecta");
            }

            var fileBytes = System.IO.File.ReadAllBytes(factura.XmlRuta);

            var fileName = factura.Uuid + ".xml";

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Text.Xml, fileName);


        }
        
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-FACTURAS")]
        public FileResult DescargarPdf(int facturaId, int proveedorId)
        {
            var cuenta = _commonManager.GetCuentaUsuarioAutenticado();
            var proveedor = _proveedorManager.Find(proveedorId, cuenta.Id);
            if (proveedor == null)
            {
                throw new Exception("Proveedor incorrecto");
            }

            var factura = _facturaManager.Find(facturaId, proveedorId);

            if (factura == null)
            {
                throw new Exception("Factura incorrecta");
            }

            var fileBytes = System.IO.File.ReadAllBytes(factura.PdfRuta);

            var fileName = factura.Uuid + ".pdf";

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, fileName);


        }
    }
}