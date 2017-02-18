using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Ppgz.Repository;

namespace Ppgz.Services
{
    public class MensajesManager
    {
        private readonly PpgzEntities _db = new PpgzEntities();
        public void Add(mensaje mensaje)
        {
            _db.mensajes.Add(mensaje);

            _db.SaveChanges();
        }

        public List<mensaje> FindAll()
        {
            return _db.mensajes.ToList();
        }

        public mensaje FindById(int id)
        {
            return _db.mensajes.Single(i => i.id == id);
        }

        public void Update(int id, mensaje valores)
        {
            var mensaje = FindById(id);

            mensaje.titulo = valores.titulo;
            mensaje.fecha_publicacion = valores.fecha_publicacion;
            mensaje.fecha_caducidad = valores.fecha_caducidad;
            mensaje.archivo = valores.archivo;
            mensaje.enviado_a = valores.enviado_a;

            _db.Entry(mensaje).State = EntityState.Modified;

            _db.SaveChanges();
        }

        public void Remove(int id)
        {
            var message = FindById(id);

            _db.mensajes.Remove(message);
            
            _db.SaveChanges();
        }
    }
}
