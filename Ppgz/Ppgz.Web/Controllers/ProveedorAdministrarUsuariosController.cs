﻿using System.Data.Entity.Infrastructure;
using System.Net;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Ppgz.Web.Infrastructure;
using Ppgz.Web.Models;

namespace Ppgz.Web.Controllers
{
    [Authorize]
    public class ProveedorAdministrarUsuariosController : Controller
    {
        private readonly CommonManager _commonManager = new CommonManager();
        private readonly UsuarioManager _usuarioManager = new UsuarioManager();

        readonly UserManager<ApplicationUser> _applicationUserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));


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

            ViewBag.Accesos = new SelectList(new[]{
                    "*FULL*",
                    "ProveedorAdministrarUsuarios",
                    "ProveedorMensajesInstitucionales"
                });

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registrar(ProveedorUsuarioViewModel model)
        {
            ViewBag.Accesos = new SelectList(new[]{
                    "*FULL*",
                    "ProveedorAdministrarUsuarios",
                    "ProveedorMensajesInstitucionales"
                });

            if (ModelState.IsValid)
            {

                if (_applicationUserManager.FindByName(model.UserName) != null)
                {
                    ModelState.AddModelError(string.Empty, ResourceErrores.NombreUsuarioExistente);
                    return View(model);
                }

                var usuario = new ApplicationUser() { UserName = model.UserName };

                var result = _applicationUserManager.Create(usuario, model.Password);

                if (result.Succeeded)
                {
                    _usuarioManager.Update(
                    usuario.Id,
                    model.Nombre,
                    model.Apellido,
                    string.Empty,
                    model.Email,
                    string.Empty,
                    TipoUsuario.Proveedor);
                }

                _commonManager.UsuarioCuentaXrefAdd(
                   usuario.Id,
                    _commonManager.GetCuentaUsuarioAutenticado().id);


                return RedirectToAction("Index");

            }

            return View(model);
        }

        public ActionResult Editar(string id)
        {
            var usuario = _usuarioManager.Find(id);

            if (usuario == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (usuario.Tipo != TipoUsuario.Proveedor)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }


            ViewBag.Accesos = new SelectList(new[]{
                    "*FULL*",
                    "ProveedorAdministrarUsuarios",
                    "ProveedorMensajesInstitucionales"
                });

            var usuarioProveedor = new ProveedorUsuarioViewModel
            {
                UserName = usuario.UserName,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email,
                Acceso = usuario.SecurityStamp
            };

            return View(usuarioProveedor);
        }

        [HttpPost, ActionName("Editar")]
        [ValidateAntiForgeryToken]
        public ActionResult EditarPost(string id, FormCollection collection)
        {
            var usuario = _usuarioManager.Find(id);

            if (usuario == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (usuario.Tipo != TipoUsuario.Proveedor)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            try
            {
                _usuarioManager.Update(
                    id,
                    collection["nombre"],
                    collection["apellido"],
                    usuario.Cargo,
                    collection["email"],
                    collection["telefono"],
                    usuario.Tipo,
                    collection["password"]);

                return RedirectToAction("Index");
            }
            catch (RetryLimitExceededException)
            {
                ModelState.AddModelError("", ResourceErrores.RegistroGeneral);
            }


            var usuarioProveedor = new ProveedorUsuarioViewModel
            {
                UserName = collection["UserName"],
                Nombre = collection["Nombre"],
                Apellido = collection["Apellido"],
                Email = collection["Email"],
            };

            ViewBag.Accesos = new SelectList(new[]{
                    "*FULL*",
                    "ProveedorAdministrarUsuarios",
                    "ProveedorMensajesInstitucionales"
                });

            return View(usuarioProveedor);
        }

        public ActionResult Eliminar(string id)
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


            if (usuario.Tipo != TipoUsuario.Proveedor)
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