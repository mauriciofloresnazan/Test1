using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Ppgz.Repository;
using Ppgz.Services;

namespace Ppgz.Web.Infraestructure
{
    public class UsuarioManager
    {
        private readonly PpgzEntities _db = new PpgzEntities();
        public void Add(usuario usuario, string password, string permiso = "")
        {

            // Encriptacion del password
            var store = new UserStore<IdentityUser>(_db);
            var userManager = new UserManager<IdentityUser>(store);
            var passwordHash = userManager.PasswordHasher.HashPassword(password);
            usuario.PasswordHash = passwordHash;

            // Normalizacion del Nombre de usuario y email
            usuario.userName = usuario.userName.ToLower().Trim();
            usuario.email = usuario.email.ToLower().Trim();

            // TODO SECURITY STAMP
            usuario.SecurityStamp = permiso;
            
            _db.usuarios.Add(usuario);
            _db.SaveChanges();
        }

        public usuario Find(int id)
        {
            return _db.usuarios.FirstOrDefault(i => i.Id == id);
        }

        public usuario FindUsuarioByUserName(string username)
        {
            return _db.usuarios.FirstOrDefault(u => u.userName == username);
        }

        public List<usuario> FindUsuariosProveedorByCuentaId(int cuentaId)
        {
            return _db.Database.SqlQuery<usuario>(@"
                SELECT  * 
                FROM    usuarios
                WHERE   id IN (SELECT usuario_id 
                               FROM usuarios_cuentas_xref 
                               WHERE cuenta_id = {0})", cuentaId).ToList();
            

            var tipoUsuarioProveedor = _db.tipos_usuario.First(t => t.codigo == "PROVEEDOR");

            var cuentaManager = new CuentaManager();

            var cuenta = cuentaManager.Find(cuentaId);

            return _db.usuarios
                        .Where(
                            u => u.tipo_usuario_id == tipoUsuarioProveedor.id
                            && u.cuentas.Contains(cuenta)).ToList();


        }


        public void UpdateUsuario(int id, string nombre, string apellido, string telefono, string email,
            string password, string permiso = "")
        {
            var commonManager =  new CommonManager();

            var usuario = _db.usuarios.Single(u => u.Id == id);

            usuario.nombre = nombre;
            usuario.apellido = apellido;
            usuario.telefono = telefono;
            usuario.email = email;
            usuario.PasswordHash = commonManager.HashPassword(password);
            // TODO SECURITY STAMP
            usuario.SecurityStamp = permiso;
            _db.Entry(usuario).State = EntityState.Modified;

            _db.SaveChanges();
        }

        public void Remove(int id)
        {
            var usuario = _db.usuarios.Single(u => u.Id == id);
            _db.usuarios.Remove(usuario);

            var xrefs = _db.usuarios_cuentas_xref.Where(a => a.usuario_id== id);

            foreach (var xref in xrefs)
            {
                _db.usuarios_cuentas_xref.Remove(xref);
            }

            _db.SaveChanges();
        }
    }
}
