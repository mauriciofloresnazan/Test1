using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataProtection;
using Ppgz.Services;
using Ppgz.Web.Infrastructure;
using Ppgz.Web.Models;

namespace Ppgz.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        public AccountController()
            : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
        {
        }

        public AccountController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public UserManager<ApplicationUser> UserManager { get; private set; }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            string log;

            if (!ModelState.IsValid) return View(model);

            try
            {
                UserManager.UserLockoutEnabledByDefault = Convert.ToBoolean(ConfigurationManager.AppSettings["UserLockoutEnabled"]);
                UserManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(Convert.ToDouble(ConfigurationManager.AppSettings["AccountLockoutTimeSpan"]));
                UserManager.MaxFailedAccessAttemptsBeforeLockout = Convert.ToInt32(ConfigurationManager.AppSettings["MaxFailedAccessAttemptsBeforeLockout"]);

                var usuario = await UserManager.FindByNameAsync(model.UserName);
                if (usuario != null)
                {

                    if (await UserManager.IsLockedOutAsync(usuario.Id))
                    {
                        ModelState.AddModelError("",
                            string.Format(CommonMensajesResource.ERROR_Identity_UsuarioBloqueadoTemporalmente,
                                ConfigurationManager.AppSettings["AccountLockoutTimeSpan"],
                                ConfigurationManager.AppSettings["MaxFailedAccessAttemptsBeforeLockout"]));
                        return View(model);
                    }

                    var passwordValid = UserManager.PasswordHasher.VerifyHashedPassword(usuario.PasswordHash, model.Password);

                    if (passwordValid == PasswordVerificationResult.Failed)
                    {
                        UserManager.AccessFailed(usuario.Id);
                        ModelState.AddModelError("", CommonMensajesResource.ERROR_Identity_UsuarioPassword);
                        return View(model);
                    }

                    await SignInAsync(usuario, model.RememberMe);

                    return RedirectToLocal(returnUrl);
                }
            }
            catch (BusinessException businessEx)
            {
                log = CommonManager.BuildMessageLog(
                    TipoMensaje.Error,
                    ControllerContext.Controller.ValueProvider.GetValue("controller").RawValue.ToString(),
                    ControllerContext.Controller.ValueProvider.GetValue("action").RawValue.ToString(),
                    businessEx.ToString(),
                    Request);

                CommonManager.WriteBusinessLog(log, TipoMensaje.Error);

               return View(model);
            }
            catch (Exception e)
            {
                log = CommonManager.BuildMessageLog(
                    TipoMensaje.Error,
                    ControllerContext.Controller.ValueProvider.GetValue("controller").RawValue.ToString(),
                    ControllerContext.Controller.ValueProvider.GetValue("action").RawValue.ToString(),
                    e.ToString(), Request);
                CommonManager.WriteAppLog(log, TipoMensaje.Error);
            }

            ModelState.AddModelError("", CommonMensajesResource.ERROR_Identity_UsuarioPassword);

            return View(model);
        }
        

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            base.Dispose(disposing);
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }


        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var usuario = await UserManager.FindByEmailAsync(model.Email);

            if (usuario == null)
            {
                    
                TempData["FlashError"] = "Cuenta de correo incorrecta";
                return RedirectToAction("ForgotPassword", "Account");
            }


            try
            {
                var provider = new DpapiDataProtectionProvider("Sample");

                UserManager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(
                    provider.Create("EmailConfirmation"));


                var code = UserManager.GeneratePasswordResetToken(usuario.Id);

                var commonManager = new CommonManager();
                await commonManager.SendHtmlMail("Portal de Proveedores del Grupo Nazan - Reestablecer Contraseña", string.Format(
                    "Estimado {0},<BR/>Por acceda al al siguiente link para cambiar su contraseña: <a href=\"{1}\" title=\"Recuperar Contraseña\">Cambio de contraseña</a>",
                    usuario.Nombre + " " + usuario.Apellido,
                    Url.Action("ResetPassword", "Account",
                        new { token = usuario.Id, code, email = usuario.Email }, Request.Url.Scheme)), usuario.Email);

                return RedirectToAction("ForgotPasswordConfirmation", "Account");

            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Error enviando correo para restablecer su contraseña, por favor Intente mas tarde.");
                return View(model);
            }
        }
        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
     


        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code, string email)
        {
            var model = new ResetPasswordViewModel {Code = code, Email = email};

            return (string.IsNullOrWhiteSpace(code)|| string.IsNullOrWhiteSpace(email)) 
                ? View("Error") 
                : View(model);
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user =  UserManager.FindByEmail(model.Email);
            if (user == null)
            {
                // No revelar que el usuario no existe
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var provider = new DpapiDataProtectionProvider("Sample");

            UserManager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(
                provider.Create("EmailConfirmation"));

            var result =  UserManager.ResetPassword(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            ModelState.AddModelError(string.Empty, "Error al reestablecer la contraseña. Por favor realizar una nueva solicitud. Si el problema persiste contate a los administradores.");
            return View(model);
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        
        
        /*//
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public ActionResult Confirm(string email)
        {
            ViewBag.Email = email;
            return View();

        }
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string token, string email)
        {
            ApplicationUser user = UserManager.FindById(token);
            if (user != null)
            {
                if (user.Email == email)
                {
                    user.EmailConfirmed = true;
                    await UserManager.UpdateAsync(user);
                    await SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home", new { ConfirmedEmail = user.Email });
                }
                else
                {
                    return RedirectToAction("Confirm", "Account", new { Email = user.Email });
                }
            }
            else
            {
                return RedirectToAction("Confirm", "Account", new { Email = "" });
            }

        }
        */

        public ActionResult CambiarPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarPassword(CambiarPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            
            var commonManager = new CommonManager();
            
            var usuario = commonManager.GetUsuarioAutenticado();

            var usuarioManager = new UsuarioManager();
            try
            {
                usuarioManager.Actualizar(usuario.Id, null, null, null, null, null, model.Password);

                TempData["FlashSuccess"] = "Contraseña actualizada exitosamente";
                return RedirectToAction("Index","Home");
            }
            catch (Exception exception)
            {
                
                ModelState.AddModelError(string.Empty, exception.Message);

                return View(model);
            }
            
            
         
        }
        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }



        
     

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri) : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}