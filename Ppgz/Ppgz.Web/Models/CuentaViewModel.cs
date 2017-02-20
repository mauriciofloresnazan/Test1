using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Ppgz.Web.Models
{
    public class CuentaViewModel
    {

        [Required(ErrorMessage = "El campo es obligatorio.")]
        [Display(Name = "Tipo de Proveedor")]
        public string TipoProveedor { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio.")]
        [RegularExpression(
            "^[a-zA-Z0-9][a-zA-Z0-9_]{2,15}$", 
            ErrorMessage = "Debe iniciar con una letra o número, puede contener guión bajo. " +
                           "Debe tener de 3 a 15 caracteres.")]
        [Display(Name = "Nombre de Usuario (Login)")]
        public string UserName { get; set; }


        
        [Required(ErrorMessage = "El campo es obligatorio.")]
        [StringLength(100, ErrorMessage = "Debe tener de 3 a 100 Carácteres,", MinimumLength =  3)]
        [Display(Name = "Nombre de Proveedor")]
        public string ProveedorNombre { get; set; }


        [Required(ErrorMessage = "El campo es obligatorio.")]
        [DataType(DataType.Text)]
        [StringLength(40, ErrorMessage = "Debe tener de 3 a 40 Carácteres.", MinimumLength = 3)]
        [Display(Name = "Nombre")]
        public string ResponsableNombre { get; set; }


        [Required(ErrorMessage = "El campo es obligatorio.")]
        [DataType(DataType.Text)]
        [Display(Name = "Apellido")]
        [StringLength(40, ErrorMessage = "Debe tener de 3 a 40 Carácteres.", MinimumLength = 3)]
        public string ResponsableApellido { get; set; }


        [Required(ErrorMessage = "El campo es obligatorio.")]
        [DataType(DataType.Text)]
        [Display(Name = "Cargo")]
        [StringLength(40, ErrorMessage = "Debe tener de 3 a 40 Carácteres.", MinimumLength = 3)]
        public string ResponsableCargo { get; set; }


        [Required(ErrorMessage = "El campo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Debe ingresar un email valído.")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Debe ingresar un email valído.")]
        [Display(Name = "Email")]
        public string ResponsableEmail { get; set; }


        [Required(ErrorMessage = "El campo es obligatorio.")]
        [Phone(ErrorMessage = "Debe ingresar un número de teléfono valido.")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "Debe ingresar un númerop de teléfono valido.")]
        [Display(Name = "Telefono")]
        public string ResponsableTelefono { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio.")]
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