using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace TestServiceWCF.TestEntitites
{
	/// <summary>Clase entidad que contiene la estructura de la cita.</summary>
	/// <summary>Clase entidad que contiene la estructura de la cita.</summary>
	[Serializable]
	[KnownType(typeof(Citation))]
	[DataContract(Name = @"Citation", Namespace = @"http://localhost/CitationControlService", IsReference = false)]
	public class Citation
	{
		/// <summary>Fecha de la cita.</summary>
		[DataMember(Name = @"fechaCita", IsRequired = true, Order = 0)]
		public string fechaCita;

		/// <summary>Nombre de la tienda.</summary>
		[DataMember(Name = @"tienda", IsRequired = true, Order = 1)]
		[StringLength(5, MinimumLength = 5, ErrorMessage = @"Sólo se admiten hasta 5 caracteres.")]
		public string tienda;

		/// <summary>Cantidad total de items.</summary>
		[DataMember(Name = @"cantidadTotal", IsRequired = true, Order = 2)]
		public int cantidadTotal;

		/// <summary>Identificador del proveedor.</summary>
		[DataMember(Name = @"proveedorId", IsRequired = true, Order = 3)]
		public int proveedorId;

		/// <summary>Cantidad total de items.</summary>
		[DataMember(Name = @"usuarioId", IsRequired = true, Order = 4)]
		[StringLength(128, MinimumLength = 128, ErrorMessage = @"Sólo se admiten hasta 128 caracteres.")]
		public string usuarioId;

		/// <summary>Listado de los asn asociados a la cita.</summary>
		[DataMember(Name = @"asnItems", IsRequired = true, Order = 5)]
		public List<Asn> asnItems;
	}

	/// <summary>Item asociados a la cita.</summary>
	[Serializable]
	[KnownType(typeof(Asn))]
	[DataContract(Name = @"Item", Namespace = @"http://localhost/CitationControlService", IsReference = false)]
	public class Asn
	{
		/// <summary>Identificador único en el documento de orden de compra.</summary>
		[DataMember(Name = @"numeroPosicion", IsRequired = true, Order = 0)]
		[StringLength(10, MinimumLength = 1, ErrorMessage = @"Rango permitido es [1-30] caracteres.")]
		public string numeroPosicion;

		/// <summary>Número de orden.</summary>
		[DataMember(Name = @"ordenNumeroDocumento", IsRequired = true, Order = 1)]
		[StringLength(30, MinimumLength = 10, ErrorMessage = @"Rango permitido es [10-30] caracteres.")]
		public string ordenNumeroDocumento;

		/// <summary>Código del material.</summary>
		[DataMember(Name = @"numeroMaterial", IsRequired = true, Order = 2)]
		[StringLength(10, MinimumLength = 10, ErrorMessage = @"Rango permitido es [10-10] caracteres.")]
		public string numeroMaterial;

		/// <summary>Nombre del material asociado.</summary>
		[DataMember(Name = @"nombreMaterial", IsRequired = true, Order = 3)]
		[StringLength(50, MinimumLength = 10, ErrorMessage = @"Rango permitido es [10-50] caracteres.")]
		public string nombreMaterial;

		/// <summary>Catidad de dicho material.</summary>
		[DataMember(Name = @"cantidad", IsRequired = true, Order = 4)]
		public int cantidad;
	}
}
