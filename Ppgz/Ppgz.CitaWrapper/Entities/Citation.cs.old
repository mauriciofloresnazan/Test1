using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ppgz.CitaWrapper.Entities
{
	/// <summary>Clase entidad que contiene la estructura de la cita.</summary>
	public class Citation
	{
		/// <summary>Fecha de la cita.</summary>
		[Required]
		public string fechaCita;

		/// <summary>Nombre de la tienda.</summary>
		[Required]
		[StringLength(5, MinimumLength = 5, ErrorMessage = @"Sólo se admiten hasta 5 caracteres.")]
		public string tienda;

		/// <summary>Cantidad total de items.</summary>
		[Required]
		[RegularExpression(@"([0-9]+)", ErrorMessage = @"Sólo se admiten valores números enteros.")]
		public int cantidadTotal;

		/// <summary>Identificador del proveedor.</summary>
		[Required]
		[RegularExpression(@"([0-9]+)", ErrorMessage = @"Sólo se admiten valores números enteros.")]
		public int proveedorId;

		/// <summary>Cantidad total de items.</summary>
		[Required]
		[StringLength(128, MinimumLength = 128, ErrorMessage = @"Sólo se admiten hasta 128 caracteres.")]
		public string usuarioId;

		/// <summary>Listado de los asn asociados a la cita.</summary>
		[Required]
		public List<Asn> asnItems;
	}

	/// <summary>Item asociados a la cita.</summary>
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
	}
}
