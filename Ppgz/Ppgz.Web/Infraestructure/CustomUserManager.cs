using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Ppgz.Repository;
using Ppgz.Web.Models;

namespace Ppgz.Web.Infraestructure
{
    public class CustomUserManager : UserManager<ApplicationUser>
    {
        private readonly PpgzEntities _db = new PpgzEntities();

        public CustomUserManager()
            : base(new CustomUserSore<ApplicationUser>())
        {

        }

        public override Task<ApplicationUser> FindAsync(string id, string userName)
        {
            var taskInvoke = Task<ApplicationUser>.Factory.StartNew(() => new ApplicationUser { Id = id, UserName = userName });

            return taskInvoke;
        }
    }
}