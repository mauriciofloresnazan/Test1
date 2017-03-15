using System.Collections;
using System.Linq;
using Ppgz.Repository;

namespace Ppgz.Services
{
    public class OrdenCompraManager
    {
        private readonly Entities _db = new Entities();

        public Hashtable FindActivaByIdAndUsuarioId(int id, string usuarioId)
        {

            var  orden = _db.ordencompras.FirstOrDefault(
                o => o.Id == id);

            var ordenDetalle = _db.detalleordencompras.FirstOrDefault(
                o => o.OrdenComprasId == id);

            var result = new Hashtable
            {
                {"orden", orden}, 
                {"ordenDetalle", ordenDetalle}
            };

            return result;

        }
    }
}
