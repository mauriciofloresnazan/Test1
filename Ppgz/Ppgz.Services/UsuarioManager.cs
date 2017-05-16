using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
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

        public AspNetUser Find(string id)
        {
            return _db.AspNetUsers.Find(id);
        }

        public AspNetUser FindByUsername(string userName)
        {
            return _db.AspNetUsers.FirstOrDefault(u => u.UserName == userName);
        }

        public List<AspNetUser> FindByCuentaId(int id)
        {
            return _db.AspNetUsers.Where(u => u.cuentas1.Any(c => c.Id == id)).ToList();
        }

        public List<AspNetUser> FindAllNazan()
        {
            return _db.AspNetUsers.Where(u => u.Tipo == Tipo.Nazan).ToList();
        }

        /// <summary>
        /// Crea y retorna un usuario
        /// </summary>
        internal AspNetUser Crear(string tipo, string userName, string nombre, string apellido,
            string email, bool activo, int perfilId, string password, string telefono = null, string cargo = null)
        {
            // Validaciones
            // TODO VALIDACIONES DE LA ESTRUCTURA DE LOS DATOS
           
            ValidarNombreApellido(nombre);
            ValidarNombreApellido(apellido);
            ValidarEmail(email);

            if (_db.AspNetUsers.FirstOrDefault(u => u.UserName == userName) != null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_NombreUsuarioExistente);
            }
			if (_db.AspNetUsers.FirstOrDefault(u => u.Email == email) != null)
            {
                throw new BusinessException("La Dirección de correo ya ha sido utilizada por otro usuario");
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
                Id = Guid.NewGuid().ToString(),
                Tipo = tipo,
                UserName = userName,
                Nombre = nombre,
                Apellido = apellido,
                Email = email,
                PhoneNumber = telefono,
                Cargo = cargo,
                Activo = activo,
                PerfilId = perfilId,
                PasswordHash = _passwordHasher.HashPassword(password), 

                //Campos requeridos por Identity
                EmailConfirmed =  false,
                SecurityStamp = Guid.NewGuid().ToString(),
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = true,
                AccessFailedCount = 0,
                Borrado = 0,
                


                
                
                
            };

            _db.AspNetUsers.Add(usuario);
            _db.SaveChanges();

            // ASIGNACION DE LOS ROLES DE ACUERDO AL PERFIL
            var perfilManager = new PerfilManager();
            var perfil = _db.perfiles.Find(perfilId);

            foreach (var role in perfil.AspNetRoles)
            {
                AgregarRoleEnUsuario(role.Id, usuario.Id);
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

            var usuario = Crear(Tipo.Nazan, userName, nombre, apellido, email,
                activo, perfilId, password, telefono, cargo);

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
			
			if(_db.AspNetUsers.Where(u => u.cuentas1.Any(c => c.Id == cuentaId)).Count() > 4)
			{
				// TODO PASAR A RESOURCE
                throw new BusinessException("No puede crear mas de 5 usuarios en la cuenta");
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

            var usuario = Crear(Tipo.Proveedor, userName, nombre, apellido, email,
                activo, perfilId, password, telefono, cargo);

            // Asiganmos el nuevo usuario a la cuenta
            cuenta.AspNetUsers.Add(usuario);
            _db.SaveChanges();

            return usuario;
        }


        /// <summary>
        /// Crea el usuario de tipo Maestro Proveedor
        /// </summary>
        public AspNetUser CrearMaestroProveedor(string userName, string nombre, string apellido,
            string email, string telefono, string cargo, bool activo, string password, int cuentaId)
        {
            // Valida la cuenta del proveedor
            var cuenta = _db.cuentas.Find(cuentaId);
            if (cuenta == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_Cuenta_Id);
            }

            // La cuenta solo puede tener un usuario maetro
            if (cuenta.AspNetUsers.SingleOrDefault(u => u.Tipo == Tipo.MaestroProveedor) != null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_Cuenta_YaTieneUsuarioMaestro);
            }


            var perfil = cuenta.Tipo == CuentaManager.Tipo.Mercaderia
                ? PerfilManager.MaestroMercaderia
                : PerfilManager.MaestroServicio;

            var usuario = Crear(Tipo.MaestroProveedor, userName, nombre, apellido, email,
                activo, perfil.Id, password, telefono, cargo);

            return usuario;
        }

        /// <summary>
        /// Actualizacion de los campos básico y genericos del usuario
        /// es valido para todos los tipos Proveedor, MaestroProveedor y Nazan
        /// </summary>
        public AspNetUser Actualizar(string id, string nombre = null, string apellido = null, string email = null,
            string telefono = null, string cargo = null, string password = null)
        {
            // Validaciones
            // TODO VALIDACIONES DE LA ESTRUCTURA DE LOS DATOS
           
            ValidarNombreApellido(nombre);
            ValidarNombreApellido(apellido);
            ValidarEmail(email);

            var usuario = _db.AspNetUsers.Find(id);

            if (usuario == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_Usuario_Id);
            }
			
			if (_db.AspNetUsers.FirstOrDefault(u => u.Email == email && u.Id != id) != null)
            {
                throw new BusinessException("La Dirección de correo ya ha sido utilizada por otro usuario");
            }
            if (nombre != null)
                usuario.Nombre = nombre;
            if (apellido != null)
                usuario.Apellido = apellido;
            if (email != null)
                usuario.Email = email;
            if (telefono != null)
                usuario.PhoneNumber = telefono;
            if (cargo != null)
                usuario.Cargo = cargo;
            if (password != null)
                usuario.PasswordHash = _passwordHasher.HashPassword(password);

            _db.Entry(usuario).State = EntityState.Modified;
            _db.SaveChanges();

            return usuario;
        }


        public void ActualizarPerfil(string usuarioId, int perfilId)
        {
            var perfil = _db.perfiles.Find(perfilId);

            if (perfil == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_Perfil_Id);
            }

            var usuario = _db.AspNetUsers.Find(usuarioId);

            // Validacion Nazan
            if (usuario.Tipo == Tipo.Nazan)
            {
                if (perfil.Tipo != PerfilManager.TipoPerfil.Nazan)
                {
                    throw new BusinessException(CommonMensajesResource.ERROR_Perfil_Id);
                }
            }

            // Validacion Maestro Proveedor
            if (usuario.Tipo == Tipo.MaestroProveedor)
            {
				// TODO NO CAMBIAR EL PERFIL SI ES MAESTRO
				return;
                //throw new BusinessException(CommonMensajesResource.ERROR_UsarioMaestroProveedor_CambiarPerfil);
            }

            // Validacion proveedor
            if (usuario.Tipo == Tipo.Proveedor)
            {
                var cuenta = _db.cuentas.FirstOrDefault(c => c.AspNetUsers.Any(u => u.Id == usuario.Id));
                if (cuenta == null)
                {
                    //TODO MERJORAR
                    //ERROR DE APLICACION 
                    throw new Exception("ERRRO DE APLICACION USUARIO PROVEEDOR SIN CUENTA");
                }
                if (cuenta.Tipo == CuentaManager.Tipo.Mercaderia)
                {
                    if (perfil.Id != PerfilManager.MaestroMercaderia.Id && !cuenta.perfiles.Contains(perfil))
                    {
                        throw new BusinessException(CommonMensajesResource.ERROR_Perfil_Id);
                    }
                }
                if (cuenta.Tipo == CuentaManager.Tipo.Servicio)
                {
                    if (perfil.Id != PerfilManager.MaestroServicio.Id && !cuenta.perfiles.Contains(perfil))
                    {
                        throw new BusinessException(CommonMensajesResource.ERROR_Perfil_Id);
                    }
                }
            }

            usuario.PerfilId = perfil.Id;
            _db.Entry(usuario).State = EntityState.Modified;
            _db.SaveChanges();

            QuitarRolesDeUsuario(usuario.Id);

            foreach (var role in perfil.AspNetRoles)
            {
                AgregarRoleEnUsuario(role.Id, usuario.Id);
            }
        }

        public void AgregarRoleEnUsuario(string roleId, string usuarioId)
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

            if (usuario.UserName.ToLower() == ConfigurationManager.AppSettings["SuperAdminUserName"].ToLower())
            {
                throw new BusinessException(CommonMensajesResource.ERROR_EliminarSuperAdmin);
            }

            QuitarRolesDeUsuario(usuarioId);

            _db.AspNetUsers.Remove(usuario);
            _db.SaveChanges();
        }
		
		public void Eliminar(string usuarioId, int cuentaId)
        {
            var usuario = _db.AspNetUsers.Find(usuarioId);
            if (usuario == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_Usuario_Id);
            }

            if (usuario.UserName.ToLower() == ConfigurationManager.AppSettings["SuperAdminUserName"].ToLower())
            {
                throw new BusinessException(CommonMensajesResource.ERROR_EliminarSuperAdmin);
            }
            var cuenta = _db.cuentas.Find(cuentaId);
            if (cuenta == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_Cuenta_Id);
            }
			
			if(usuario.Tipo == Tipo.MaestroProveedor)
			{
				// TODO PASAR A UN RESOURCE
                throw new BusinessException("No puede eliminar el usuario Maestro de la cuenta");				
			}
			cuenta.AspNetUsers.Remove(usuario);
            _db.SaveChanges();
            QuitarRolesDeUsuario(usuarioId);

            _db.AspNetUsers.Remove(usuario);
            _db.SaveChanges();
        }
        #region Validaciones

        static void ValidarNombreApellido(string valor)
        {
            // TODO HACER CONFIGURABLE EN EL FUTURO
            var regex = new Regex(@"^[A - Za - z'\-\p{L}\p{Zs}\p{Lu}\p{Ll}\']+$");
            if (!regex.IsMatch(valor))
            {
                throw new BusinessException(CommonMensajesResource.Error_NombreApellido);
            }
        }

        static void ValidarTelefono(string valor)
        {
            // TODO HACER CONFIGURABLE EN EL FUTURO
            var regex = new Regex(@"^\s*(?:\+?(\d{1,3}))?[-. (]*(\d{3})[-. )]*(\d{3})[-. ]*(\d{4})(?: *x(\d+))?\s*$");
            if (!regex.IsMatch(valor))
            {
                throw new BusinessException(CommonMensajesResource.Error_Telefono);
            }
        }
        static void ValidarEmail(string valor)
        {
            // TODO HACER CONFIGURABLE EN EL FUTURO
            var regex = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
            if (!regex.IsMatch(valor))
            {
                //throw new BusinessException(CommonMensajesResource.Error_Email);
            }
        }

        #endregion
    }
}