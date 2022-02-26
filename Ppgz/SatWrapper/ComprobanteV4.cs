using System.Collections.Generic;
using System.Xml.Serialization;

namespace SatWrapper
{
    [XmlRoot(ElementName = "DomicilioFiscal", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class DomicilioFiscalV
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

    [XmlRoot(ElementName = "ExpedidoEn", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class ExpedidoEnV
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

    [XmlRoot(ElementName = "RegimenFiscal", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class RegimenFiscalV
    {
        [XmlAttribute(AttributeName = "Regimen")]
        public string Regimen { get; set; }
    }

    [XmlRoot(ElementName = "Emisor", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class EmisorV
    {
        [XmlElement(ElementName = "DomicilioFiscal", Namespace = "http://www.sat.gob.mx/cfd/4")]
        public DomicilioFiscal DomicilioFiscal { get; set; }
        [XmlElement(ElementName = "ExpedidoEn", Namespace = "http://www.sat.gob.mx/cfd/4")]
        public ExpedidoEn ExpedidoEn { get; set; }
        [XmlElement(ElementName = "RegimenFiscal", Namespace = "http://www.sat.gob.mx/cfd/4")]
        public RegimenFiscal RegimenFiscal { get; set; }
        [XmlAttribute(AttributeName = "rfc")]
        public string Rfc { get; set; }
        [XmlAttribute(AttributeName = "nombre")]
        public string Nombre { get; set; }

        //Version 3.3
        [XmlAttribute(AttributeName = "Rfc")]
        public string Rfc33 { get; set; }
        [XmlAttribute(AttributeName = "Nombre")]
        public string Nombre33 { get; set; }
    }

    [XmlRoot(ElementName = "Domicilio", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class DomicilioV
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

    [XmlRoot(ElementName = "Receptor", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class ReceptorV
    {
        [XmlElement(ElementName = "Domicilio", Namespace = "http://www.sat.gob.mx/cfd/4")]
        public Domicilio Domicilio { get; set; }
        [XmlAttribute(AttributeName = "rfc")]
        public string Rfc { get; set; }
        [XmlAttribute(AttributeName = "nombre")]
        public string Nombre { get; set; }

        //Version 3.3
        [XmlAttribute(AttributeName = "Rfc")]
        public string Rfc33 { get; set; }
        [XmlAttribute(AttributeName = "Nombre")]
        public string Nombre33 { get; set; }
    }

    [XmlRoot(ElementName = "Concepto", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class ConceptoV
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

        //CFDI 3.3
        [XmlAttribute(AttributeName = "Cantidad")]
        public string Cantidad33 { get; set; }
        [XmlAttribute(AttributeName = "Unidad")]
        public string Unidad33 { get; set; }
        [XmlAttribute(AttributeName = "Descripcion")]
        public string Descripcion33 { get; set; }
        [XmlAttribute(AttributeName = "ValorUnitario")]
        public string ValorUnitario33 { get; set; }
        [XmlAttribute(AttributeName = "Importe")]
        public string Importe33 { get; set; }
    }

    [XmlRoot(ElementName = "Conceptos", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class ConceptosV
    {
        [XmlElement(ElementName = "Concepto", Namespace = "http://www.sat.gob.mx/cfd/4")]
        public List<ConceptoV> Concepto { get; set; }
    }

    [XmlRoot(ElementName = "Traslado", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class TrasladoV
    {
        [XmlAttribute(AttributeName = "impuesto")]
        public string Impuesto { get; set; }
        [XmlAttribute(AttributeName = "tasa")]
        public string Tasa { get; set; }
        [XmlAttribute(AttributeName = "importe")]
        public string Importe { get; set; }

        //CFDI 3.3
        [XmlAttribute(AttributeName = "Impuesto")]
        public string Impuesto33 { get; set; }
        [XmlAttribute(AttributeName = "Tasa")]
        public string Tasa33 { get; set; }
        [XmlAttribute(AttributeName = "Importe")]
        public string Importe33 { get; set; }
    }

    [XmlRoot(ElementName = "Traslados", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class TrasladosV
    {
        [XmlElement(ElementName = "Traslado", Namespace = "http://www.sat.gob.mx/cfd/4")]
        public TrasladoV Traslados { get; set; }
    }

    [XmlRoot(ElementName = "Impuestos", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class ImpuestosV
    {
        [XmlElement(ElementName = "Traslados", Namespace = "http://www.sat.gob.mx/cfd/4")]
        public Traslados Traslados { get; set; }
        [XmlAttribute(AttributeName = "totalImpuestosTrasladados")]
        public string TotalImpuestosTrasladados { get; set; }
    }

    [XmlRoot(ElementName = "TimbreFiscalDigital", Namespace = "http://www.sat.gob.mx/TimbreFiscalDigital")]
    public class TimbreFiscalDigitalV
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

    [XmlRoot(ElementName = "Complemento", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class ComplementoV
    {
        [XmlElement(ElementName = "TimbreFiscalDigital", Namespace = "http://www.sat.gob.mx/TimbreFiscalDigital")]
        public TimbreFiscalDigitalV TimbreFiscalDigital { get; set; }
    }

    [XmlRoot(ElementName = "Comprobante", Namespace = "http://www.sat.gob.mx/cfd/4")]
    public class ComprobanteVersion4
    {
        [XmlElement(ElementName = "Emisor", Namespace = "http://www.sat.gob.mx/cfd/4")]
        public EmisorV Emisor { get; set; }
        [XmlElement(ElementName = "Receptor", Namespace = "http://www.sat.gob.mx/cfd/4")]
        public ReceptorV Receptor { get; set; }
        [XmlElement(ElementName = "Conceptos", Namespace = "http://www.sat.gob.mx/cfd/4")]
        public ConceptosV Conceptos { get; set; }
        [XmlElement(ElementName = "Impuestos", Namespace = "http://www.sat.gob.mx/cfd/4")]
        public ImpuestosV Impuestos { get; set; }
        [XmlElement(ElementName = "Complemento", Namespace = "http://www.sat.gob.mx/cfd/4")]
        public ComplementoV Complemento { get; set; }
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

        //CFDI3.3
        [XmlAttribute(AttributeName = "Version")]
        public string Version33 { get; set; }
        [XmlAttribute(AttributeName = "Serie")]
        public string Serie33 { get; set; }
        [XmlAttribute(AttributeName = "Folio")]
        public string Folio33 { get; set; }
        [XmlAttribute(AttributeName = "Fecha")]
        public string Fecha33 { get; set; }
        [XmlAttribute(AttributeName = "Total")]
        public string Total33 { get; set; }
        [XmlAttribute(AttributeName = "SubTotal")]
        public string SubTotal33 { get; set; }

    }








}
