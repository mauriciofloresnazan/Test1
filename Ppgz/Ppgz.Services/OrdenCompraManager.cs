using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Ppgz.Repository;
using SapWrapper;

namespace Ppgz.Services
{
    public class OrdenCompraManager
    {
        private readonly Entities _db = new Entities();

        internal ordencompra MapearOrden(DataRow dr)
        {
            try
            {
                return new ordencompra
                {
                    NumeroDocumento = dr["EBELN"].ToString(),
                    Sociedad = dr["BUKRS"].ToString(),
                    TipoDocumento = dr["BSTYP"].ToString(),
                    ClaseDocumento = dr["BSART"].ToString(),
                    IndicadorControl = dr["BSAKZ"].ToString(),
                    IndicadorBorrado = dr["LOEKZ"].ToString(),
                    EstatusDocumento = dr["STATU"].ToString(),
                    FechaCreacionSap = dr["AEDAT"].ToString(),
                    IntervaloPosicion = dr["PINCR"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["PINCR"]),
                    UltimoNumeroPos = dr["LPONR"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["LPONR"]),
                    NumeroProveedor = dr["LIFNR"].ToString(),
                    ClavePago = dr["ZTERM"].ToString(),
                    OrganizacionCompra = dr["EKORG"].ToString(),
                    GrupoCompra = dr["EKGRP"].ToString(),
                    Moneda = dr["WAERS"].ToString(),
                    TipoCambio = dr["WKURS"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["WKURS"]),
                    IndTipoCambio = dr["KUFIX"].ToString(),
                    FechaDocCompra = dr["BEDAT"].ToString(),
                    Destino = dr["CROSS_D"].ToString() == "X" ? dr["T_DEST"].ToString() :  dr["WERKS"].ToString(), 



                };
            }
            catch (Exception)
            {
                // TODO ESCRIBIR EN EL LOG
                throw new BusinessException("Error al relacionar los datos de sap con la Orden");
            }

        }

        public List<ordencompra> FindOrdenesDecompraActivas(int proveedorId)
        {
            var proveedor = _db.proveedores.FirstOrDefault(p => p.Id == proveedorId);
            if (proveedor == null)
            {
                throw new Exception(CommonMensajesResource.ERROR_Proveedor_Id);
            }

            var organizacionCompras = string.Empty;
            if (proveedor.cuenta.Tipo == CuentaManager.Tipo.Mercaderia)
            {
                organizacionCompras =
                    _db.configuraciones.Single(c => c.Clave == "rfc.common.function.param.ekorg.mercaderia").Valor;
            }
            if (proveedor.cuenta.Tipo == CuentaManager.Tipo.Servicio)
            {
                organizacionCompras =
                    _db.configuraciones.Single(c => c.Clave == "rfc.common.function.param.ekorg.servicio").Valor;
            }



            var sapOrdenCompraManager = new SapOrdenCompraManager();
            var result = sapOrdenCompraManager.GetOrdenesDeCompraHeader(proveedor.NumeroProveedor, organizacionCompras);
            var detalle = new List<ordencompra>();

            if (result.Rows.Count <= 0) return detalle;

            var numerosDocumento = result.AsEnumerable().Select(r => r["EBELN"].ToString()).ToList();

            var ordenesVistas = _db.ordencompras
                .Where(o => numerosDocumento.Contains(o.NumeroDocumento))
                .Where(o => o.ProveedorId == proveedor.Id).ToList();

            foreach (DataRow dr in result.Rows)
            {
                var numeroDocumento = dr["EBELN"].ToString();
                var ordenCompra = ordenesVistas.FirstOrDefault(o => o.NumeroDocumento == numeroDocumento);

                var orden = MapearOrden(dr);
                if (ordenCompra != null)
                {
                    orden.FechaVisualizado = ordenCompra.FechaVisualizado;
                }
                
                orden.proveedore = proveedor;
                orden.ProveedorId = proveedor.Id;
                
                detalle.Add(orden);
            }
            return detalle;
        }

        public List<ordencompra> FindOrdenesDecompraImprimir(string proveedorId)
        {
            //var proveedor = _db.proveedores.FirstOrDefault(p => p.Id == proveedorId);
            //if (proveedor == null)
            //{
            //    throw new Exception(CommonMensajesResource.ERROR_Proveedor_Id);
            //}

            var organizacionCompras = string.Empty;
           
                organizacionCompras =
                    _db.configuraciones.Single(c => c.Clave == "rfc.common.function.param.ekorg.mercaderia").Valor;
          



            var sapOrdenCompraManager = new SapOrdenCompraManager();
            var result = sapOrdenCompraManager.GetOrdenesDeCompraHeader(proveedorId , organizacionCompras);
            var detalle = new List<ordencompra>();

            if (result.Rows.Count <= 0) return detalle;

            foreach (DataRow dr in result.Rows)
            {
                var orden = MapearOrden(dr);
                var numeroDocumento = dr["EBELN"].ToString();
                detalle.Add(orden);
            }



            return detalle;
        }

        public List<ordencompra> FindOrdenesDecompraActivasByCuenta(int cuentaId)
        {
            var cuenta = _db.cuentas.Find(cuentaId);
            if (cuenta == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_Cuenta_Id);
            }

            var detalle = new List<ordencompra>();
            
            foreach (var provedor in cuenta.proveedores)
            {
                var proveedorOrdenes = FindOrdenesDecompraActivas(provedor.Id);
                if (proveedorOrdenes.Any())
                {
                    detalle.AddRange(proveedorOrdenes);
                }
            }
            return detalle;
        }
        
        public void NotificarOrdenCompra(string numeroDocumento, int proveedorId)
        {
            if (_db.ordencompras.FirstOrDefault(
             oc => oc.NumeroDocumento == numeroDocumento) != null) return;

            var proveedor = _db.proveedores.Find(proveedorId);
            if (proveedor == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_Proveedor_Id);
            }

            var organizacionCompras = string.Empty;
            if (proveedor.cuenta.Tipo == CuentaManager.Tipo.Mercaderia)
            {
                organizacionCompras =
                    _db.configuraciones.Single(c => c.Clave == "rfc.common.function.param.ekorg.mercaderia").Valor;
            }
            if (proveedor.cuenta.Tipo == CuentaManager.Tipo.Servicio)
            {
                organizacionCompras =
                    _db.configuraciones.Single(c => c.Clave == "rfc.common.function.param.ekorg.servicio").Valor;
            }


            var sapOrdenCompraManager = new SapOrdenCompraManager();
            var result = sapOrdenCompraManager.GetOrdenesDeCompraHeader(numeroDocumento, proveedor.NumeroProveedor, organizacionCompras);
            if (result.Rows.Count > 1)
            {

                throw new BusinessException("Multiples ordenes de compra con el mismo número de documento");
            }

            if (result.Rows.Count <= 0) return;
            
            var dr = result.Rows[0];

            var orden = MapearOrden(dr);
            //orden.proveedore = proveedor;
            orden.FechaVisualizado = DateTime.Today;
            orden.ProveedorId = proveedor.Id;

            _db.ordencompras.Add(orden);
            _db.SaveChanges();
        }
  
        public SapOrdenCompra FindOrdenConDetalles(int proveedorId,  string numeroDocumento)
        {
            var proveedor = _db.proveedores.Find(proveedorId);
            if (proveedor == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_Proveedor_Id);
            }

            NotificarOrdenCompra(numeroDocumento, proveedorId);

            var organizacionCompras = string.Empty;
            if (proveedor.cuenta.Tipo == CuentaManager.Tipo.Mercaderia)
            {
                organizacionCompras =
                    _db.configuraciones.Single(c => c.Clave == "rfc.common.function.param.ekorg.mercaderia").Valor;
            }
            if (proveedor.cuenta.Tipo == CuentaManager.Tipo.Servicio)
            {
                organizacionCompras =
                    _db.configuraciones.Single(c => c.Clave == "rfc.common.function.param.ekorg.servicio").Valor;
            }

            var sapOrdenCompraManager = new SapOrdenCompraManager();

            return  sapOrdenCompraManager.GetOrdenConDetalle(proveedor.NumeroProveedor, organizacionCompras, numeroDocumento);
        }
        public SapOrdenCompra FindOrdenConDetallesSN(int proveedorId, string numeroDocumento)
        {
            var proveedor = _db.proveedores.Find(proveedorId);
            if (proveedor == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_Proveedor_Id);
            }

            var organizacionCompras = string.Empty;
            if (proveedor.cuenta.Tipo == CuentaManager.Tipo.Mercaderia)
            {
                organizacionCompras =
                    _db.configuraciones.Single(c => c.Clave == "rfc.common.function.param.ekorg.mercaderia").Valor;
            }
            if (proveedor.cuenta.Tipo == CuentaManager.Tipo.Servicio)
            {
                organizacionCompras =
                    _db.configuraciones.Single(c => c.Clave == "rfc.common.function.param.ekorg.servicio").Valor;
            }

            var sapOrdenCompraManager = new SapOrdenCompraManager();

            return sapOrdenCompraManager.GetOrdenConDetalle(proveedor.NumeroProveedor, organizacionCompras, numeroDocumento);
        }

    }


}
