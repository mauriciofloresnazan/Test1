using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Ppgz.Repository;
using Ppgz.Web.Areas.Nazan;
using Ppgz.Web.Models;

namespace Ppgz.Web.Infrastructure.Nazan
{
    public class UsuarioNazanManager
    {
        private readonly UserManager<ApplicationUser> _applicationUserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
       
        private readonly Entities _db = new Entities();
       
        private readonly PerfilNazanManager _perfilNazanManager = new PerfilNazanManager();

        public static string[] Tipos
        {
            get
            {
                return new[]
                {
                    "NAZAN-MAESTRO",
                    "NAZAN"
                };
            }
        }

        public List<aspnetuser> FindAll()
        {
            return _db.aspnetusers.Where(
                u => Tipos.Contains(u.Tipo)).ToList();
        }

        public aspnetuser Find(string id)
        {
            return _db.aspnetusers
                .FirstOrDefault(u => u.Id == id && Tipos.Contains(u.Tipo));
        }

        public void Create(string userName, string nombre, string apellido, string email,
            string password, int perfilId)
        {
            if (_applicationUserManager.FindByName(userName) != null)
            {
                // TODO CARLOS Y JUAN DELGADO
                throw new Exception(Errores.UsuarioNazanLoginExistente);
            }

            var perfil = _perfilNazanManager.Find(perfilId);

            if (perfil == null)
            {
                throw new Exception(Errores.UsuarioNazanPerfilIncorrecto);
            }

            var usuario = new ApplicationUser()
            {
                UserName = userName,
                Nombre = nombre,
                Apellido = apellido,
                Email = email.Trim(),
                Tipo = TipoUsuario.Nazan,
                Activo = true,
                PerfilId = perfil.Id
            };

            var result = _applicationUserManager.Create(usuario, password);

            if (!result.Succeeded)
            {
                throw new Exception(ResourceErrores.RegistroGeneral);
            }
   

            foreach (var role in perfil.aspnetroles)
            {
                _applicationUserManager.AddToRole(usuario.Id, role.Id);
            }

  

        }

        public void Remove(string id)
        {
            var usuario = _db.aspnetusers
                .Single(u => u.Id == id);

            _db.aspnetusers.Remove(usuario);
            _db.SaveChanges();
        }


        public void Update(string id, string nombre, string apellido, string email, int perfilId, string password = null)
        {
            var usuario = Find(id);

            if (usuario == null)
            {
                throw new Exception(Errores.UsuarioNazanIncorrecto);
            }

            var perfil = _perfilNazanManager.Find(perfilId);

            if (perfil == null)
            {
                throw new Exception(Errores.UsuarioNazanPerfilIncorrecto);
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
            usuario.aspnetroles.Clear();

            _db.Entry(usuario).State = EntityState.Modified;
            _db.SaveChanges();

     
            foreach (var role in perfil.aspnetroles)
            {
                _applicationUserManager.AddToRole(usuario.Id, role.Id);
            }
        }


    }
}