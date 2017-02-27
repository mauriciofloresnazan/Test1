using System;
using System.Linq;
using Ppgz.Repository;

namespace Ppgz.Web.Infrastructure
{
    public class SapManager
    {
        private readonly Entities _db = new Entities();

        private static readonly string[] NumerosProveedor = {
            "001", 
            "002", 
            "003", 
            "004", 
            "005", 
            "006", 
            "007", 
            "008", 
            "009", 
            "010", 
            "011", 
            "012", 
            "013", 
            "014", 
            "015", 
            "016", 
            "017", 
            "018", 
            "019", 
            "020",
        };

        public cuentaproveedore GetProveedor(string numeroProveedor)
        {
            
            if (_db.cuentaproveedores.FirstOrDefault(
                 cp=> cp.NumeroProveedor == numeroProveedor) != null)
            {
                throw new Exception("El proveedor ya ha sido asosiado a otra cuenta.");
            }
            if (!NumerosProveedor.Contains(numeroProveedor))
            {
                throw new Exception("El Número de proveedor incorrecto.");
            }
             var ramdom = new Random();
            var proveedor = new cuentaproveedore()
            {
                NumeroProveedor = numeroProveedor,
                Ciudad = Guid.NewGuid().ToString("D"),
                CompradorImpulsEmail =
                    Guid.NewGuid().ToString("D").Substring(0, 10).ToLower() + '@'
                    + Guid.NewGuid().ToString("D").Substring(0, 4).ToLower() + ".com",
                Nombre =
                    Guid.NewGuid().ToString("D").Substring(0, 10) + " " + Guid.NewGuid().ToString("D").Substring(0, 10),
                VendedorNombre =
                    Guid.NewGuid().ToString("D").Substring(0, 10) + " " + Guid.NewGuid().ToString("D").Substring(0, 10),
                CompradorImpulsNombre =
                    Guid.NewGuid().ToString("D").Substring(0, 10) + " " + Guid.NewGuid().ToString("D").Substring(0, 10),
                Distrito =
                    Guid.NewGuid().ToString("D").Substring(0, 10) + " " + Guid.NewGuid().ToString("D").Substring(0, 3),
                CompradorImpulsTelefono = ramdom.Next(100000000, 190000000).ToString(),
                VendedorTelefono = ramdom.Next(100000000, 190000000).ToString()
            };

            return proveedor;

        }

    }
}