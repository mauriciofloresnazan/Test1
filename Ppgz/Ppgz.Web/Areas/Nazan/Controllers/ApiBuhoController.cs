using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Ppgz.Repository;
using Ppgz.Services;
using ScaleWrapper;
using System.IO;
using System.Net;
using SapWrapper;
using System.Data;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;

namespace Ppgz.Web.Areas.Nazan.Controllers
{
    public class ApiBuhoController : Controller
    {


  
        public ActionResult ApiBuho()
        {
            var sapbuho = new SapBuho();
            var Api = "";
            var result = sapbuho.GetApi(Api);
            if (result == null)
            {
                TempData["FlashError"] = "No hay documentos";
            }
            else
            {

                foreach (DataRow dtRow in result.Rows)
                {

                    var sk = dtRow.ItemArray[0];
                    var bar = dtRow.ItemArray[2];
                    var weight = dtRow.ItemArray[7];
                    var lenght = dtRow.ItemArray[9];
                    var width = dtRow.ItemArray[10];
                    var height = dtRow.ItemArray[11];


                    System.Net.ServicePointManager.SecurityProtocol =
                    System.Net.SecurityProtocolType.Tls12;
                    var url = "http://mwbuho.wosh.com.mx/api/products-module/update";

                    var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                    httpRequest.Method = "POST";
                    httpRequest.Accept = "application/json";
                    httpRequest.ContentType = "application/json";

                    var data = @"{
""sku"":'" + sk + @"',
""barcode"": '" + bar + @"',
""weight"": '" + weight + @"',
""length"": '" + lenght + @"',
""width"": '" + width + @"',
""heigth"": '" + height + @"'
}
";
                    var cas = data.Replace("'", "");
                    var stringified = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<dynamic>(data));
                    using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                    {
                        streamWriter.Write(stringified);
                    }

                    var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var results = streamReader.ReadToEnd();
                    }

                }

            }



            return View();
        }
        
    }
}