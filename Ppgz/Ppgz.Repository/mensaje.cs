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
    
    public partial class mensaje
    {
        public mensaje()
        {
            this.cuentasmensajes = new HashSet<cuentasmensaje>();
        }
    
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Contenido { get; set; }
        public string Archivo { get; set; }
        public System.DateTime FechaPublicacion { get; set; }
        public System.DateTime FechaCaducidad { get; set; }
        public string EnviadoA { get; set; }
        public bool Borrado { get; set; }
        public Nullable<System.DateTime> FechaTx { get; set; }
        public string UsuarioIdTx { get; set; }
        public string OperacionTx { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual ICollection<cuentasmensaje> cuentasmensajes { get; set; }
    }
}
