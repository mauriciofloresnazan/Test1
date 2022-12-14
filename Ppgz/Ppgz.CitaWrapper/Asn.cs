using System.ComponentModel.DataAnnotations;

namespace Ppgz.CitaWrapper
{
    public class Asn
    {
        /// <summary>Identificador único en el documento de orden de compra.</summary>
        [Required]
        [StringLength(10, MinimumLength = 1, ErrorMessage = @"Rango permitido es [1-30] caracteres.")]
        public string NumeroPosicion;

        /// <summary>Número de orden.</summary>
        [Required]
        [StringLength(30, MinimumLength = 10, ErrorMessage = @"Rango permitido es [10-30] caracteres.")]
        public string OrdenNumeroDocumento;

        /// <summary>Código del material.</summary>
        [Required]
        [StringLength(10, MinimumLength = 10, ErrorMessage = @"Rango permitido es [10-10] caracteres.")]
        public string NumeroMaterial;

        /// <summary>Nombre del material asociado.</summary>
        [Required]
        [StringLength(50, MinimumLength = 10, ErrorMessage = @"Rango permitido es [10-50] caracteres.")]
        public string NombreMaterial;

        /// <summary>Catidad de dicho material.</summary>
        [Required]
        [RegularExpression(@"([0-9]+)", ErrorMessage = @"Sólo se admiten valores números enteros")]
        public int Cantidad;

        /// <summary>Tienda destino. Es opcional</summary>
        public string Tienda;

        /// <summary>Tienda origen. Es opcional</summary>
        public string TiendaOrigen;
        
        /// <summary>Precio. </summary>
        public decimal Precio;

        /// <summary>Precio. </summary>
        public string UnidadMedida;

        /// <summary>Cantidad Solicitada en Sap. </summary>
        public int CantidadSolicitada;

        /// <summary>InOut. </summary>
        public string InOut;
   
        /// <summary>Numero de Surtido. </summary>
        public string NumeroSurtido;


        /// <summary>Numero de Material2 Usado par Scale. </summary>
        public string NumeroMaterial2;

        /// <summary>Centro que viene del ERP. </summary>
        public string Centro;
    }
}
