using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Ppgz.Web.Infrastructure
{
    public class MensajesManager
    {
        private readonly Entities _db = new Entities();
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


        public List<mensaje> GetMensajesMercaderia()
        {
            return _db.Database.SqlQuery<mensaje>(@"
                SELECT  * 
                FROM    mensajes
                WHERE   enviado_a IN ('MERCADERIA',  'TODOS' )").ToList();
            
            
        }

        public List<mensaje> GetByEnviadoA(string enviadoA)
        {

            return _db.Database.SqlQuery<mensaje>(@"
                SELECT  * 
                FROM    mensajes
                WHERE   enviado_a IN ({0},  'TODOS' )", enviadoA).ToList();
        }

        public List<usuario_mensajes> GetByUsuarioId(string usuarioId)
        {
            return _db.usuario_mensajes.Where(u => u.usuario_id == usuarioId).ToList();

  
        }

        public void Visualizar(string usuarioId, int mensajeId)
        {



            const string sql = @"
                        INSERT INTO  usuario_mensajes (usuario_id, mensaje_id)
                        VALUES ({0},{1})";
            _db.Database.ExecuteSqlCommand(sql, usuarioId, mensajeId);

            _db.SaveChanges();


        }
    }
}
