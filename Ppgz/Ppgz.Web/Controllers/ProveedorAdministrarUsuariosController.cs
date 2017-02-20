using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Ppgz.Repository;
using Ppgz.Services;
using Ppgz.Web.Infraestructure;
using Ppgz.Web.Models;

namespace Ppgz.Web.Controllers
{
    [Authorize]
    public class ProveedorAdministrarUsuariosController : Controller
    {
        private readonly CommonManager _commonManager = new CommonManager();
        private readonly UsuarioManager _usuarioManager = new UsuarioManager();
        private readonly TipoUsuarioManager _tipoUsuarioManager = new TipoUsuarioManager();
        //
        // GET: /ProveedorAdministrarUsuarios/
        public ActionResult Index()
        {
            var usuarios = _usuarioManager.FindUsuariosProveedorByCuentaId(_commonManager.GetCuentaUsuarioAutenticado().id);

            ViewBag.usuarios = usuarios;

            return View();
        }

        public ActionResult Registrar()
        {

            ViewBag.Accesos = new SelectList(PermisosManager.NazanAccessos); 

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar(ProveedorUsuarioViewModel model)
        {
            ViewBag.Accesos = new SelectList(PermisosManager.NazanAccessos); 

            if (ModelState.IsValid)
            {
                if (_usuarioManager.FindUsuarioByUserName(model.UserName) != null)
                {
                    ModelState.AddModelError(string.Empty, "Este nombre de usuario ya fue utilizado. Por favor intente con otro");
                    return View(model);
                }

                var usuario = new usuario()
                {
                    userName = model.UserName,
                    nombre = model.Nombre,
                    apellido = model.Apellido,
                    email = model.Email,
                    tipo_usuario_id = _tipoUsuarioManager.GetProveedor().id

                };

                _usuarioManager.Add(usuario, model.Password, model.Acceso);

                _commonManager.UsuarioCuentaXrefAdd(
                   usuario,
                    _commonManager.GetCuentaUsuarioAutenticado().id);


                return RedirectToAction("Index");

            }

            return View(model);
        }

        public ActionResult Editar(int id)
        {
            var usuario = _usuarioManager.Find(id);

            if (usuario == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var tipoUsuarioProveedor = _tipoUsuarioManager.GetProveedor();

            if (usuario.tipo_usuario_id != tipoUsuarioProveedor.id)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }


            ViewBag.Accesos = new SelectList(PermisosManager.NazanAccessos); 

            var usuarioProveedor = new ProveedorUsuarioViewModel
            {
                UserName = usuario.userName,
                Nombre = usuario.nombre,
                Apellido = usuario.apellido,
                Email = usuario.email,
                Acceso = usuario.SecurityStamp
            };

            return View(usuarioProveedor);
        }

        [HttpPost, ActionName("Editar")]
        [ValidateAntiForgeryToken]
        public ActionResult EditarPost(int id, FormCollection collection)
        {
            var usuario = _usuarioManager.Find(id);

            if (usuario == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var tipoUsuarioProveedor = _tipoUsuarioManager.GetProveedor();

            if (usuario.tipo_usuario_id != tipoUsuarioProveedor.id)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            try
            {
                _usuarioManager.UpdateUsuario(
                                id,
                                collection["nombre"],
                                collection["apellido"],
                                collection["telefono"],
                                collection["email"],
                                collection["password"],
                                collection["acceso"]);

                return RedirectToAction("Index");
            }
            catch (RetryLimitExceededException)
            {
                ModelState.AddModelError("", "No se pudo guardar. Intente nuevamente si el problema persiste contacte los adminsitradores del sistema.");
            }


            var usuarioProveedor = new ProveedorUsuarioViewModel
            {
                UserName = collection["UserName"],
                Nombre = collection["Nombre"],
                Apellido = collection["Apellido"],
                Email = collection["Email"],
            };

            ViewBag.Accesos = new SelectList(PermisosManager.NazanAccessos); 

            return View(usuarioProveedor);
        }

        public ActionResult Eliminar(int id)
        {


            // TODO INFRORMAR QUE NO PUEDE ELIMINAR EL USUARIO AUTENTICADO
            if (_commonManager.GetUsuarioAutenticado().Id == id)
            {
                return RedirectToAction("Index");
            }

            var usuario = _usuarioManager.Find(id);

            // TODO VALIDAR QUE LA CUENTA DEL USUARIO ELIMINADO SEA IGUAL A LA DEL AUTENTICADO

   


            if (usuario == null)
            {
                return HttpNotFound();
            }


            if (usuario.tipo_usuario_id != _tipoUsuarioManager.GetProveedor().id)
            {

                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }


            try
            {

                _usuarioManager.Remove(id);
            }
            catch (RetryLimitExceededException)
            {

                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
    }
}