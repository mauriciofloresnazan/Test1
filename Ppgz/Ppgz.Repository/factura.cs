
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



    public virtual proveedore proveedore { get; set; }

}

}
