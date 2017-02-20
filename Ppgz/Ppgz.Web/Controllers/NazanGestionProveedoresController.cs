﻿using System;
using System.Web.Mvc;
using Ppgz.Repository;
using Ppgz.Services;
using Ppgz.Web.Infraestructure;
using Ppgz.Web.Models;
using UsuarioManager = Ppgz.Web.Infraestructure.UsuarioManager;

namespace Ppgz.Web.Controllers
{
    [Authorize]
    public class NazanGestionProveedoresController : Controller
    {
        readonly CuentaManager _cuentaManager = new CuentaManager();
        readonly TipoProveedorManager _tipoProveedorManager = new TipoProveedorManager();
        readonly UsuarioManager _usuarioManager = new UsuarioManager();
        readonly TipoUsuarioManager _tipoUsuarioManager = new TipoUsuarioManager();
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

                if (_usuarioManager.FindUsuarioByUserName(model.UserName) != null)
                {
                    ModelState.AddModelError(string.Empty, "Este nombre de usuario ya fue utilizado. Por favor intente con otro.");
                    return View(model);
                }
                
                var tipoProveedor = _tipoProveedorManager.GetByCodigo(model.TipoProveedor);
                
                // Registro del Usairo
                var usuario = new usuario()
                {
                    userName = model.UserName.ToLower(),
                    nombre = model.ResponsableNombre,
                    apellido = model.ResponsableApellido,
                    cargo = model.ResponsableCargo,
                    email = model.ResponsableEmail,
                    telefono = model.ResponsableTelefono,
                    tipo_usuario_id = _tipoUsuarioManager.GetProveedor().id,
                };
                _usuarioManager.Add(usuario, model.ResponsablePassword);

                // Registro de la cuenta
                var cuenta = new cuenta
                {
                    nombre_proveedor = model.ProveedorNombre,
                    codigo_proveedor = DateTime.Now.ToString("yyyyMMddHHmmssf"),
                    reponsable_usuario_id = usuario.Id,
                    tipo_proveedor_id = tipoProveedor.id,
                    activo = true
                };
                _cuentaManager.Add(cuenta);

                _commonManager.UsuarioCuentaXrefAdd(usuario, cuenta.id);

                return RedirectToAction("Index");
            }

            return View(model);

        }
    }
}