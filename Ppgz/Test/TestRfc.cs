using System;
using System.Data;
using Newtonsoft.Json;
using SapWrapper;

namespace Test
{
    public class TestRfc
    {


        public void TestOrdenesDeCompraHeader(string nuemroProveedor)
        {
            var ordenCompraManager = new SapOrdenCompraManager();

            Console.ReadLine();
        }

        public void TestConsultarDetalleDeOrdenCompra(string documento)
        {/*
            var ordenCompraManager = new SapOrdenCompraManager();
            var resultDt = ordenCompraManager.GetOrdenDeCompraDetalle(documento);
            Console.WriteLine(JsonConvert.SerializeObject(resultDt));
            Console.ReadLine();
            */

        }
        public void TestProveedores()
        {       


            var sapProveedores = new SapProveedorManager();
            //var resultDt = sapProveedores.GetProveedores();
            //Console.WriteLine(JsonConvert.SerializeObject(resultDt));
            //Console.ReadLine();


             sapProveedores = new SapProveedorManager();
             var resultDt = sapProveedores.GetProveedor("0000001726");
            Console.WriteLine(JsonConvert.SerializeObject(resultDt));
            Console.ReadLine();

        }
        public void TestOrdenesDeCompra()
        {/*
            var ordenCompraManager = new SapOrdenCompraManager();
            var resultDt = ordenCompraManager.GetOrdenDeCompraDetalle("0000001725");
            Console.WriteLine(JsonConvert.SerializeObject(resultDt));
            Console.ReadLine();
            */
        }

        public void BuscarCodigosProveedores()
        {/*
            var proveedorManager = new SapProveedorManager();
            var resultDt = proveedorManager.GetProveedores();

            foreach (DataRow dr in resultDt.Rows)
            {
                Console.WriteLine(dr["LIFNR"]);
                
            }
            //Console.WriteLine(JsonConvert.SerializeObject(resultDt));
            Console.ReadLine();
        */
        }

    }
}
