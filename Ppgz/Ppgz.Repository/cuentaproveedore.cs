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
    
    public partial class cuentaproveedore
    {
        public string NumeroProveedor { get; set; }
        public string Nombre { get; set; }
        public string Ciudad { get; set; }
        public string Distrito { get; set; }
        public string VendedorNombre { get; set; }
        public string VendedorTelefono { get; set; }
        public string CompradorImpulsNombre { get; set; }
        public string CompradorImpulsTelefono { get; set; }
        public string CompradorImpulsEmail { get; set; }
        public int CuentaId { get; set; }
    
        public virtual cuenta cuenta { get; set; }
    }
}
