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
        
        public bool DeleteFacturas(string [] facturaList, int solid)
        {
            bool result = false;
            try
            {
                List<int> ids = new List<int>();
                foreach(var item in facturaList)
                {
                    if (!String.IsNullOrEmpty(item))
                    {
                        int idfactura = Convert.ToInt32(item);
                        ids.Add(idfactura);
                    }
                }
                List<facturasfactoraje> lfacturasfactoraje = _db.facturasfactoraje.Where(ff => !ids.Contains(ff.idFacturasFactoraje) && ff.idSolicitudesFactoraje == solid).ToList();

                foreach (var item in lfacturasfactoraje)
                {
                    _db.facturasfactoraje.Remove(item);
                    _db.SaveChanges();
                }
                result = true;
            }
            catch
            {
                return false;
            }

            return result;
        }

        public int ActualizarPorcentaje(int idFactura, int porcentaje)
        {
            var facturafactoraje = _db.facturasfactoraje.Where(ff => ff.idFacturasFactoraje == idFactura).FirstOrDefault();

            if(facturafactoraje != null)
            {
                facturafactoraje.Porcentaje = porcentaje;
                double porcentajeAux = (Convert.ToDouble(porcentaje) / 100);
                facturafactoraje.interes = (Convert.ToDouble(facturafactoraje.Monto) * ((porcentajeAux) / 30) * facturafactoraje.DiasPP);
                _db.Entry(facturafactoraje).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
                return facturafactoraje.idFacturasFactoraje;
            }
            else
            {
                return 0;
            }
        }
		public int ActualizarNumeroDocumento(int idFactura, string numeroDocumento)
		{
			var facturafactoraje = _db.facturasfactoraje.Where(ff => ff.idFacturasFactoraje == idFactura).FirstOrDefault();

			if (facturafactoraje != null)
			{
				facturafactoraje.NumeroDocumento = numeroDocumento;
				_db.Entry(facturafactoraje).State = System.Data.Entity.EntityState.Modified;
				_db.SaveChanges();
				return facturafactoraje.idFacturasFactoraje;
			}
			else
			{
				return 0;
			}
		}
	}
}
