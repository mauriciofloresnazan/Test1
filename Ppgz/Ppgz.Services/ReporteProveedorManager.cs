using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Ppgz.Repository;
using SapWrapper;

namespace Ppgz.Services
{
    public class ReporteProveedorManager
    {
        public class ReporteProveedor
        {
            public string NumeroProveedor { get; set; }
            public string NombreProveedor { get; set; }
            public string Material { get; set; }
            public string NombreMaterial { get; set; }
            public string FechaProceso { get; set; }
            public string UnidadMedida { get; set; }
            public string CantidadVentas2 { get; set; }
            public string CantidadVentas1 { get; set; }
            public string CantidadVentas { get; set; }
            public string CantidadTotal { get; set; }
            public string CalculoTotal { get; set; }
            public string InvTienda { get; set; }
            public string InvTransito { get; set; }
            public string InvCedis { get; set; }
            public string PedidosPendiente { get; set; }
            public string EstadoMaterial { get; set; }

        }

        private readonly Entities _db = new Entities();


        public List<ReporteProveedor> FindReporteProveedor(string codigoProveedor)
        {

            var bwreporteproveedorManager = new BwReporteProveedorManager();
            var result = bwreporteproveedorManager.GetReporteProveedor(codigoProveedor);

            var reporteProveedor = new List<ReporteProveedor>();

            if (result.Rows.Count > 0)
            {
                reporteProveedor.AddRange(from DataRow dr in result.Rows
                    select new ReporteProveedor
                    {
                        NumeroProveedor = dr["VENDOR"].ToString(),
                        NombreProveedor = dr["VENDOR_TXT"].ToString(),
                        Material = dr["MATERIAL"].ToString(),
                        NombreMaterial = dr["MATERIAL_TXT"].ToString(),
                        FechaProceso = dr["CALDAY"].ToString(),
                        UnidadMedida = dr["BASE_UOM"].ToString(),
                        CantidadVentas2 = dr["MESACT2"].ToString(),
                        CantidadVentas1 = dr["MESACT1"].ToString(),
                        CantidadVentas = dr["MESACT"].ToString(),
                        CantidadTotal = dr["TOTAL"].ToString(),
                        CalculoTotal = dr["SELLTHRU"].ToString(),
                        InvTienda = dr["INVENTDA"].ToString(),
                        InvTransito = dr["TRANSITO"].ToString(),
                        InvCedis = dr["INVENCEDIS"].ToString(),
                        PedidosPendiente = dr["PEDIDOSPEN"].ToString(),
                        EstadoMaterial = dr["ZMMSTA"].ToString()
                    });
            }
            return reporteProveedor;

        }

    }
}
