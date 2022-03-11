using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ppgz.Web.Models.ProntoPago
{
	public class NotaCreditoView
	{
		public int proveedorId { get; set; }
		public int idSolicitudesFactoraje { get; set; }
		public HttpPostedFileBase notaCreditoPdf { get; set; }
		public HttpPostedFileBase notaCreditoXml { get; set; }
	}
}