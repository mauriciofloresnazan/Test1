using System.Web.Mvc;
using Ppgz.Web.Areas.Nazan.Models;
using Ppgz.Web.Infrastructure.Nazan;
using Ppgz.Web.Infrastructure.Proveedor;

namespace Ppgz.Web.Areas.Servicio.Models
{
    public class PefilProveedorViewModel : PefilNazanViewModel
    {
        public PefilProveedorViewModel()
        {
            var perfilProveedorManager = new PerfilProveedorManager();

            var roles = perfilProveedorManager.GetRoles("SERVICIO"); ;

            Roles = new MultiSelectList(roles, "Id", "Description");
        }
    }
}