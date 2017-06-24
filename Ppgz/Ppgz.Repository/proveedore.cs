//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Ppgz.Repository
{
    using System;
    using System.Collections.Generic;
    
    public partial class proveedore
    {
        public proveedore()
        {
            this.citas = new HashSet<cita>();
            this.etiquetas = new HashSet<etiqueta>();
            this.facturas = new HashSet<factura>();
            this.niveleseervicios = new HashSet<niveleseervicio>();
            this.ordencompras = new HashSet<ordencompra>();
        }
    
        public int Id { get; set; }
        public string NumeroProveedor { get; set; }
        public string ClavePais { get; set; }
        public string Nombre1 { get; set; }
        public string Nombre2 { get; set; }
        public string Nombre3 { get; set; }
        public string Nombre4 { get; set; }
        public string Poblacion { get; set; }
        public string Distrito { get; set; }
        public string Apartado { get; set; }
        public string CodigoApartado { get; set; }
        public string CodigoPostal { get; set; }
        public string Region { get; set; }
        public string Calle { get; set; }
        public string Direccion { get; set; }
        public string Sociedad { get; set; }
        public string OrganizacionCompra { get; set; }
        public string ClaveMoned { get; set; }
        public string VendedorResponsable { get; set; }
        public string NumeroTelefono { get; set; }
        public string CondicionPago { get; set; }
        public string IncoTerminos1 { get; set; }
        public string IncoTerminos2 { get; set; }
        public string GrupoCompras { get; set; }
        public string DenominacionGrupo { get; set; }
        public string TelefonoGrupoCompra { get; set; }
        public string TelefonoPrefijo { get; set; }
        public string TelefonoExtension { get; set; }
        public string Correo { get; set; }
        public int CuentaId { get; set; }
        public System.DateTime FechaCargaPortal { get; set; }
        public string UsuarioIdTx { get; set; }
        public string OperacionTx { get; set; }
        public string Rfc { get; set; }
        public bool Borrado { get; set; }
        public string EstadoNombre { get; set; }
    
        public virtual ICollection<cita> citas { get; set; }
        public virtual cuenta cuenta { get; set; }
        public virtual ICollection<etiqueta> etiquetas { get; set; }
        public virtual ICollection<factura> facturas { get; set; }
        public virtual ICollection<niveleseervicio> niveleseervicios { get; set; }
        public virtual ICollection<ordencompra> ordencompras { get; set; }
    }
}
