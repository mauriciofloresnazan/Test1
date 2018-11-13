using Ppgz.Repository;
using Ppgz.Services;
using Ppgz.Web.Infrastructure;
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
		public List<descuentofactoraje> DescuentosFactoraje { get; set; }
		public TotalView(DataSet dsPagosPendientes, int porcentaje, int idProveedor, string[] facturasList)
		{
			MontoOriginal = 0;
			DescuentosTotal = 0;
			DescuentoProntoPago = 0;
			Interes = 0;
			double Descuentos = 0;

			FacturasFactoraje = new List<facturasfactoraje>();
			DescuentosFactoraje = new List<descuentofactoraje>();

			string DiaPago = CommonManager.GetConfiguraciones().Single(c => c.Clave == "prontopago.default.day").Valor;
			DateTime FechaPago = DateTime.Now;

			switch (DiaPago)
			{
				case "LUNES":
					FechaPago = GetFechaPago(DayOfWeek.Monday);
					break;
				case "MARTES":
					FechaPago = GetFechaPago(DayOfWeek.Tuesday);
					break;
				case "MIERCOLES":
					FechaPago = GetFechaPago(DayOfWeek.Wednesday);
					break;
				case "JUEVES":
					FechaPago = GetFechaPago(DayOfWeek.Thursday);
					break;
				case "VIERNES":
					FechaPago = GetFechaPago(DayOfWeek.Friday);
					break;
				case "SABADO":
					FechaPago = GetFechaPago(DayOfWeek.Saturday);
					break;
				case "DOMINGO":
					FechaPago = GetFechaPago(DayOfWeek.Sunday);
					break;
			}

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
				{
					//Agregamos la factura a los descuentos
					DescuentosFactoraje.Add(new descuentofactoraje()
					{
						Monto = item.importe.ToString(),
						Descripcion = item.descripcion,
						EstatusFactoraje = 1,
						NumeroDocumento = item.numeroDocumento
					});
					Descuentos = (Descuentos > 0) ? Descuentos*-1 + item.importe: Descuentos + item.importe ;
				}

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
						dias = (int)(vencimiento - FechaPago).TotalDays;
						double porcentajeAux = (Convert.ToDouble(item.porcentaje) / 100);
                        item.interes = (item.importe * ((porcentajeAux) / 30) * dias);
                        Interes = Interes + item.interes;
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
						NumeroDocumento = item.numeroDocumento,
						Porcentaje = porcentaje,
						Referencia = item.referencia, 
                        interes = item.interes,
						uuid = "uuid",
					});
				}

				
				Descuentos = (Descuentos>0)? Descuentos : Descuentos * -1;

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
					Comentario = "Recien creada",
					MontoOriginal = (int)MontoOriginal,
					MontoAFacturar = (int)TotalSolicitado,
					Descuentos = (int)DescuentosTotal,
					DescuentoPP =(int)Interes,
					DiasPP = 11,//FacturasFactoraje.Sum(x=>x.DiasPP)/FacturasFactoraje.Count, //promedio de los dias
					NDocumentos = FacturasFactoraje.Count + DescuentosFactoraje.Count,
					EstatusOperacionSAP = "",
					xml = "",
					pdf = "",
					NumeroGenerado = 0
				};
			}
		}
		public TotalView(int idSolicitud, string[] facturas, string[] descuentos)
		{
			MontoOriginal = 0;
			DescuentosTotal = 0;
			DescuentoProntoPago = 0;
			Interes = 0;
			double Descuentos = 0;

			FacturasFactoraje = new List<facturasfactoraje>();
			DescuentosFactoraje = new List<descuentofactoraje>();

			SolicitudFManager solicitudFManager = new SolicitudFManager();
			FacturaFManager facturaFManager = new FacturaFManager();
			DescuentoFManager descuentoFManager = new DescuentoFManager();

			//SolicitudFactoraje = solicitudFManager.GetSolicitudById(idSolicitud);
			FacturasFactoraje = facturaFManager.GetFacturasBySolicitud(idSolicitud);
			DescuentosFactoraje = descuentoFManager.GetDescuentosBySolicitud(idSolicitud);

			//Calculamos los descuentos
			foreach(descuentofactoraje item in DescuentosFactoraje)
			{
				//Validamos si el descuento esta marcado
				if (descuentos.Contains(item.idDescuentosFactoraje.ToString()))
				{
					Descuentos = Descuentos + Convert.ToDouble(item.Monto);
				}
			}

			foreach(facturasfactoraje item in FacturasFactoraje)
			{
				//Validamos si la factura esta marcada
				if (facturas.Contains(item.idFacturasFactoraje.ToString()))
				{
					double porcentaje = (Convert.ToDouble(item.Porcentaje) / 100);
					MontoOriginal = MontoOriginal + Convert.ToDouble(item.Monto);
					//Calculamos el interes
					Interes = Interes + item.interes;
				}
			}
			
			Descuentos = Descuentos * -1;
			MontoOriginal = MontoOriginal;
			DescuentosTotal = Descuentos;
			DescuentoProntoPago = Interes;
			TotalSolicitado = (MontoOriginal - Descuentos - Interes);
		}
		public DateTime GetFechaPago(DayOfWeek day)
		{
			DateTime result = DateTime.Now.AddDays(1);
			while (result.DayOfWeek != day)
				result = result.AddDays(1);
			return result;
		}

        public TotalView(int idSolicitud)
        {
            MontoOriginal = 0;
            DescuentosTotal = 0;
            DescuentoProntoPago = 0;
            Interes = 0;
            double Descuentos = 0;

            FacturasFactoraje = new List<facturasfactoraje>();
            DescuentosFactoraje = new List<descuentofactoraje>();

            SolicitudFManager solicitudFManager = new SolicitudFManager();
            FacturaFManager facturaFManager = new FacturaFManager();
            DescuentoFManager descuentoFManager = new DescuentoFManager();

            //SolicitudFactoraje = solicitudFManager.GetSolicitudById(idSolicitud);
            FacturasFactoraje = facturaFManager.GetFacturasBySolicitud(idSolicitud);
            DescuentosFactoraje = descuentoFManager.GetDescuentosBySolicitud(idSolicitud);

            //Calculamos los descuentos
            foreach (descuentofactoraje item in DescuentosFactoraje)
            {
                //Validamos si el descuento esta marcado
                Descuentos = Descuentos + Convert.ToDouble(item.Monto);
            }

            foreach (facturasfactoraje item in FacturasFactoraje)
            {
                //Validamos si la factura esta marcada
                double porcentaje = (Convert.ToDouble(item.Porcentaje) / 100);
                MontoOriginal = MontoOriginal + Convert.ToDouble(item.Monto);
                //Calculamos el interes
                Interes = Interes + item.interes;
            }

            Descuentos = Descuentos * -1;
            MontoOriginal = MontoOriginal;
            DescuentosTotal = Descuentos;
            DescuentoProntoPago = Interes;
            TotalSolicitado = (MontoOriginal - Descuentos - Interes);
        }

        public TotalView(int idSolicitud, string[] facturas, string[] descuentos, List<descuentofactoraje> listdescuentos)
        {
            MontoOriginal = 0;
            DescuentosTotal = 0;
            DescuentoProntoPago = 0;
            Interes = 0;
            double Descuentos = 0;

            FacturasFactoraje = new List<facturasfactoraje>();
            DescuentosFactoraje = new List<descuentofactoraje>();

            SolicitudFManager solicitudFManager = new SolicitudFManager();
            FacturaFManager facturaFManager = new FacturaFManager();
            DescuentoFManager descuentoFManager = new DescuentoFManager();

            //SolicitudFactoraje = solicitudFManager.GetSolicitudById(idSolicitud);
            FacturasFactoraje = facturaFManager.GetFacturasBySolicitud(idSolicitud);
            DescuentosFactoraje = listdescuentos;

            //Calculamos los descuentos
            foreach (descuentofactoraje item in DescuentosFactoraje)
            {
                //Validamos si el descuento esta marcado
                if (descuentos.Contains(item.NumeroDocumento.ToString()))
                {
                    Descuentos = Descuentos + Convert.ToDouble(item.Monto);
                }
            }

            foreach (facturasfactoraje item in FacturasFactoraje)
            {
                //Validamos si la factura esta marcada
                if (facturas.Contains(item.idFacturasFactoraje.ToString()))
                {
                    double porcentaje = (Convert.ToDouble(item.Porcentaje) / 100);
                    MontoOriginal = MontoOriginal + Convert.ToDouble(item.Monto);
                    //Calculamos el interes
                    Interes = Interes + item.interes;
                }
            }

            Descuentos = Descuentos * -1;
            MontoOriginal = MontoOriginal;
            DescuentosTotal = Descuentos;
            DescuentoProntoPago = Interes;
            TotalSolicitado = (MontoOriginal - Descuentos - Interes);
        }
    }
}