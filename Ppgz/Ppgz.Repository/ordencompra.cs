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
    
    public partial class ordencompra
    {
        public int Id { get; set; }
        public Nullable<System.DateTime> Fecha { get; set; }
        public string NumeroOrden { get; set; }
        public Nullable<decimal> PrecioOrden { get; set; }
        public string Almacen { get; set; }
        public Nullable<System.DateTime> FechaEntrega { get; set; }
        public Nullable<int> CantidadItem { get; set; }
        public Nullable<int> TotalLinea { get; set; }
        public Nullable<int> ProveedoresId { get; set; }
        public Nullable<int> FacturasId { get; set; }
        public string CodigoProveedor { get; set; }
        public string FechaSAP { get; set; }
        public Nullable<System.DateTime> VisualizadoFecha { get; set; }
    }
}
