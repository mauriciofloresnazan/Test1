using System;
using System.Data;

namespace SapWrapper
{
    public class SapOrdenCompra
    {
        public SapOrdenCompra(DataRow dataRow)
        {
            NumeroDocumento = dataRow["EBELN"].ToString();
            Sociedad = dataRow["BUKRS"].ToString();
            TipoDocumento = dataRow["BSTYP"].ToString();
            ClaseDocumento = dataRow["BSART"].ToString();
            IndicadorControl = dataRow["BSAKZ"].ToString();
            IndicadorBorrado = dataRow["LOEKZ"].ToString();
            EstatusDocumento = dataRow["STATU"].ToString();
            FechaRegistro = dataRow["AEDAT"].ToString();
            IntervaloPosicion = dataRow["PINCR"] .ToString();
            Lponr = dataRow["LPONR"].ToString();
            NumeroProveedor = dataRow["LIFNR"].ToString();
            Zterm = dataRow["ZTERM"].ToString();
            OrganizacionCompras = dataRow["EKORG"].ToString();
            GrupoCompras = dataRow["EKGRP"].ToString();
            ClaveMoneda = dataRow["WAERS"].ToString();
            TipoCambioMoneda = (decimal)dataRow["WKURS"];
            Kufix = dataRow["KUFIX"].ToString();
            FechaDocumento = dataRow["BEDAT"].ToString();
            FechaEntrega = DateTime.ParseExact(
                dataRow["EINDT"].ToString(), 
                "yyyyMMdd", 
                System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// EBELN
        /// </summary>
        public String NumeroDocumento;

        /// <summary>
        /// BUKRS
        /// </summary>
        public String Sociedad;

        /// <summary>
        /// BSTYP
        /// </summary>
        public String TipoDocumento;

        /// <summary>
        /// BSART
        /// </summary>
        public String ClaseDocumento;

        /// <summary>
        /// BSAKZ - Indicador de control para clase de documento de compras
        /// </summary>
        public String IndicadorControl;

        /// <summary>
        /// LOEKZ Indicador de borrado en el documento de compras
        /// </summary>
        public String IndicadorBorrado;

        /// <summary>
        /// STATU
        /// </summary>
        public String EstatusDocumento;

        /// <summary>
        /// AEDAT
        /// </summary>
        public String FechaRegistro;

        /// <summary>
        /// PINCR Intervalo de posición
        /// </summary>
        public String IntervaloPosicion;

        /// <summary>
        /// LPONR Último número de posición
        /// </summary>
        public String Lponr;

        /// <summary>
        /// LIFNR
        /// </summary>
        public String NumeroProveedor;

        /// <summary>
        /// ZTERM Clave de condiciones de pago
        /// </summary>
        public String Zterm;

        /// <summary>
        /// EKORG
        /// </summary>
        public String OrganizacionCompras;

        /// <summary>
        /// EKGRP
        /// </summary>
        public String GrupoCompras;

        /// <summary>
        /// WAERS
        /// </summary>
        public String ClaveMoneda;

        /// <summary>
        /// WKURSp
        /// </summary>
        public Decimal TipoCambioMoneda;

        /// <summary>
        /// KUFIX Indicador: Fijación del tipo de cambio
        /// </summary>
        public String Kufix;

        /// <summary>
        /// BEDAT
        /// </summary>
        public String FechaDocumento;

        /// <summary>
        /// EINDT con formato YYYYMMDD
        /// </summary>
        public DateTime FechaEntrega;


    }


}
