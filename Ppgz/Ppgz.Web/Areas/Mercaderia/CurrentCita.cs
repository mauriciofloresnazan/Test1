using System;
using System.Collections.Generic;
using System.Linq;
using Ppgz.Repository;
using Ppgz.Services;

namespace Ppgz.Web.Areas.Mercaderia
{
    public class CurrentCita
    {

        public class OrdenDuplicadaException : Exception
        {
            
        }
        public class OrdenSinDetalleException: Exception
        {
            
        }

        private readonly proveedore _proveedor;
        public proveedore Proveedor
        {
            get { return _proveedor; }
        }

        private readonly List<ordencompra> _ordenes;
        public DateTime Fecha
        {
            get { return _fecha; }
        }
        private readonly DateTime _fecha;
        
        public CurrentCita(int cuentaId, int proveedorId, DateTime fecha)
        {
            var proveedorManager = new ProveedorManager();

            var proveedor = proveedorManager.Find(proveedorId);

            if (proveedor.CuentaId != cuentaId)
            {
                throw new BusinessException("Error en la cuenta del proveedor");
            }

            _proveedor = proveedor;

            if (fecha.Date < DateTime.Today.Date)
            {
                throw new BusinessException("Fecha incorrecta");
            }

            _fecha = fecha;

            _ordenes = new List<ordencompra>();

        }

        public void AddOrden(ordencompra orden)
        {
            if (_ordenes.Any(o => o.NumeroDocumento == orden.NumeroDocumento))
            {
                throw new OrdenDuplicadaException();
            }

            if (orden.ordencompradetalles == null)
            {
                throw new OrdenSinDetalleException();
            }
            if (!orden.ordencompradetalles.Any())
            {
                throw new OrdenSinDetalleException();
            }

            _ordenes.Add(orden);
        }

        public void RemoveOrden(string numeroDocumento)
        {
            var orden = _ordenes.FirstOrDefault(o => o.NumeroDocumento == numeroDocumento);
            if (orden != null)
            {
                _ordenes.Remove(orden);
            }
        }


        public bool HasOrdenes()
        {
            return _ordenes.Any();
        }

        public bool HasOrden(string numeroDocumento)
        {
            return _ordenes.Any(o=> o.NumeroDocumento == numeroDocumento);
        }

        public ordencompra GetOrden(string numeroDocumento)
        {
            return _ordenes.Single(o => o.NumeroDocumento == numeroDocumento);
        }


        public List<ordencompra> GetOrdenes()
        {
            return _ordenes;
        }
        public int UpdateElementoEnLista(string numeroDocumento, string numeroMaterial, 
            int oldValue, int newValue)
        {

            /**************************************
            METODO PARA ACTUALIZAR LA CANTIDAD
             DE LA TALLA, EXISTENTE EN UN ITEMS
             PARA UN NUMERO DE ORDEN
            ***************************************/

            var resultado = 0;

            var orden = _ordenes.FirstOrDefault(o => o.NumeroDocumento == numeroDocumento);

            if (orden == null)
            {
                return resultado;
            }

            if (!orden.ordencompradetalles.Any())
            {
                return resultado;
            }

            var detalle = orden.ordencompradetalles.FirstOrDefault(d => d.NumeroMaterial == numeroMaterial);
          
            if (detalle == null)
            {
                return resultado;
            }

            detalle.CantidadComprometida = newValue;
            resultado = 1;

            return resultado;

        }
        
    }
}