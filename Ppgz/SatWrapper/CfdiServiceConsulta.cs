using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using log4net;
using SatWrapper.CFDIService;

namespace SatWrapper
{
	/// <summary>Clase estática que realiza la consulta con el Servicio del SAT, en base a un archivo XML enviado. </summary>
	public static class CfdiServiceConsulta
	{
        public static readonly ILog ErrorAppLog = LogManager.GetLogger(@"ErrorAppLog");
        public static bool Validar(string contenidoArchivo, string token, string password, string user, string cuenta, string rfcReceptor)
        {

            ValidadorIntegradoresPortTypeClient cient = null;

            try
            {
                GC.GetTotalMemory(true);
                cient = new ValidadorIntegradoresPortTypeClient();

                var response = cient.validarDocumentos(new[] { contenidoArchivo }, token, password, user, cuenta, rfcReceptor);

                var xDoc = new XmlDocument();
                xDoc.LoadXml(response);

                var element = xDoc.GetElementsByTagName("UUID")[0];
                Debug.Assert(element.Attributes != null, "element.Attributes != null");
                var estatus = element.Attributes["status"].Value.ToLower() == "true";

                if (estatus) return true;
                var errores = new StringBuilder();
                foreach (XmlNode error in xDoc.GetElementsByTagName("ERROR").Cast<XmlNode>().Where(error => error.Attributes != null))
                {
                    Debug.Assert(error.Attributes != null, "error.Attributes != null");
                    errores.AppendLine("Error PAC: " + error.Attributes["codigo"].Value + " - " + error.Attributes["mensaje"].Value);
                }

                throw new Exception(errores.ToString());
            }
            catch (InvalidOperationException)
            {
                if (cient != null)
                    cient.Abort();
            }
            catch (Exception ex)
            {
                ErrorAppLog.Error(string.Format(ex.Message));
                if (cient != null)
                    cient.Abort();
                //throw new Exception("No es posible realizar la validación de la factura en este momento. Por favor intente mas tarde.");
                throw new Exception(ex.Message);
            }
            finally
            {
                if (cient != null) cient.Close();
                GC.GetTotalMemory(true);
            }
            return false;
        }
    
	}
}
