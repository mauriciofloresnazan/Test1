using System;

namespace Ppgz.Services
{
	//Descomentar en caso de utilizar estas clases de excepción. De lo contrario use BusinessException.
	/*/// <summary> Excepción de lógica de negocio del módulo de Citas.</summary>
	public class CitasException : Exception
	{
		/// <summary> Mensaje de negocio para el módulo de Citas. </summary>
		/// <param name="mensaje">Contiene el mensaje a mostrar/almacenar.</param>
		public CitasException(string mensaje) : base(mensaje)
		{
		}
	}

	/// <summary> Excepción de lógica de negocio del módulo de Nazan.</summary>
	public class NazanException : Exception
	{
		/// <summary> Mensaje de negocio para el módulo de Nazan. </summary>
		/// <param name="mensaje">Contiene el mensaje a mostrar/almacenar.</param>
		public NazanException(string mensaje) : base(mensaje)
		{
		}
	}

	/// <summary> Excepción de lógica de negocio para el módulo de Proveedor.</summary>
	public class ProveedorException : Exception
	{
		/// <summary> Mensaje de negocio para el módulo de Proveedor. </summary>
		/// <param name="mensaje">Contiene el mensaje a mostrar/almacenar.</param>
		public ProveedorException(string mensaje) : base(mensaje)
		{
		}
	}

	/// <summary> Excepción de lógica de negocio del módulo de Servicio.</summary>
	public class ServicioException : Exception
	{
		/// <summary> Mensajes de negocio para el módulo de Servicio. </summary>
		/// <param name="mensaje">Contiene el mensaje a mostrar/almacenar.</param>
		public ServicioException(string mensaje) : base(mensaje)
		{
		}
	}*/

	/// <summary>
	/// Excepción específica para el manejo de errores de negocio. 
	/// </summary>
	public class BusinessException : Exception
	{
		public BusinessException(string mensaje) : base(mensaje)
		{

		}
	}
}