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
    
    public partial class asn
    {
        public int Id { get; set; }
        public Nullable<int> CitaId { get; set; }
        public string OrdenNumeroDocumento { get; set; }
        public string NumeroPosicion { get; set; }
        public string NumeroMaterial { get; set; }
        public string NombreMaterial { get; set; }
        public int Cantidad { get; set; }
        public string Tienda { get; set; }
    
        public virtual cita cita { get; set; }
    }
}
