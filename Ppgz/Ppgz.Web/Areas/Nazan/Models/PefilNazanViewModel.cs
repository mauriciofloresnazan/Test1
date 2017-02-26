using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Ppgz.Repository;
using Ppgz.Web.Infrastructure.Nazan;

namespace Ppgz.Web.Areas.Nazan.Models
{
    public class PefilNazanViewModel
    {

        [Required(ErrorMessage = "El campo es obligatorio.")]
        [DataType(DataType.Text)]
        [StringLength(40, ErrorMessage = "Debe tener de 3 a 40 Carácteres.", MinimumLength = 3)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

       [Required(ErrorMessage = "El campo es obligatorio.")]
       [Display(Name = "Permisos")]
        public string[] RolesIds { get; set; }
        public MultiSelectList Roles { get; set; }
   
    }
}