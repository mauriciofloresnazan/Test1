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
    
    public partial class descuentofactoraje
    {
        public int idDescuentosFactoraje { get; set; }
        public int idSolicitudesFactoraje { get; set; }
        public int EstatusFactoraje { get; set; }
        public string NumeroDocumento { get; set; }
        public double Monto { get; set; }
        public string Descripcion { get; set; }
        public System.DateTime FechaDescuento { get; set; }
    }
}
