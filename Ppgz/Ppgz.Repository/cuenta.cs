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
    
    public partial class cuenta
    {
        public cuenta()
        {
            this.cuentaproveedores = new HashSet<cuentaproveedore>();
            this.perfiles = new HashSet<perfile>();
            this.cuentasmensajes = new HashSet<cuentasmensaje>();
            this.proveedores = new HashSet<proveedore>();
            this.AspNetUsers = new HashSet<AspNetUser>();
        }
    
        public int Id { get; set; }
        public string CodigoProveedor { get; set; }
        public string NombreCuenta { get; set; }
        public Nullable<System.DateTime> FechaRegistro { get; set; }
        public string Tipo { get; set; }
        public bool EsEspecial { get; set; }
        public bool SinASN { get; set; }
        public bool Factoraje { get; set; }
        public bool Activo { get; set; }
        public bool Borrado { get; set; }
        public Nullable<System.DateTime> FechaTx { get; set; }
        public string UsuarioIdTx { get; set; }
        public string OperacionTx { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual ICollection<cuentaproveedore> cuentaproveedores { get; set; }
        public virtual ICollection<perfile> perfiles { get; set; }
        public virtual ICollection<cuentasmensaje> cuentasmensajes { get; set; }
        public virtual ICollection<proveedore> proveedores { get; set; }
        public virtual ICollection<AspNetUser> AspNetUsers { get; set; }
    }
}
