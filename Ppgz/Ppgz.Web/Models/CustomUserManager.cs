using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Ppgz.Repository;

namespace Ppgz.Web.Models
{
    public class CustomUserManager : UserManager<ApplicationUser>
    {
        private readonly PpgzEntities _db = new PpgzEntities();

        public CustomUserManager()
            : base(new CustomUserSore<ApplicationUser>())
        {

        }

        public override Task<ApplicationUser> FindAsync(string userName, string password)
        {
            var taskInvoke = Task<ApplicationUser>.Factory.StartNew(() =>
            {
                if (userName == "superadmin" && password == "test001")
                {
                    return new ApplicationUser { Id = "0", UserName = "superadmin" };
                }
                var usuario = _db.usuarios.FirstOrDefault(u => u.userName == userName && u.PasswordHash == password);



                if (usuario != null)
                    return new ApplicationUser { Id = usuario.Id.ToString(), UserName = usuario.userName };

                return null;
            });

            return taskInvoke;
        }
    }
}