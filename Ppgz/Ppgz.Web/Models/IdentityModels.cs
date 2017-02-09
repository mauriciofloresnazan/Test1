using Microsoft.AspNet.Identity.EntityFramework;

namespace Ppgz.Web.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public string EmpresaRfc { get; set; }

        public string EmpresaRazonSocial { get; set; }

        public string ResponsableNombre { get; set; }
        
        public string ResponsableApellido { get; set; }
        
        public string ResponsableEmail { get; set; }

        public string ResponsableTelefono { get; set; }


    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
        }
    }
}