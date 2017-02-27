using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Ppgz.Repository;
using Ppgz.Web.Infrastructure;
using Ppgz.Web.Infrastructure.Nazan;
using Ppgz.Web.Models;

namespace Ppgz.Web
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Fix DbContext
            var applicationUserManager =
            new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

            /*

            if (filterContext.RouteData.Values.ContainsValue("Cancel"))
            {
                filterContext.Result = new RedirectResult("~/Home/Index");
                //Trace.WriteLine(" Redirecting from Simple filter to /Home/Index");
            }


            app.Use(async (context, next) =>
            {
                // Do work that doesn't write to the Response.
                await next.Invoke();
                // Do logging or other work that doesn't write to the Response.
            });
            /*
            app.Run(async context =>
            {
                await context.Response.WriteAsync("Hello from 2nd delegate.");
            });

            app.Run(async (context, next) =>
            {
                
                CommonManager commonManager = new CommonManager();
                var usuarioAutenticado = commonManager.GetUsuarioAutenticado();
                if(usuarioAutenticado.Tipo == "PROVEEDOR"|| usuarioAutenticado.Tipo == "PROVEEDOR")
                {                 
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("Hello ASP.NET 5!");

                } 
            });

            */


            // SuperUsuario
            var usuarioNazanManager = new UsuarioNazanManager();
            var perfilNazanManager = new PerfilNazanManager();

            if (applicationUserManager.FindByName("superusuario") == null)
            {

               usuarioNazanManager.Create(
                    "superusuario",
                    "superusuario",
                    "superusuario",
                    "superusuario",
                    "123456",
                    perfilNazanManager.GetMaestro().Id);
            }

            //Roles
            var db = new Entities();

            string [] rolesNazan = 
            {
                "SUPERADMIN",
                "NAZAN-ADMINISTRARPERFILESNAZAN-LISTAR",
                "NAZAN-ADMINISTRARPERFILESNAZAN-MODIFICAR",
                "NAZAN-ADMINISTRARUSUARIOSNAZAN-LISTAR",
                "NAZAN-ADMINISTRARUSUARIOSNAZAN-MODIFICAR",
                "NAZAN-ADMINISTRAMENSAJESINSTITUCIONALES-LISTAR",
                "NAZAN-ADMINISTRAMENSAJESINSTITUCIONALES-MODIFICAR",
            };

            foreach (var role in rolesNazan)
            {
                if (db.aspnetroles
                    .FirstOrDefault(r => r.Name == role) != null) continue;

                var aspnetrole = new aspnetrole()
                {
                    Id = role,
                    Name = role,
                    Description = role,
                    Tipo = "NAZAN"
                };

                db.aspnetroles.Add(aspnetrole);
                db.SaveChanges();
            }

            string[] rolesMercaderia = 
            {
                "MERCADERIA-ADMINISTRARPERFILES-LISTAR",
                "MERCADERIA-ADMINISTRARPERFILES-MODIFICAR",
                "MERCADERIA-ADMINISTRARUSUARIOS-LISTAR",
                "MERCADERIA-ADMINISTRARUSUARIOS-MODIFICAR",
                "MERCADERIA-MENSAJESINSTITUCIONALES",
            };

            foreach (var role in rolesMercaderia)
            {
                if (db.aspnetroles
                    .FirstOrDefault(r => r.Name == role) != null) continue;

                var aspnetrole = new aspnetrole()
                {
                    Id = role,
                    Name = role,
                    Description = role,
                    Tipo = "MERCADERIA"
                };

                db.aspnetroles.Add(aspnetrole);
                db.SaveChanges();
            }

            string[] rolesServicio = 
            {
                "SERVICIO-ADMINISTRARPERFILES-LISTAR",
                "SERVICIO-ADMINISTRARPERFILES-MODIFICAR",
                "SERVICIO-ADMINISTRARUSUARIOS-LISTAR",
                "SERVICIO-ADMINISTRARUSUARIOS-MODIFICAR",
                "SERVICIO-MENSAJESINSTITUCIONALES",
            };

            foreach (var role in rolesServicio)
            {
                if (db.aspnetroles
                    .FirstOrDefault(r => r.Name == role) != null) continue;

                var aspnetrole = new aspnetrole()
                {
                    Id = role,
                    Name = role,
                    Description = role,
                    Tipo = "SERVICIO"
                };

                db.aspnetroles.Add(aspnetrole);
                db.SaveChanges();
            }

            // Enable the application to use a cookie to store information for the signed in user
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login")
            });
            // Use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");

            //app.UseFacebookAuthentication(
            //   appId: "",
            //   appSecret: "");

            //app.UseGoogleAuthentication();
        }
    }
}