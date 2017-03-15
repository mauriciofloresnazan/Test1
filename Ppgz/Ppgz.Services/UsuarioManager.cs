using System;
using System.Data.Entity;
using System.Linq;
using Microsoft.AspNet.Identity;
using Ppgz.Repository;

namespace Ppgz.Services
{
    public class UsuarioManager
    {
        private readonly Entities _db = new Entities();
        private readonly PasswordHasher _passwordHasher = new PasswordHasher();

        /// <summary>
        /// Tipos de usuario de acuerdo al modelo de datos
        /// </summary>
        public static class Tipo
        {
            public static string Nazan { get { return "NAZAN"; } }
            public static string MaestroProveedor { get { return "MAESTRO-PROVEEDOR"; } }
            public static string Proveedor { get { return "PROVEEDOR"; } }

        }

        public AspNetUser FindByUsername(string userName)
        {
            return _db.AspNetUsers.FirstOrDefault(u => u.UserName == userName);
        }

        /// <summary>
        /// Crea y retorna un usuario
        /// </summary>
        public AspNetUser Crear(string tipo, string userName, string nombre, string apellido,
            string email, string telefono, string cargo, bool activo, int perfilId, string password)
        {
            // Validaciones
            // TODO VALIDACIONES DE LA ESTRUCTURA DE LOS DATOS
            
            if (_db.AspNetUsers.FirstOrDefault(u => u.UserName == userName) != null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_NombreUsuarioExistente);
            }
            var tipos = new[]
            {
                Tipo.Nazan,
                Tipo.MaestroProveedor,
                Tipo.Proveedor
            };

            if (!tipos.Contains(tipo))
            {
                throw new BusinessException(CommonMensajesResource.ERROR_Usuario_Tipo);
            }

            var usuario = new AspNetUser()
            {
                Id= Guid.NewGuid().ToString(),
                Tipo = tipo,
                UserName = userName,
                Nombre = nombre,
                Apellido = apellido,
                Email = email,
                PhoneNumber = telefono,
                Cargo = cargo,
                Activo = activo,
                PerfilId = perfilId,
                PasswordHash = _passwordHasher.HashPassword(password)
            };

            _db.AspNetUsers.Add(usuario);
            _db.SaveChanges();

            // ASIGNACION DE LOS ROLES DE ACUERDO AL PERFIL
            var perfilManager = new PerfilManager();
            var perfil = perfilManager.FindPerfilNazan(perfilId);

            foreach (var role in perfil.AspNetRoles)
            {
                AgregarRoleEnUsuario(usuario.Id, role.Id);
            }

            return usuario;
        }

        /// <summary>
        /// Crea el usuario de tipo Nazan
        /// </summary>
        public AspNetUser CrearNazan(string userName, string nombre, string apellido, string email, string telefono,
            string cargo, bool activo, int perfilId, string password)
        {
            var perfilManager = new PerfilManager();
            
            // Vlida que el perfil sea de tipo nazan
            var perfil = perfilManager.FindPerfilNazan(perfilId);
            if (perfil == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_PerfilProveedor_PefilIdIncorrecto);
            }
            
            var usuario = Crear(Tipo.Nazan, userName, nombre, apellido, email, telefono, cargo,
                activo, perfilId, password);

            return usuario;
        }

        /// <summary>
        /// Crea el usuario de tipo Proveedor
        /// </summary>
        public AspNetUser CrearProveedor(string userName, string nombre, string apellido, string email, 
            string telefono, string cargo, bool activo, int perfilId, string password, int cuentaId)
        {
            // Valida la cuenta del proveedor
            var cuenta = _db.cuentas.Find(cuentaId);
            if (cuenta == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_Cuenta_Id);
            }
                        
            // Valida el prefil que este relacionado con la cuenta o sea maestro
            var perfil = _db.perfiles.Find(perfilId);
            if (!cuenta.perfiles.Contains(perfil))
            {
                if (perfil.Nombre != "MAESTRO-" + cuenta.Tipo)
                {
                    throw new BusinessException(CommonMensajesResource.ERROR_Cuenta_Id);
                }
            }

            var usuario = Crear(Tipo.Proveedor, userName, nombre, apellido, email, telefono, cargo,
                activo, perfilId, password);
                
            // Asiganmos el nuevo usuario a la cuenta
            cuenta.AspNetUsers.Add(usuario);
            _db.SaveChanges();

            return usuario;
        }


        /// <summary>
        /// Crea el usuario de tipo Maestro Proveedor
        /// </summary>
        public AspNetUser CrearMaestroProveedor(string userName, string nombre, string apellido,
            string email, string telefono, string cargo, bool activo, string password, string cuentaId)
        {
            // Valida la cuenta del proveedor
            var cuenta = _db.cuentas.Find(cuentaId);
            if (cuenta == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_Cuenta_Id);
            }

            var perfil = cuenta.Tipo == CuentaManager.Tipo.Mercaderia
                ? PerfilManager.MaestroMercaderia
                : PerfilManager.MaestroServicio;

            var usuario = Crear(Tipo.MaestroProveedor, userName, nombre, apellido, email, telefono, cargo,
                activo, perfil.Id, password);

            return usuario;
        }


        public void AgregarRoleEnUsuario(string usuarioId, string roleId)
        {
            var usuario = _db.AspNetUsers.Find(usuarioId);

            if (usuario == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_Usuario_Id);
            }
            var role = _db.AspNetRoles.Find(roleId);

            if (role == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_Role_Id);
            }

            usuario.AspNetRoles.Add(role);
            _db.Entry(usuario).State = EntityState.Modified;
            _db.SaveChanges();
        }

        public void QuitarRolesDeUsuario(string usuarioId)
        {
            var usuario = _db.AspNetUsers.Find(usuarioId);
            if (usuario == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_Usuario_Id);
            }

            usuario.AspNetRoles.Clear();
            _db.Entry(usuario).State = EntityState.Modified;
            _db.SaveChanges();
        }

        public void Eliminar(string usuarioId)
        {
            var usuario = _db.AspNetUsers.Find(usuarioId);
            if (usuario == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_Usuario_Id);
            }

            _db.AspNetUsers.Remove(usuario);
            _db.SaveChanges();
        }
    }
}