using System;
using System.Collections;
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

        public List<ordencompra> FindOrdenesDecompraActivas(int proveedorId)
        {
            var proveedor = _db.proveedores.FirstOrDefault(p => p.Id == proveedorId);
            if (proveedor == null)
            {
                throw new Exception(CommonMensajesResource.ERROR_Proveedor_Id);
            }

            var sapOrdenCompraManager = new SapOrdenCompraManager();
            var result = sapOrdenCompraManager.GetOrdenesDeCompraHeader(proveedor.NumeroProveedor);
            var detalle = new List<ordencompra>();

            if (result.Rows.Count <= 0) return detalle;

            foreach (DataRow dr in result.Rows)
            {
                var numeroDocumento = dr["EBELN"].ToString();
                var ordenCompra = _db.ordencompras.FirstOrDefault(o => o.NumeroDocumento == numeroDocumento);

                DateTime? fechaVisualizado = null;
                if (ordenCompra != null)
                {
                    fechaVisualizado = ordenCompra.FechaVisualizado;
                }
                 
                detalle.Add(new ordencompra
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
                    TipoCambio = dr["WKURS"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["ERFMG"]),
                    IndTipoCambio = dr["KUFIX"].ToString(),
                    FechaDocCompra = dr["BEDAT"].ToString(),
                    ProveedorId = proveedor.Id,
                    FechaVisualizado = fechaVisualizado,
                  
                });
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

        public Hashtable FindActivaByIdAndUsuarioId(int id, string usuarioId)
        {

            var  orden = _db.ordencompras.FirstOrDefault(
                o => o.Id == id);

            var ordenDetalle = _db.detalleordencompras.FirstOrDefault(
                o => o.OrdenComprasId == id);

            var result = new Hashtable
            {
                {"orden", orden}, 
                {"ordenDetalle", ordenDetalle}
            };

            return result;

        }

        public List<ordencompradetalle> FindDetalleByDocumento(string numeroDocumento)
        {

            var sapOrdenCompraManager = new SapOrdenCompraManager();
            var result = sapOrdenCompraManager.GetOrdenDeCompraDetalle(numeroDocumento);
            var detalle = new List<ordencompradetalle>();

            if (result.Rows.Count > 0)
            {
                detalle.AddRange(from DataRow dr in result.Rows
                                 select new ordencompradetalle
                    {
                        NumeroDocumento = dr["EBELN"].ToString(),
                        Ebelp = dr["Ebelp"].ToString(),
                        DescripcionMaterial = dr["TXZ01"].ToString(),
                        NumeroMaterial = dr["MATNR"].ToString(),
                        Centro = dr["WERKS"].ToString(),
                        Almacen = dr["LGORT"].ToString(),
                        GrupoArticulos = dr["MATKL"].ToString(),
                        CantidadPedido = dr["MENGE"].ToString(),
                        UnidadMedidaPedido = dr["BPRME"].ToString(),
                        UnidadMedidaPrecio = dr["BPUMZ"].ToString(),
                        ConversionUmpruMP = dr["BPUMN"] == DBNull.Value ? (int?) null : Convert.ToInt32(dr["BPUMN"]),
                        DenominadorUmUmBase = dr["UMREZ"] == DBNull.Value ? (int?) null : Convert.ToInt32(dr["UMREZ"]),
                        PrecioNeto = dr["NETPR"] == DBNull.Value ? (decimal?) null : Convert.ToDecimal(dr["NETPR"]),
                        CantidadBase = dr["PEINH"] == DBNull.Value ? (int?) null : Convert.ToInt32(dr["PEINH"]),
                        ValorNeto =
                            dr["NETWR"] == DBNull.Value ? (int?) null : Convert.ToInt32(dr["NETWR"]),
                        CantidadUnidadMedida =
                            dr["ERFMG"] == DBNull.Value ? (decimal?) null : Convert.ToDecimal(dr["ERFMG"]),
                        /*SapOrdenCompraId = dr[""].ToString(),*/ //TODO
                        /*FechaCargaPortal = dr[""].ToString(),*/ //TODO
                    });
            } 
            return detalle;
        }


        public List<ordencompradetalle> FindDetalleByProveedorIdAndNumeroDocumento(int proveedorId, string numeroDocumento)
        {
            var proveedor = _db.proveedores.Find(proveedorId);
            if (proveedor == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_Proveedor_Id);
            }

            var ordenesCompraActivas = FindOrdenesDecompraActivas(proveedorId);

            var ordenCompraActiva = ordenesCompraActivas.FirstOrDefault(
                o => o.NumeroDocumento == numeroDocumento && o.NumeroProveedor == proveedor.NumeroProveedor);
            if (ordenCompraActiva == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_OrdenCompraActiva_NumeroDocumento);
            }

            return FindDetalleByDocumento(ordenCompraActiva.NumeroDocumento);

        }

    }
}
