using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using Ppgz.Repository;
using Ppgz.Services;
using Ppgz.Web.Areas.Nazan.Models;
using Ppgz.Web.Infrastructure;

namespace Ppgz.Web.Areas.Nazan.Controllers
{
	[Authorize]
	public class AdministrarProveedoresController : Controller
	{
		readonly CuentaManager _cuentaManager = new CuentaManager();
		readonly ProveedorManager _proveedorManager = new ProveedorManager();
		//
		// GET: /Nazan/AdministrarProveedores/
		[Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-LISTAR,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
		public ActionResult Index()
		{
			var cuentas = _cuentaManager.FindAllWithUsuarioMaestro();

			ViewBag.cuentas = cuentas;
			return View();
		}

		[Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
		public ActionResult Registrar()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
        public async Task<ActionResult> Registrar(CuentaViewModel model)
		{
            if (!ModelState.IsValid) return View(model);
            try
            {
                _cuentaManager.Crear(         
                                    model.TipoCuenta,
                                    model.NombreCuenta,
                                    model.UserName,
                                    model.ResponsableNombre,
                                    model.ResponsableApellido,
                                    model.ResponsableCargo,
                                    model.ResponsableEmail,
                                    model.ResponsableTelefono,
                                    model.ResponsablePassword,
                                    model.Especial,
                                    model.SinASN,
                                    model.Factoraje);

                var commonManager = new CommonManager();

                //TODO pasar al manejador
                var db = new Entities();
                var confEnlace = db.configuraciones.Single(
                    c => c.Clave == "site.url");

                await commonManager.SendHtmlMail(
                   "Registro de Nueva Cuenta - Portal de Proveedores del Grupo Nazan",
                   "Se ha registrado una cuenta de proveedores en el portal.<br>" +
                   "Puede ingresar en el portal con los siguientes Datos: <br> " +
                   "<strong>Usuario:</strong> " + model.UserName + "<br> " +
                   "<strong>Contraseña:</strong> " + model.ResponsablePassword + "<br> " +
                   "<strong>Enlace:</strong> <a href='" + confEnlace.Valor + "'>" + confEnlace.Valor + "</a><br>" +
                   "Al ingresar por primera vez debe cambiar su contraseña.",
                   model.ResponsableEmail
                   );

                TempData["FlashSuccess"] = MensajesResource.INFO_Cuenta_CreadaCorrectamente;
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

                ModelState.AddModelError(string.Empty, e.Message);
                return View(model);
            }
		}

		[Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
		public ActionResult Editar(int id, string successMessage = null)
		{
            var cuentaConUsuarioMaestro = _cuentaManager.FindWithUsuarioMaestro(id);
			if (cuentaConUsuarioMaestro == null)
			{
				TempData["FlashError"] = MensajesResource.ERROR_Cuenta_IdIncorrecto;
				return RedirectToAction("Index");
			}

            foreach( proveedore proveedor in cuentaConUsuarioMaestro.Cuenta.proveedores)
            {
                proveedor.Sociedades = JsonConvert.DeserializeObject<SociedadesProv[]>(proveedor.Sociedad);
            }

            ViewBag.cuentaConUsuarioMaestro = cuentaConUsuarioMaestro;

            var model = new CuentaViewModel
            {
                UserName = cuentaConUsuarioMaestro.UsuarioMaestro.UserName,
                ResponsableNombre = cuentaConUsuarioMaestro.UsuarioMaestro.Nombre,
                ResponsableApellido= cuentaConUsuarioMaestro.UsuarioMaestro.Apellido,
                ResponsableCargo= cuentaConUsuarioMaestro.UsuarioMaestro.Cargo,
                ResponsableTelefono = cuentaConUsuarioMaestro.UsuarioMaestro.PhoneNumber,
                ResponsableEmail= cuentaConUsuarioMaestro.UsuarioMaestro.Email
            

            };

		    if (!string.IsNullOrWhiteSpace(successMessage))
		    {
		        TempData["FlashSuccess"] = successMessage;
		    }

            return View(model);

		}

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
        public JsonResult EstablecerCuentaEspecial(int cuentaId, bool esEspecial)
        { 
            try
            {
                _cuentaManager.EstablecerCuentaEspecial(
                    cuentaId, esEspecial);

                return Json("success");
            }

            catch (Exception e)
            {
                var log = CommonManager.BuildMessageLog(
                    TipoMensaje.Error,
                    ControllerContext.Controller.ValueProvider.GetValue("controller").RawValue.ToString(),
                    ControllerContext.Controller.ValueProvider.GetValue("action").RawValue.ToString(),
                    e.ToString(), Request);

                CommonManager.WriteAppLog(log, TipoMensaje.Error);

                return Json(e.Message);
            }
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
        public JsonResult SwichSociedad(int idProveedor, string sociedadAcambiar, bool Activa)
        {
            try
            {
                var db = new Entities();
                var proveedor = db.proveedores.Single( p => p.Id == idProveedor);
                SociedadesProv[] sociedades = JsonConvert.DeserializeObject<SociedadesProv[]>(proveedor.Sociedad);

                foreach (SociedadesProv sociedad in sociedades)
                {
                    if (sociedad.Sociedad== sociedadAcambiar)
                    {
                        sociedad.Activa = Activa;
                    }
                }

                proveedor.Sociedad = JsonConvert.SerializeObject(sociedades);
                db.SaveChanges();

                return Json("success");
            }

            catch (Exception e)
            {
                

                return Json(e.Message);
            }
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
        public JsonResult EstablecerCuentaFactoraje(int cuentaId, bool Factoraje)
        {

            if (!this.User.IsInRole("NAZAN-PRONTOPAGO-APROBADOR"))
            {
               

                return Json("No tiene permisos para realizar el cambio");
            }
            else
            {
                try
                {
                    _cuentaManager.EstablecerCuentaFactoraje(
                        cuentaId, Factoraje);

                    return Json("success");
                }

                catch (Exception e)
                {
                    var log = CommonManager.BuildMessageLog(
                        TipoMensaje.Error,
                        ControllerContext.Controller.ValueProvider.GetValue("controller").RawValue.ToString(),
                        ControllerContext.Controller.ValueProvider.GetValue("action").RawValue.ToString(),
                        e.ToString(), Request);

                    CommonManager.WriteAppLog(log, TipoMensaje.Error);

                    return Json(e.Message);
                }
            }

            
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
        public JsonResult EstablecerCuentaSinASN(int cuentaId, bool SinASN)
        {
            try
            {
                _cuentaManager.EstablecerCuentaSinASN(
                    cuentaId, SinASN);

                return Json("success");
            }

            catch (Exception e)
            {
                var log = CommonManager.BuildMessageLog(
                    TipoMensaje.Error,
                    ControllerContext.Controller.ValueProvider.GetValue("controller").RawValue.ToString(),
                    ControllerContext.Controller.ValueProvider.GetValue("action").RawValue.ToString(),
                    e.ToString(), Request);

                CommonManager.WriteAppLog(log, TipoMensaje.Error);

                return Json(e.Message);
            }
        }

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
		public JsonResult BuscarProveedor(string numeroProveedor)
		{
		    try
		    {
                var proveedor = _proveedorManager.FindProveedorEnSap(numeroProveedor);
		        return proveedor == null
		            ? Json(new {error = MensajesResource.ERROR_Proveedor_IdIncorrecto})
		            : Json(new
		            {
		                proveedor.Rfc,
		                Nombre = string.Format
		                    ("{0} {1} {2} {3}",
		                        proveedor.Nombre1,
		                        proveedor.Nombre2,
		                        proveedor.Nombre3,
		                        proveedor.Nombre4),
                        Teléfono = proveedor.NumeroTelefono,
                        Email = proveedor.Correo,
                        Vendedor = proveedor.VendedorResponsable,
                        proveedor.Region,
                        proveedor.Poblacion,
                        proveedor.Apartado,
                        proveedor.Distrito,
                        proveedor.CodigoPostal,
                        Direccion = proveedor.Calle,
                        proveedor.EstadoNombre,
                        sociedades= proveedor.Sociedad
                    });
		    }
            catch (BusinessException businessEx)
            {
                return Json(new { error = businessEx.Message });
            }
            catch (Exception e)
            {
                var log = CommonManager.BuildMessageLog(
                    TipoMensaje.Error,
                    ControllerContext.Controller.ValueProvider.GetValue("controller").RawValue.ToString(),
                    ControllerContext.Controller.ValueProvider.GetValue("action").RawValue.ToString(),
                    e.ToString(), Request);

                CommonManager.WriteAppLog(log, TipoMensaje.Error);
                // TODO PASAR A UN RESOURCE
                return Json(e.Message == "NO_DATA_BUKRS" ? new { error = "Número de proveedor incorrecto" } : new { error = e.Message });
            }
		}


		[Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
		public ActionResult AsociarProveedor(int id, string numeroProveedor)
		{
			try
            {
                _cuentaManager.AsociarProveedorSapEnCuenta(id, numeroProveedor);

				TempData["FlashSuccess"] = "Proveedor asociado con éxito.";
				return RedirectToAction("Editar", new {  id });
			}
            catch (BusinessException businessEx)
            {
                TempData["FlashError"] = businessEx.Message;
                return RedirectToAction("Editar", new { id });

            }
            catch (Exception e)
            {
                var log = CommonManager.BuildMessageLog(
                    TipoMensaje.Error,
                    ControllerContext.Controller.ValueProvider.GetValue("controller").RawValue.ToString(),
                    ControllerContext.Controller.ValueProvider.GetValue("action").RawValue.ToString(),
                    e.ToString(), Request);

                CommonManager.WriteAppLog(log, TipoMensaje.Error);

                TempData["FlashError"] = MensajesResource.ERROR_General;
                return RedirectToAction("Editar", new { id });
            }

		}
		
        
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
		public ActionResult Eliminar(int id)
        {
            try
            {
                _cuentaManager.Eliminar(id);
                //_proveedorManager.Eliminar(id);

                TempData["FlashSuccess"] = MensajesResource.INFO_Proveedor_EliminadoCorrectamente;
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

                TempData["FlashError"] = e.Message;
                return RedirectToAction("Index");
            }
			
		}


        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
		public ActionResult DesasociarProveedor(int cuentaId, int proveedorId)
		{
			// TODO MEJORAR
			try
			{
                // todo mejorar 

                _cuentaManager.EliminarProveedorEnCuenta(cuentaId, proveedorId);



				TempData["FlashSuccess"] = "Proveedor desvinculado con éxito.";
                return RedirectToAction("Editar", new { id = cuentaId });
			}
			catch (Exception exception)
			{
				TempData["FlashError"] = exception.Message;
                return RedirectToAction("Editar", new { id = cuentaId });
			}

		}

        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
	    public ActionResult RefrescarProveedor(int cuentaId, int proveedorId)
	    {
            try { 
	            _cuentaManager.RefrescarProveedorSapEnCuenta(cuentaId, proveedorId);
                TempData["FlashSuccess"] = "Proveedor actualizado con éxito.";
                return RedirectToAction("Editar", new { id = cuentaId });
            }
            catch (Exception exception)
            {
                TempData["FlashError"] = exception.Message;
                return RedirectToAction("Editar", new { id = cuentaId });
            }
	    }

 
        [Authorize(Roles = "MAESTRO-NAZAN,NAZAN-ADMINISTRARPROVEEDORESNAZAN-MODIFICAR")]
        public JsonResult ActualizarResponsable(string usuarioId, string nombre, string apellido, string cargo, string telefono, string email)
        {
            var model = new CuentaViewModel
            {
                ResponsableNombre = nombre,
                ResponsableApellido = apellido,
                ResponsableCargo = cargo,
                ResponsableTelefono = telefono,
                ResponsableEmail = email
            };

            ModelState.Clear();

            TryValidateModel(model);
            
            var usuarioManager = new UsuarioManager();

            if (!ModelState.IsValidField("ResponsableNombre"))
                return Json(new { error = "Nombre incorrecto" });
            if (!ModelState.IsValidField("ResponsableApellido"))
                return Json(new { error = "Apellido incorrecto" });
            if (!ModelState.IsValidField("ResponsableCargo"))
                return Json(new { error = "Cargo incorrecto" });
            if (!ModelState.IsValidField("ResponsableTelefono"))
                return Json(new { error = "Teléfno incorrecto" });
            if (!ModelState.IsValidField("ResponsableEmail"))
                return Json(new { error = "Email incorrecto" }); 

            try
            {
                usuarioManager.Actualizar(usuarioId, nombre, apellido, email, telefono, cargo);
            }
            catch (Exception exception)
            {

                return Json(new { error = exception.Message}); 
            }
            
            return Json(new { success = "Responsable actualizado correctamente" });

	    }
	}
}