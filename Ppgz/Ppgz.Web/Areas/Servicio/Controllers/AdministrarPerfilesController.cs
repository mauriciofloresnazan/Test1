using System;
using System.Linq;
using System.Web.Mvc;
using Ppgz.Web.Areas.Servicio.Models;
using Ppgz.Web.Infrastructure;
using Ppgz.Web.Infrastructure.Proveedor;

namespace Ppgz.Web.Areas.Servicio.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class AdministrarPerfilesController : Controller
    {
        private readonly PerfilProveedorManager _perfilProveedorManager = new PerfilProveedorManager();
        private readonly CommonManager _commonManager = new CommonManager();

        [Authorize(Roles = "MAESTRO-SERVICIO,SERVICIO-ADMINISTRARPERFILES-LISTAR,SERVICIO-ADMINISTRARPERFILES-MODIFICAR")]
        public ActionResult Index()
        {
            var perfiles = _perfilProveedorManager
                .FindByCuentaId(_commonManager.GetCuentaUsuarioAutenticado().Id);

            perfiles.Add(_perfilProveedorManager.GetMaestroByUsuarioTipo(CuentaManager.Tipo.Servicio));

            ViewBag.Perfiles = perfiles;
            return View();
        }

        [Authorize(Roles = "MAESTRO-SERVICIO,SERVICIO-ADMINISTRARPERFILES-MODIFICAR")]
        public ActionResult Crear()
        {
            return View(new PefilProveedorViewModel());
        }

        [Authorize(Roles = "MAESTRO-SERVICIO,SERVICIO-ADMINISTRARPERFILES-MODIFICAR")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Crear(PefilProveedorViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                _perfilProveedorManager
                    .Crear(
                        model.Nombre, model.RolesIds,
                        _commonManager.GetCuentaUsuarioAutenticado().Id);

                TempData["FlashSuccess"] = CommonMensajesResource.INFO_PerfilProveedor_CreadoCorrectamente;
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

        [Authorize(Roles = "MAESTRO-SERVICIO,SERVICIO-ADMINISTRARPERFILES-MODIFICAR")]
        public ActionResult Editar(int id)
        {
            var perfil = _perfilProveedorManager.Find(id);

            if (perfil == null)
            {
                TempData["FlashError"] = CommonMensajesResource.ERROR_PerfilProveedor_PefilIdIncorrecto;
                return RedirectToAction("Index");
            }

            var model = new PefilProveedorViewModel
            {
                Nombre = perfil.Nombre,
                RolesIds = perfil.aspnetroles.Select(p => p.Id).ToArray()

            };

            return View(model);
        }


        [Authorize(Roles = "MAESTRO-SERVICIO,SERVICIO-ADMINISTRARPERFILES-MODIFICAR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(int id, PefilProveedorViewModel model)
        {

            var perfil = _perfilProveedorManager.Find(id);

            if (perfil == null)
            {
                TempData["FlashError"] = CommonMensajesResource.ERROR_PerfilProveedor_PefilIdIncorrecto;
                return RedirectToAction("Index");
            }

            try
            {
                _perfilProveedorManager.Actualizar(
                    id,
                    model.Nombre,
                    model.RolesIds,
                    perfil.CuentaId);

                TempData["FlashSuccess"] = "Perfil actualizado con éxito.";
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

        [Authorize(Roles = "MAESTRO-SERVICIO,SERVICIO-ADMINISTRARPERFILES-MODIFICAR")]
        public ActionResult Eliminar(int id)
        {
            var perfil = _perfilProveedorManager.Find(id);

            if (perfil == null)
            {
                TempData["FlashError"] = CommonMensajesResource.ERROR_PerfilProveedor_PefilIdIncorrecto;
                return RedirectToAction("Index");
            }

            try
            {
                _perfilProveedorManager.Eliminar(id);
                TempData["FlashSuccess"] = "Perfil eliminado con éxito.";
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