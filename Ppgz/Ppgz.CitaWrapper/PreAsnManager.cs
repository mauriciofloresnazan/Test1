using System;
using System.Collections.Generic;
using System.Linq;
using SapWrapper;

namespace Ppgz.CitaWrapper
{
    public class PreAsnManager
    {
        public List<PreAsn> GetOrdenesActivas(int proveedorId)
        {
            var db = new Repository.Entities();
            var proveedor = db.proveedores.Single(p => p.Id == proveedorId);
            
            
            // TODO confirmar con el cliente
            const string organizacionCompras = "OC01";

            var sapOrdenCompraManager = new SapOrdenCompraManager();
            var ordenes = sapOrdenCompraManager.GetActivasSinDetalle(proveedor.NumeroProveedor, organizacionCompras);

            var result = (from orden in ordenes
                let fechas = RulesManager.GetFechasPermitidas(orden.FechaEntrega, proveedor.cuenta.EsEspecial)
                select new PreAsn
                {
                    FechaEntrega = orden.FechaEntrega, 
                    NumeroDocumento = orden.NumeroDocumento, 
                    NumeroProveedor = orden.NumeroProveedor, 
                    FechasPermitidas = fechas
                }).ToList();

            
            return result.Any() ? result.Where(o => o.FechasPermitidas.Count > 0).ToList() : result;
        }
        
        public List<PreAsnDetalle> GetDetalles(int proveedorId, string numerDocumento)
        {
            var db = new Repository.Entities();
            var proveedor = db.proveedores.Single(p => p.Id == proveedorId);
            
            var asnFuturos = db.asns
                .Where(asn => asn.OrdenNumeroDocumento == numerDocumento 
                    && asn.cita.FechaCita > DateTime.Today).ToList();

            // TODO confirmar con el cliente
            const string organizacionCompras = "OC01";

            var sapOrdenCompraManager = new SapOrdenCompraManager();
            var detalles = sapOrdenCompraManager.GetDetalle(proveedor.NumeroProveedor, organizacionCompras, numerDocumento);

            var result = new List<PreAsnDetalle>();
            foreach (var detalle in detalles)
            {
                var preAsnDetalle = new PreAsnDetalle()
                {
                    NumeroPosicion = detalle.NumeroPosicion,
                    Centro = detalle.Centro,
                    Almacen = detalle.Almacen,
                    CantidadCitasFuturas = asnFuturos
                        .Where(asn => asn.OrdenNumeroDocumento == detalle.NumeroDocumento && 
                            asn.NumeroPosicion == detalle.NumeroPosicion &&
                            asn.NumeroMaterial == detalle.NumeroMaterial)
                        .Sum(asn => asn.Cantidad),
                    CantidadEntregada = detalle.CantidadEntregada,
                    CantidadPedido = detalle.CantidadPedido,
                    DescripcionMaterial = detalle.Descripcion,
                    NumeroMaterial = detalle.NumeroMaterial

                };

                preAsnDetalle.Cantidad = preAsnDetalle.CantidadPermitida;
                result.Add(preAsnDetalle);
            }

            return result;


            

        }


        public List<PreAsn> GetOrdenesActivasConDetalle(int proveedorId)
        {
            var db = new Repository.Entities();
            var proveedor = db.proveedores.Single(p => p.Id == proveedorId);


            // TODO confirmar con el cliente
            const string organizacionCompras = "OC01";

            var sapOrdenCompraManager = new SapOrdenCompraManager();
            var ordenes = sapOrdenCompraManager.GetActivasConDetalle(proveedor.NumeroProveedor, organizacionCompras);

            // TODO POSIBLE CARGA DE DATOS INCORRECTA A CAUSA DE LA SINCORNIZACIÓN DE LA RECEPCIÓN CON SAP
            var asnFuturos = db.asns
                .Where(asn => asn.cita.ProveedorId == proveedor.Id && asn.cita.FechaCita > DateTime.Today).ToList();

            var result = new List<PreAsn>();

            foreach (var orden in ordenes)
            {
                var preAsn = new PreAsn
                {
                    FechaEntrega = orden.FechaEntrega,
                    NumeroDocumento = orden.NumeroDocumento,
                    NumeroProveedor = orden.NumeroProveedor,
                    FechasPermitidas = RulesManager.GetFechasPermitidas(orden.FechaEntrega, proveedor.cuenta.EsEspecial),
                };

                var detalles = orden.Detalles.Select(detalle => new PreAsnDetalle
                {
                    NumeroPosicion = detalle.NumeroPosicion, 
                    Centro = detalle.Centro, 
                    Almacen = detalle.Almacen, 
                    CantidadCitasFuturas = asnFuturos
                        .Where(asn => asn.OrdenNumeroDocumento == detalle.NumeroDocumento 
                               && asn.NumeroPosicion == detalle.NumeroPosicion 
                               && asn.NumeroMaterial == detalle.NumeroMaterial)
                        .Sum(asn => asn.Cantidad), 
                    CantidadEntregada = detalle.CantidadEntregada, 
                    CantidadPedido = detalle.CantidadPedido, 
                    DescripcionMaterial = detalle.Descripcion, 
                    NumeroMaterial = detalle.NumeroMaterial
                }).ToList();

                preAsn.Detalles = detalles;
                result.Add(preAsn);
            }


            return result.Any() ? result.Where(o => o.FechasPermitidas.Count > 0).ToList() : result;
        }

    }
}
