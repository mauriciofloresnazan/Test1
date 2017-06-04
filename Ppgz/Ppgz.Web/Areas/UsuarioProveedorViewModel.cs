using System.ComponentModel.DataAnnotations;

namespace Ppgz.Web.Areas
{
    public class UsuarioProveedorViewModel
    {
        [Required(ErrorMessage = "El campo es obligatorio.")]
        [RegularExpression(
            "^[a-zA-Z0-9][a-zA-Z0-9_]{2,15}$",
            ErrorMessage = "Debe iniciar con una letra o número, puede contener guión bajo. " +
                           "Debe tener de 3 a 15 caracteres.")]
        [Display(Name = "Login")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio.")]
        [Display(Name = "Perfil")]
        public int Perfil { get; set; }
        
        [Required(ErrorMessage = "El campo es obligatorio.")]
        [DataType(DataType.Text)]
        [StringLength(40, ErrorMessage = "Debe tener de 3 a 40 Carácteres.", MinimumLength = 3)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }



        [Required(ErrorMessage = "El campo es obligatorio.")]
        [DataType(DataType.Text)]
        [Display(Name = "Apellido")]
        [StringLength(40, ErrorMessage = "Debe tener de 3 a 40 Carácteres.", MinimumLength = 3)]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio.")]
        [Phone(ErrorMessage = "Debe ingresar un número de teléfono valido.")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "Debe ingresar un número de teléfono valido.")]
        [Display(Name = "Teléfono")]
        [StringLength(18, ErrorMessage = "Debe tener formato +52 (ddd) ddd-dddd", MinimumLength = 18)]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Debe ingresar un email valído.")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Debe ingresar un email valído.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio.")]
        [StringLength(100, ErrorMessage = "El Password debe tener al menos 6 caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [System.Web.Mvc.Compare("Password", ErrorMessage = "El password y la confirmación no coinciden.")]
        public string ConfirmarPassword { get; set; }

    }
}