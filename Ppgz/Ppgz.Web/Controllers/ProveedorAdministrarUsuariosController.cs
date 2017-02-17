using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Ppgz.Repository;
using Ppgz.Web.Models;

namespace Ppgz.Web.Controllers
{
    [Authorize]
    public class ProveedorAdministrarUsuariosController : Controller
    {
        private readonly PpgzEntities _db = new PpgzEntities();

        //
        // GET: /ProveedorAdministrarUsuarios/
        public ActionResult Index()
        {
            var tipoUsuarioProveedor = _db.tipos_usuario.First(t => t.codigo == "PROVEEDOR");

            var usuarios = _db.usuarios.Where(u => u.tipo_usuario_id == tipoUsuarioProveedor.id).ToList();

            ViewBag.usuarios = usuarios;

            return View();
        }

        public ActionResult Registrar()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar(ProveedorUsuarioViewModel model)
        {
            if (ModelState.IsValid)
            {

                if (_db.usuarios.Any(u => u.userName == model.UserName))
                {

                    ModelState.AddModelError(string.Empty, "Este nombre de usuario ya fue utilizado. Por favor intente con otro");
                    return View(model);
                }

                var tipoUsuarioNazan = _db.tipos_usuario.First(t => t.codigo == "PROVEEDOR");

                var userName = User.Identity.GetUserName();
                var usuarioAutenticado = _db.usuarios.Single(u => u.userName == userName);
                var cuenta = usuarioAutenticado.cuentas.First();

                var usuario = new usuario()
                {
                    userName = model.UserName,
                    nombre = model.Nombre,
                    apellido = model.Apellido,
                    email = model.Email,
                    PasswordHash = model.Password,
                    SecurityStamp = model.Password,
                    tipo_usuario_id = tipoUsuarioNazan.id

                };
                
                usuario.cuentas.Add(cuenta);

                _db.usuarios.Add(usuario);

                _db.SaveChanges();


                return RedirectToAction("Index");

            }

            return View(model);
        }

        public ActionResult Editar(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }


            var usuario = _db.usuarios.Single(i => i.Id == id);


            if (usuario == null)
            {
                return HttpNotFound();
            }


            var tipoUsuarioProveedor = _db.tipos_usuario.First(t => t.codigo == "PROVEEDOR");

            if (usuario.tipo_usuario_id != tipoUsuarioProveedor.id)
            {

                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            var usuarioProveedor = new ProveedorUsuarioViewModel
            {
                UserName = usuario.userName,
                Nombre = usuario.nombre,
                Apellido = usuario.apellido,
                Email = usuario.email,
            };

            return View(usuarioProveedor);
        }

        [HttpPost, ActionName("Editar")]
        [ValidateAntiForgeryToken]
        public ActionResult EditarPost(int? id, FormCollection collection)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var usuarioParaActualizar = _db.usuarios.Single(i => i.Id == id);

            if (TryUpdateModel(usuarioParaActualizar, "",
               new string[] { "nombre", "apellido", "email", "password" }))
            {
                try
                {
                    _db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException)
                {
                    ModelState.AddModelError("", "No se pudo guardar. Intente nuevamente si el problema persiste contacte los adminsitradores del sistema.");
                }
            }

            var usuarioProveedor = new ProveedorUsuarioViewModel
            {
                UserName = collection["UserName"],
                Nombre = collection["Nombre"],
                Apellido = collection["Apellido"],
                Email = collection["Email"],
            };

            return View(usuarioProveedor);
        }

        public ActionResult Eliminar(int id)
        {


            var usuario = _db.usuarios.Single(i => i.Id == id);


            if (usuario == null)
            {
                return HttpNotFound();
            }


            var tipoUsuarioProveedor = _db.tipos_usuario.First(t => t.codigo == "PROVEEDOR");

            if (usuario.tipo_usuario_id != tipoUsuarioProveedor.id)
            {

                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }


            try
            { 
                _db.usuarios.Remove(usuario);
                _db.SaveChanges();
            }
            catch (RetryLimitExceededException)
            {
      
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
    }
}