
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ppgz.Web.Models.ProntoPago
{
	public class FacturaView
	{
		public string referencia { get; set; }
		public double importe { get; set; }
		public string ml { get; set; }
		public string vencimiento { get; set; }
		public string tipoMovimiento { get; set; }
		public string fechaDocumento { get; set; }
		public string descripcion { get; set; }
		public string numeroDocumento { get; set; }
		public string idProveedor { get; set; }
		public bool pagar { get; set; }
		public int porcentaje { get; set; }
		public List<string> facturas { get; set; }
        public double interes { get; set; }
		
	}
}