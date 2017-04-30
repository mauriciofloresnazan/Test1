using System.Collections.Generic;
using System.Xml.Serialization;

namespace SatWrapper
{
	[XmlRoot(ElementName = "DomicilioFiscal", Namespace = "http://www.sat.gob.mx/cfd/3")]
	public class DomicilioFiscal
	{
		[XmlAttribute(AttributeName = "calle")]
		public string Calle { get; set; }
		[XmlAttribute(AttributeName = "noExterior")]
		public string NoExterior { get; set; }
		[XmlAttribute(AttributeName = "noInterior")]
		public string NoInterior { get; set; }
		[XmlAttribute(AttributeName = "colonia")]
		public string Colonia { get; set; }
		[XmlAttribute(AttributeName = "municipio")]
		public string Municipio { get; set; }
		[XmlAttribute(AttributeName = "estado")]
		public string Estado { get; set; }
		[XmlAttribute(AttributeName = "pais")]
		public string Pais { get; set; }
		[XmlAttribute(AttributeName = "codigoPostal")]
		public string CodigoPostal { get; set; }
	}

	[XmlRoot(ElementName = "ExpedidoEn", Namespace = "http://www.sat.gob.mx/cfd/3")]
	public class ExpedidoEn
	{
		[XmlAttribute(AttributeName = "calle")]
		public string Calle { get; set; }
		[XmlAttribute(AttributeName = "noExterior")]
		public string NoExterior { get; set; }
		[XmlAttribute(AttributeName = "noInterior")]
		public string NoInterior { get; set; }
		[XmlAttribute(AttributeName = "colonia")]
		public string Colonia { get; set; }
		[XmlAttribute(AttributeName = "localidad")]
		public string Localidad { get; set; }
		[XmlAttribute(AttributeName = "municipio")]
		public string Municipio { get; set; }
		[XmlAttribute(AttributeName = "estado")]
		public string Estado { get; set; }
		[XmlAttribute(AttributeName = "pais")]
		public string Pais { get; set; }
		[XmlAttribute(AttributeName = "codigoPostal")]
		public string CodigoPostal { get; set; }
	}

	[XmlRoot(ElementName = "RegimenFiscal", Namespace = "http://www.sat.gob.mx/cfd/3")]
	public class RegimenFiscal
	{
		[XmlAttribute(AttributeName = "Regimen")]
		public string Regimen { get; set; }
	}

	[XmlRoot(ElementName = "Emisor", Namespace = "http://www.sat.gob.mx/cfd/3")]
	public class Emisor
	{
		[XmlElement(ElementName = "DomicilioFiscal", Namespace = "http://www.sat.gob.mx/cfd/3")]
		public DomicilioFiscal DomicilioFiscal { get; set; }
		[XmlElement(ElementName = "ExpedidoEn", Namespace = "http://www.sat.gob.mx/cfd/3")]
		public ExpedidoEn ExpedidoEn { get; set; }
		[XmlElement(ElementName = "RegimenFiscal", Namespace = "http://www.sat.gob.mx/cfd/3")]
		public RegimenFiscal RegimenFiscal { get; set; }
		[XmlAttribute(AttributeName = "rfc")]
		public string Rfc { get; set; }
		[XmlAttribute(AttributeName = "nombre")]
		public string Nombre { get; set; }
	}

	[XmlRoot(ElementName = "Domicilio", Namespace = "http://www.sat.gob.mx/cfd/3")]
	public class Domicilio
	{
		[XmlAttribute(AttributeName = "calle")]
		public string Calle { get; set; }
		[XmlAttribute(AttributeName = "noExterior")]
		public string NoExterior { get; set; }
		[XmlAttribute(AttributeName = "colonia")]
		public string Colonia { get; set; }
		[XmlAttribute(AttributeName = "localidad")]
		public string Localidad { get; set; }
		[XmlAttribute(AttributeName = "municipio")]
		public string Municipio { get; set; }
		[XmlAttribute(AttributeName = "estado")]
		public string Estado { get; set; }
		[XmlAttribute(AttributeName = "pais")]
		public string Pais { get; set; }
		[XmlAttribute(AttributeName = "codigoPostal")]
		public string CodigoPostal { get; set; }
	}

	[XmlRoot(ElementName = "Receptor", Namespace = "http://www.sat.gob.mx/cfd/3")]
	public class Receptor
	{
		[XmlElement(ElementName = "Domicilio", Namespace = "http://www.sat.gob.mx/cfd/3")]
		public Domicilio Domicilio { get; set; }
		[XmlAttribute(AttributeName = "rfc")]
		public string Rfc { get; set; }
		[XmlAttribute(AttributeName = "nombre")]
		public string Nombre { get; set; }
	}

	[XmlRoot(ElementName = "Concepto", Namespace = "http://www.sat.gob.mx/cfd/3")]
	public class Concepto
	{
		[XmlAttribute(AttributeName = "cantidad")]
		public string Cantidad { get; set; }
		[XmlAttribute(AttributeName = "unidad")]
		public string Unidad { get; set; }
		[XmlAttribute(AttributeName = "descripcion")]
		public string Descripcion { get; set; }
		[XmlAttribute(AttributeName = "valorUnitario")]
		public string ValorUnitario { get; set; }
		[XmlAttribute(AttributeName = "importe")]
		public string Importe { get; set; }
	}

	[XmlRoot(ElementName = "Conceptos", Namespace = "http://www.sat.gob.mx/cfd/3")]
	public class Conceptos
	{
		[XmlElement(ElementName = "Concepto", Namespace = "http://www.sat.gob.mx/cfd/3")]
		public List<Concepto> Concepto { get; set; }
	}

	[XmlRoot(ElementName = "Traslado", Namespace = "http://www.sat.gob.mx/cfd/3")]
	public class Traslado
	{
		[XmlAttribute(AttributeName = "impuesto")]
		public string Impuesto { get; set; }
		[XmlAttribute(AttributeName = "tasa")]
		public string Tasa { get; set; }
		[XmlAttribute(AttributeName = "importe")]
		public string Importe { get; set; }
	}

	[XmlRoot(ElementName = "Traslados", Namespace = "http://www.sat.gob.mx/cfd/3")]
	public class Traslados
	{
		[XmlElement(ElementName = "Traslado", Namespace = "http://www.sat.gob.mx/cfd/3")]
		public Traslado Traslado { get; set; }
	}

	[XmlRoot(ElementName = "Impuestos", Namespace = "http://www.sat.gob.mx/cfd/3")]
	public class Impuestos
	{
		[XmlElement(ElementName = "Traslados", Namespace = "http://www.sat.gob.mx/cfd/3")]
		public Traslados Traslados { get; set; }
		[XmlAttribute(AttributeName = "totalImpuestosTrasladados")]
		public string TotalImpuestosTrasladados { get; set; }
	}

	[XmlRoot(ElementName = "TimbreFiscalDigital", Namespace = "http://www.sat.gob.mx/TimbreFiscalDigital")]
	public class TimbreFiscalDigital
	{
		[XmlAttribute(AttributeName = "tfd", Namespace = "http://www.w3.org/2000/xmlns/")]
		public string Tfd { get; set; }
		[XmlAttribute(AttributeName = "schemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
		public string SchemaLocation { get; set; }
		[XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
		public string Xsi { get; set; }
		[XmlAttribute(AttributeName = "version")]
		public string Version { get; set; }
		[XmlAttribute(AttributeName = "UUID")]
		public string UUID { get; set; }
		[XmlAttribute(AttributeName = "FechaTimbrado")]
		public string FechaTimbrado { get; set; }
		[XmlAttribute(AttributeName = "selloCFD")]
		public string SelloCFD { get; set; }
		[XmlAttribute(AttributeName = "noCertificadoSAT")]
		public string NoCertificadoSAT { get; set; }
		[XmlAttribute(AttributeName = "selloSAT")]
		public string SelloSAT { get; set; }
	}

	[XmlRoot(ElementName = "Complemento", Namespace = "http://www.sat.gob.mx/cfd/3")]
	public class Complemento
	{
		[XmlElement(ElementName = "TimbreFiscalDigital", Namespace = "http://www.sat.gob.mx/TimbreFiscalDigital")]
		public TimbreFiscalDigital TimbreFiscalDigital { get; set; }
	}

	[XmlRoot(ElementName = "Comprobante", Namespace = "http://www.sat.gob.mx/cfd/3")]
	public class Comprobante
	{
		[XmlElement(ElementName = "Emisor", Namespace = "http://www.sat.gob.mx/cfd/3")]
		public Emisor Emisor { get; set; }
		[XmlElement(ElementName = "Receptor", Namespace = "http://www.sat.gob.mx/cfd/3")]
		public Receptor Receptor { get; set; }
		[XmlElement(ElementName = "Conceptos", Namespace = "http://www.sat.gob.mx/cfd/3")]
		public Conceptos Conceptos { get; set; }
		[XmlElement(ElementName = "Impuestos", Namespace = "http://www.sat.gob.mx/cfd/3")]
		public Impuestos Impuestos { get; set; }
		[XmlElement(ElementName = "Complemento", Namespace = "http://www.sat.gob.mx/cfd/3")]
		public Complemento Complemento { get; set; }
		[XmlAttribute(AttributeName = "cfdi", Namespace = "http://www.w3.org/2000/xmlns/")]
		public string Cfdi { get; set; }
		[XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
		public string Xsi { get; set; }
		[XmlAttribute(AttributeName = "schemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
		public string SchemaLocation { get; set; }
		[XmlAttribute(AttributeName = "version")]
		public string Version { get; set; }
		[XmlAttribute(AttributeName = "serie")]
		public string Serie { get; set; }
		[XmlAttribute(AttributeName = "folio")]
		public string Folio { get; set; }
		[XmlAttribute(AttributeName = "fecha")]
		public string Fecha { get; set; }
		[XmlAttribute(AttributeName = "sello")]
		public string Sello { get; set; }
		[XmlAttribute(AttributeName = "formaDePago")]
		public string FormaDePago { get; set; }
		[XmlAttribute(AttributeName = "condicionesDePago")]
		public string CondicionesDePago { get; set; }
		[XmlAttribute(AttributeName = "noCertificado")]
		public string NoCertificado { get; set; }
		[XmlAttribute(AttributeName = "certificado")]
		public string Certificado { get; set; }
		[XmlAttribute(AttributeName = "subTotal")]
		public string SubTotal { get; set; }
		[XmlAttribute(AttributeName = "TipoCambio")]
		public string TipoCambio { get; set; }
		[XmlAttribute(AttributeName = "Moneda")]
		public string Moneda { get; set; }
		[XmlAttribute(AttributeName = "total")]
		public string Total { get; set; }
		[XmlAttribute(AttributeName = "tipoDeComprobante")]
		public string TipoDeComprobante { get; set; }
		[XmlAttribute(AttributeName = "metodoDePago")]
		public string MetodoDePago { get; set; }
		[XmlAttribute(AttributeName = "LugarExpedicion")]
		public string LugarExpedicion { get; set; }
		[XmlAttribute(AttributeName = "NumCtaPago")]
		public string NumCtaPago { get; set; }
	}
}
