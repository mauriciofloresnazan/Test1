using System.ComponentModel.DataAnnotations;

namespace Ppgz.Web.Areas.Nazan.Models
{
    public class MensajeViewModel
    {

        [Required(ErrorMessage = "El campo es obligatorio.")]
        [Display(Name = "Tipo de Proveedor")]
        public string TipoProveedor { get; set; }
        [Required(ErrorMessage = "El campo es obligatorio.")]
        [Display(Name = "Fecha de Publicación")]
        public string FechaPublicacion { get; set; }
        [Required(ErrorMessage = "El campo es obligatorio.")]
        [Display(Name = "Fecha de Caducidad")]
        public string FechaCaducidad { get; set; }
        [Required(ErrorMessage = "El campo es obligatorio.")]
        [StringLength(100, ErrorMessage = "Debe tener de 3 a 100 Carácteres,", MinimumLength = 3)]
        [Display(Name = "Título")]
        public string Titulo { get; set; }

        [Display(Name = "Contenido")]
        public string Contenido { get; set; }

        [Display(Name = "PDF")]
        public string Pdf { get; set; }

    }
}