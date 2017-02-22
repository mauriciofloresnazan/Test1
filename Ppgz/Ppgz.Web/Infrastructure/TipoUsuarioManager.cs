using System.Collections.Generic;
using System.Linq;
using Ppgz.Web;

namespace Ppgz.Services
{
    public class TipoUsuarioManager
    {
        private readonly Entities _db = new Entities();
        public List<tipos_usuario> FindAll()
        {
            return _db.tipos_usuario.ToList();

        }

        // TODO VALIDAR LOST CODIGOS DE TIPOS DE USUARIO
        public tipos_usuario GetNazan()
        {
            
            var tiposUsuario = FindAll();

            // TODO VALIDAR LOST CODIGOS DE TIPOS DE USUARIO
            return _db.tipos_usuario.First(t => t.codigo == "NAZAN");
            
        }

        // TODO VALIDAR LOST CODIGOS DE TIPOS DE USUARIO
        public tipos_usuario GetProveedor()
        {

            var tiposUsuario = FindAll();

            // TODO VALIDAR LOST CODIGOS DE TIPOS DE USUARIO
            return _db.tipos_usuario.First(t => t.codigo == "PROVEEDOR");

        }
    }
}
