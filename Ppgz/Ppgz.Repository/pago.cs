//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Ppgz.Repository
{
    using System;
    using System.Collections.Generic;
    
    public partial class pago
    {
        public int id { get; set; }
        public string RFC { get; set; }
        public string RazonSocial { get; set; }
        public string NumeroCompensacion { get; set; }
        public string Referencia { get; set; }
        public string FechaDePago { get; set; }
        public string FechaBase { get; set; }
        public string Importe { get; set; }
        public string ML { get; set; }
        public string TipoMovimiento { get; set; }
    }
}
