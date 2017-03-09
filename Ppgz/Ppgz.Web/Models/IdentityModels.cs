using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using Ppgz.Web.Infrastructure.MySqlIdentity;

namespace Ppgz.Web.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser
    // class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public override string Email { get; set; }
        public override string PhoneNumber { get; set; }
        public string Tipo { get; set; }
        public bool Activo { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Cargo { get; set; }

        public int PerfilId { get; set; }
        

    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        static ApplicationDbContext()
        {
            Database.SetInitializer(new MySqlInitializer());
        }

        public ApplicationDbContext()
            : base("DefaultConnection")
        {
        }
    }
}