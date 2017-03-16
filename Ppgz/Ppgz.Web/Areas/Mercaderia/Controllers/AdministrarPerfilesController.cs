using System;
using System.Linq;
using System.Web.Mvc;
using Ppgz.Services;
using Ppgz.Web.Areas.Mercaderia.Models;
using Ppgz.Web.Infrastructure;


namespace Ppgz.Web.Areas.Mercaderia.Controllers
{
    [Authorize]
    [TerminosCondiciones]
    public class AdministrarPerfilesController : Controller
    {
        private readonly PerfilManager _perfilManager = new PerfilManager();
        private readonly CommonManager _commonManager = new CommonManager();

        //
        // GET: /Mercaderia/AdministrarPerfiles/
        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-ADMINISTRARPERFILES-LISTAR,MERCADERIA-ADMINISTRARPERFILES-MODIFICAR")]
        public ActionResult Index()
        {
            // Perfiles de la cuenta
            var perfiles = _perfilManager.FindPerfilProveedorByCuentaId(_commonManager.GetCuentaUsuarioAutenticado().Id);
            
            // Perfil maestro
            perfiles.Add(PerfilManager.MaestroMercaderia);

            ViewBag.Perfiles = perfiles;
            return View();
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-ADMINISTRARPERFILES-MODIFICAR")]
        public ActionResult Crear()
        {
            return View(new PefilProveedorViewModel());
        }

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-ADMINISTRARPERFILES-MODIFICAR")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Crear(PefilProveedorViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                _perfilManager
                    .CrearProveedor(
                        model.Nombre, 
                        model.RolesIds,
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

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-ADMINISTRARPERFILES-MODIFICAR")]
        public ActionResult Editar(int id)
        {
            var perfil = _perfilManager.Find(id);

            if (perfil == null)
            {
                TempData["FlashError"] = CommonMensajesResource.ERROR_PerfilProveedor_PefilIdIncorrecto;
                return RedirectToAction("Index");
            }

            var model = new PefilProveedorViewModel
            {
                Nombre = perfil.Nombre,
                RolesIds = perfil.AspNetRoles.Select(p => p.Id).ToArray()

            };

            return View(model);
        }


        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-ADMINISTRARPERFILES-MODIFICAR")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(int id, PefilProveedorViewModel model)
        {
            var perfil = _perfilManager.Find(id);

            if (perfil == null)
            {
                TempData["FlashError"] = CommonMensajesResource.ERROR_PerfilProveedor_PefilIdIncorrecto;
                return RedirectToAction("Index");
            }

            try
            {
                _perfilManager.ActualizarProveedor(
                    id,
                    model.Nombre,
                    model.RolesIds);

                TempData["FlashSuccess"] = CommonMensajesResource.INFO_PerfilProveedor_ActualizadoCorrectamente;
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

        [Authorize(Roles = "MAESTRO-MERCADERIA,MERCADERIA-ADMINISTRARPERFILES-MODIFICAR")]
        public ActionResult Eliminar(int id)
        {
            var perfil = _perfilManager.Find(id);

            if (perfil == null)
            {
                TempData["FlashError"] = CommonMensajesResource.ERROR_PerfilProveedor_PefilIdIncorrecto;
                return RedirectToAction("Index");
            }

            try
            {
                _perfilManager.Eliminar(id);
                TempData["FlashSuccess"] = CommonMensajesResource.INFO_PerfilProveedor_EliminadoCorrectamente;
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