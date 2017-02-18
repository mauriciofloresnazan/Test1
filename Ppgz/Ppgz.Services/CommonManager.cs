using Ppgz.Repository;

namespace Ppgz.Services
{
    public class CommonManager
    {
        private readonly PpgzEntities _db = new PpgzEntities();

        public void UsuarioCuentaXrefAdd(usuarios_cuentas_xref xref)
        {
            _db.usuarios_cuentas_xref.Add(xref);
            _db.SaveChanges();

        }
    }
}
