using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Ppgz.Web.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {


        public string tipo_usuario_id { get; set; }

        public string perfil_id { get; set; }
        

        
        public string ResponsableEmail { get; set; }

        public string ResponsableTelefono { get; set; }


    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("name=nazanEntities")
        {
        }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
 
        modelBuilder.Entity<IdentityUser>().ToTable("Custom_Users");

        modelBuilder.Entity<ApplicationUser>()
            .ToTable("Custom_Users")
            .Property(u => u.UserName)
            .IsRequired();
}
    }
}