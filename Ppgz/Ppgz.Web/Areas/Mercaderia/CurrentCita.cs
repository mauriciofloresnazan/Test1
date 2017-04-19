using System;
using System.Collections.Generic;
using System.Linq;
using Ppgz.Repository;
using Ppgz.Services;

namespace Ppgz.Web.Areas.Mercaderia
{
    public class CurrentCita
    {
        #region Escepciones
        public class NumeroDocumentoException : Exception
        {

        }
        public class OrdenDuplicadaException : Exception
        {
            
        }
        public class OrdenSinDetalleException: Exception
        {
            
        }
        #endregion Escepciones

        private readonly proveedore _proveedor;
        public proveedore Proveedor
        {
            get { return _proveedor; }
        }

        private readonly List<PreAsn> _ordenes;

        public DateTime? Fecha { get; private set; }

        private readonly List<ordencompra> _ordenesActivas;

        public List<ordencompra> GetOrdenesActivasDisponibles()
        {
            var documentos = _ordenes
                .Select(o => o.NumeroDocumento).ToArray();

            return  _ordenesActivas
                .Where(o => !documentos.Contains(o.NumeroDocumento))
                .ToList();
        }

        public ordencompra GetOrdenActivaDisponible(string numeroDocumento)
        {
            var ordenesActivasDisponibles = GetOrdenesActivasDisponibles();
            return !ordenesActivasDisponibles.Any() ? 
                null : 
                ordenesActivasDisponibles.FirstOrDefault(oa => oa.NumeroDocumento == numeroDocumento);
        }

        public CurrentCita(int cuentaId, int proveedorId)
        {
            var proveedorManager = new ProveedorManager();

            var proveedor = proveedorManager.Find(proveedorId, cuentaId);

            if (proveedor== null)
            {
                throw new BusinessException("Proveedor incorrecto");
            }

            _proveedor = proveedor;

            _ordenes = new List<PreAsn>();

            var ordenCompraManager = new OrdenCompraManager();

            _ordenesActivas = ordenCompraManager
                .FindOrdenesDecompraActivas(proveedor.Id);

            if (!_ordenesActivas.Any())
            {
                throw new BusinessException("El proveedor no tiene ordenes de compra activas");
            }
        }

        public void SetFecha(DateTime fecha)
        {
            if (fecha < DateTime.Today)
            {
                // TODO 
                throw new BusinessException("Fecha incorrecta");
                
            }
            
            Fecha = fecha;
            
        }

        public void AddPreAsn(string numeroDocumento)
        {
            var orden = _ordenesActivas.FirstOrDefault(o => o.NumeroDocumento == numeroDocumento);

        
            if (orden == null)
            {
                throw new NumeroDocumentoException();
            }
            

            if (_ordenes.Any(o => o.NumeroDocumento == numeroDocumento))
            {
                throw new OrdenDuplicadaException();
            }

            var asnManager = new AsnManager();
            var details = asnManager.GetPreAsnDetails(numeroDocumento, Proveedor.NumeroProveedor);


            if (!details.Any())
            {
                throw new OrdenSinDetalleException();
            }

            var preAsn = new PreAsn
            {
                NumeroDocumento =  orden.NumeroDocumento,
                NumeroProveedor = orden.NumeroProveedor,
                ProveedorId = orden.ProveedorId,
                proveedore = orden.proveedore,
                PreAsnDetails = details
            };

     

            _ordenes.Add(preAsn);
        }

        public void RemovePreAsn(string numeroDocumento)
        {
            var orden = _ordenes.FirstOrDefault(o => o.NumeroDocumento == numeroDocumento);
            if (orden != null)
            {
                _ordenes.Remove(orden);
            }
        }

        /*
        public bool HasOrdenes()
        {
            return _ordenes.Any();
        }*/

        public bool HasPreAsn(string numeroDocumento)
        {
            return _ordenes.Any(o=> o.NumeroDocumento == numeroDocumento);
        }

        public PreAsn GetPreAsn(string numeroDocumento)
        {
            return _ordenes.Single(o => o.NumeroDocumento == numeroDocumento);
        }


        public List<PreAsn> GetPreAsns()
        {
            return _ordenes;
        }


        public void UpdateDetail(string numeroDocumento, string numeroMaterial, int cantidad)
        {
            var preAsn = _ordenes.FirstOrDefault(o => o.NumeroDocumento == numeroDocumento);

            if (preAsn == null)
            {
                throw new BusinessException("Numero de documento incorrecto");
            }

            var detalle = preAsn.PreAsnDetails.FirstOrDefault(d => d.NumeroMaterial == numeroMaterial);

            if (detalle == null)
            {
                throw new BusinessException("Numero de material incorrecto");
            }

            if (cantidad > detalle.CantidadPermitida)
            {
                throw new BusinessException("Cantidad incorrecta");
            }

            detalle.CantidadComprometida = cantidad;
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