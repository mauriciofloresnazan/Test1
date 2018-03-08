
namespace Ppgz.Repository
{

    public partial class factura
    {
        public int Id { get; set; }
        public int proveedor_id { get; set; }
        public string Uuid { get; set; }
        public string Serie { get; set; }
        public string Folio { get; set; }
        public System.DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string XmlRuta { get; set; }
        public string PdfRuta { get; set; }
        public string Estatus { get; set; }
        public string Comentario { get; set; }
        public string NumeroGenerado { get; set; }
        public bool Procesado { get; set; }
        public string numeroProveedor { get; set; }

        public virtual proveedore proveedore { get; set; }
    }
}