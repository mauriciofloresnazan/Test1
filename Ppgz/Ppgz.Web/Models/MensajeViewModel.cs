using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Ppgz.Repository;

namespace Ppgz.Web.Models
{
    public class MensajeViewModel
    {

        [Required]
        [Display(Name = "Tipo de Proveedor")]
        public string TipoProveedor { get; set; }
        [Required]
        [Display(Name = "Fecha de Publicación")]
        public string FechaPublicacion { get; set; }
        [Required]
        [Display(Name = "Fecha de Caducidad")]
        public string FechaCaducidad { get; set; }
        [Required]
        [Display(Name = "Título")]
        public string Titulo { get; set; }

        [Display(Name = "Contenido")]
        public string Contenido { get; set; }

        [Display(Name = "PDF")]
        public string Pdf { get; set; }

    }
}