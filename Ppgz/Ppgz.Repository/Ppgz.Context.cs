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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class Entities : DbContext
    {
        public Entities()
            : base("name=Entities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<andene> andenes { get; set; }
        public virtual DbSet<asn> asns { get; set; }
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<cita> citas { get; set; }
        public virtual DbSet<configuracionfactoraje> configuracionfactoraje { get; set; }
        public virtual DbSet<configuracione> configuraciones { get; set; }
        public virtual DbSet<cr> crs { get; set; }
        public virtual DbSet<crforaneo> crsforaneos { get; set; }
        public virtual DbSet<cuentaproveedore> cuentaproveedores { get; set; }
        public virtual DbSet<Penalizacionauditor> Penalizacionauditores { get; set; }
        public virtual DbSet<RevisarDatos> RevisarDatos { get; set; }
        public virtual DbSet<RevisarPedidos> RevisarPedidos { get; set; }
        public virtual DbSet<EnviarDatos> EnviarDatos { get; set; }
        public virtual DbSet<MensajeResultado> MensajeResultado { get; set; }
        public virtual DbSet<EnviarEtiquetas> EnviarEtiquetas { get; set; }
        public virtual DbSet<citasscal> citasscal { get; set; }
        public virtual DbSet<reenvio> reenvio { get; set; }
        public virtual DbSet<audi> auditores { get; set; }
        public virtual DbSet<cuenta> cuentas { get; set; }
        public virtual DbSet<cuentasmensaje> cuentasmensajes { get; set; }
        public virtual DbSet<descuentofactoraje> descuentosfactoraje { get; set; }
        public virtual DbSet<estatuscita> estatuscitas { get; set; }
        public virtual DbSet<estatusfactoraje> estatusfactoraje { get; set; }
        public virtual DbSet<factura> facturas { get; set; }
        public virtual DbSet<facturasfactoraje> facturasfactoraje { get; set; }
        public virtual DbSet<horarioriele> horariorieles { get; set; }
        public virtual DbSet<horario> horarios { get; set; }
        public virtual DbSet<impuls_logs> impuls_logs { get; set; }
        public virtual DbSet<logfactoraje> logfactoraje { get; set; }
        public virtual DbSet<mensaje> mensajes { get; set; }
        public virtual DbSet<niveleseervicio> niveleseervicios { get; set; }
        public virtual DbSet<ordencompra> ordencompras { get; set; }
        public virtual DbSet<ordencompradetalle> ordencompradetalles { get; set; }
        public virtual DbSet<perfile> perfiles { get; set; }
        public virtual DbSet<proveedore> proveedores { get; set; }
        public virtual DbSet<proveedorfactoraje> proveedoresfactoraje { get; set; }
        public virtual DbSet<riele> rieles { get; set; }
        public virtual DbSet<ScaleAlmacen> ScaleAlmacens { get; set; }
        public virtual DbSet<solicitudesfactoraje> solicitudesfactoraje { get; set; }
    }
}
