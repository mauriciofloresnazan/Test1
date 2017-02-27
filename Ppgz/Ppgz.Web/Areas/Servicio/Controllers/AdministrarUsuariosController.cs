using System;
using System.Data.Entity.Infrastructure;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Ppgz.Web.Infrastructure;
using Ppgz.Web.Infrastructure.Nazan;

namespace Ppgz.Web.Areas.Servicio.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class AdministrarUsuariosController : Controller
    {

        private readonly UsuarioProveedorManager _usuarioProveedorManager = new UsuarioProveedorManager();
        private readonly PerfilProveedorManager _perfilProveedorManager = new PerfilProveedorManager();
        private readonly CommonManager _commonManager = new CommonManager();


        [Authorize(Roles = "MAESTRO-SERVICIO,SERVICIO-ADMINISTRARUSUARIOS-LISTAR,SERVICIO-ADMINISTRARUSUARIOS-MODIFICAR")]
        public ActionResult Index()
        {
            ViewBag.Usuarios = _usuarioProveedorManager
                .FindByCuentaId(_commonManager.GetCuentaUsuarioAutenticado().Id);

            return View();
        }

        [Authorize(Roles = "MAESTRO-SERVICIO,SERVICIO-ADMINISTRARUSUARIOS-MODIFICAR")]
        public ActionResult Crear()
        {
            ViewBag.Perfiles =
                new SelectList(
                    _perfilProveedorManager.FindByCuentaId(
                        _commonManager.GetCuentaUsuarioAutenticado().Id), "Id", "Nombre");

            return View();
        }

        [Authorize(Roles = "MAESTRO-SERVICIO,SERVICIO-ADMINISTRARUSUARIOS-MODIFICAR")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Crear(UsuarioProveedorViewModel model)
        {
            ViewBag.Perfiles =
                new SelectList(
                    _perfilProveedorManager.FindByCuentaId(
                        _commonManager.GetCuentaUsuarioAutenticado().Id), "Id", "Nombre");

            if (!ModelState.IsValid) return View(model);

            try
            {
                _usuarioProveedorManager
                    .Create(model.UserName, model.Nombre, model.Apellido,
                    model.Email, model.Password, model.Perfil, "PROVEEDOR");

                var usuarioManager = new UsuarioManager();
                var usuario = usuarioManager.FindUsuarioByUserName(model.UserName);

                var cuentaManager = new CuentaManager();
                cuentaManager.AsociarUsuarioEnCuenta(
                    usuario.Id,
                    _commonManager.GetCuentaUsuarioAutenticado().Id);

                TempData["FlashSuccess"] = "Usuario creado con éxito.";
                return RedirectToAction("Index");
            }
            catch (Exception exception)
            {

                ModelState.AddModelError(string.Empty, exception.Message);

                return View(model);
            }

        }

        [Authorize(Roles = "MAESTRO-SERVICIO,SERVICIO-ADMINISTRARUSUARIOS-MODIFICAR")]
        public ActionResult Editar(string id)
        {
            ViewBag.Perfiles =
                new SelectList(
                    _perfilProveedorManager.FindByCuentaId(
                        _commonManager.GetCuentaUsuarioAutenticado().Id), "Id", "Nombre");

            var usuario = _usuarioProveedorManager.Find(id);

            if (usuario == null)
            {
                //TODO ACTUALIZAR MENSAJE AL RESOURCE
                TempData["FlashError"] = "Usuario incorrecto.";
                return RedirectToAction("Index");
            }

            var usuarioProveedorViewModel = new UsuarioProveedorViewModel()
            {
                UserName = usuario.UserName,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email,
                Perfil = usuario.PerfilId
            };

            return View(usuarioProveedorViewModel);
        }

        [Authorize(Roles = "MAESTRO-SERVICIO,SERVICIO-ADMINISTRARUSUARIOS-MODIFICAR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(string id, UsuarioProveedorViewModel model)
        {
            var usuario = _usuarioProveedorManager.Find(id);

            if (usuario == null)
            {
                //TODO ACTUALIZAR MENSAJE AL RESOURCE
                TempData["FlashError"] = "Usuario incorrecto.";
                return RedirectToAction("Index");
            }

            try
            {
                _usuarioProveedorManager.Update(
                    id,
                    model.Nombre,
                    model.Apellido,
                    model.Email,
                    model.Perfil,
                    model.Password);

                TempData["FlashSuccess"] = "Usuario actualizado con éxito.";
                return RedirectToAction("Index");
            }
            catch (RetryLimitExceededException)
            {
                ModelState.AddModelError("", ResourceErrores.RegistroGeneral);
            }

            ViewBag.Perfiles =
                new SelectList(
                    _perfilProveedorManager.FindByCuentaId(
                        _commonManager.GetCuentaUsuarioAutenticado().Id), "Id", "Nombre");

            return View(model);
        }

        [Authorize(Roles = "MAESTRO-SERVICIO,SERVICIO-ADMINISTRARUSUARIOS-MODIFICAR")]
        public ActionResult Eliminar(string id)
        {
            var usuario = _usuarioProveedorManager.Find(id);

            if (usuario == null)
            {
                //TODO ACTUALIZAR MENSAJE AL RESOURCE
                TempData["FlashError"] = "Usuario incorrecto.";
                return RedirectToAction("Index");
            }

            if (usuario.Id == User.Identity.GetUserId())
            {
                //TODO ACTUALIZAR MENSAJE AL RESOURCE
                TempData["FlashError"] = "No puede eliminar su propio usuario.";
                return RedirectToAction("Index");
            }

            if (usuario.UserName.ToLower() == "superadmin")
            {

                //TODO ACTUALIZAR MENSAJE AL RESOURCE
                TempData["FlashError"] = "No puede eliminar el usuario SuperAdmin.";
                return RedirectToAction("Index");
            }

            try
            {
                var cuentaManager = new CuentaManager();
                cuentaManager.DesAsociarUsuarioEnCuenta(usuario.Id,
                    _commonManager.GetCuentaUsuarioAutenticado().Id);
                _usuarioProveedorManager.Remove(id);
            }
            catch (RetryLimitExceededException)
            {
                //TODO ACTUALIZAR MENSAJE AL RESOURCE
                TempData["FlashError"] = "INTENTOS EXEDIDOS";
                return RedirectToAction("Index");
            }
            catch (Exception exception)
            {
                //TODO ACTUALIZAR MENSAJE AL RESOURCE
                TempData["FlashError"] = exception.Message;
                return RedirectToAction("Index");
            }

            TempData["FlashSuccess"] = "Usuario eliminado con éxito.";
            return RedirectToAction("Index");
        }
    }
}