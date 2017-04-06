using System;
using System.Web.Mvc;
using Ppgz.Services;
using Ppgz.Web.Infrastructure;


namespace Ppgz.Web.Areas.Servicio.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class AdministrarUsuariosController : Controller
    {
        private readonly UsuarioManager _usuarioManager = new UsuarioManager();
        private readonly CommonManager _commonManager = new CommonManager();

        internal void CargarPerfiles()
        {
            var perfilManager = new PerfilManager();

            var perfiles = perfilManager
                .FindPerfilProveedorByCuentaId(_commonManager.GetCuentaUsuarioAutenticado().Id);

            perfiles.Add(PerfilManager.MaestroServicio);

            ViewBag.Perfiles = new SelectList(perfiles, "Id", "Nombre");
        }

        [Authorize(Roles = "MAESTRO-SERVICIO,SERVICIO-ADMINISTRARUSUARIOS-LISTAR,SERVICIO-ADMINISTRARUSUARIOS-MODIFICAR")]
        public ActionResult Index()
        {
            ViewBag.Usuarios = _usuarioManager
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
            CargarPerfiles();
            try
            {
                _usuarioManager
                    .CrearProveedor(
                        model.UserName,
                        model.Nombre,
                        model.Apellido,
                        model.Email,
                        model.Telefono,//TODO
                        null,//TODO
                        true,
                        model.Perfil,
                        model.Password,
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
            var usuario = _usuarioManager.Find(id);

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
                Telefono = usuario.PhoneNumber,
                Perfil = usuario.PerfilId
            };

            return View(usuarioProveedorViewModel);
        }

        [Authorize(Roles = "MAESTRO-SERVICIO,SERVICIO-ADMINISTRARUSUARIOS-MODIFICAR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(string id, UsuarioProveedorViewModel model)
        {
            var usuario = _usuarioManager.Find(id);

            if (usuario == null)
            {
                TempData["FlashError"] = CommonMensajesResource.ERROR_UsuarioProveedor_IdIncorrecto;
                return RedirectToAction("Index");
            }
            CargarPerfiles();
            try
            {
                _usuarioManager.Actualizar(
                    id,
                    model.Nombre,
                    model.Apellido,
                    model.Email,
                    model.Telefono, null,
                    model.Password);

                _usuarioManager.ActualizarPerfil(id, model.Perfil);

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
            var usuario = _usuarioManager.Find(id);

            if (usuario == null)
            {
                TempData["FlashError"] = CommonMensajesResource.ERROR_UsuarioProveedor_IdIncorrecto;
                return RedirectToAction("Index");
            }

            try
            {
                _usuarioManager.Eliminar(id, _commonManager.GetCuentaUsuarioAutenticado().Id);

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