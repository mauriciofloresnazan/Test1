using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Ppgz.Repository;

namespace Ppgz.Web.Infrastructure.Nazan
{
    public class UsuarioManager
    {
        private readonly Entities _db = new Entities();
        
        public static string[] Tipos
        {
            get 
            {
                return  new []
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
            return _db.aspnetusers.Find(id);

        }
        public void Remove(string id)
        {
            var usuario = _db.aspnetusers
                .Single(u => u.Id == id);

             _db.aspnetusers.Remove(usuario);
            _db.SaveChanges();
        }


        public void Update(string id, string nombre, string apellido, string email, string password = null)
        {
            var usuario = _db.aspnetusers
                .Single(u => u.Id == id);

            usuario.Nombre = nombre;
            usuario.Apellido= apellido;
           
            if (!string.IsNullOrWhiteSpace(usuario.Email = email))
            {
                usuario.Email = email.ToLower();
            }
            
            if (!string.IsNullOrWhiteSpace(password))
            {
                var commonManager =  new CommonManager();
                usuario.PasswordHash = commonManager.HashPassword(password);
            }

            _db.Entry(usuario).State = EntityState.Modified;
            _db.SaveChanges();
        }
    }
}