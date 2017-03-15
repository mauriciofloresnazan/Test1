using System;
using System.Linq;
using Ppgz.Repository;

namespace Ppgz.Services
{
    public class PerfilManager
    {
        private readonly Entities _db = new Entities();
        /// <summary>
        /// Tipos de perfil de acuerdo al modelo de datos
        /// </summary>
        public static class TipoPerfil
        {
            public static string Nazan { get { return "NAZAN"; } }
            public static string Proveedor { get { return "PROVEEDOR"; } }
        }

        /// <summary>
        /// Estos perfiles se crean al incio del sistema y siempre estaran disponibles en la base de datos
        /// </summary>
        #region Prefilesmaestros 
        public static perfile MaestroMercaderia
        {
            get
            {
                var db = new Entities();
                return db.perfiles.Single(p => p.Nombre == "MAESTRO-MERCADERIA");
            }
        }
        public static perfile MaestroServicio
        {
            get
            {
                var db = new Entities();
                return db.perfiles.Single(p => p.Nombre == "MAESTRO-SERVICIO");
            }
        }
        // PREFIL CREADO POR EL SISTEMA AL INICIO
        public static perfile MaestroNazan
        {
            get
            {
                var db = new Entities();
                return db.perfiles.Single(p => p.Nombre == "MAESTRO-NAZAN");
            }
        }
        #endregion

        public perfile FindPerfilProveedor(string nombrePerfil, int? cuentaId)
        {
            return _db.perfiles
                .FirstOrDefault(p => p.Nombre == nombrePerfil && p.Tipo == TipoPerfil.Proveedor && p.CuentaId == cuentaId);
        }
        public perfile FindPerfilNazan(int id)
        {
            return _db.perfiles
                .FirstOrDefault(p => p.Id == id && p.Tipo == TipoPerfil.Nazan);
        }


        public perfile Crear(string tipo, string nombre, string[] rolesIds, int? cuentaId = null)
        {
            var tipos = new[]
            {
                TipoPerfil.Nazan,
                TipoPerfil.Proveedor
            };

            if (!tipos.Contains(tipo))
            {
                throw new BusinessException(CommonMensajesResource.ERROR_Perfil_Tipo);
            }

            // TODO VALIDACIONES
            nombre = nombre.Trim();

            if (!rolesIds.Any())
            {
                throw new Exception(CommonMensajesResource.ERROR_Perfil_AccesosRequeridos);
            }
            var aspnetroles =
                _db.AspNetRoles.Where(r => rolesIds.Contains(r.Name));

            if (!aspnetroles.Any())
            {
                throw new Exception(CommonMensajesResource.ERROR_Perfil_AccesosRequeridos);
            }

            if (cuentaId != null)
            {            
                if (_db.cuentas.Find((int)cuentaId)!= null)
                {
                    throw new Exception(CommonMensajesResource.ERROR_Cuenta_Id);
                }
            }

            var perfil = new perfile { Nombre = nombre, Tipo = tipo, CuentaId = cuentaId };

            foreach (var role in aspnetroles)
            {
                perfil.AspNetRoles.Add(role);
            }

            _db.perfiles.Add(perfil);
            _db.SaveChanges();

            return perfil;
        }
        public perfile CrearProveedor(string nombre, string[] rolesIds, int cuentaId)
        {
            var perfil = _db.perfiles.FirstOrDefault(p => p.Nombre == nombre && p.CuentaId == cuentaId);

            if (perfil != null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_Perfil_NombreExistente);
            }

           return Crear(TipoPerfil.Proveedor, nombre,rolesIds, cuentaId);
        }
    }
}
