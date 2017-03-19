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

        public List<sapordencompradetalle> FindDetalleByDocumento(string documento)
        {

            var sapOrdenCompraManager = new SapOrdenCompraManager();
            var result = sapOrdenCompraManager.GetOrdenDeCompraDetalle(documento);
            var detalle = new List<sapordencompradetalle>();

            if (result.Rows.Count > 0)
            {
                detalle.AddRange(from DataRow dr in result.Rows
                    select new sapordencompradetalle
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

    }
}
