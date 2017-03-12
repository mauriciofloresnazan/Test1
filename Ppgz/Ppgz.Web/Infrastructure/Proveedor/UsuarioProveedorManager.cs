using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Ppgz.Repository;
using Ppgz.Web.Models;

namespace Ppgz.Web.Infrastructure.Proveedor
{
    public class UsuarioProveedorManager
    {
        private readonly UserManager<ApplicationUser> _applicationUserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
       
        private readonly Entities _db = new Entities();
       
        private readonly PerfilProveedorManager _perfilProveedorManager = new PerfilProveedorManager();

        public static string[] Tipos
        {
            get
            {
                return new[]
                {
                    "PROVEEDOR-MAESTRO",
                    "PROVEEDOR"
                };
            }
        }

        public List<AspNetUser> FindAll()
        {
            return _db.AspNetUsers.Where(
                u => Tipos.Contains(u.Tipo)).ToList();
        }

        public AspNetUser Find(string id)
        {
            return _db.AspNetUsers
                .FirstOrDefault(u => u.Id == id && Tipos.Contains(u.Tipo));
        }

        public List<AspNetUser> FindByCuentaId(int id)
        {
            // TODO PASAR A UN ARCHIVO DE RECURSOS O STORE PROCEDURE
            return _db.Database.SqlQuery<AspNetUser>(@"
                SELECT  * 
                FROM    aspnetusers
                WHERE   id IN (SELECT UsuarioId 
                               FROM cuentasusuarios 
                               WHERE CuentaId = {0})", id).ToList();
        }

        public AspNetUser FindMaestroByLogin(string login)
        {
            return _db.AspNetUsers
                .FirstOrDefault(u => u.UserName == login && u.Tipo == "PROVEEDOR-MAESTRO");
        }
        public void Crear(string userName, string nombre, string apellido, string email,
            string password, int perfilId, string tipo, int? cuentaId = null)
        {
            var cuentaManager = new CuentaManager();
         
            if (!Tipos.Contains(tipo))
            {
                throw new BusinessException(CommonMensajesResource.ERROR_UsuarioProveedor_TipoIncorrecto);
                
            }
            if (_applicationUserManager.FindByName(userName) != null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_UsuarioProveedor_LoginExistente);
            }

            var perfil = _perfilProveedorManager.Find(perfilId);

            if (perfil == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_PerfilProveedor_PefilIdIncorrecto);
            }

            if (cuentaId != null)
            {
                var cuenta = cuentaManager.Find((int)cuentaId);
                if (cuenta == null)
                {
                    throw new BusinessException(CommonMensajesResource.ERROR_CuentaManager_IdIncorrecto);                    
                }
            }

            var usuario = new ApplicationUser()
            {
                UserName = userName,
                Nombre = nombre,
                Apellido = apellido,
                Email = email.Trim(),
                Tipo = tipo,
                Activo = true,
                PerfilId = perfil.Id
            };

            var result = _applicationUserManager.Create(usuario, password);

            if (!result.Succeeded)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_General);
            }

            foreach (var role in perfil.AspNetRoles)
            {
                _applicationUserManager.AddToRole(usuario.Id, role.Id);
            }


            if (cuentaId != null)
            {
                cuentaManager.AsociarUsuarioEnCuenta(usuario.Id, (int) cuentaId);
            }
        }

        public void Eliminar(string id)
        {
            var usuario = _db.AspNetUsers
                .Single(u => u.Id == id);

            if (usuario == null)
                throw new BusinessException(CommonMensajesResource.ERROR_UsuarioProveedor_IdIncorrecto);

            var commonManager = new CommonManager();

            if (usuario.Id == commonManager.GetUsuarioAutenticado().Id)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_UsuarioProveedor_EliminarPropioUsuario);
            }

            if (usuario.Tipo.ToUpper().Trim() == "PROVEEDOR-MAESTRO")
            {
                throw new BusinessException(CommonMensajesResource.ERROR_UsuarioProveedor_EliminarMaestro);
            }

            var cuentaManager = new CuentaManager();
            cuentaManager.DesAsociarUsuarioEnCuenta(usuario.Id);


            _db.AspNetUsers.Remove(usuario);
            _db.SaveChanges();
        }


        public void Update(string id, string nombre, string apellido, string email, 
            int perfilId, string password = null)
        {
            var usuario = Find(id);

            if (usuario == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_UsuarioProveedor_IdIncorrecto);
            }

            var perfil = _perfilProveedorManager.Find(perfilId);

            if (perfil == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_PerfilProveedor_PefilIdIncorrecto);
            }

            usuario.Nombre = nombre;
            usuario.Apellido = apellido;

            if (!string.IsNullOrWhiteSpace(usuario.Email = email))
            {
                usuario.Email = email.ToLower();
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                var commonManager = new CommonManager();
                usuario.PasswordHash = commonManager.HashPassword(password);
            }
            usuario.PerfilId = perfilId;
            usuario.AspNetUserRoles.Clear();

            _db.Entry(usuario).State = EntityState.Modified;
            _db.SaveChanges();

     
            foreach (var role in perfil.AspNetRoles)
            {
                _applicationUserManager.AddToRole(usuario.Id, role.Id);
            }
        }
    }
}