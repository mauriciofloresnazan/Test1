using System;
using System.Web.Mvc;
using Ppgz.Web.Infrastructure;
using Ppgz.Web.Infrastructure.Proveedor;

namespace Ppgz.Web.Areas.Servicio.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class AdministrarUsuariosController : Controller
    {
        private readonly UsuarioProveedorManager _usuarioProveedorManager = new UsuarioProveedorManager();
        private readonly CommonManager _commonManager = new CommonManager();
        internal void CargarPerfiles()
        {
            var perfilProveedorManager = new PerfilProveedorManager();

            var perfiles = perfilProveedorManager
                .FindByCuentaId(_commonManager.GetCuentaUsuarioAutenticado().Id);

            perfiles.Add(perfilProveedorManager.GetMaestroByUsuarioTipo(CuentaManager.Tipo.Servicio));

            ViewBag.Perfiles = new SelectList(perfiles, "Id", "Nombre");
        }

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
            CargarPerfiles();
            return View();
        }

        [Authorize(Roles = "MAESTRO-SERVICIO,SERVICIO-ADMINISTRARUSUARIOS-MODIFICAR")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Crear(UsuarioProveedorViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                _usuarioProveedorManager
                    .Crear(model.UserName, model.Nombre, model.Apellido,
                    model.Email, model.Password, model.Perfil, "PROVEEDOR",
                    _commonManager.GetCuentaUsuarioAutenticado().Id);


                TempData["FlashSuccess"] = CommonMensajesResource.INFO_UsuarioProveedor_CreadoCorrectamente;
                return RedirectToAction("Index");
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
                return View(model);
            }

        }

        [Authorize(Roles = "MAESTRO-SERVICIO,SERVICIO-ADMINISTRARUSUARIOS-MODIFICAR")]
        public ActionResult Editar(string id)
        {
            var usuario = _usuarioProveedorManager.Find(id);

            if (usuario == null)
            {
                TempData["FlashError"] = CommonMensajesResource.ERROR_UsuarioProveedor_IdIncorrecto;
                return RedirectToAction("Index");
            }

            CargarPerfiles();

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
                TempData["FlashError"] = CommonMensajesResource.ERROR_UsuarioProveedor_IdIncorrecto;
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

                TempData["FlashSuccess"] = CommonMensajesResource.INFO_UsuarioProveedor_ActualizadoCorrectamente;
                return RedirectToAction("Index");
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
                return View(model);
            }
        }

        [Authorize(Roles = "MAESTRO-SERVICIO,SERVICIO-ADMINISTRARUSUARIOS-MODIFICAR")]
        public ActionResult Eliminar(string id)
        {
            var usuario = _usuarioProveedorManager.Find(id);

            if (usuario == null)
            {
                TempData["FlashError"] = CommonMensajesResource.ERROR_UsuarioProveedor_IdIncorrecto;
                return RedirectToAction("Index");
            }

            try
            {
                _usuarioProveedorManager.Eliminar(id);

                TempData["FlashSuccess"] = CommonMensajesResource.INFO_UsuarioProveedor_EliminadoCorrectamente;
                return RedirectToAction("Index");
            }
            catch (BusinessException businessEx)
            {
                TempData["FlashError"] = businessEx.Message;
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                var log = CommonManager.BuildMessageLog(
                    TipoMensaje.Error,
                    ControllerContext.Controller.ValueProvider.GetValue("controller").RawValue.ToString(),
                    ControllerContext.Controller.ValueProvider.GetValue("action").RawValue.ToString(),
                    e.ToString(), Request);

                CommonManager.WriteAppLog(log, TipoMensaje.Error);

                TempData["FlashError"] = CommonMensajesResource.ERROR_General;
                return RedirectToAction("Index");
            }
        }
    }
}