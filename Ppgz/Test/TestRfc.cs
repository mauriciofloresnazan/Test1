using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SapWrapper;

namespace Test
{
    public class TestRfc
    {
        public void TestProveedores()
        {
            var sapProveedores = new SapProveedores();
            var resultDt = sapProveedores.GetProveedores();
            Console.WriteLine(JsonConvert.SerializeObject(resultDt));
            Console.ReadLine();

        }
    }
}
