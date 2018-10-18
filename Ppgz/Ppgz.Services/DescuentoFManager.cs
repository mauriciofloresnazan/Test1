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

        public bool DeleteDescuentos(string [] descuentosList, int solid)
        {
            bool result = false;
            try
            {
                List<int> ids = new List<int>();
                foreach(var item in descuentosList)
                {
                    if (!String.IsNullOrEmpty(item))
                    {
                        int i = Convert.ToInt32(item);
                        ids.Add(i);
                    }
                }

                List<descuentofactoraje> df = _db.descuentosfactoraje.Where(d => !ids.Contains(d.idDescuentosFactoraje) && d.idSolicitudesFactoraje == solid).ToList();
                foreach(var item in df)
                {
                    _db.descuentosfactoraje.Remove(item);
                    _db.SaveChanges();
                }
                result = true;
            }
            catch(Exception ex)
            {
                return false;
            }
            return result;
        }
	}
}
