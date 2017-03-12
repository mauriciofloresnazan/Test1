using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Ppgz.Repository;

namespace Ppgz.Web.Infrastructure.Proveedor
{
    public class PerfilProveedorManager
    {
        private readonly Entities _db = new Entities();

        public static readonly string Tipo = "PROVEEDOR";

        public List<perfile> FindAll()
        {
            return _db.perfiles.Where(
                p => p.Tipo == Tipo).ToList();
        }

        public List<perfile> FindByCuentaId(int cuentaId)
        {
            return _db.perfiles.Where(p => p.CuentaId == cuentaId).ToList();
        } 

        public perfile Find(int id)
        {
            return _db.perfiles
                .FirstOrDefault(p => p.Id == id && p.Tipo == Tipo);
        }

        public perfile FindByNombre(string nombre, int? cuentaId)
        {
            return _db.perfiles
                .FirstOrDefault(p => p.Nombre == nombre && p.Tipo == Tipo && p.CuentaId == cuentaId);
        }

        internal void ValidarPermisos(string[] rolesIds, CuentaManager.Tipo cuentaTipo)
        {
            var tipo = CuentaManager.GetTipoString(cuentaTipo);

            if (!rolesIds.Any())
            {
                throw new BusinessException(CommonMensajesResource.ERROR_PerfilProveedor_AccesosRequeridos);
            }
            var aspnetroles =
                _db.AspNetRoles.Where(r => r.Tipo == tipo && rolesIds.Contains(r.Name));

            if (!aspnetroles.Any())
            {
                throw new BusinessException(CommonMensajesResource.ERROR_PerfilProveedor_AccesosRequeridos);
            }
        }
        public void Crear(string nombre, string[] rolesIds, int? cuentaId = null)
        {
            nombre = nombre.Trim();
            var perfil = FindByNombre(nombre, cuentaId);

            if (perfil != null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_PerfilProveedor_NombreExistente);
            }

            if (!rolesIds.Any())
            {
                throw new Exception(CommonMensajesResource.ERROR_PerfilProveedor_AccesosRequeridos);
            }
            var aspnetroles =
                _db.AspNetRoles.Where(r => rolesIds.Contains(r.Name));

            if (!aspnetroles.Any())
            {
                throw new Exception(CommonMensajesResource.ERROR_PerfilProveedor_AccesosRequeridos);
            }
            
            perfil = new perfile { Nombre = nombre, Tipo = Tipo, CuentaId = cuentaId };

            foreach (var role in aspnetroles)
            {
                perfil.AspNetRoles.Add(role);
            }

            _db.perfiles.Add(perfil);
            _db.SaveChanges();
        }

        public void Eliminar(int id)
        {
            var perfil = Find(id);

            if (perfil == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_PerfilProveedor_PefilIdIncorrecto);
            }
            // TODO PASAR A UNA COLECCION DE LOS MAESTROS
            if (perfil.Nombre == "MAESTRO-MERCADERIA" || perfil.Nombre == "MAESTRO-SERVICIO")
            {
                throw new BusinessException(CommonMensajesResource.ERROR_PerfilProveedor_EditarEliminarMaestro);
            }

            if (perfil.AspNetUsers.Any())
            {
                throw new BusinessException(CommonMensajesResource.ERROR_PerfilProveedor_EliminarConUsuarios);
            }

            perfil.AspNetRoles.Clear();
            _db.perfiles.Remove(perfil);
            _db.SaveChanges();
        }

        public void Actualizar(int id, string nombre, string[] rolesIds, int? cuentaId)
        {
            // Validacion del nombre que no este repetido
            var perfil = FindByNombre(nombre, cuentaId);
            if (perfil != null)
            {
                if (perfil.Id != id)
                {
                    throw new Exception(CommonMensajesResource.ERROR_PerfilProveedor_NombreExistente);
                }
            }

            perfil = Find(id);
            if (perfil == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_PerfilProveedor_PefilIdIncorrecto);
            }
            // TODO PASAR A UNA COLECCION DE LOS MAESTROS
            if (perfil.Nombre == "MAESTRO-MERCADERIA" || perfil.Nombre == "MAESTRO-SERVICIO")
            {
                throw new BusinessException(CommonMensajesResource.ERROR_PerfilProveedor_EditarEliminarMaestro);
            }

            ValidarPermisos(rolesIds, CuentaManager.GetTipoByString(perfil.cuenta.Tipo));

            var aspnetroles =
                _db.AspNetRoles.Where(r => r.Tipo == perfil.cuenta.Tipo && rolesIds.Contains(r.Name));
            

            perfil.Nombre = nombre;
            perfil.AspNetRoles.Clear();
            foreach (var role in aspnetroles)
            {
                perfil.AspNetRoles.Add(role);
            }

            _db.Entry(perfil).State = EntityState.Modified;
            _db.SaveChanges();


            var commonManager = new CommonManager();
            commonManager.ActualizarPermisosByPefilId(perfil.Id);
        }


        public List<AspNetRole> GetRoles(string tipo)
        {
            return _db.AspNetRoles
                .Where(r => r.Tipo == tipo).ToList();
        }

        public perfile GetMaestroByUsuarioTipo(CuentaManager.Tipo tipo)
        {
            var perfilNombre = string.Empty;

            switch (tipo)
            {
                case CuentaManager.Tipo.Mercaderia:
                    perfilNombre = "MAESTRO-MERCADERIA";
                    break;
                case CuentaManager.Tipo.Servicio:
                    perfilNombre = "MAESTRO-SERVICIO";
                    break;
            }

            var perfil = _db.perfiles.FirstOrDefault(pt => pt.Nombre == perfilNombre);

            if (perfil == null)
            {
                // TODO PASAR A UN MANEJADOR DE ROLES
                if (_db.AspNetRoles
                    .FirstOrDefault(ro => ro.Name == perfilNombre) == null)
                {
                    var role = new AspNetRole()
                    {
                        Id = perfilNombre,
                        Name = perfilNombre,
                        Description = "PERFIL MAESTR tiene acceso a toda la aplicación.",
                        Tipo =  CuentaManager.GetTipoString(tipo)
                    };

                    _db.AspNetRoles.Add(role);
                    _db.SaveChanges();
                }

                var perfilProveedorManager = new PerfilProveedorManager();
                perfilProveedorManager.Crear(perfilNombre, new[] {perfilNombre});
            }

            perfil = _db.perfiles.FirstOrDefault(pt => pt.Nombre == perfilNombre);

            if (perfil == null)
            {
                //todo revisar esto
                throw new Exception("");
            }

            return perfil;

        }
    }
}