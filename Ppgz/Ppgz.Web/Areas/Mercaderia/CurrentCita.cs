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
        public class OrdenCentroException : Exception
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
        
        public readonly string Centro;
        public DateTime? Fecha { get; private set; }

        private readonly List<PreAsn> _ordenesActivas;

        public int Cantidad
        {
            get {
                if (cantidadSinASN > 0) {
                    return cantidadSinASN;
                } else {
                        return _ordenes.Sum(o => o.TotalCantidad);
                }
                }
        }


        //Se agrego para poder tener una cantidad sin ASN sin modificar la logica actual
        public int CantidadSinASN { get => cantidadSinASN; set => cantidadSinASN = value; }

        private int cantidadSinASN=0;
        //fin


        public readonly bool  EsCrossDock;
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
                .Where(o=> o.TotalPermitido > 0)
                // todo incluir el detalle en la consulta inicial
                //.Where( o=> o.Detalles.Any(de=> de.Almacen.ToUpper() == Centro))
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

            var result = preAsnManager.GetOrdenesActivasConDetalle(proveedor.Id);

            if (String.Equals(centro, "CROSS DOCK", StringComparison.CurrentCultureIgnoreCase))
            {
                EsCrossDock = true;
                _ordenesActivas = result
                    .Where(o => o.EsCrossDock)
                    .Where(o => o.TotalPermitido > 0)
                    .ToList();
            }
            else
            {
                _ordenesActivas = result
                    .Where(o => !o.EsCrossDock)
                    .Where(o => o.Detalles.Any(de => String.Equals(de.Centro, centro, StringComparison.CurrentCultureIgnoreCase)))
                    .Where(o => o.TotalPermitido > 0)
                    .ToList();
            }



            if (!_ordenesActivas.Any())
            {
                throw new BusinessException("No hay Órdenes de Compras con entregas disponibles para crear una Cita. Por favor verifique su selección (Proveedor, Almacén, Directo a Tienda).");
            }

            Centro = centro;
        }

        public CurrentCita(int cuentaId, int proveedorId)
        {
            var proveedorManager = new ProveedorManager();

            var proveedor = proveedorManager.Find(proveedorId, cuentaId);

            if (proveedor == null)
            {
                throw new BusinessException("Proveedor incorrecto");
            }

            _proveedor = proveedor;

            

            Centro = "Sin ASN";
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

        public void SetFecha(DateTime fecha)
        {

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

           /* var preAsnManager = new PreAsnManager();
            var detalles = preAsnManager.GetDetalles(Proveedor.Id, numeroDocumento);


            if (!detalles.Any())
            {
                throw new OrdenSinDetalleException();
            }*/
            if (orden.Detalles.Sum(de => de.CantidadPermitida) < 1)
            {
                throw new OrdenSinDetalleException();
            }

            if (!EsCrossDock)
            {
                if (orden.Detalles.All(de => de.Centro != Centro))
                {
                    throw new OrdenCentroException();
                }
            }


            if (orden.TotalPermitido < 1)
            {
                throw new OrdenSinDetalleException();
            }

     

            _ordenes.Add(orden);
        }

        public void RemovePreAsn(string numeroDocumento)
        {
            var orden = _ordenes.FirstOrDefault(o => o.NumeroDocumento == numeroDocumento);
            if (orden != null)
            {
                _ordenes.Remove(orden);
            }
        }


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