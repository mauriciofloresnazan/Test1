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
    
    public partial class perfil
    {
        public perfil()
        {
            this.aspnetroles = new HashSet<aspnetrole>();
        }
    
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
    
        public virtual ICollection<aspnetrole> aspnetroles { get; set; }
    }
}