using Microsoft.AspNet.Identity;

namespace Ppgz.Web.Models
{
    public class ApplicationUser : IUser
    {
        public string Id { get; set; }
        public string UserName { get; set; }
    }

}