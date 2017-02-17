using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ppgz.Repository;
using Ppgz.Web.Models;

namespace Ppgz.Web.Controllers
{
    public class NazanMensajesInstitucionalesController : Controller
    {
        //
        // GET: /NazanMensajesInstitucionales/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(MensajeViewModel model)
        {
            throw new NotImplementedException();
            /*
            if (ModelState.IsValid)
            {

                if (_db.usuarios.Any(u => u.userName == model.UserName))
                {

                    ModelState.AddModelError(string.Empty, "Este nombre de usuario ya fue utilizado. Por favor intente con otro");
                    return View(model);
                }

                var tipoUsuarioNazan = _db.tipos_usuario.First(t => t.codigo == "NAZAN");

                var usuario = new usuario()
                {
                    userName = model.UserName,
                    nombre = model.ResponsableNombre,
                    apellido = model.ResponsableApellido,
                    cargo = model.ResponsableCargo,
                    email = model.ResponsableEmail,
                    telefono = model.ResponsableTelefono,
                    PasswordHash = model.ResponsablePassword,
                    SecurityStamp = model.ResponsablePassword,
                    tipo_usuario_id = tipoUsuarioNazan.id

                };

                _db.usuarios.Add(usuario);

                _db.SaveChanges();


                var cuenta = new cuenta
                {
                    nombre_proveedor = model.ProveedorNombre,

                    codigo_proveedor = DateTime.Now.ToString("yyyyMMddHHmmssf"),
                    reponsable_usuario_id = usuario.Id

                };

                _db.cuentas.Add(cuenta);

                _db.SaveChanges();

                var xref = new usuarios_cuentas_xref
                {
                    usuario_id = usuario.Id,
                    cuenta_id = cuenta.id
                };
                _db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(model);
            */
        }
	}
}