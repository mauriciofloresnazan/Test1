using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Ppgz.Web.Models
{
    public class CuentaViewModel
    {
  
        [Required]
        [Display(Name = "Nombre de Usuario (Login)")]
        public string UserName { get; set; }
        [Required]
        [Display(Name = "Nombre de Proveedor")]
        public string ProveedorNombre { get; set; }
        [Required]
        [Display(Name = "Nombre")]
        public string ResponsableNombre { get; set; }
        [Required]
        [Display(Name = "Apellido")]
        public string ResponsableApellido { get; set; }
        [Required]
        [Display(Name = "Cargo")]
        public string ResponsableCargo { get; set; }
        [Required]
        [Display(Name = "Email")]
        public string ResponsableEmail { get; set; }
        [Required]
        [Display(Name = "Telefono")]
        public string ResponsableTelefono { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "El Password debe tener al menos 6 caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string ResponsablePassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("ResponsablePassword", ErrorMessage = "El password y la confirmación no coinciden.")]
        public string ResponsableConfirmarPassword { get; set; }
    }
}