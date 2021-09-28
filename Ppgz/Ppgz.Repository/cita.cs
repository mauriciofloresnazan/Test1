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
    
    public partial class cita
    {
        public cita()
        {
            this.asns = new HashSet<asn>();
            this.crs = new HashSet<cr>();
            this.horariorieles = new HashSet<horarioriele>();
        }
    
        public int Id { get; set; }
        public string Codigo { get; set; }
        public System.DateTime FechaCita { get; set; }
        public System.DateTime FechaCreacion { get; set; }
        public string Almacen { get; set; }
        public Nullable<int> CantidadTotal { get; set; }
        public int ProveedorId { get; set; }
        public bool Borrado { get; set; }
        public Nullable<System.DateTime> FechaTx { get; set; }
        public string UsuarioIdTx { get; set; }
        public string OperacionTx { get; set; }
        public sbyte RielesOcupados { get; set; }
        public Nullable<int> EstatusCitaId { get; set; }
        public bool Penalizado { get; set; }
        public Nullable<System.DateTime> MovimientoCita { get; set; }
        public string TipoCita { get; set; }
        public virtual ICollection<asn> asns { get; set; }
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual estatuscita estatuscita { get; set; }
        public virtual proveedore proveedore { get; set; }
        public virtual ICollection<cr> crs { get; set; }
        public virtual ICollection<horarioriele> horariorieles { get; set; }
    }
}
