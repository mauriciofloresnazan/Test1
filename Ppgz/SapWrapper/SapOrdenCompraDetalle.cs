using System;
using System.Data;

namespace SapWrapper
{
    public class SapOrdenCompraDetalle
    {
        public SapOrdenCompraDetalle(DataRow dataRow)
        {
            NumeroDocumento = dataRow["EBELN"].ToString();

            NumeroPosicion = dataRow["EBELP"].ToString();

            Descripcion = dataRow["TXZ01"].ToString();

            NumeroMaterial = dataRow["MATNR"].ToString();

            Centro = dataRow["WERKS"].ToString();

            Almacen = dataRow["LGORT"].ToString();
 
            GrupoArticulos = dataRow["MATKL"].ToString();

            CantidadPedido = decimal.ToInt32(Convert.ToDecimal(dataRow["MENGE"].ToString()));

            UnidadMedidaPedido = dataRow["BPRME"].ToString();

            UnidadMedidaPrecioPedido = dataRow["BPUMZ"].ToString();

            Bpumn = Convert.ToDecimal(dataRow["BPUMN"].ToString());

            Umrez = Convert.ToDecimal(dataRow["UMREZ"].ToString());

            Umren = Convert.ToDecimal(dataRow["UMREN"].ToString());
 
            PrecioNeto = Convert.ToDecimal(dataRow["NETPR"].ToString());

            CantidadBase = Convert.ToDecimal(dataRow["PEINH"].ToString());

            ValorNeto = Convert.ToDecimal(dataRow["NETWR"].ToString());

            ValorBruto = Convert.ToDecimal(dataRow["BRTWR"].ToString());

         CantidadPorEntregar = decimal.ToInt32(Convert.ToDecimal(dataRow["ERFMG"].ToString()));

         FechaEntrega = DateTime.ParseExact(
                dataRow["EINDT"].ToString(),
                "yyyyMMdd",
                System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// EBELN Número del documento de compras
        /// </summary>
        public string NumeroDocumento;
        /// <summary>
        /// EBELP Número de posición del documento de compras
        /// </summary>
        public string NumeroPosicion;
        /// <summary>
        /// TXZ01	Texto breve
        /// </summary>
        public string Descripcion;
        /// <summary>
        /// MATNR	Número de material
        /// </summary>
        public string NumeroMaterial;
        /// <summary>
        /// WERKS	Centro
        /// </summary>
        public string Centro;
        /// <summary>
        /// LGORT	Almacén
        /// </summary>
        public string Almacen;
        /// <summary>
        /// MATKL	Grupo de artículos
        /// </summary>
        public string GrupoArticulos;
        /// <summary>
        /// MENGE	Cantidad de pedido
        /// </summary>
        public int CantidadPedido;
        /// <summary>
        /// BPRME	Unidad de medida de pedido
        /// </summary>
        public string UnidadMedidaPedido;
        /// <summary>
        /// BPUMZ	Unidad medida de precio de pedido
        /// </summary>
        public string UnidadMedidaPrecioPedido;
        /// <summary>
        /// BPUMN	Numerador para la conversión UMPRP en UMP
        /// </summary>
        public decimal Bpumn;
        /// <summary>
        /// UMREZ	Numerador para la conversión de UM pedido en UM base
        /// </summary>
        public decimal Umrez;
        /// <summary>
        /// UMREN	Denominador para la conversión de UM-pedido en UM-base
        /// </summary>
        public decimal Umren;
        /// <summary>
        /// NETPR	Precio neto en doc.compras moneda documento
        /// </summary>
        public decimal PrecioNeto;
        /// <summary>
        /// PEINH	Cantidad base
        /// </summary>
        public decimal CantidadBase;
        /// <summary>
        /// NETWR	Valor neto de pedido en moneda de pedido
        /// </summary>
        public decimal ValorNeto;
        /// <summary>
        /// BRTWR	Valor bruto del pedido en moneda de pedido
        /// </summary>
        public decimal ValorBruto;
        /// <summary>
        /// ERFMG	Cantidad en unidad de medida de entrada cantidad por entregar 
        /// segun nuevas especificaciones de consultores de SAP
        /// </summary>
        public int CantidadPorEntregar;
        /// <summary>
        /// EINDT	Fecha de entrega de posición formatoyyyyMMdd segun especificaciones
        /// </summary>
        public DateTime FechaEntrega;
        /// <summary>
        /// ELIKZ Indicador de Entrega completada devuelve el valor X
        /// </summary>
        public string EntregaCompleta;
    }
}
