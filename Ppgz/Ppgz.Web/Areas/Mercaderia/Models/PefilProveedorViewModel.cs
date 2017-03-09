using System.Web.Mvc;
using Ppgz.Web.Areas.Nazan.Models;
using Ppgz.Web.Infrastructure.Nazan;

namespace Ppgz.Web.Areas.Mercaderia.Models
{
    public class PefilProveedorViewModel : PefilNazanViewModel
    {
        public PefilProveedorViewModel()
        {
            var perfilProveedorManager = new PerfilProveedorManager();

            var roles = perfilProveedorManager.GetRoles("MERCADERIA"); ;

            Roles = new MultiSelectList(roles, "Id", "Description");
        }
    }
}