using System;
using System.Collections.Generic;
using System.Linq;
using Ppgz.Repository;
using Ppgz.Services;

namespace Ppgz.Web.Areas.Mercaderia
{
    public class CurrentCita
    {
        private readonly proveedore _proveedor;
        public proveedore Proveedor
        {
            get
            {
                return _proveedor;
                
            }
        }

        public DateTime? Fecha;

        public List<ordencompra> Ordenes;

        public CurrentCita(int cuentaId, int proveedorId)
        {
            var proveedorManager = new ProveedorManager();

            var proveedor = proveedorManager.Find(proveedorId);

            if (proveedor.CuentaId != cuentaId)
            {
                throw new BusinessException("Error en la cuenta del proveedor");
            }
            _proveedor = proveedor;

        }

        public void AddOrden(ordencompra orden)
        {
            if (Ordenes.Any(o => o.Id == orden.Id))
            {
                throw new BusinessException("La orden ya se encuentra incluida");
            }
            Ordenes.Add(orden);
        }

        public void RemoveOrden(string numeroDocumento)
        {
            var orden = Ordenes.FirstOrDefault(o => o.NumeroDocumento == numeroDocumento);
            if (orden != null)
            {
                Ordenes.Remove(orden);
            }
        }


    }
}