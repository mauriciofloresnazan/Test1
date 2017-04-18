using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Ppgz.Repository;
using SapWrapper;

namespace Ppgz.Services
{
    public class AsnManager
    {
        private readonly Entities _db = new Entities();

        public List<DateTime> GetFechasPermitidas(string numeroDocumento, int proveedorId)
        {
            var proveedor = _db.proveedores.Find(proveedorId);
            if (proveedor == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_Proveedor_Id);
            }

            var sapOrdenCompraManager = new SapOrdenCompraManager();
            var result = sapOrdenCompraManager.GetOrdenDeCompraDetalle(numeroDocumento, proveedor.NumeroProveedor);

            var fechasPermitidas = new List<DateTime>();

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

                        if (EsFechaValida(day, proveedor))
                        {
                            fechasPermitidas.Add(day);
                        }
   
                    }

                    day = day.AddDays(1);
                }

            }

            return fechasPermitidas;
        }


        internal bool EsFechaValida(DateTime dateTime, proveedore proveedor)
        {
            var fecha = dateTime.Date;

            var manana = DateTime.Today.Date.AddDays(1);
            // Las fechas deben ser superiores al dia en curso
            if (fecha < manana)
            {
                return false;
            }

            // Solo tien hasta las 5pm del día en curso se poder incluir el día siguiente 
            // TODO pasar al la tabla de configuraciones
            if (fecha == manana)
            {
                if (DateTime.Now.Hour > 17)
                {
                    return false;
                }
            }
            
            var diaDeSemana = fecha.DayOfWeek.ToString();
            if (!proveedor.cuenta.EsEspecial)
            {
                // TODO OBTENER DE CONFIGURACIONES
                var diasEspeciales = new[]
                {
                    "",
                };
                if (diasEspeciales.Contains(diaDeSemana))
                {
                    return false;
                }
            }
           
            // TODO INCLUIR SOLO LOS DIAS CONFIGURADOS PARA OPERAR
            return true;
        }

        public List<PreAsnDetail> GetPreAsnDetails(string numeroDocumento, string numeroProveedor)
        {
            var sapOrdenCompraManager = new SapOrdenCompraManager();
            var result = sapOrdenCompraManager.GetOrdenDeCompraDetalle(numeroDocumento, numeroProveedor);
            var detalle = new List<PreAsnDetail>();

            if (result.Rows.Count > 0)
            {

                foreach (DataRow dr in result.Rows)
                {
                    var asnDetail = new PreAsnDetail
                    {
                        NumeroDocumento = dr["EBELN"].ToString(),
                        Ebelp = dr["Ebelp"].ToString(),
                        DescripcionMaterial = dr["TXZ01"].ToString(),
                        NumeroMaterial = dr["MATNR"].ToString(),
                        Centro = dr["WERKS"].ToString(),
                        Almacen = dr["LGORT"].ToString(),
                        GrupoArticulos = dr["MATKL"].ToString(),
                        UnidadMedidaPedido = dr["BPRME"].ToString(),
                        UnidadMedidaPrecio = dr["BPUMZ"].ToString(),
                        ConversionUmpruMP = dr["BPUMN"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["BPUMN"]),
                        DenominadorUmUmBase = dr["UMREZ"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["UMREZ"]),
                        PrecioNeto = dr["NETPR"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(dr["NETPR"]),
                        CantidadBase = dr["PEINH"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["PEINH"]),

                        ValorNeto =
                            dr["NETWR"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["NETWR"]),

                        CantidadPedido = decimal.ToInt32((decimal)dr["MENGE"]),
                        CantidadEntregada = decimal.ToInt32((decimal)dr["ERFMG"]),

                    };
                    asnDetail.CantidadComprometida = asnDetail.CantidadPermitidaSap;

                    // TODO BUSCAR ESTA INFORMACION EN LA BASE DE DATOS
                    asnDetail.CantidadCitasFuturas = 0;
                    detalle.Add(asnDetail);
                }


            }
            return detalle;
        }


    }

    public class PreAsn : ordencompra
    {

        public ICollection<PreAsnDetail> PreAsnDetails { get; set; }

        public int TotalPedido
        {
            get
            {
                return PreAsnDetails.AsEnumerable().Sum(x => x.CantidadPedido);
            }
        }
        public int TotalPermitido
        {
            get
            {
                return PreAsnDetails.AsEnumerable().Sum(x => x.CantidadPermitida);
            }
        }
    }

    public class PreAsnDetail : ordencompradetalle
    {
        /// <summary>
        /// MENGE
        /// </summary>
        public new int CantidadPedido { get; set; }
        /// <summary>
        /// ERFMG
        /// </summary>
        public int CantidadEntregada { get; set; }
        /// <summary>
        /// ERFMG - ERFMG
        /// </summary>
        public int CantidadPermitidaSap
        {
            get { return CantidadPedido - CantidadEntregada; }
        }

        // TODO VALIDAR EL CALCULO CON LAS REGLAS DE NEGOCIO
        /// <summary>
        /// Cantidad registrada en citas superiores a la fecha actual
        /// </summary>
        public int CantidadCitasFuturas { get; set; }

        /// <summary>
        /// CantidadPermitidaSap - CantidadCitasFuturas
        /// </summary>
        public int CantidadPermitida
        {
            get { return CantidadPermitidaSap - CantidadCitasFuturas; }
        }
        /// <summary>
        /// Cantidad del ASN
        /// </summary>
        public int Cantidad { get; set; }


    }
}
