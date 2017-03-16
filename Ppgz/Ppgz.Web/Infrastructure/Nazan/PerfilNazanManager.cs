using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Ppgz.Repository;
using Ppgz.Services;
using Ppgz.Web.Areas.Nazan;

namespace Ppgz.Web.Infrastructure.Nazan
{
    public class PerfilNazanManager
    {
        private readonly Entities _db = new Entities();

        public static readonly string Tipo = "NAZAN";

        public List<perfile> FindAll()
        {
            return _db.perfiles.Where(
                p => p.Tipo == Tipo).ToList();
        }

        public perfile Find(int id)
        {
            return _db.perfiles
                .FirstOrDefault(p => p.Id == id && p.Tipo == Tipo);
        }

        public perfile FindByNombre(string nombre)
        {
            return _db.perfiles
                .FirstOrDefault(p => p.Nombre == nombre && p.Tipo == Tipo);
        }

        internal void ValidarPermisos(string[] rolesIds)
        {

            if (!rolesIds.Any())
            {
                throw new BusinessException(MensajesResource.ERROR_PerfilNazan_AccesosRequeridos);
            }
            var aspnetroles =
                _db.AspNetRoles.Where(r => r.Tipo == Tipo && rolesIds.Contains(r.Name));

            if (!aspnetroles.Any())
            {
                throw new BusinessException(MensajesResource.ERROR_PerfilNazan_AccesosRequeridos);
            }
        }

        public void Crear(string nombre, string[] rolesIds)
        {
            nombre = nombre.Trim();

            var perfil = FindByNombre(nombre);

            if (perfil != null)
            {
                throw new BusinessException(MensajesResource.ERROR_PerfilNazan_NombreExistente);
            }

            ValidarPermisos(rolesIds);

            var aspnetroles = _db.AspNetRoles.Where(r => r.Tipo == Tipo && rolesIds.Contains(r.Name));
            perfil = new perfile { Nombre = nombre, Tipo = Tipo };

            foreach (var role in aspnetroles)
            {
                perfil.AspNetRoles.Add(role);
            }

            _db.perfiles.Add(perfil);
            _db.SaveChanges();
        }
        
        public void Actualizar(int id, string nombre, string[] rolesIds)
        {
            var perfil = FindByNombre(nombre);
            if (perfil != null)
            {
                if (perfil.Id != id)
                    throw new BusinessException(MensajesResource.ERROR_PerfilNazan_NombreExistente);

            }
            perfil = Find(id);

            if (perfil == null)
            {
                throw new BusinessException(MensajesResource.ERROR_PerfilNazan_PefilIdIncorrecto);
            }

            if (perfil.Nombre == "MAESTRO-NAZAN")
            {
                throw new BusinessException(MensajesResource.ERROR_PerfilNazan_EditarEliminarMaestro);
            }

            ValidarPermisos(rolesIds);

            var aspnetroles =
                _db.AspNetRoles.Where(r => r.Tipo == Tipo && rolesIds.Contains(r.Name));

            perfil = Find(id);
            perfil.Nombre = nombre;
            perfil.AspNetRoles.Clear();

            foreach (var role in aspnetroles)
            {
                perfil.AspNetRoles.Add(role);
            }

            _db.Entry(perfil).State = EntityState.Modified;
            _db.SaveChanges();


            // Relacionar los usuarios con los roles
            var commonManager = new CommonManager();
            commonManager.ActualizarPermisosByPefilId(perfil.Id);
        }
        
        public void Eliminar(int id)
        {
            var perfil = Find(id);

            if (perfil == null)
            {
                throw new BusinessException(MensajesResource.ERROR_PerfilNazan_PefilIdIncorrecto);
            }

            if (perfil.Nombre == "MAESTRO-NAZAN")
            {
                throw new BusinessException(MensajesResource.ERROR_PerfilNazan_EditarEliminarMaestro);
            }

            if (perfil.AspNetUsers.Any())
            {
                throw new BusinessException(MensajesResource.ERROR_PerfilNazan_EliminarConUsuarios);
            }
            perfil.AspNetRoles.Clear();
            _db.perfiles.Remove(perfil);
            _db.SaveChanges();
        }
        public List<AspNetRole> GetRoles()
        {
            return _db.AspNetRoles
                .Where(r => r.Tipo == Tipo).ToList();

        }

        public perfile GetMaestro()
        {
            const string perfilNombre = "MAESTRO-NAZAN";

            var perfil = _db.perfiles.FirstOrDefault(pt => pt.Nombre == perfilNombre);

            if (perfil == null)
            {
                // TODO PASAR A UN MANEJADOR DE ROLES
                if (_db.AspNetRoles
                    .FirstOrDefault(ro => ro.Name == "MAESTRO-NAZAN") == null)
                {
                    var role = new AspNetRole()
                    {
                        Id = "MAESTRO-NAZAN",
                        Name = "MAESTRO-NAZAN",
                        Description = "MAESTRO-NAZAN tiene acceso a toda la aplicación.",
                        Tipo = "NAZAN"
                    };

                    _db.AspNetRoles.Add(role);
                    _db.SaveChanges();

                }

                var perfilNazanManager = new PerfilNazanManager();
                perfilNazanManager.Crear(perfilNombre, new[] { "MAESTRO-NAZAN" });
            }

            perfil = _db.perfiles.FirstOrDefault(pt => pt.Nombre == perfilNombre);

            if (perfil == null)
            {
                // TODO REVISAR ESTA FUNCION 
                throw new Exception("");

            }

            return perfil;

        }



    }
}