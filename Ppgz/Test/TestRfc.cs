using System;
using Newtonsoft.Json;
using SapWrapper;

namespace Test
{
    public class TestRfc
    {
        public void TestProveedores()
        {
            var sapProveedores = new SapProveedores();
            var resultDt = sapProveedores.GetProveedor("CD06");
            Console.WriteLine(JsonConvert.SerializeObject(resultDt));
            Console.ReadLine();

        }
    }
}
