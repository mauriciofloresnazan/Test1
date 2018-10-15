using Ppgz.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Ppgz.Web.Models.ProntoPago
{
	public class TotalView
	{
		public double MontoOriginal { get; set; }
		public double DescuentosTotal { get; set; }
		public double DescuentoProntoPago { get; set; }
		public double Interes { get; set; }
		public double TotalSolicitado { get; set; }
		public solicitudesfactoraje SolicitudFactoraje { get; set; }
		public List<facturasfactoraje> FacturasFactoraje { get; set; }
		public TotalView(DataSet dsPagosPendientes, int porcentaje, int idProveedor, string[] facturasList)
		{
			MontoOriginal = 0;
			DescuentosTotal = 0;
			DescuentoProntoPago = 0;
			Interes = 0;
			double Descuentos = 0;

			FacturasFactoraje = new List<facturasfactoraje>();

			for (int i = 0; i < dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows.Count; i++)
			{

				int dias = 0;

				Web.Models.ProntoPago.FacturaView item = new Web.Models.ProntoPago.FacturaView()
				{
					referencia = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["XBLNR"].ToString(),
					importe = Convert.ToDouble(dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["DMBTR"].ToString()),
					ml = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["WAERS"].ToString(),
					vencimiento = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["FECHA_PAGO"].ToString(),
					tipoMovimiento = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["BLART"].ToString(),
					fechaDocumento = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["BLDAT"].ToString(),
					descripcion = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["SGTXT"].ToString(),
					pagar = false,
					idProveedor = idProveedor.ToString(),
					numeroDocumento = dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["BELNR"].ToString(),
					porcentaje = porcentaje
				};

				

				//Acumulamos el monto original
				if (item.importe < 0)
					Descuentos = Descuentos + item.importe;

				//si la factura esta en la lista
				if (facturasList.Contains(dsPagosPendientes.Tables["T_PARTIDAS_ABIERTAS"].Rows[i]["BELNR"].ToString()))
				{
					//Acumulamos el monto original y los descuentos
					if (item.importe > 0)
					{
						MontoOriginal = MontoOriginal + item.importe;
						//DescuentoProntoPago = DescuentoProntoPago + (item.importe * (Convert.ToDouble(item.porcentaje) / 100));
						//Dias de diferencia de la fecha de documento a la fecha de pago
						DateTime fechaDocumento = DateTime.ParseExact(item.fechaDocumento, "yyyyMMdd", CultureInfo.InvariantCulture);
						DateTime vencimiento = DateTime.ParseExact(item.vencimiento, "yyyyMMdd", CultureInfo.InvariantCulture);
						dias = (int)(vencimiento - fechaDocumento).TotalDays;
						double porcentajeAux = (Convert.ToDouble(item.porcentaje) / 100);
						Interes = Interes + (item.importe * ((porcentajeAux) / 30) * dias);
					}
					//Guardamos la factura factoraje
					FacturasFactoraje.Add(new facturasfactoraje()
					{
						FacturaEstatus = 1,
						DiasPP = dias,
						FechaFactura = DateTime.ParseExact(item.fechaDocumento, "yyyyMMdd", CultureInfo.InvariantCulture),
						FechaSolicitud = DateTime.Now,
						IdProveedor = idProveedor,
						Monto = item.importe.ToString(),
						NumeroDocumento = Convert.ToInt32("123"),
						Porcentaje = porcentaje,
						Referencia = item.referencia,
						uuid = "uuid",
					});
				}

				
				Descuentos = Descuentos * -1;
				MontoOriginal = MontoOriginal;
				DescuentosTotal = Descuentos;
				DescuentoProntoPago = Interes;
				TotalSolicitado = (MontoOriginal - Descuentos - Interes);


				//Creamos la solicitud
				SolicitudFactoraje = new solicitudesfactoraje()
				{
					IdProveedor = idProveedor,
					FechaSolicitud = DateTime.Now,
					EstatusFactoraje = 1,
					Comentario = "",
					xml = "",
					pdf = "",
					MontoOriginal = (int)MontoOriginal,
					MontoAFacturar = (int)TotalSolicitado,
					Descuentos = (int)DescuentosTotal,
					DescuentoPP =(int)Interes,
					DiasPP = 1,
					EstatusOperacionSAP = "",
					NDocumentos = 0,
					NumeroGenerado = 0
				};
			}
		}
	}
}