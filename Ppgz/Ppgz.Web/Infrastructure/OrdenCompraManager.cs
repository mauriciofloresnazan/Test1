using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using Ppgz.Repository;

namespace Ppgz.Web.Infrastructure
{
    public class OrdenCompraManager
    {
        private readonly Entities _db = new Entities();
        public List<ordencompra> FindByProveedorId(int id )
        {
            return _db.ordencompras.Where(o => o.ProveedoresId == id).ToList();

        }
        public DataTable FindByCuentaId(int id)
        {
            var commonManager = new CommonManager();


            MySqlParameter[] parametes = {
                    new MySqlParameter("id", id)
                };


            const string sql = @"
            SELECT oc.*, p.Rfc, p.NombreProveedor 
            FROM   ordencompras oc
                   JOIN proveedores p ON p.Id = oc.Proveedoresid
            WHERE  p.CuentaId = @id;";

            return commonManager.QueryToTable(sql, parametes);


        }

        public DataTable FindActivaByCuentaId(int id)
        {
            var commonManager = new CommonManager();

            MySqlParameter[] parametes = {
                    new MySqlParameter("id", id)
                };

            const string sql = @"
            SELECT oc.*, p.Rfc, p.NombreProveedor 
            FROM   ordencompras oc
                   JOIN proveedores p ON p.Id = oc.Proveedoresid
            WHERE  p.CuentaId = @id;";

            return commonManager.QueryToTable(sql, parametes);


        } 
    }
}