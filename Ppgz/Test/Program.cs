using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ppgz.Repository;

namespace Test
{
    class Program
    {
        static void CrearSuperAdmin()
        {
            var db = new PpgzEntities();

            usuario usuario = new usuario
            {
                activo = true,
                telefono = "5555555",
                perfil_id = 1,
                PasswordHash = "AAsBMosd+ucHSrjSp4bIgmVT2BByakZ/8x90GYXWeaSk75MaLfX04CDZRuqJzUQ3RA==",
                userName = "superadmin",
                SecurityStamp = "",
                tipo_usuario_id = 1,
                nombre = "Super Admin",
                apellido = "Nazan",
                email = "test@test.com",
                cargo = "ADMINSITRADOR"

            };

            db.usuarios.Add(usuario);
            db.SaveChanges();
        }
        static void Main(string[] args)
        {
           CrearSuperAdmin();

        }
    }
}
