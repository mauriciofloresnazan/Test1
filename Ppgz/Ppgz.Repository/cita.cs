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
        public int Id { get; set; }
        public string Codigo { get; set; }
        public Nullable<System.DateTime> FechaCita { get; set; }
        public Nullable<System.DateTime> FechaCreacion { get; set; }
        public string Tienda { get; set; }
        public Nullable<int> CantidadAproximada { get; set; }
        public string Estatus { get; set; }
        public string DataEntrega { get; set; }
        public int ProveedorId { get; set; }
        public bool Borrado { get; set; }
        public Nullable<System.DateTime> FechaTx { get; set; }
        public string UsuarioIdTx { get; set; }
        public string OperacionTx { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
    }
}
