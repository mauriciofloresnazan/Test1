using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Ppgz.Repository;
using SapWrapper;
using SatWrapper;
using ScaleWrapper;



namespace Ppgz.Services
{
    public class FacturaManager
    {
        private readonly Entities _db = new Entities();

        public List<factura> GetFacturas(int proveedorId)
        {
            return _db.facturas.Where(f => f.proveedor_id == proveedorId).ToList();

        }
        public factura Find(int facturaId, int proveedorId)
        {
            return _db.facturas.FirstOrDefault(f => f.Id == facturaId && f.proveedor_id == proveedorId);
        }

        public void ValidarFactura(Stream xmlFileStream)
        {
            var serializer = new XmlSerializer(typeof(Comprobante));

            try
            {

                serializer.Deserialize(xmlFileStream);
            }
            catch (Exception)
            {

                throw new BusinessException("-1 - Documento no validable (¿no es un cfdi?)");
            }
        }

        public void ProcesarFacturasAtoradas()
        {
            List<factura> Facturas = _db.facturas.Where(f => f.Procesado == false).ToList();

            foreach ( factura itemFactura in Facturas)
            {
                var resultado=CargarFacturaAtorada(itemFactura.proveedor_id, itemFactura.XmlRuta);
                if (resultado != null)
                {
                    itemFactura.Estatus = resultado.Estatus;
                    itemFactura.Procesado = true;
                    if (itemFactura.Estatus != "S" && itemFactura.Estatus != "H")
                    {
                        itemFactura.Comentario = string.Format
                            ("Tipo:{1} {0}Mensaje:{2}",
                            Environment.NewLine,
                            resultado.ErrorTable.Rows[0]["TYPE"],
                            resultado.ErrorTable.Rows[0]["MESSAGE"]);
                    }
                    else
                    {
                        itemFactura.NumeroGenerado = resultado.FacturaNumero;
                        itemFactura.Comentario = "Factura Procesada";
                   
                    }
                    _db.SaveChanges();
                }
            }

        }


        //CargarFacturas de Servicio
        public void CargarFacturaServicio(int proveedorId, int cuentaId, string xmlFilePath, string pdfFilePath)
        {
            // Validaciones de los archivos
            if (!File.Exists(xmlFilePath))
            {
                throw new BusinessException("Xml incorrecto");
            }
            if (Path.GetExtension(xmlFilePath).ToLower() != ".xml")
            {
                throw new BusinessException("Xml incorrecto");
            }
            if (!File.Exists(pdfFilePath))
            {
                throw new BusinessException("Pdf incorrecto");
            }
            if (Path.GetExtension(pdfFilePath).ToLower() != ".pdf")
            {
                throw new BusinessException("Pdf incorrecto");
            }

            // Validacion de la factura 
            var serializer = new XmlSerializer(typeof(Comprobante));

            var xmlFileStream = new FileStream(xmlFilePath, FileMode.Open);


            var comprobante = (Comprobante)serializer.Deserialize(xmlFileStream);

            if (_db.facturas.FirstOrDefault(fa => fa.Uuid == comprobante.Complemento.TimbreFiscalDigital.UUID) != null)
            {
                xmlFileStream.Close();
                xmlFileStream.Dispose();
                throw new BusinessException("La factura ya esta registrada en el sistema");
            }

            var proveedor = new proveedore();
            var RFC = "";
            var RFCReceptor = "";
            var Serie = "";
            var Folio = "";
            var SubTotal = "";
            var Fecha = "";
            var Total = "";

            if (comprobante.Version33 == "3.3")
            {
                proveedor = _db.proveedores.FirstOrDefault(p => p.Rfc == comprobante.Emisor.Rfc33);
                RFC = comprobante.Emisor.Rfc33;
                RFCReceptor = comprobante.Receptor.Rfc33;
                Serie = comprobante.Serie33;
                Folio = comprobante.Folio33;
                SubTotal = comprobante.SubTotal33;
                Total = comprobante.Total33;
                Fecha = comprobante.Fecha33;
            }
            else
            {
                proveedor = _db.proveedores.FirstOrDefault(p => p.Rfc == comprobante.Emisor.Rfc);
                RFC = comprobante.Emisor.Rfc;
                RFCReceptor = comprobante.Receptor.Rfc;
                Serie = comprobante.Serie;
                Folio = comprobante.Folio;
                SubTotal = comprobante.SubTotal;
                Total = comprobante.Total;
                Fecha = comprobante.Fecha;
            }

            var refe = "";
            if (comprobante.Serie == "")
            {
                refe = Folio;
            }
            else
            {
                refe = Serie + Folio;
            }



            if (proveedor == null)
            {
                xmlFileStream.Close();
                xmlFileStream.Dispose();
                throw new BusinessException("Proveedor incorrecto en la factura");
            }

            if (proveedor.Id != proveedorId)
            {
                xmlFileStream.Close();
                xmlFileStream.Dispose();
                throw new BusinessException("Proveedor incorrecto en la factura");
            }

            if (_db.facturas.FirstOrDefault(fa => fa.Uuid == comprobante.Complemento.TimbreFiscalDigital.UUID) != null)
            {
                xmlFileStream.Close();
                xmlFileStream.Dispose();
                throw new BusinessException("La factura ya esta registrada en el sistema");
            }
            xmlFileStream.Close();
            xmlFileStream.Dispose();

            var contenido = File.ReadAllText(xmlFilePath);

            // Validación ante el sat

            var configuraciones = _db.configuraciones.ToList();
            var cfdiToken = configuraciones.Single(c => c.Clave == "cfdi.token").Valor;
            var cfdiPassword = configuraciones.Single(c => c.Clave == "cfdi.password").Valor;
            var cfdiUser = configuraciones.Single(c => c.Clave == "cfdi.user").Valor;
            var cfdiCuenta = configuraciones.Single(c => c.Clave == "cfdi.cuenta").Valor;
            try
            {
                CfdiServiceConsulta.Validar(contenido, cfdiToken, cfdiPassword, cfdiUser, cfdiCuenta, RFCReceptor);
            }
            catch (Exception)
            {
                xmlFileStream.Close();
                xmlFileStream.Dispose();
                throw;
            }



            // Se crea el directorio de acuerdo a la fecha del comprobante
            var fecha = DateTime.ParseExact(Fecha.Substring(0, 10), "yyyy-MM-dd",
                CultureInfo.InvariantCulture);
            var directory = GetFacturaDirectory(fecha);

            // Movemos el xml
            var xmlFileName = comprobante.Complemento.TimbreFiscalDigital.UUID + ".xml";
            var newXmlPath = Path.Combine(directory.FullName, xmlFileName);
            File.Copy(xmlFilePath, newXmlPath);

            // Movemos el pdf
            var pdfFileName = comprobante.Complemento.TimbreFiscalDigital.UUID + ".pdf";
            var newPdfPath = Path.Combine(directory.FullName, pdfFileName);
            File.Copy(pdfFilePath, newPdfPath);


            var cantidad = new decimal();

            if (comprobante.Version33 == "3.3")
            {
                //Creacion de la factura para la miro
                cantidad = comprobante.Conceptos.Concepto.Aggregate<Concepto, decimal>(0, (current, concepto) => current + Convert.ToDecimal(concepto.Cantidad33));
            }
            else
            {
                //Creacion de la factura para la miro
                cantidad = comprobante.Conceptos.Concepto.Aggregate<Concepto, decimal>(0, (current, concepto) => current + Convert.ToDecimal(concepto.Cantidad));

            }


            var sapFacturaManager = new SapFacturaManager();

            var facturaSap = sapFacturaManager.CrearFacturaServicio(
                proveedor.NumeroProveedor,
                refe,
                DateTime.ParseExact(Fecha, "yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture),
                SubTotal,
                Total,
                cantidad.ToString(CultureInfo.InvariantCulture),
                 comprobante.Complemento.TimbreFiscalDigital.UUID,
                 RFC);
            var factura = new factura
            {
                Serie = Serie ?? string.Empty,
                Folio = Folio,
                Fecha = fecha,
                Total = decimal.Parse(Total, CultureInfo.InvariantCulture),
                proveedor_id = proveedor.Id,
                Uuid = comprobante.Complemento.TimbreFiscalDigital.UUID,
                XmlRuta = newXmlPath,
                PdfRuta = newPdfPath,
                Estatus = facturaSap.Estatus,
                Procesado = true,
                numeroProveedor = proveedor.NumeroProveedor,
                EstatusOriginal = facturaSap.Estatus,
                FechaPortal = fecha
            };

            if (factura.Estatus != "S" && factura.Estatus != "H")
            {

                string errorsap = facturaSap.ErrorTable.Rows[0]["MESSAGE"].ToString();

                File.Delete(newXmlPath);
                File.Delete(newPdfPath);
                throw new BusinessException(errorsap);

            }
            else
            {
                factura.NumeroGenerado = facturaSap.FacturaNumero;
                _db.facturas.Add(factura);
                _db.SaveChanges();
            }

           

        }




        public SapFacturaManager.Resultado CargarFacturaAtorada(int proveedorId, string xmlFilePath)
        {
            var serializer = new XmlSerializer(typeof(Comprobante));
            var xmlFileStream = new FileStream(xmlFilePath, FileMode.Open);
            var comprobante = (Comprobante)serializer.Deserialize(xmlFileStream);
            var proveedor = new proveedore();
            var RFC = "";
            var RFCReceptor = "";
            var Serie = "";
            var Folio = "";
            var SubTotal = "";
            var Fecha = "";
            var Total = "";

            if (comprobante.Version33 == "3.3")
            {
                proveedor = _db.proveedores.FirstOrDefault(p => p.Rfc == comprobante.Emisor.Rfc33);
                RFC = comprobante.Emisor.Rfc33;
                RFCReceptor = comprobante.Receptor.Rfc33;
                Serie = comprobante.Serie33;
                Folio = comprobante.Folio33;
                SubTotal = comprobante.SubTotal33;
                Total = comprobante.Total33;
                Fecha = comprobante.Fecha33;
            }
            else
            {
                proveedor = _db.proveedores.FirstOrDefault(p => p.Rfc == comprobante.Emisor.Rfc);
                RFC = comprobante.Emisor.Rfc;
                RFCReceptor = comprobante.Receptor.Rfc;
                Serie = comprobante.Serie;
                Folio = comprobante.Folio;
                SubTotal = comprobante.SubTotal;
                Total = comprobante.Total;
                Fecha = comprobante.Fecha;
            }

            var refe = "";
            if (comprobante.Serie == "")
            {
                refe = Folio;
            }
            else
            {
                refe = Serie + Folio;
            }
            xmlFileStream.Close();
            xmlFileStream.Dispose();

            var result = DbScale.GetDataTable("SELECT * FROM GNZN_Cifras_Control_CR_Facturas  WHERE Proveedor = '" + proveedor.NumeroProveedor + "' AND Factura='" + refe + "';");

            if (result.Rows.Count == 1)
            {

                var row = result.Rows[0];
                int paresScale = Int32.Parse(row["Pares"].ToString());

                DateTime fechaFactura = DateTime.ParseExact(Fecha, "yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture);
                

                SapOrdenCompraManager ConsultaSap = new SapOrdenCompraManager();
                var totalParesSAP = ConsultaSap.GetCantidadValidacionSAP(fechaFactura.Year.ToString(), proveedor.Sociedad, refe, proveedor.NumeroProveedor);
                //var totalParesSAP = ConsultaSap.GetCantidadValidacionSAP("2017", "1000", "9549");

                if (paresScale == totalParesSAP)
                {
                    ///////////////////
                    //Se ejecuta si todo coincide
                    ///////////////////
                    var cantidad = new decimal();

                    if (comprobante.Version33 == "3.3")
                    {
                        //Creacion de la factura para la miro
                        cantidad = comprobante.Conceptos.Concepto.Aggregate<Concepto, decimal>(0, (current, concepto) => current + Convert.ToDecimal(concepto.Cantidad33));
                    }
                    else
                    {
                        //Creacion de la factura para la miro
                        cantidad = comprobante.Conceptos.Concepto.Aggregate<Concepto, decimal>(0, (current, concepto) => current + Convert.ToDecimal(concepto.Cantidad));

                    }


                    var sapFacturaManager = new SapFacturaManager();

                    var facturaSap = sapFacturaManager.CrearFactura(
                        proveedor.NumeroProveedor,
                        refe,
                        DateTime.ParseExact(Fecha, "yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture),
                        SubTotal,
                        Total,
                        cantidad.ToString(CultureInfo.InvariantCulture),
                         comprobante.Complemento.TimbreFiscalDigital.UUID,
                         RFC);

                    if (facturaSap.Estatus == "S" || facturaSap.Estatus == "H")
                    {
                        DateTime myDateTime = DateTime.Now;

                        DbScale.Update("UPDATE GNZN_Cifras_Control_CR_Facturas SET Fecha_CXP = '" + myDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + "' WHERE Proveedor = '" + proveedor.NumeroProveedor + "' AND Factura='" + refe + "';");
                    }

                    return facturaSap;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }   

        }

            public void CargarFactura(int proveedorId, int cuentaId, string xmlFilePath, string pdfFilePath)
        {
            // Validaciones de los archivos
            if (!File.Exists(xmlFilePath))
            {
                throw new BusinessException("Xml incorrecto");
            }
            if (Path.GetExtension(xmlFilePath).ToLower() != ".xml")
            {
                throw new BusinessException("Xml incorrecto");
            }
            if (!File.Exists(pdfFilePath))
            {
                throw new BusinessException("Pdf incorrecto");
            }
            if (Path.GetExtension(pdfFilePath).ToLower() != ".pdf")
            {
                throw new BusinessException("Pdf incorrecto");
            }

            // Validacion de la factura 
            var serializer = new XmlSerializer(typeof(Comprobante));

            var xmlFileStream = new FileStream(xmlFilePath, FileMode.Open);


            var comprobante = (Comprobante)serializer.Deserialize(xmlFileStream);

            if (_db.facturas.FirstOrDefault(fa => fa.Uuid == comprobante.Complemento.TimbreFiscalDigital.UUID) != null)
            {
                xmlFileStream.Close();
                xmlFileStream.Dispose();
                throw new BusinessException("La factura ya esta registrada en el sistema");
            }

            var proveedor = new proveedore();
            var RFC = "";
            var RFCReceptor = "";
            var Serie = "";
            var Folio = "";
            var SubTotal = "";
            var Fecha = "";
            var Total = "";

            if (comprobante.Version33 == "3.3")
            {
                proveedor = _db.proveedores.FirstOrDefault(p => p.Rfc == comprobante.Emisor.Rfc33);
                RFC = comprobante.Emisor.Rfc33;
                RFCReceptor = comprobante.Receptor.Rfc33;
                Serie = comprobante.Serie33;
                Folio = comprobante.Folio33;
                SubTotal = comprobante.SubTotal33;
                Total = comprobante.Total33;
                Fecha = comprobante.Fecha33;
            }
            else
            {
                proveedor = _db.proveedores.FirstOrDefault(p => p.Rfc == comprobante.Emisor.Rfc);
                RFC = comprobante.Emisor.Rfc;
                RFCReceptor = comprobante.Receptor.Rfc;
                Serie = comprobante.Serie;
                Folio = comprobante.Folio;
                SubTotal = comprobante.SubTotal;
                Total = comprobante.Total;
                Fecha = comprobante.Fecha;
            }

            var refe = "";
            if (comprobante.Serie == "")
            {
                refe = Folio;
            }
            else
            {
                refe = Serie + Folio;
            }



            if (proveedor == null)
            {
                xmlFileStream.Close();
                xmlFileStream.Dispose();
                throw new BusinessException("Proveedor incorrecto en la factura");
            }

            if (proveedor.Id != proveedorId)
            {
                xmlFileStream.Close();
                xmlFileStream.Dispose();
                throw new BusinessException("Proveedor incorrecto en la factura");
            }

            if (_db.facturas.FirstOrDefault(fa => fa.Uuid == comprobante.Complemento.TimbreFiscalDigital.UUID) != null)
            {
                xmlFileStream.Close();
                xmlFileStream.Dispose();
                throw new BusinessException("La factura ya esta registrada en el sistema");
            }
            xmlFileStream.Close();
            xmlFileStream.Dispose();

            var contenido = File.ReadAllText(xmlFilePath);

            // Validación ante el sat

            var configuraciones = _db.configuraciones.ToList();
            var cfdiToken = configuraciones.Single(c => c.Clave == "cfdi.token").Valor;
            var cfdiPassword = configuraciones.Single(c => c.Clave == "cfdi.password").Valor;
            var cfdiUser = configuraciones.Single(c => c.Clave == "cfdi.user").Valor;
            var cfdiCuenta = configuraciones.Single(c => c.Clave == "cfdi.cuenta").Valor;
            try
            {
                CfdiServiceConsulta.Validar(contenido, cfdiToken, cfdiPassword, cfdiUser, cfdiCuenta, RFCReceptor);
            }
            catch (Exception)
            {
                xmlFileStream.Close();
                xmlFileStream.Dispose();
                throw;
            }



            // Se crea el directorio de acuerdo a la fecha del comprobante
            var fecha = DateTime.ParseExact(Fecha.Substring(0, 10), "yyyy-MM-dd",
                CultureInfo.InvariantCulture);
            var directory = GetFacturaDirectory(fecha);

            // Movemos el xml
            var xmlFileName = comprobante.Complemento.TimbreFiscalDigital.UUID + ".xml";
            var newXmlPath = Path.Combine(directory.FullName, xmlFileName);
            File.Copy(xmlFilePath, newXmlPath);

            // Movemos el pdf
            var pdfFileName = comprobante.Complemento.TimbreFiscalDigital.UUID + ".pdf";
            var newPdfPath = Path.Combine(directory.FullName, pdfFileName);
            File.Copy(pdfFilePath, newPdfPath);


            ////////////////////////////////
            //Inicio de validacion Scale
            ////////////////////////////////
            var result = DbScale.GetDataTable("SELECT * FROM GNZN_Cifras_Control_CR_Facturas  WHERE Proveedor = '"+ proveedor.NumeroProveedor + "' AND Factura='"+ refe +"';");

            ////////////////////////////////
            //Fin de validacion Scale
            ////////////////////////////////

            if (result.Rows.Count == 1)
            {
                
                var row = result.Rows[0];
                int paresScale = Int32.Parse(row["Pares"].ToString());

                DateTime fechaFactura = DateTime.ParseExact(Fecha, "yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture);

                SapOrdenCompraManager ConsultaSap = new SapOrdenCompraManager();
                var totalParesSAP = ConsultaSap.GetCantidadValidacionSAP(fechaFactura.Year.ToString(), proveedor.Sociedad, refe, proveedor.NumeroProveedor);

                if (paresScale == totalParesSAP)
                {
                    ///////////////////
                    //Se ejecuta si todo coincide
                    ///////////////////
                    var cantidad = new decimal();

                    if (comprobante.Version33 == "3.3")
                    {
                        //Creacion de la factura para la miro
                        cantidad = comprobante.Conceptos.Concepto.Aggregate<Concepto, decimal>(0, (current, concepto) => current + Convert.ToDecimal(concepto.Cantidad33));
                    }
                    else
                    {
                        //Creacion de la factura para la miro
                        cantidad = comprobante.Conceptos.Concepto.Aggregate<Concepto, decimal>(0, (current, concepto) => current + Convert.ToDecimal(concepto.Cantidad));

                    }


                    var sapFacturaManager = new SapFacturaManager();

                    var facturaSap = sapFacturaManager.CrearFactura(
                        proveedor.NumeroProveedor,
                        refe,
                        DateTime.ParseExact(Fecha, "yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture),
                        SubTotal,
                        Total,
                        cantidad.ToString(CultureInfo.InvariantCulture),
                         comprobante.Complemento.TimbreFiscalDigital.UUID,
                         RFC);
                    var factura = new factura
                    {
                        Serie = Serie ?? string.Empty,
                        Folio = Folio ?? string.Empty,
                        Fecha = fecha,
                        Total = decimal.Parse(Total, CultureInfo.InvariantCulture),
                        proveedor_id = proveedor.Id,
                        Uuid = comprobante.Complemento.TimbreFiscalDigital.UUID,
                        XmlRuta = newXmlPath,
                        PdfRuta = newPdfPath,
                        Estatus = facturaSap.Estatus,
                        Procesado = true,
                        numeroProveedor = proveedor.NumeroProveedor,
                         EstatusOriginal = facturaSap.Estatus,
                        FechaPortal = fecha
                    };

                    if (factura.Estatus != "S" && factura.Estatus != "H")
                    {
                        factura.Comentario = string.Format
                            ("Tipo:{1} {0}Mensaje:{2}",
                            Environment.NewLine,
                            facturaSap.ErrorTable.Rows[0]["TYPE"],
                            facturaSap.ErrorTable.Rows[0]["MESSAGE"]);
                    }
                    else
                    {
                        DateTime myDateTime = DateTime.Now;

                        factura.NumeroGenerado = facturaSap.FacturaNumero;
                        DbScale.Update("UPDATE GNZN_Cifras_Control_CR_Facturas SET Fecha_CXP = '" + myDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + "' WHERE Proveedor = '" + proveedor.NumeroProveedor + "' AND Factura='" + refe + "';");
                    }


                    _db.facturas.Add(factura);

                    _db.SaveChanges();
                }
                else
                {
                    ///////////////////
                    //Se ejecuta si los montos de Scale y Sap son diferentes
                    ///////////////////
                    var factura = new factura
                    {
                        Serie = Serie ?? string.Empty,
                        Folio = Folio,
                        Fecha = fecha,
                        Total = decimal.Parse(Total, CultureInfo.InvariantCulture),
                        proveedor_id = proveedor.Id,
                        Uuid = comprobante.Complemento.TimbreFiscalDigital.UUID,
                        XmlRuta = newXmlPath,
                        PdfRuta = newPdfPath,
                        Estatus = "P",
                        Procesado = false,
                        numeroProveedor = proveedor.NumeroProveedor,
                        Comentario = "N° de pares en Scale y SAP son diferentes",
                        EstatusOriginal = "p",
                        FechaPortal = fecha

                    };

                    _db.facturas.Add(factura);

                    _db.SaveChanges();
                }
                
            }
            else
            {
                ///////////////////
                //Se ejecuta si La factura todavia no esta en Scale
                ///////////////////
                var factura = new factura
                {
                    Serie = Serie ?? string.Empty,
                    Folio = Folio ?? string.Empty,
                    Fecha = fecha,
                    Total = decimal.Parse(Total, CultureInfo.InvariantCulture),
                    proveedor_id = proveedor.Id,
                    Uuid = comprobante.Complemento.TimbreFiscalDigital.UUID,
                    XmlRuta = newXmlPath,
                    PdfRuta = newPdfPath,
                    Estatus = "P",
                    Procesado = false,
                    numeroProveedor = proveedor.NumeroProveedor,
                    Comentario = "Factura no disponible en Scale",
                    EstatusOriginal = "P",
                    FechaPortal = fecha

                };

                _db.facturas.Add(factura);

                _db.SaveChanges();
            }

        }

		public factura CargarFacturaFactoraje(int proveedorId, int cuentaId, string xmlFilePath, string pdfFilePath)
		{
			// Validaciones de los archivos
			if (!File.Exists(xmlFilePath))
			{
				throw new BusinessException("Xml incorrecto");
			}
			if (Path.GetExtension(xmlFilePath).ToLower() != ".xml")
			{
				throw new BusinessException("Xml incorrecto");
			}
			if (!File.Exists(pdfFilePath))
			{
				throw new BusinessException("Pdf incorrecto");
			}
			if (Path.GetExtension(pdfFilePath).ToLower() != ".pdf")
			{
				throw new BusinessException("Pdf incorrecto");
			}

			// Validacion de la factura 
			var serializer = new XmlSerializer(typeof(Comprobante));

			var xmlFileStream = new FileStream(xmlFilePath, FileMode.Open);


			var comprobante = (Comprobante)serializer.Deserialize(xmlFileStream);

			var proveedor = new proveedore();
			var RFC = "";
			var RFCReceptor = "";
			var Serie = "";
			var Folio = "";
			var SubTotal = "";
			var Fecha = "";
			var Total = "";

			if (comprobante.Version33 == "3.3")
			{
				proveedor = _db.proveedores.FirstOrDefault(p => p.Rfc == comprobante.Emisor.Rfc33);
				RFC = comprobante.Emisor.Rfc33;
				RFCReceptor = comprobante.Receptor.Rfc33;
				Serie = comprobante.Serie33;
				Folio = comprobante.Folio33;
				SubTotal = comprobante.SubTotal33;
				Total = comprobante.Total33;
				Fecha = comprobante.Fecha33;
			}
			else
			{
				proveedor = _db.proveedores.FirstOrDefault(p => p.Rfc == comprobante.Emisor.Rfc);
				RFC = comprobante.Emisor.Rfc;
				RFCReceptor = comprobante.Receptor.Rfc;
				Serie = comprobante.Serie;
				Folio = comprobante.Folio;
				SubTotal = comprobante.SubTotal;
				Total = comprobante.Total;
				Fecha = comprobante.Fecha;
			}

			var refe = "";
			if (comprobante.Serie == "")
			{
				refe = Folio;
			}
			else
			{
				refe = Serie + Folio;
			}

			if (proveedor == null)
			{
				xmlFileStream.Close();
				xmlFileStream.Dispose();
				throw new BusinessException("Proveedor incorrecto en la factura");
			}

			if (proveedor.Id != proveedorId)
			{
				xmlFileStream.Close();
				xmlFileStream.Dispose();
				throw new BusinessException("Proveedor incorrecto en la factura");
			}

			if (_db.facturas.FirstOrDefault(fa => fa.Uuid == comprobante.Complemento.TimbreFiscalDigital.UUID) != null)
			{
				xmlFileStream.Close();
				xmlFileStream.Dispose();
				throw new BusinessException("La nota de credito ya esta registrada en el sistema");
			}
			xmlFileStream.Close();
			xmlFileStream.Dispose();

			var contenido = File.ReadAllText(xmlFilePath);

			// Validación ante el sat

			var configuraciones = _db.configuraciones.ToList();
			var cfdiToken = configuraciones.Single(c => c.Clave == "cfdi.token").Valor;
			var cfdiPassword = configuraciones.Single(c => c.Clave == "cfdi.password").Valor;
			var cfdiUser = configuraciones.Single(c => c.Clave == "cfdi.user").Valor;
			var cfdiCuenta = configuraciones.Single(c => c.Clave == "cfdi.cuenta").Valor;
			try
			{
				CfdiServiceConsulta.Validar(contenido, cfdiToken, cfdiPassword, cfdiUser, cfdiCuenta, RFCReceptor);
			}
			catch (Exception)
			{
				xmlFileStream.Close();
				xmlFileStream.Dispose();
				throw;
			}



			// Se crea el directorio de acuerdo a la fecha del comprobante
			var fecha = DateTime.ParseExact(Fecha.Substring(0, 10), "yyyy-MM-dd",
				CultureInfo.InvariantCulture);
			var directory = GetFacturaDirectory(fecha);

			// Movemos el xml
			var xmlFileName = comprobante.Complemento.TimbreFiscalDigital.UUID + ".xml";
			var newXmlPath = Path.Combine(directory.FullName, xmlFileName);
			File.Copy(xmlFilePath, newXmlPath);

			// Movemos el pdf
			var pdfFileName = comprobante.Complemento.TimbreFiscalDigital.UUID + ".pdf";
			var newPdfPath = Path.Combine(directory.FullName, pdfFileName);
			File.Copy(pdfFilePath, newPdfPath);

			//Se agrega llamado a la aplicacion de la nota de credito en SAP

			//[PENDIENTE]

			//Se guarda en MySql la factura
			factura factura = new factura
			{
				Serie = Serie ?? string.Empty,
				Folio = Folio ?? string.Empty,
				Fecha = fecha,
				Total = decimal.Parse(Total, CultureInfo.InvariantCulture),
				proveedor_id = proveedor.Id,
				Uuid = comprobante.Complemento.TimbreFiscalDigital.UUID,
				XmlRuta = newXmlPath,
				PdfRuta = newPdfPath,
				Estatus = "S",
				Procesado = true,
				Comentario = "Nota de credito factoraje",
				numeroProveedor = proveedor.NumeroProveedor,
                EstatusOriginal = "S",
                FechaPortal = fecha
            };

			return factura;
			
		}

		public void GuardaFacturaFactoraje(factura model)
		{
			_db.facturas.Add(model);
			_db.SaveChanges();
		}
		public DirectoryInfo GetFacturaDirectory(DateTime fecha)
        {
            var facturaRootDirectory = new DirectoryInfo(_db.configuraciones.Single(c => c.Clave == "facturas.rootdirectory").Valor);

            var yearPath = facturaRootDirectory.GetDirectories(fecha.Year.ToString()).Length == 0 ?
                facturaRootDirectory.CreateSubdirectory(fecha.Year.ToString()) :
                facturaRootDirectory.GetDirectories(fecha.Year.ToString())[0];

            var monthPath = yearPath.GetDirectories(fecha.Month.ToString()).Length == 0 ?
                yearPath.CreateSubdirectory(fecha.Month.ToString()) :
                yearPath.GetDirectories(fecha.Month.ToString())[0];

            return monthPath;
        }

    }
}
