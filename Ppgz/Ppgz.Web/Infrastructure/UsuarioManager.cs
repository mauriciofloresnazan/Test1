using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Ppgz.Repository;

namespace Ppgz.Web.Infrastructure
{
    public class UsuarioManager
    {
        private readonly Entities _db = new Entities();
        public void Update(string id, string nombre, string apellido, string cargo,string email,string telefono, string tipo, string password = null )
        {
            var usuario = _db.aspnetusers.Find(id);

            usuario.Nombre = nombre;
            usuario.Apellido = apellido;
            usuario.Cargo = cargo;
            usuario.Tipo = tipo;

            if (!string.IsNullOrWhiteSpace(email))
            {
                usuario.Email = email.ToLower().Trim();
                
            }

            if (!string.IsNullOrWhiteSpace(telefono))
            {
                usuario.PhoneNumber = telefono.ToLower().Trim();

            }

            if (!string.IsNullOrWhiteSpace(password))
            {

            var commonManager =  new CommonManager();
                usuario.PasswordHash = commonManager.HashPassword(password);
            }

            _db.Entry(usuario).State = EntityState.Modified;
            _db.SaveChanges();
        }

        public aspnetuser Find(string id)
        {
            return _db.aspnetusers.FirstOrDefault(i => i.Id == id);
        }

        public aspnetuser FindUsuarioByUserName(string username)
        {
            return _db.aspnetusers.FirstOrDefault(u => u.UserName == username);
        }

        public List<aspnetuser> FindUsuariosProveedorByCuentaId(int cuentaId)
        {
            return _db.Database.SqlQuery<aspnetuser>(@"
                SELECT  * 
                FROM    aspnetusers
                WHERE   id IN (SELECT usuario_id 
                               FROM usuarios_cuentas_xref 
                               WHERE cuenta_id = {0})", cuentaId).ToList();
            
/*
            var tipoUsuarioProveedor = _db.tipos_usuario.First(t => t.codigo == "PROVEEDOR");

            var cuentaManager = new CuentaManager();

            var cuenta = cuentaManager.Find(cuentaId);

            return _db.usuarios
                        .Where(
                            u => u.tipo_usuario_id == tipoUsuarioProveedor.id
                            && u.cuentas.Contains(cuenta)).ToList();
            */

        }


        public void UpdateUsuario(string id, string nombre, string apellido, string telefono, string email,
            string password, string permiso = "")
        {
            // TODO INCLUIR EN EL PRIMERO??

            var commonManager =  new CommonManager();

            var usuario = _db.aspnetusers.Single(u => u.Id == id);

            usuario.Nombre = nombre;
            usuario.Apellido = apellido;
            usuario.PhoneNumber = telefono;
            usuario.Email = email;
            usuario.PasswordHash = commonManager.HashPassword(password);
            // TODO SECURITY STAMP
            usuario.SecurityStamp = permiso;
            _db.Entry(usuario).State = EntityState.Modified;

            _db.SaveChanges();
        }

        public void Remove(string id)
        {
            var usuario = _db.aspnetusers.Single(u => u.Id == id);
            _db.aspnetusers.Remove(usuario);

            usuario.cuentas.Clear();
            

            _db.SaveChanges();
        }
    }
}
