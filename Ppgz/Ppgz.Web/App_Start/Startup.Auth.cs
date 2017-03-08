using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Ppgz.Repository;
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

            //Roles
            var db = new Entities();

            var rolesNazan = new Dictionary<string, string>
            {
                {"MAESTRO-NAZAN", "Acceso Total al sistema de Nazan"},
                {"NAZAN-ADMINISTRARPERFILESNAZAN-LISTAR", "Consultar Perfiles"},
                {"NAZAN-ADMINISTRARPERFILESNAZAN-MODIFICAR", "Modificar Perfiles"},
                {"NAZAN-ADMINISTRARUSUARIOSNAZAN-LISTAR", "Consultar Usuarios"},
                {"NAZAN-ADMINISTRARUSUARIOSNAZAN-MODIFICAR", "Modificar Usuarios"},
                {"NAZAN-ADMINISTRARMENSAJESINSTITUCIONALES-LISTAR", "Consultar Mensajes Insitucionales"},
                {"NAZAN-ADMINISTRARMENSAJESINSTITUCIONALES-MODIFICAR", "Modificar Mensajes Institucionales"}
            };
 

            foreach (var role in rolesNazan)
            {
                if (db.aspnetroles
                    .FirstOrDefault(r => r.Name == role.Key) != null) continue;

                var aspnetrole = new aspnetrole()
                {
                    Id = role.Key,
                    Name = role.Key,
                    Description = role.Value,
                    Tipo = "NAZAN"
                };

                db.aspnetroles.Add(aspnetrole);
                db.SaveChanges();
            }

            var rolesMercaderia = new Dictionary<string, string>
            {
                {"MAESTRO-MERCADERIA","Acceso Total al sistema de Mercadería"},
                {"MERCADERIA-ADMINISTRARPERFILES-LISTAR","Consultar perfiles"},
                {"MERCADERIA-ADMINISTRARPERFILES-MODIFICAR","Modificar perfiles"},
                {"MERCADERIA-ADMINISTRARUSUARIOS-LISTAR","Consultar usuarios"},
                {"MERCADERIA-ADMINISTRARUSUARIOS-MODIFICAR","Modificar usuarios"},
                {"MERCADERIA-MENSAJESINSTITUCIONALES","Mensajes Insitucionales"},
            };

            foreach (var role in rolesMercaderia)
            {
                if (db.aspnetroles
                    .FirstOrDefault(r => r.Name == role.Key) != null) continue;

                var aspnetrole = new aspnetrole()
                {
                    Id = role.Key,
                    Name = role.Key,
                    Description = role.Value,
                    Tipo = "MERCADERIA"
                };

                db.aspnetroles.Add(aspnetrole);
                db.SaveChanges();
            }

            var rolesServicio = new Dictionary<string, string>
            {
                {"MAESTRO-SERVICIO","Acceso Total al sistema de Mercadería"},
                {"SERVICIO-ADMINISTRARPERFILES-LISTAR","Consultar perfiles"},
                {"SERVICIO-ADMINISTRARPERFILES-MODIFICAR","Modificar perfiles"},
                {"SERVICIO-ADMINISTRARUSUARIOS-LISTAR","Consultar usuarios"},
                {"SERVICIO-ADMINISTRARUSUARIOS-MODIFICAR","Modificar usuarios"},
                {"SERVICIO-MENSAJESINSTITUCIONALES","Mensajes Insitucionales"},
            };

            foreach (var role in rolesServicio)
            {
                if (db.aspnetroles
                    .FirstOrDefault(r => r.Name == role.Key) != null) continue;

                var aspnetrole = new aspnetrole()
                {
                    Id = role.Key,
                    Name = role.Key,
                    Description = role.Value,
                    Tipo = "SERVICIO"
                };

                db.aspnetroles.Add(aspnetrole);
                db.SaveChanges();
            }


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

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                ExpireTimeSpan = TimeSpan.FromMinutes(30)
            });


        }
    }
}