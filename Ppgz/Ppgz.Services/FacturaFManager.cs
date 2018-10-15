using Ppgz.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ppgz.Services
{
    public class FacturaFManager
    {
        private readonly Entities _db = new Entities();

        public List<facturasfactoraje> GetFacturasBySolicitud(int id)
        {
            return _db.facturasfactoraje.Where(ff => ff.idSolicitudesFactoraje == id).ToList();
        }
		public int InsFacturaFactoraje(facturasfactoraje model)
		{
			var result = _db.facturasfactoraje.Add(model);
			_db.SaveChanges();

			return model.idFacturasFactoraje;
		}
    }
}
