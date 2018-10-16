using Ppgz.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ppgz.Services
{
    public class DescuentoFManager
    {
        public readonly Entities _db = new Entities();

        public List<descuentofactoraje> GetDescuentosBySolicitud(int SolicitudId)
        {
            return _db.descuentosfactoraje.Where(df => df.idSolicitudesFactoraje == SolicitudId).ToList();
        }
		public int InsDescuentoFactoraje(descuentofactoraje model)
		{
			var result = _db.descuentosfactoraje.Add(model);
			_db.SaveChanges();

			return model.idDescuentosFactoraje;
		}
	}
}
