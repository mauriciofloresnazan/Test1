using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Ppgz.Repository;
using Ppgz.Services;
using Ppgz.Web.Models;

namespace Ppgz.Web
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        //AFI
        public Dictionary<string, string> GetRolesNazan()
        {
            var rolesNazan = new Dictionary<string, string>
            {
                {"MAESTRO-NAZAN", "Acceso Total al sistema de Nazan"},
                {"NAZAN-ADMINISTRARPERFILESNAZAN-LISTAR", "Consultar Perfiles"},
                {"NAZAN-ADMINISTRARPERFILESNAZAN-MODIFICAR", "Modificar Perfiles"},
                {"NAZAN-ADMINISTRARUSUARIOSNAZAN-LISTAR", "Consultar Usuarios"},
                {"NAZAN-ADMINISTRARUSUARIOSNAZAN-MODIFICAR", "Modificar Usuarios"},
                {"NAZAN-ADMINISTRARMENSAJESINSTITUCIONALES-LISTAR", "Consultar Mensajes Insitucionales"},
                {"NAZAN-ADMINISTRARMENSAJESINSTITUCIONALES-MODIFICAR", "Modificar Mensajes Institucionales"},

                {"NAZAN-CONFIGSYS", "Actualizar las configuraciones del sistema"},
                {"NAZAN-NIVELSERVICIO", "Actualizar niveles de servicio por proveedores"},
                {"NAZAN-ADMINISTRARCITAS", "Administracion de citas"}
            };
            return rolesNazan;
        }
        //AFI
        public Dictionary<string, string> GetRolesMercaderia()
        {
            var rolesMercaderia = new Dictionary<string, string>
            {
                {"MAESTRO-MERCADERIA","Acceso Total al Sistema"},
                {"MERCADERIA-ADMINISTRARPERFILES-LISTAR","Consultar Perfiles"},
                {"MERCADERIA-ADMINISTRARPERFILES-MODIFICAR","Modificar Perfiles"},
                {"MERCADERIA-ADMINISTRARUSUARIOS-LISTAR","Consultar Usuarios"},
                {"MERCADERIA-ADMINISTRARUSUARIOS-MODIFICAR","Modificar Usuarios"},
                {"MERCADERIA-MENSAJESINSTITUCIONALES","Mensajes Institucionales"},
                {"MERCADERIA-ORDENESCOMPRA","Ordenes de Compra"},
                {"MERCADERIA-CUENTASPAGAR","Cuentas por Pagar"},

                {"MERCADERIA-CONTROLCITAS","Control de Citas"},
                {"MERCADERIA-COMPROBANTESRECIBO", "Comprobantes de Recibo"},
                {"MERCADERIA-IMPRESIONETIQUETAS", "Impresion de Etiquetas"},
                {"MERCADERIA-REPORTESPROVEEDORES", "Reportes Proveedores"},
                {"MERCADERIA-FACTURAS", "Gestion de Facturas"},
                {"MERCADERIA-GESTIONPROVEEDORES", "Gestión de Proveedores"}


               
            };
            return rolesMercaderia;
        }
        //AFI
        public Dictionary<string,string> GetRolesServicio()
        {
            var rolesServicio = new Dictionary<string, string>
            {
                {"MAESTRO-SERVICIO","Acceso Total al sistema de Servicio"},
                {"SERVICIO-ADMINISTRARPERFILES-LISTAR","Consultar perfiles"},
                {"SERVICIO-ADMINISTRARPERFILES-MODIFICAR","Modificar perfiles"},
                {"SERVICIO-ADMINISTRARUSUARIOS-LISTAR","Consultar usuarios"},
                {"SERVICIO-ADMINISTRARUSUARIOS-MODIFICAR","Modificar usuarios"},
                {"SERVICIO-MENSAJESINSTITUCIONALES","Mensajes Insitucionales"},
                {"SERVICIO-ORDENESCOMPRA","Acceso a Ordenes de Compra"},
                {"SERVICIO-CUENTASPAGAR","Cuentas por Pagar"},
                
                {"SERVICIO-REPORTESPROVEEDORES", "Reportes Proveedores"},
                {"SERVICIO-FACTURAS", "Gestion de facturas"},
                {"SERVICIO-GESTIONPROVEEDORES", "Gestionar los proveedores asociados"}


            };
            return rolesServicio;
        }

        public void CargarConfiguraciones()
        {
            const string sql = @"
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'warehouse.working-day.enabled' AS Clave, '2,3,4,5,6' AS Valor, 1 AS Habilitado, 'Indica los dias de la semana, donde 1=Domingo, 2=Lunes,...7=Sabado, donde se pueden hacer las recepciones de mercancias o entregas' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'warehouse.working-day.enabled');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'warehouse.special-day.provider' AS Clave, '4' AS Valor, 1 AS Habilitado, 'Indica el dia de entrega para los proveedores especiales o vip de impuls' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'warehouse.special-day.provider');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'warehouse.max-pairs.per-day' AS Clave, '27000' AS Valor, 1 AS Habilitado, 'Indica la cantidad maxima de pares por dia permitida' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'warehouse.max-pairs.per-day');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'warehouse.max-pairs.per-week' AS Clave, '90000' AS Valor, 1 AS Habilitado, 'Indica la cantidad maxima de pares por semana permitida' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'warehouse.max-pairs.per-week');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'warehouse.platform-rail.max-pair.hour' AS Clave, '600' AS Valor, 1 AS Habilitado, 'Indica la cantidad maxima de pares por hora por riel permitida' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'warehouse.platform-rail.max-pair.hour');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'site.url' AS Clave, 'http://ppgz.com' AS Valor, 1 AS Habilitado, 'Indica la dirección del portal' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'site.url');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'mail.main.address' AS Clave, 'impuls.ppgz@gmail.com' AS Valor, 1 AS Habilitado, 'Indica  la dirección del del correo emisor' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'mail.main.address');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'mail.main.password' AS Clave, 'Venezuela2017' AS Valor, 1 AS Habilitado, 'Indica la contraseña del del correo emisor' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'mail.main.password');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'mail.main.smtp.host' AS Clave, 'smtp.gmail.com' AS Valor, 1 AS Habilitado, 'Indica el host del servidor smtp del correo emisor' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'mail.main.smtp.host');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'mail.main.smtp.port' AS Clave, '587' AS Valor, 1 AS Habilitado, 'Indica el puerto del servidor smtp del correo emisor' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'mail.main.smtp.port');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'mail.main.smtp.enablessl' AS Clave, '1' AS Valor, 1 AS Habilitado, 'Indica si la conexión del servidor smtp require ssl' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'mail.main.smtp.enablessl');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'rfc.bw.name' AS Clave, 'QIM' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'rfc.bw.name');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'rfc.bw.appserverhost' AS Clave, '172.18.3.16' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'rfc.bw.appserverhost');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'rfc.bw.user' AS Clave, 'USRPORTAL' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'rfc.bw.user');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'rfc.bw.password' AS Clave, 'wspportalp' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'rfc.bw.password');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'rfc.bw.client' AS Clave, '600' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'rfc.bw.client');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'rfc.bw.systemnumber' AS Clave, '50' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'rfc.bw.systemnumber');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'rfc.bw.language' AS Clave, 'EN' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'rfc.bw.language');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'rfc.bw.poolsize' AS Clave, '5' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'rfc.bw.poolsize');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'rfc.bw.peakconnectionslimit' AS Clave, '35' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'rfc.bw.peakconnectionslimit');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'rfc.bw.idletimeout' AS Clave, '500' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'rfc.bw.idletimeout');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'rfc.main.name' AS Clave, 'QIM' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'rfc.main.name');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'rfc.main.appserverhost' AS Clave, '172.18.3.21' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'rfc.main.appserverhost');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'rfc.main.user' AS Clave, 'WSPRR' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'rfc.main.user');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'rfc.main.password' AS Clave, 'wsp2017' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'rfc.main.password');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'rfc.main.client' AS Clave, '300' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'rfc.main.client');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'rfc.main.systemnumber' AS Clave, '22' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'rfc.main.systemnumber');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'rfc.main.language' AS Clave, 'EN' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'rfc.main.language');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'rfc.main.poolsize' AS Clave, '5' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'rfc.main.poolsize');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'rfc.main.peakconnectionslimit' AS Clave, '35' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'rfc.main.peakconnectionslimit');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'rfc.main.idletimeout' AS Clave, '500' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'rfc.main.idletimeout');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'warehouse.platform-rail.max-pair-hour.tolerance' AS Clave, '0.16' AS Valor, 1 AS Habilitado, 'Indica la tolerancia en márgenes de pares por hora' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'warehouse.platform-rail.max-pair-hour.tolerance');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'batchfile.wrongfilespath' AS Clave, 'C:\\temp\\Ppgz\\WrongFiles' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'batchfile.wrongfilespath');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'batchfile.crinboxpath' AS Clave, 'C:\\temp\\Ppgz\\Cr\\Inbox' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'batchfile.crinboxpath');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'batchfile.crpath' AS Clave, 'C:\\temp\\Ppgz\\Cr\\Processed' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'batchfile.crpath');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'batchfile.crfilter' AS Clave, 'cr_*.pdf' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'batchfile.crfilter');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'batchfile.etiquetasinboxpath' AS Clave, 'C:\\temp\\Ppgz\\Etiquetas\\Inbox' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'batchfile.etiquetasinboxpath');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'batchfile.etiquetaspath' AS Clave, 'C:\\temp\\Ppgz\\Etiquetas\\Processed' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'batchfile.etiquetaspath');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'batchfile.etiquetasfilter' AS Clave, 'etq_*.csv' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'batchfile.etiquetasfilter');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'rfc.common.function.param.bukrs.mercaderia' AS Clave, '1001' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'rfc.common.function.param.bukrs.mercaderia');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'rfc.common.function.param.bukrs.servicio' AS Clave, '1000' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'rfc.common.function.param.bukrs.servicio');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'facturas.rootdirectory' AS Clave, 'c:\\temp\\facturas\\' AS Valor, 1 AS Habilitado, '' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'facturas.rootdirectory');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'warehouse.warehouses' AS Clave, 'CD01,CD02,CD03,CD04,CD05,CD06' AS Valor, 1 AS Habilitado, 'Codigos SAP de todos los almacenes' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'warehouse.warehouses');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'warehouse.limited-warehouses-per-day' AS Clave, 'CD01,CD06' AS Valor, 1 AS Habilitado, 'Codigos SAP de los almacenes que tienen cantidades limitadas por dia' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'warehouse.limited-warehouses-per-day');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'warehouse.limited-warehouses-per-week' AS Clave, 'CD01,CD06' AS Valor, 1 AS Habilitado, 'Codigos SAP de los almacenes que tienen cantidades limitadas por semana' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'warehouse.limited-warehouses-per-week');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'rfc.common.function.param.ekorg.mercaderia' AS Clave, 'OC01' AS Valor, 1 AS Habilitado, 'Parámetro de la Organización de Compras que será enviado para todos los proveedores registrados como Mercaderías' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'rfc.common.function.param.ekorg.mercaderia');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'rfc.common.function.param.ekorg.servicio' AS Clave, 'OC02' AS Valor, 1 AS Habilitado, 'Parámetro de la Organización de Compras que será enviado para todos los proveedores registrados como Servicio' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'rfc.common.function.param.ekorg.servicio');
                INSERT INTO configuraciones (Clave, Valor, Habilitado, Descripcion) SELECT 'wfc.url.cita.add' AS Clave, 'http://localhost:14766/CitationControlService.svc/rest/AddCitation' AS Valor, 1 AS Habilitado, 'Url para el servicio que persiste las citas' AS Descripcion FROM dual WHERE NOT EXISTS(SELECT * FROM configuraciones WHERE Clave = 'wfc.url.cita.add');                
";
            Db.Insert(sql);
        }

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
            var usuarioManager = new UsuarioManager();
  

            //Roles
            var db = new Entities();
            //AFI
            var rolesNazan = GetRolesNazan();

            foreach (var role in rolesNazan)
            {
                if (db.AspNetRoles
                    .FirstOrDefault(r => r.Name == role.Key) != null) continue;

                var aspnetrole = new AspNetRole()
                {
                    Id = role.Key,
                    Name = role.Key,
                    Description = role.Value,
                    Tipo = "NAZAN"
                };

                db.AspNetRoles.Add(aspnetrole);
                db.SaveChanges();
            }
            //AFI
            var rolesMercaderia = GetRolesMercaderia();

            foreach (var role in rolesMercaderia)
            {
                if (db.AspNetRoles
                    .FirstOrDefault(r => r.Name == role.Key) != null) continue;

                var aspnetrole = new AspNetRole()
                {
                    Id = role.Key,
                    Name = role.Key,
                    Description = role.Value,
                    Tipo = "MERCADERIA"
                };

                db.AspNetRoles.Add(aspnetrole);
                db.SaveChanges();
            }

            //AFI
            var rolesServicio = GetRolesServicio();

            foreach (var role in rolesServicio)
            {
                if (db.AspNetRoles
                    .FirstOrDefault(r => r.Name == role.Key) != null) continue;

                var aspnetrole = new AspNetRole()
                {
                    Id = role.Key,
                    Name = role.Key,
                    Description = role.Value,
                    Tipo = "SERVICIO"
                };

                db.AspNetRoles.Add(aspnetrole);
                db.SaveChanges();
            }


            if (applicationUserManager.FindByName("superusuario") == null)
            {

                usuarioManager.CrearNazan(
                   
                     "superusuario",
                     "superusuario",
                     "superusuario",
                     "superusuario",
                     null,
                     null,
                     true,
                     PerfilManager.MaestroNazan.Id,
                     "123456");
            }

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                ExpireTimeSpan = TimeSpan.FromMinutes(30)
            });

             CargarConfiguraciones();

        }
    }
}