using SatWrapper.CFDIService;
using SatWrapper.Properties;
using System;
using System.IO;
using System.Xml.Serialization;

namespace SatWrapper
{
	/// <summary>Clase estática que realiza la consulta con el Servicio del SAT, en base a un archivo XML enviado. </summary>
	public static class CfdiServiceConsulta
	{
		/// <summary> Realiza la consulta de la factura o comprobante a partir de un archivo XML.</summary>
		/// <param name="fileXmlPath">Ruta del archivo XML.</param>
		
		public static bool Consulta(string fileXmlPath)
		{
			GC.GetTotalMemory(true);
			ConsultaCFDIServiceClient service = null;


			try
			{
				service = new ConsultaCFDIServiceClient();

			    var serializer = new XmlSerializer(typeof(Comprobante));
                var archivoXml = new FileStream(fileXmlPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				var comprobante = (Comprobante)serializer.Deserialize(archivoXml);
				var acuse = service.Consulta(
                    string.Format(
                        Settings.Default.QueryString, 
                        comprobante.Emisor.Rfc, 
                        comprobante.Receptor.Rfc, 
                        comprobante.Total, 
                        comprobante.Complemento.TimbreFiscalDigital.UUID));

                if (acuse.CodigoEstatus == "S - Comprobante obtenido satisfactoriamente." && acuse.Estado == "Vigente")
                {
                    return true;
			    }
			}
			catch (InvalidOperationException)
			{
				if (service != null)
					service.Abort();
			}
			catch (Exception)
			{
				if (service != null)
					service.Abort();
			}
			finally
			{
			    if (service != null) service.Close();
			    GC.GetTotalMemory(true);
			}
		    return false;
		}
	}
}
