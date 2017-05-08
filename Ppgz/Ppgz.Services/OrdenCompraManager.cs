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
                    FechaDocCompra = dr["BEDAT"].ToString()

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

                throw new BusinessException("Multiples ordenes de compra con el miso numero de documento");
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
/*
        public ordencompra FindOrdenCompra(string numeroDocumento, int proveedorId)
        {
            var proveedor = _db.proveedores.FirstOrDefault(p => p.Id == proveedorId);
            if (proveedor == null)
            {
                throw new Exception(CommonMensajesResource.ERROR_Proveedor_Id);
            }

            var orden = FindOrdenCompraActiva(numeroDocumento, proveedorId);

            if (orden == null) return null;

            NotificarOrdenCompra(numeroDocumento,proveedorId);

            return orden;
        }
        */
        public ordencompra FindOrdenCompraActiva(string numeroDocumento, int proveedorId)
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
            var result = sapOrdenCompraManager.GetOrdenesDeCompraHeader(numeroDocumento, proveedor.NumeroProveedor, organizacionCompras);


            if (result.Rows.Count > 1)
                throw new BusinessException("Multiples ordenes de compra con el miso numero de documento");

            if (result.Rows.Count <= 0) return null;
            
            var dr = result.Rows[0];

            var ordenCompra = _db.ordencompras.FirstOrDefault(o => o.NumeroDocumento == numeroDocumento);
            
            var orden = MapearOrden(dr);
            orden.proveedore = proveedor;
            orden.ProveedorId = proveedor.Id;

            if (ordenCompra == null) return orden;
            
            orden.FechaVisualizado = ordenCompra.FechaVisualizado;
 
            return orden;
        }

  
        public List<ordencompradetalle> FindDetalleByDocumento(string numeroDocumento, string numeroProveedor, string organizacionCompras)
        {

            var sapOrdenCompraManager = new SapOrdenCompraManager();
            var result = sapOrdenCompraManager.GetOrdenDeCompraDetalle(numeroDocumento, numeroProveedor, organizacionCompras);
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

                        CantidadComprometida = Convert.ToDecimal(dr["MENGE"]),

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
        
        public List<ordencompradetalle> FindDetalle(string numeroDocumento, int proveedorId)
        {
            var proveedor = _db.proveedores.Find(proveedorId);
            if (proveedor == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_Proveedor_Id);
            }

            var ordenCompraActiva = FindOrdenCompraActiva(numeroDocumento,proveedorId);
            if (ordenCompraActiva == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_OrdenCompraActiva_NumeroDocumento);
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
            
            return FindDetalleByDocumento(ordenCompraActiva.NumeroDocumento, proveedor.NumeroProveedor, organizacionCompras);

        }
        /*  
          public List<DateTime> GetAvailableDatesByOrdenCompra(string numeroDocumento, int proveedorId)
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
              var result = sapOrdenCompraManager.GetOrdenDeCompraDetalle(numeroDocumento, proveedor.NumeroProveedor, organizacionCompras);

              var availableDates = new List<DateTime>();

              if (result.Rows.Count > 0)
              {
                  // Se toma la fecha de entrega del primer registro del detalle de acuerdo a la solicitud del cliente
                  var sapFechaEntrega = DateTime.ParseExact(
                      result.Rows[0]["EINDT"].ToString(), 
                      "yyyyMMdd", 
                      CultureInfo.InvariantCulture);

                  // Se calcula el numero de la semana del año
                  var semana = CultureInfo
                      .GetCultureInfo("es-MX")
                      .Calendar
                      .GetWeekOfYear(sapFechaEntrega, CalendarWeekRule.FirstDay, sapFechaEntrega.DayOfWeek);

                  // TODO MEJORAR
                  // Se agregan los días del rango entre 2 semanas antes y 2 semanas despues de la fecha de entrega
                  var day = sapFechaEntrega.AddDays(-30);

                  while (day < sapFechaEntrega.AddDays(30))
                  {
                      var semana2 = CultureInfo
                          .GetCultureInfo("es-MX")
                          .Calendar
                          .GetWeekOfYear(day, CalendarWeekRule.FirstDay, day.DayOfWeek);

                      if (semana2 >= semana - 2 && semana2 <= semana + 2)
                      {
                          // TODO INCLUIR SOLO LOS DIAS CONFIGURADOS PARA OPERAR
                          // TODO EXCLUIR LOS DIAS ESPECIALES SI EL PROVEEDOR NO TIENE ESA CATEGÍA

                          availableDates.Add(day);
                      }
                      day = day.AddDays(1);
                  }
            
              }

              return availableDates;
          }
      
          public ordencompra FindOrdenCompraActiva(string numeroDocumento, int proveedorId, string fecha)
          {
              // TODO HACER
              return FindOrdenCompraActiva(numeroDocumento, proveedorId);
          }  */




    }


}
