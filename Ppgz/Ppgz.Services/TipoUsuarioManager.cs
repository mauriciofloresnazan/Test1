using System.Collections.Generic;
using System.Linq;
using Ppgz.Repository;

namespace Ppgz.Services
{
    public class TipoUsuarioManager
    {
        private readonly PpgzEntities _db = new PpgzEntities();
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
