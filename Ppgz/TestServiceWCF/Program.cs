using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestServiceWCF.TestEntitites;

namespace TestServiceWCF
{
	class Program
	{
		static void Main(string[] args)
		{
			TestCitationControlService();
		}

		/// <summary>
		/// Tests para el verdadero servicio de Citas
		/// </summary>
		private static void TestCitationControlService()
		{
			//Descomentar para probarlo mediante un clase.
			/*Citation data = new Citation
			{
				fechaCita = DateTime.Now.ToString(),
				tienda = "Carlos2",
				cantidadTotal = 500,
				proveedorId = 33,
				usuarioId = "15e814e8-0967-46e1-9a9d-fdfb7a1f2d4b",
				asnItems = new List<Asn>
				{
					new Asn
					{
						numeroPosicion = @"00004",
						ordenNumeroDocumento = @"00002",
						numeroMaterial = @"MAT09",
						nombreMaterial = @"Testing",
						cantidad = 28
					}
				}
			};*/
			//Descomentar en caso de probarlo con un string json directo.
			Citation data = Newtonsoft.Json.JsonConvert.DeserializeObject<Citation>(Settings.Default.TestJSON);
			string sUri = @"http://localhost:14766/CitationControlService.svc/rest/AddCitation";
			RestClient client = new RestClient(sUri);
			RestRequest request = new RestRequest(string.Empty, Method.POST);
			request.RequestFormat = DataFormat.Json;
			request.AddBody(data);
			RestResponse response = (RestResponse)client.Execute(request);
			Console.WriteLine(response.Content + ", " + response.StatusDescription);
			Console.ReadLine();
		}
	}
}
