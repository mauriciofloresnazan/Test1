using System.ComponentModel.DataAnnotations;

namespace Ppgz.Web.Areas
{
    public class FacturaViewModel
    {

        [Required]
        public int ProveedorId { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio.")]
        [Display(Name = "Xml")]
        public string Xml { get; set; }

        [Required(ErrorMessage = "El campo es obligatorio.")]
        [Display(Name = "Pdf")]
        public string Pdf { get; set; }
    }
}