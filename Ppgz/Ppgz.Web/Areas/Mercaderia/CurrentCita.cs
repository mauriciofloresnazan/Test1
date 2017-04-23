using System;
using System.Collections.Generic;
using System.Linq;
using Ppgz.CitaWrapper;
using Ppgz.Repository;
using Ppgz.Services;
using PreAsn = Ppgz.CitaWrapper.PreAsn;


namespace Ppgz.Web.Areas.Mercaderia
{
    public class CurrentCita
    {
        #region Excepciones
        public class NumeroDocumentoException : Exception
        {

        }
        public class OrdenDuplicadaException : Exception
        {
            
        }
        public class OrdenSinDetalleException: Exception
        {
            
        }
        public class FechaException : Exception
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

        private readonly List<PreAsn> _ordenesActivas;

        public List<PreAsn> GetOrdenesActivasDisponibles()
        {
            var documentos = _ordenes
                .Select(o => o.NumeroDocumento).ToArray();



            if (Fecha != null)
            {
                var fecha = (DateTime)Fecha;
                return _ordenesActivas
                    .Where(o => !documentos.Contains(o.NumeroDocumento))
                    .Where(o => o.FechasPermitidas.Contains(fecha))
                    .ToList();
            }


            return  _ordenesActivas
                .Where(o => !documentos.Contains(o.NumeroDocumento))
                .ToList();
        }

        public PreAsn GetOrdenActivaDisponible(string numeroDocumento)
        {
            var ordenesActivasDisponibles = GetOrdenesActivasDisponibles();
            return !ordenesActivasDisponibles.Any() ? 
                null : 
                ordenesActivasDisponibles.FirstOrDefault(oa => oa.NumeroDocumento == numeroDocumento);
        }

        public CurrentCita(int cuentaId, int proveedorId, string centro)
        {
            var proveedorManager = new ProveedorManager();

            var proveedor = proveedorManager.Find(proveedorId, cuentaId);

            if (proveedor== null)
            {
                throw new BusinessException("Proveedor incorrecto");
            }

            _proveedor = proveedor;

            _ordenes = new List<PreAsn>();

            var preAsnManager = new PreAsnManager();

            _ordenesActivas = preAsnManager.GetOrdenesActivas(proveedor.Id);

            if (!_ordenesActivas.Any())
            {
                throw new BusinessException("El proveedor no tiene ordenes de compra activas");
            }
        }

        public void SetFecha(DateTime fecha, string numeroDocumento)
        {
            var orden = GetOrdenActivaDisponible(numeroDocumento);

            if (orden == null)
            {
                // TODO pasar a resource
                throw new BusinessException("Numero de documento incorrecto");
            }
            
            if (!orden.FechasPermitidas.Contains(fecha))
            {
                // TODO pasar a resource
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

            if (!orden.FechasPermitidas.Contains((DateTime) Fecha))
            {
                // TODO pasar a resource
                throw new FechaException();
            }

            var preAsnManager = new PreAsnManager();
            var detalles = preAsnManager.GetDetalles(Proveedor.Id, numeroDocumento);


            if (!detalles.Any())
            {
                throw new OrdenSinDetalleException();
            }

            var preAsn = new PreAsn
            {
                NumeroProveedor =  orden.NumeroProveedor,
                NumeroDocumento =  orden.NumeroDocumento,
                Detalles =  detalles,
                
     
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


        public void UpdateDetail(string numeroDocumento, string numeroPosicion, string numeroMaterial, int cantidad)
        {
            var preAsn = _ordenes.FirstOrDefault(o => o.NumeroDocumento == numeroDocumento);

            if (preAsn == null)
            {
                throw new BusinessException("Numero de documento incorrecto");
            }

            var detalle = preAsn.Detalles.FirstOrDefault(d => d.NumeroMaterial == numeroMaterial && d.NumeroPosicion == numeroPosicion);

            if (detalle == null)
            {
                throw new BusinessException("Item incorrecto");
            }

            if (cantidad > detalle.CantidadPermitida)
            {
                throw new BusinessException("Cantidad incorrecta");
            }

            detalle.Cantidad = cantidad;
        } 


    
    }
}