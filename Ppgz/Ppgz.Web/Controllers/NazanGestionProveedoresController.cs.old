using System;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Ppgz.Repository;
using Ppgz.Web.Infrastructure;
using Ppgz.Web.Models;


namespace Ppgz.Web.Controllers
{
    [Authorize]
    public class NazanGestionProveedoresController : Controller
    {
        readonly CuentaManager _cuentaManager = new CuentaManager();
        readonly TipoProveedorManager _tipoProveedorManager = new TipoProveedorManager();
        readonly UserManager<ApplicationUser> _applicationUserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
        readonly UsuarioManager _usuarioManager = new UsuarioManager();
        readonly CommonManager _commonManager = new CommonManager();
        //
        // GET: /NazanGestionProveedores/
        public ActionResult Index()
        {
            var proveedores = _cuentaManager.FinAll();

            var proveedorMercaderia = _tipoProveedorManager.GetByCodigo("MERCADERIA");
            var proveedorServicio = _tipoProveedorManager.GetByCodigo("SERVICIO");

            ViewBag.proveedorMercaderia = proveedorMercaderia;
            ViewBag.proveedorServicio = proveedorServicio;
            ViewBag.proveedores = proveedores;

            return View();
        }

        // GET: /NazanGestionProveedores/
        public ActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar(CuentaViewModel model)
        {
            if (ModelState.IsValid)
            {
          
                if (_applicationUserManager.FindByName(model.UserName) != null)
                {
                    ModelState.AddModelError(string.Empty, ResourceErrores.NombreUsuarioExistente);
                    return View(model);
                }

                var usuario = new ApplicationUser() { UserName = model.UserName };

                var result = _applicationUserManager.Create(usuario, model.ResponsablePassword);
              
                if (result.Succeeded)
                {

                    _usuarioManager.Update(
                        usuario.Id, 
                        model.ProveedorNombre, 
                        model.ResponsableApellido,
                        model.ResponsableCargo, 
                        model.ResponsableEmail,
                        model.ResponsableTelefono, 
                        TipoUsuario.ProveedorMaestro);
                    
                    var tipoProveedor = _tipoProveedorManager.GetByCodigo(model.TipoProveedor);

                    // Registro de la cuenta
                    var cuenta = new cuenta
                    {
                        nombre_proveedor = model.ProveedorNombre,
                        // TODO MERJORAR EL CODIGO DEL PROVEDOR
                        codigo_proveedor = DateTime.Now.ToString("yyyyMMddHHmmssf"),
                        reponsable_usuario_id = usuario.Id,
                        tipo_proveedor_id = tipoProveedor.id,
                        activo = true
                    };
                    
                    _cuentaManager.Add(cuenta);

                    _commonManager.UsuarioCuentaXrefAdd(usuario.Id, cuenta.id);

                    return RedirectToAction("Index");
                }

                
                
            }

            return View(model);

        }
    }
}