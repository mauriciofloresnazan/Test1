using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Ppgz.Repository;

namespace Ppgz.Web.Infrastructure.Nazan
{
    public class PerfilManager
    {
        private readonly Entities _db = new Entities();
        
        public static string[] Tipos
        {
            get 
            {
                return  new []
                {
                    "NAZAN"
                };
            }
        }

        public List<perfile> FindAll()
        {
            return _db.perfiles.Where(
                u => Tipos.Contains(u.Tipo)).ToList();
        }

        public perfile Find(int id)
        {
            return _db.perfiles.Find(id);

        }
        public void Remove(int id)
        {
            var perfil = _db.perfiles
                .Single(u => u.Id == id);

            _db.perfiles.Remove(perfil);
            _db.SaveChanges();
        }


        public void Update(int id, string nombre)
        {
            var perfil = _db.perfiles
                .Single(u => u.Id == id);

            perfil.Nombre = nombre.Trim();


            _db.Entry(perfil).State = EntityState.Modified;
            _db.SaveChanges();
        }
    }
}