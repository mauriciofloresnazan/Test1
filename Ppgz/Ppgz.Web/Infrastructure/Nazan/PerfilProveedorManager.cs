using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Ppgz.Repository;

namespace Ppgz.Web.Infrastructure.Nazan
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

        public perfile FindByNombre(string nombre)
        {
            return _db.perfiles
                .FirstOrDefault(p => p.Nombre == nombre && p.Tipo == Tipo);
        }

        public void Create(string nombre, string[] rolesIds)
        {
            nombre = nombre.Trim();

            var perfil = FindByNombre(nombre);
            if (perfil != null)
            {
                //TODO CARLOS CAMPOS
                throw new Exception(Areas.Nazan.Errores.PerfilNazanNombreExistente);

            }

            if (!rolesIds.Any())
                //TODO CARLOS CAMPOS
                throw new Exception(Areas.Nazan.Errores.PerfilNazanRolesRequeridos);


            var aspnetroles =
                _db.aspnetroles.Where(r => rolesIds.Contains(r.Name));

            if (!aspnetroles.Any())
                //TODO CARLOS CAMPOS
                throw new Exception(Areas.Nazan.Errores.PerfilNazanRolesRequeridos);


            perfil = new perfile {Nombre = nombre, Tipo = Tipo};

            foreach (var role in aspnetroles)
            {
                perfil.aspnetroles.Add(role);
            }


            _db.perfiles.Add(perfil);
            _db.SaveChanges();
        }

        public void Remove(int id)
        {
            var perfil = Find(id);

            if (perfil.aspnetusers.Any())
            {
                //TODO CARLOS CAMPOS
                throw new Exception(Areas.Nazan.Errores.PerfilNazanRolesEliminarConUsuarios);
            }
            perfil.aspnetroles.Clear();
            _db.perfiles.Remove(perfil);
            _db.SaveChanges();
        }


        public void Update(int id, string nombre, string[] rolesIds)
        {
            var perfil = FindByNombre(nombre);
            if (perfil != null)
            {
                if (perfil.Id != id)
                    //TODO CARLOS CAMPOS
                    throw new Exception(Areas.Nazan.Errores.PerfilNazanNombreExistente);

            }

            if (!rolesIds.Any())
                //TODO CARLOS CAMPOS
                throw new Exception(Areas.Nazan.Errores.PerfilNazanRolesRequeridos);


            var aspnetroles =
                _db.aspnetroles.Where(r => r.Tipo == Tipo && rolesIds.Contains(r.Name));

            if (!aspnetroles.Any())
                //TODO CARLOS CAMPOS
                throw new Exception(Areas.Nazan.Errores.PerfilNazanRolesRequeridos);

            perfil = Find(id);
            perfil.Nombre = nombre;
            perfil.aspnetroles.Clear();
            foreach (var role in aspnetroles)
            {
                perfil.aspnetroles.Add(role);
            }

            _db.Entry(perfil).State = EntityState.Modified;
            _db.SaveChanges();
        }


        public List<aspnetrole> GetRoles()
        {
            return _db.aspnetroles
                .Where(r => r.Tipo == Tipo).ToList();

        }

        public perfile GetMaestroByUsuarioTipo(CuentaManager.Tipo tipo)
        {
            var perfilNombre = string.Empty;

            switch (tipo)
            {
                case CuentaManager.Tipo.MERCADERIA:
                    perfilNombre = "MAESTRO-MERCADERIA";
                    break;
                case CuentaManager.Tipo.SERVICIO:
                    perfilNombre = "MAESTRO-SERVICIO";
                    break;
            }



            if (string.IsNullOrWhiteSpace(perfilNombre))
            {
                // TODO CARLOS CAMPOS
                throw new NotImplementedException("");
            }

            var perfil = _db.perfiles.FirstOrDefault(pt => pt.Nombre == perfilNombre);

            if (perfil == null)
            {
                // TODO PASAR A UN MANEJADOR DE ROLES
                if (_db.aspnetroles
                    .FirstOrDefault(ro => ro.Name == perfilNombre) == null)
                {
                    var role = new aspnetrole()
                    {
                        Id = perfilNombre,
                        Name = perfilNombre,
                        Description = "PERFIL MAESTR tiene acceso a toda la aplicación.",
                        Tipo =  CuentaManager.GetTipoString(tipo)
                      

                    };

                    _db.aspnetroles.Add(role);
                    _db.SaveChanges();

                }

                var perfilProveedorManager = new PerfilProveedorManager();
                perfilProveedorManager.Create(perfilNombre, new[] {perfilNombre});
            }

            perfil = _db.perfiles.FirstOrDefault(pt => pt.Nombre == perfilNombre);

            if (perfil == null)
            {

                //todo carlos campos
                throw new Exception("");

            }

            return perfil;

        }
    }
}