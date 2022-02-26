using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ppgz.Repository;
using System.Data.Entity;

namespace Ppgz.Services
{
    public class ConfiguracionesFManager
    {
        private readonly Entities _db = new Entities();

        public List<configuracionfactoraje> GetConfiguraciones()
        {
            return _db.configuracionfactoraje.ToList();
        }

        public configuracionfactoraje GetConfiguracionById(int id)
        {
            return _db.configuracionfactoraje.Where(c => c.idConfigoracionFactoraje == id).FirstOrDefault();
        }

        public bool UpdateConfiguracion(int id, string key, string value)
        {
            var result = false;

            var config = GetConfiguracionById(id);
            if (config != null)
            {
                config.key = key;
                config.value = value;

                _db.Entry(config).State = EntityState.Modified;
                _db.SaveChanges();
                result = true;
            }

            return result;
        }
    }
}
