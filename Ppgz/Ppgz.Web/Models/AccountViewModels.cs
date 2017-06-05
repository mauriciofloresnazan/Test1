using System.ComponentModel.DataAnnotations;
using System.Web.ModelBinding;

namespace Ppgz.Web.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required(ErrorMessage = "El campo es obligatorio.")]
        [Display(Name = "Usuario")]
        public string UserName { get; set; }
    }

    public class ManageUserViewModel
    {
        [Required(ErrorMessage = "El campo es obligatorio.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio.")]
        [StringLength(100, ErrorMessage = "El número de caracteres de {0} deben ser al menos {2}.", MinimumLength = 6)]       
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nueva contraseña")]
        [System.Web.Mvc.Compare("NewPassword", ErrorMessage = "La nueva contraseña y confirmación no coinciden.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "El campo es obligatorio.")]
        [Display(Name = "Usuario")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Display(Name = "Recordarme?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El campo es obligatorio.")]
        [Display(Name = "Usuario")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio.")]
        [StringLength(100, ErrorMessage = "El número de caracteres de {0} deben ser al menos {2}.", MinimumLength = 6)]
        [RegularExpression(
            "^[a-zA-Z0-9$&+,:;=?@#|'<>.^*()%!-]+$",
           ErrorMessage = "No Debe contener espacios, solo letras, números o caracteres especiales, sin importar minusculas y mayusculas.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [DataType(DataType.Password)]      
        [Display(Name = "Confirmar Contraseña")]
        [System.Web.Mvc.Compare("Password", ErrorMessage = "Contraseña y confirmación no coinciden.")]
        public string ConfirmPassword { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "El campo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Debe colocar un correo electrónico válido")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "El campo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Debe colocar un correo electrónico válido")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio.")]
        [StringLength(100, ErrorMessage = "El número de caracteres de {0} deben ser al menos {2}.", MinimumLength = 6)]
        [RegularExpression(
            "^[a-zA-Z0-9$&+,:;=?@#|'<>.^*()%!-]+$",
            ErrorMessage = "No Debe contener espacios, solo letras, números o caracteres especiales, sin importar minusculas y mayusculas.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [System.Web.Mvc.Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }



    public class CambiarPasswordViewModel
    {

        [Required(ErrorMessage = "El campo es obligatorio.")]
        [StringLength(100, ErrorMessage = "El número de caracteres de {0} deben ser al menos {2}.", MinimumLength = 6)]
        [RegularExpression(
            "^[a-zA-Z0-9$&+,:;=?@#|'<>.^*()%!-]+$",
            ErrorMessage = "No Debe contener espacios, solo letras, números o caracteres especiales, sin importar minusculas y mayusculas.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [System.Web.Mvc.Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
        public string ConfirmPassword { get; set; }
   
    }
}
