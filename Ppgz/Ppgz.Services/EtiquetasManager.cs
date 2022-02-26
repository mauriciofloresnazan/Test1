using System;
using System.Collections;
using System.Linq;
using System.Text;
using Ppgz.Repository;
using SapWrapper;

namespace Ppgz.Services
{
    public class EtiquetasManager
    {
        private readonly Entities _db = new Entities();

        /// <summary>
        /// Devuelve un hashtable con lo siguiente:
        /// 1 return: Contiene un datatable con el resultado del rfc
        /// 2 "csv" el contenido del archivo si hay ordenes correctas.
        /// </summary>
        public Hashtable GetArchivoCsv(int proveedorId, int cuentaId, bool etiquetaNazan, string[] ordenes, bool zapato)
        {
            var proveedor = _db.proveedores.FirstOrDefault(p => p.Id == proveedorId && p.CuentaId == cuentaId);

            if (proveedor == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_Proveedor_Id);
            }

            var sapEtiquetasManager = new SapEtiquetasManager();
            var resultado = sapEtiquetasManager.GetContenidoCsv(proveedor.NumeroProveedor, etiquetaNazan, ordenes);

            var contenidoCsv = resultado["csv"];

            var hashTable = new Hashtable
            {
                {"return", resultado["return"]},
                {"csv", contenidoCsv}
            };
            
            return hashTable;

        }
    }
}
