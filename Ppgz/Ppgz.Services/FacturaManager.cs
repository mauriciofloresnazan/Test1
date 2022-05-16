using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Serialization;
using Ppgz.Repository;
using SapWrapper;
using SatWrapper;
using ScaleWrapper;
using System.Diagnostics;
using System.Xml;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Web.Script.Serialization;

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


        public void ValidarFacturaVersion4(Stream xmlFileStream)
        {
            var serializer = new XmlSerializer(typeof(ComprobanteVersion4));

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
            List<factura> Facturas = _db.facturas.Where(f => f.Procesado == false && f.TipoFactura == "Mercaderia").ToList();

            foreach (factura itemFactura in Facturas)
            {
                var resultado = CargarFacturaAtorada(itemFactura.proveedor_id, itemFactura.XmlRuta);
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

        public void ProcesarFacturasAtoradasServicio()
        {
            List<factura> Facturas = _db.facturas.Where(f => f.Procesado == false && f.TipoFactura == "Servicios").ToList();

            foreach (factura itemFactura in Facturas)
            {
                var resultado = CargarFacturaAtoradaServicios(itemFactura.proveedor_id, itemFactura.XmlRuta);
                if (resultado != null)
                {
                    if (resultado.Estatus != "S" && resultado.Estatus != "H")
                    {
                        itemFactura.Estatus = resultado.Estatus;
                        itemFactura.Procesado = false;
                        itemFactura.Comentario = string.Format
                            ("Tipo:{1} {0}Mensaje:{2}",
                            Environment.NewLine,
                            resultado.ErrorTable.Rows[0]["TYPE"],
                            resultado.ErrorTable.Rows[0]["MESSAGE"]);
                    }
                    else
                    {
                        itemFactura.Estatus = resultado.Estatus;
                        itemFactura.Procesado = true;
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
            if(comprobante.Receptor.Rfc33== "NAZ890526N46")
            {

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


                //var sapFacturaManager = new SapFacturaManager();

                //var facturaSap = sapFacturaManager.CrearFacturaServicio(
                //    proveedor.NumeroProveedor,
                //    refe,
                //    DateTime.ParseExact(Fecha, "yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture),
                //    SubTotal,
                //    Total,
                //    cantidad.ToString(CultureInfo.InvariantCulture),
                //     comprobante.Complemento.TimbreFiscalDigital.UUID,
                //     RFC);
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
                    Estatus = "S",
                    numeroProveedor = proveedor.NumeroProveedor,
                    EstatusOriginal = "S",
                    FechaPortal = fecha,
                    RFCReceptor = RFCReceptor ?? string.Empty,
                    TipoFactura = "Servicios",
                    Comentario = "Factura Validada",
                    Procesado = true
            };

                //if (factura.Estatus != "S" && factura.Estatus != "H")
                //{

                //    string errorsap = facturaSap.ErrorTable.Rows[0]["MESSAGE"].ToString();
                //    factura.Comentario = string.Format
                //       ("Tipo:{1} {0}Mensaje:{2}",
                //       Environment.NewLine,
                //       facturaSap.ErrorTable.Rows[0]["TYPE"],
                //       facturaSap.ErrorTable.Rows[0]["MESSAGE"]);

                //    factura.Procesado = false;

                //}
                //else
                //{
                //    factura.Procesado = true;
                //    factura.NumeroGenerado = facturaSap.FacturaNumero;

                //}

                _db.facturas.Add(factura);
                _db.SaveChanges();

            }
            else if(comprobante.Receptor.Rfc33 == "ASC090330TT3")
            {
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


                //var sapFacturaManager = new SapFacturaManager();

                //var facturaSap = sapFacturaManager.CrearFacturaServicio(
                //    proveedor.NumeroProveedor,
                //    refe,
                //    DateTime.ParseExact(Fecha, "yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture),
                //    SubTotal,
                //    Total,
                //    cantidad.ToString(CultureInfo.InvariantCulture),
                //     comprobante.Complemento.TimbreFiscalDigital.UUID,
                //     RFC);
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
                    Estatus = "S",
                    numeroProveedor = proveedor.NumeroProveedor,
                    EstatusOriginal = "S",
                    FechaPortal = fecha,
                    RFCReceptor = RFCReceptor ?? string.Empty,
                    TipoFactura = "Servicios",
                    Procesado = true,
                    Comentario = "Factura Validada"
                };

                //if (factura.Estatus != "S" && factura.Estatus != "H")
                //{

                //    string errorsap = facturaSap.ErrorTable.Rows[0]["MESSAGE"].ToString();
                //    factura.Comentario = string.Format
                //       ("Tipo:{1} {0}Mensaje:{2}",
                //       Environment.NewLine,
                //       facturaSap.ErrorTable.Rows[0]["TYPE"],
                //       facturaSap.ErrorTable.Rows[0]["MESSAGE"]);

                //    factura.Procesado = false;

                //}
                //else
                //{
                //    factura.Procesado = true;
                //    factura.NumeroGenerado = facturaSap.FacturaNumero;

                //}

                _db.facturas.Add(factura);
                _db.SaveChanges();





            }
            else if(comprobante.Receptor.Rfc33 == "NCC1011058I0")
            {
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
                    numeroProveedor = proveedor.NumeroProveedor,
                    EstatusOriginal = facturaSap.Estatus,
                    FechaPortal = fecha,
                    RFCReceptor = RFCReceptor ?? string.Empty,
                    TipoFactura = "Servicios"
                };

                if (factura.Estatus != "S" && factura.Estatus != "H")
                {

                    string errorsap = facturaSap.ErrorTable.Rows[0]["MESSAGE"].ToString();
                    factura.Comentario = string.Format
                       ("Tipo:{1} {0}Mensaje:{2}",
                       Environment.NewLine,
                       facturaSap.ErrorTable.Rows[0]["TYPE"],
                       facturaSap.ErrorTable.Rows[0]["MESSAGE"]);

                    factura.Procesado = false;

                }
                else
                {
                    factura.Procesado = true;
                    factura.NumeroGenerado = facturaSap.FacturaNumero;

                }

                _db.facturas.Add(factura);
                _db.SaveChanges();
            }
            

        }

        //CargarFacturas de Servicio
        public void CargarFacturaServicioV4(int proveedorId, int cuentaId, string xmlFilePath, string pdfFilePath)
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
            var serializer = new XmlSerializer(typeof(ComprobanteVersion4));

            var xmlFileStream = new FileStream(xmlFilePath, FileMode.Open);


            var comprobante = (ComprobanteVersion4)serializer.Deserialize(xmlFileStream);

            if (comprobante.Receptor.Rfc33 == "NAZ890526N46")
            {


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

                if (comprobante.Version33 == "4.0")
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
                var cadenaBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(File.ReadAllText(xmlFilePath)));
                // Validación ante el sat

                var configuraciones = _db.configuraciones.ToList();
                var cfdiToken = configuraciones.Single(c => c.Clave == "cfdi.token").Valor;
                var cfdiPassword = configuraciones.Single(c => c.Clave == "cfdi.password").Valor;
                var cfdiUser = configuraciones.Single(c => c.Clave == "cfdi.user").Valor;
                var cfdiCuenta = configuraciones.Single(c => c.Clave == "cfdi.cuenta").Valor;
                System.Net.ServicePointManager.SecurityProtocol =
                System.Net.SecurityProtocolType.Tls12;
                var url = "https://lb04.cfdinova.com.mx/KTimbradoService/webresources/TimbradoService/request";
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "POST";
                httpRequest.Accept = "application/json";
                httpRequest.ContentType = "application/json";
                var data = @"{

            ""credentials"": {
            ""usuario"":'" + cfdiUser + @"',
            ""token"":'" + cfdiToken + @"',
            ""password"":'" + cfdiPassword + @"',
           ""cuenta"": '" + cfdiCuenta + @"'
                               },

            ""issuer"": {
            ""rfc"": '" + RFCReceptor + @"',
            ""business"": ""TESTFD""
                                },

             ""document"": {
             ""format"": ""xml"",
             ""type"": ""cfdiv4"",
             ""operation"":""validation"",
             ""content"": '" + cadenaBase64 + @"' 
} }";
                var stringified = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<dynamic>(data));


                using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                {
                    streamWriter.Write(stringified);
                }

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var rt = streamReader.ReadToEnd();


                    dynamic jsonObj = JsonConvert.DeserializeObject(rt);
                    string story = (string)jsonObj["result"];



                    string storys = (string)jsonObj["result"];

                    if (storys == "INVALID")
                    {
                        string message = (string)jsonObj["error_details"][0]["message"];
                        xmlFileStream.Close();
                        xmlFileStream.Dispose();
                        throw new BusinessException(message);
                    }
                    else
                    {
                        string estado = (string)jsonObj["efos"]["estado"];
                        if (storys == "VALID" && estado == "Inexistente")
                        {
                            string estatus = (string)jsonObj["efos"]["estatus"];
                            xmlFileStream.Close();
                            xmlFileStream.Dispose();
                            throw new BusinessException(estatus);
                        }

                    }


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

                if (comprobante.Version33 == "4.0")
                {
                    //Creacion de la factura para la miro
                    cantidad = comprobante.Conceptos.Concepto.Aggregate<ConceptoV, decimal>(0, (current, concepto) => current + Convert.ToDecimal(concepto.Cantidad33));
                }
                else
                {
                    //Creacion de la factura para la miro
                    cantidad = comprobante.Conceptos.Concepto.Aggregate<ConceptoV, decimal>(0, (current, concepto) => current + Convert.ToDecimal(concepto.Cantidad));

                }


                //var sapFacturaManager = new SapFacturaManager();

                //var facturaSap = sapFacturaManager.CrearFacturaServicio(
                //    proveedor.NumeroProveedor,
                //    refe,
                //    DateTime.ParseExact(Fecha, "yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture),
                //    SubTotal,
                //    Total,
                //    cantidad.ToString(CultureInfo.InvariantCulture),
                //     comprobante.Complemento.TimbreFiscalDigital.UUID,
                //     RFC);
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
                    Estatus = "S",
                    numeroProveedor = proveedor.NumeroProveedor,
                    EstatusOriginal = "S",
                    FechaPortal = fecha,
                    RFCReceptor = RFCReceptor ?? string.Empty,
                    Comentario = "Factura Validada",
                    Procesado = true,
                    TipoFactura = "Servicios"
                };

                //if (factura.Estatus != "S" && factura.Estatus != "H")
                //{

                //    string errorsap = facturaSap.ErrorTable.Rows[0]["MESSAGE"].ToString();
                //    factura.Comentario = string.Format
                //       ("Tipo:{1} {0}Mensaje:{2}",
                //       Environment.NewLine,
                //       facturaSap.ErrorTable.Rows[0]["TYPE"],
                //       facturaSap.ErrorTable.Rows[0]["MESSAGE"]);

                //    factura.Procesado = false;

                //}
                //else
                //{
                //    factura.Procesado = true;
                //    factura.NumeroGenerado = facturaSap.FacturaNumero;

                //}

                _db.facturas.Add(factura);
                _db.SaveChanges();



            }
            else if (comprobante.Receptor.Rfc33 == "ASC090330TT3")
            {

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

                if (comprobante.Version33 == "4.0")
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
                var cadenaBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(File.ReadAllText(xmlFilePath)));
                // Validación ante el sat

                var configuraciones = _db.configuraciones.ToList();
                var cfdiToken = configuraciones.Single(c => c.Clave == "cfdi.token").Valor;
                var cfdiPassword = configuraciones.Single(c => c.Clave == "cfdi.password").Valor;
                var cfdiUser = configuraciones.Single(c => c.Clave == "cfdi.user").Valor;
                var cfdiCuenta = configuraciones.Single(c => c.Clave == "cfdi.cuenta").Valor;
                System.Net.ServicePointManager.SecurityProtocol =
                System.Net.SecurityProtocolType.Tls12;
                var url = "https://lb04.cfdinova.com.mx/KTimbradoService/webresources/TimbradoService/request";
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "POST";
                httpRequest.Accept = "application/json";
                httpRequest.ContentType = "application/json";
                var data = @"{

            ""credentials"": {
            ""usuario"":'" + cfdiUser + @"',
            ""token"":'" + cfdiToken + @"',
            ""password"":'" + cfdiPassword + @"',
           ""cuenta"": '" + cfdiCuenta + @"'
                               },

            ""issuer"": {
            ""rfc"": '" + RFCReceptor + @"',
            ""business"": ""TESTFD""
                                },

             ""document"": {
             ""format"": ""xml"",
             ""type"": ""cfdiv4"",
             ""operation"":""validation"",
             ""content"": '" + cadenaBase64 + @"' 
} }";
                var stringified = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<dynamic>(data));


                using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                {
                    streamWriter.Write(stringified);
                }

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var rt = streamReader.ReadToEnd();


                    dynamic jsonObj = JsonConvert.DeserializeObject(rt);
                    string story = (string)jsonObj["result"];



                    string storys = (string)jsonObj["result"];

                    if (storys == "INVALID")
                    {
                        string message = (string)jsonObj["error_details"][0]["message"];
                        xmlFileStream.Close();
                        xmlFileStream.Dispose();
                        throw new BusinessException(message);
                    }
                    else
                    {
                        string estado = (string)jsonObj["efos"]["estado"];
                        if (storys == "VALID" && estado == "Inexistente")
                        {
                            string estatus = (string)jsonObj["efos"]["estatus"];
                            xmlFileStream.Close();
                            xmlFileStream.Dispose();
                            throw new BusinessException(estatus);
                        }

                    }


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

                if (comprobante.Version33 == "4.0")
                {
                    //Creacion de la factura para la miro
                    cantidad = comprobante.Conceptos.Concepto.Aggregate<ConceptoV, decimal>(0, (current, concepto) => current + Convert.ToDecimal(concepto.Cantidad33));
                }
                else
                {
                    //Creacion de la factura para la miro
                    cantidad = comprobante.Conceptos.Concepto.Aggregate<ConceptoV, decimal>(0, (current, concepto) => current + Convert.ToDecimal(concepto.Cantidad));

                }


                //var sapFacturaManager = new SapFacturaManager();

                //var facturaSap = sapFacturaManager.CrearFacturaServicio(
                //    proveedor.NumeroProveedor,
                //    refe,
                //    DateTime.ParseExact(Fecha, "yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture),
                //    SubTotal,
                //    Total,
                //    cantidad.ToString(CultureInfo.InvariantCulture),
                //     comprobante.Complemento.TimbreFiscalDigital.UUID,
                //     RFC);
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
                    Estatus = "S",
                    numeroProveedor = proveedor.NumeroProveedor,
                    EstatusOriginal = "S",
                    FechaPortal = fecha,
                    RFCReceptor = RFCReceptor ?? string.Empty,
                    Comentario = "Factura Validada",
                    Procesado = true,
                    TipoFactura = "Servicios"
                };

                //if (factura.Estatus != "S" && factura.Estatus != "H")
                //{

                //    string errorsap = facturaSap.ErrorTable.Rows[0]["MESSAGE"].ToString();
                //    factura.Comentario = string.Format
                //       ("Tipo:{1} {0}Mensaje:{2}",
                //       Environment.NewLine,
                //       facturaSap.ErrorTable.Rows[0]["TYPE"],
                //       facturaSap.ErrorTable.Rows[0]["MESSAGE"]);

                //    factura.Procesado = false;

                //}
                //else
                //{
                //    factura.Procesado = true;
                //    factura.NumeroGenerado = facturaSap.FacturaNumero;

                //}

                _db.facturas.Add(factura);
                _db.SaveChanges();


            }
            else if (comprobante.Receptor.Rfc33 == "NCC1011058I0")
            {
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

                if (comprobante.Version33 == "4.0")
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
                var cadenaBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(File.ReadAllText(xmlFilePath)));
                // Validación ante el sat

                var configuraciones = _db.configuraciones.ToList();
                var cfdiToken = configuraciones.Single(c => c.Clave == "cfdi.token").Valor;
                var cfdiPassword = configuraciones.Single(c => c.Clave == "cfdi.password").Valor;
                var cfdiUser = configuraciones.Single(c => c.Clave == "cfdi.user").Valor;
                var cfdiCuenta = configuraciones.Single(c => c.Clave == "cfdi.cuenta").Valor;
                System.Net.ServicePointManager.SecurityProtocol =
                System.Net.SecurityProtocolType.Tls12;
                var url = "https://lb04.cfdinova.com.mx/KTimbradoService/webresources/TimbradoService/request";
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "POST";
                httpRequest.Accept = "application/json";
                httpRequest.ContentType = "application/json";
                var data = @"{

            ""credentials"": {
            ""usuario"":'" + cfdiUser + @"',
            ""token"":'" + cfdiToken + @"',
            ""password"":'" + cfdiPassword + @"',
           ""cuenta"": '" + cfdiCuenta + @"'
                               },

            ""issuer"": {
            ""rfc"": '" + RFCReceptor + @"',
            ""business"": ""TESTFD""
                                },

             ""document"": {
             ""format"": ""xml"",
             ""type"": ""cfdiv4"",
             ""operation"":""validation"",
             ""content"": '" + cadenaBase64 + @"' 
} }";
                var stringified = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<dynamic>(data));


                using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                {
                    streamWriter.Write(stringified);
                }

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var rt = streamReader.ReadToEnd();


                    dynamic jsonObj = JsonConvert.DeserializeObject(rt);
                    string story = (string)jsonObj["result"];



                    string storys = (string)jsonObj["result"];

                    if (storys == "INVALID")
                    {
                        string message = (string)jsonObj["error_details"][0]["message"];
                        xmlFileStream.Close();
                        xmlFileStream.Dispose();
                        throw new BusinessException(message);
                    }
                    else
                    {
                        string estado = (string)jsonObj["efos"]["estado"];
                        if (storys == "VALID" && estado == "Inexistente")
                        {
                            string estatus = (string)jsonObj["efos"]["estatus"];
                            xmlFileStream.Close();
                            xmlFileStream.Dispose();
                            throw new BusinessException(estatus);
                        }

                    }


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

                if (comprobante.Version33 == "4.0")
                {
                    //Creacion de la factura para la miro
                    cantidad = comprobante.Conceptos.Concepto.Aggregate<ConceptoV, decimal>(0, (current, concepto) => current + Convert.ToDecimal(concepto.Cantidad33));
                }
                else
                {
                    //Creacion de la factura para la miro
                    cantidad = comprobante.Conceptos.Concepto.Aggregate<ConceptoV, decimal>(0, (current, concepto) => current + Convert.ToDecimal(concepto.Cantidad));

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
                    numeroProveedor = proveedor.NumeroProveedor,
                    EstatusOriginal = facturaSap.Estatus,
                    FechaPortal = fecha,
                    RFCReceptor = RFCReceptor ?? string.Empty,
                    TipoFactura = "Servicios"
                };

                if (factura.Estatus != "S" && factura.Estatus != "H")
                {

                    string errorsap = facturaSap.ErrorTable.Rows[0]["MESSAGE"].ToString();
                    factura.Comentario = string.Format
                       ("Tipo:{1} {0}Mensaje:{2}",
                       Environment.NewLine,
                       facturaSap.ErrorTable.Rows[0]["TYPE"],
                       facturaSap.ErrorTable.Rows[0]["MESSAGE"]);

                    factura.Procesado = false;

                }
                else
                {
                    factura.Procesado = true;
                    factura.NumeroGenerado = facturaSap.FacturaNumero;

                }

                _db.facturas.Add(factura);
                _db.SaveChanges();
            }

           

        }



        public SapFacturaManager.Resultado CargarFacturaAtorada(int proveedorId, string xmlFilePath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlFilePath);//Leer el XML

            //agregamos un Namespace, que usaremos para buscar que el nodo no exista:
            XmlNamespaceManager nsm = new XmlNamespaceManager(doc.NameTable);
            nsm.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/3");
            XmlNode nodeComprobante = doc.SelectSingleNode("//cfdi:Comprobante", nsm);

            if (nodeComprobante == null)
            {
                var serializer = new XmlSerializer(typeof(ComprobanteVersion4));
                var xmlFileStream = new FileStream(xmlFilePath, FileMode.Open);
                var comprobante = (ComprobanteVersion4)serializer.Deserialize(xmlFileStream);
                var proveedor = new proveedore();
                var RFC = "";
                var RFCReceptor = "";
                var Serie = "";
                var Folio = "";
                var SubTotal = "";
                var Fecha = "";
                var Total = "";

                if (comprobante.Version33 == "4.0")
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
                    var totalParesSAP = ConsultaSap.GetCantidadValidacionSAP(fechaFactura.Year.ToString(), "1001", refe, proveedor.NumeroProveedor);
                    //var totalParesSAP = ConsultaSap.GetCantidadValidacionSAP(fechaFactura.Year.ToString(), proveedor.Sociedad, refe, proveedor.NumeroProveedor);
                    //var totalParesSAP = ConsultaSap.GetCantidadValidacionSAP("2017", "1000", "9549");

                    if (paresScale == totalParesSAP)
                    {
                        ///////////////////
                        //Se ejecuta si todo coincide
                        ///////////////////
                        var cantidad = new decimal();

                        if (comprobante.Version33 == "4.0")
                        {
                            //Creacion de la factura para la miro
                            cantidad = comprobante.Conceptos.Concepto.Aggregate<ConceptoV, decimal>(0, (current, concepto) => current + Convert.ToDecimal(concepto.Cantidad33));
                        }
                        else
                        {
                            //Creacion de la factura para la miro
                            cantidad = comprobante.Conceptos.Concepto.Aggregate<ConceptoV, decimal>(0, (current, concepto) => current + Convert.ToDecimal(concepto.Cantidad));

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
            else
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
                    var totalParesSAP = ConsultaSap.GetCantidadValidacionSAP(fechaFactura.Year.ToString(), "1001", refe, proveedor.NumeroProveedor);
                    //var totalParesSAP = ConsultaSap.GetCantidadValidacionSAP(fechaFactura.Year.ToString(), proveedor.Sociedad, refe, proveedor.NumeroProveedor);
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

        }

        public SapFacturaManager.Resultado CargarFacturaAtoradaServicios(int proveedorId, string xmlFilePath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlFilePath);//Leer el XML

            //agregamos un Namespace, que usaremos para buscar que el nodo no exista:
            XmlNamespaceManager nsm = new XmlNamespaceManager(doc.NameTable);
            nsm.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/3");
            XmlNode nodeComprobante = doc.SelectSingleNode("//cfdi:Comprobante", nsm);

            if (nodeComprobante == null)
            {

                var serializer = new XmlSerializer(typeof(ComprobanteVersion4));
                var xmlFileStream = new FileStream(xmlFilePath, FileMode.Open);
                var comprobante = (ComprobanteVersion4)serializer.Deserialize(xmlFileStream);
                var proveedor = new proveedore();
                var RFC = "";
                var RFCReceptor = "";
                var Serie = "";
                var Folio = "";
                var SubTotal = "";
                var Fecha = "";
                var Total = "";

                if (comprobante.Version33 == "4.0")
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

                var cantidad = new decimal();

                if (comprobante.Version33 == "4.0")
                {
                    //Creacion de la factura para la miro
                    cantidad = comprobante.Conceptos.Concepto.Aggregate<ConceptoV, decimal>(0, (current, concepto) => current + Convert.ToDecimal(concepto.Cantidad33));
                }
                else
                {
                    //Creacion de la factura para la miro
                    cantidad = comprobante.Conceptos.Concepto.Aggregate<ConceptoV, decimal>(0, (current, concepto) => current + Convert.ToDecimal(concepto.Cantidad));

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

                return facturaSap;
            }
            else
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
                return facturaSap;
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
            var result = DbScale.GetDataTable("SELECT * FROM GNZN_Cifras_Control_CR_Facturas  WHERE Proveedor = '" + proveedor.NumeroProveedor + "' AND Factura='" + refe + "';");

            ////////////////////////////////
            //Fin de validacion Scale
            ////////////////////////////////

            if (result.Rows.Count == 1)
            {

                var row = result.Rows[0];
                int paresScale = Int32.Parse(row["Pares"].ToString());

                DateTime fechaFactura = DateTime.ParseExact(Fecha, "yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture);

                SapOrdenCompraManager ConsultaSap = new SapOrdenCompraManager();
                var totalParesSAP = ConsultaSap.GetCantidadValidacionSAP(fechaFactura.Year.ToString(), "1001", refe, proveedor.NumeroProveedor);
                //var totalParesSAP = ConsultaSap.GetCantidadValidacionSAP(fechaFactura.Year.ToString(), proveedor.Sociedad, refe, proveedor.NumeroProveedor);

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
                        FechaPortal = fecha,
                        RFCReceptor = RFCReceptor ?? string.Empty,
                        TipoFactura = "Mercaderia"

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
                        FechaPortal = fecha,
                        RFCReceptor = RFCReceptor ?? string.Empty,
                        TipoFactura = "Mercaderia"

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
                    FechaPortal = fecha,
                    RFCReceptor = RFCReceptor ?? string.Empty,
                    TipoFactura = "Mercaderia"

                };

                _db.facturas.Add(factura);

                _db.SaveChanges();
            }

        }

        public void CargarFacturaV4(int proveedorId, int cuentaId, string xmlFilePath, string pdfFilePath)
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
            var serializer = new XmlSerializer(typeof(ComprobanteVersion4));

            var xmlFileStream = new FileStream(xmlFilePath, FileMode.Open);


            var comprobante = (ComprobanteVersion4)serializer.Deserialize(xmlFileStream);

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

            if (comprobante.Version33 == "4.0")
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
            var cadenaBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(File.ReadAllText(xmlFilePath)));
            // Validación ante el sat

            var configuraciones = _db.configuraciones.ToList();
            var cfdiToken = configuraciones.Single(c => c.Clave == "cfdi.token").Valor;
            var cfdiPassword = configuraciones.Single(c => c.Clave == "cfdi.password").Valor;
            var cfdiUser = configuraciones.Single(c => c.Clave == "cfdi.user").Valor;
            var cfdiCuenta = configuraciones.Single(c => c.Clave == "cfdi.cuenta").Valor;
            System.Net.ServicePointManager.SecurityProtocol =
            System.Net.SecurityProtocolType.Tls12;
            var url = "https://lb04.cfdinova.com.mx/KTimbradoService/webresources/TimbradoService/request";
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "POST";
            httpRequest.Accept = "application/json";
            httpRequest.ContentType = "application/json";
            var data = @"{

            ""credentials"": {
            ""usuario"":'" + cfdiUser + @"',
            ""token"":'" + cfdiToken + @"',
            ""password"":'" + cfdiPassword + @"',
           ""cuenta"": '" + cfdiCuenta + @"'
                               },

            ""issuer"": {
            ""rfc"": '" + RFCReceptor + @"',
            ""business"": ""TESTFD""
                                },

             ""document"": {
             ""format"": ""xml"",
             ""type"": ""cfdiv4"",
             ""operation"":""validation"",
             ""content"": '" + cadenaBase64 + @"' 
} }";
            var stringified = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<dynamic>(data));


            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(stringified);
            }

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var rt = streamReader.ReadToEnd();


                dynamic jsonObj = JsonConvert.DeserializeObject(rt);
                string story = (string)jsonObj["result"];



                string storys = (string)jsonObj["result"];

                if (storys == "INVALID")
                {
                    string message = (string)jsonObj["error_details"][0]["message"];
                    xmlFileStream.Close();
                    xmlFileStream.Dispose();
                    throw new BusinessException(message);
                }
                else
                {
                    string estado = (string)jsonObj["efos"]["estado"];
                    if (storys == "VALID" && estado == "Inexistente")
                    {
                        string estatus = (string)jsonObj["efos"]["estatus"];
                        xmlFileStream.Close();
                        xmlFileStream.Dispose();
                        throw new BusinessException(estatus);
                    }

                }


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
            var result = DbScale.GetDataTable("SELECT * FROM GNZN_Cifras_Control_CR_Facturas  WHERE Proveedor = '" + proveedor.NumeroProveedor + "' AND Factura='" + refe + "';");

            ////////////////////////////////
            //Fin de validacion Scale
            ////////////////////////////////

            if (result.Rows.Count == 1)
            {

                var row = result.Rows[0];
                int paresScale = Int32.Parse(row["Pares"].ToString());

                DateTime fechaFactura = DateTime.ParseExact(Fecha, "yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture);

                SapOrdenCompraManager ConsultaSap = new SapOrdenCompraManager();
                var totalParesSAP = ConsultaSap.GetCantidadValidacionSAP(fechaFactura.Year.ToString(), "1001", refe, proveedor.NumeroProveedor);
                //var totalParesSAP = ConsultaSap.GetCantidadValidacionSAP(fechaFactura.Year.ToString(), proveedor.Sociedad, refe, proveedor.NumeroProveedor);

                if (paresScale == totalParesSAP)
                {
                    ///////////////////
                    //Se ejecuta si todo coincide
                    ///////////////////
                    var cantidad = new decimal();

                    if (comprobante.Version33 == "4.0")
                    {
                        //Creacion de la factura para la miro
                        cantidad = comprobante.Conceptos.Concepto.Aggregate<ConceptoV, decimal>(0, (current, concepto) => current + Convert.ToDecimal(concepto.Cantidad33));
                    }
                    else
                    {
                        //Creacion de la factura para la miro
                        cantidad = comprobante.Conceptos.Concepto.Aggregate<ConceptoV, decimal>(0, (current, concepto) => current + Convert.ToDecimal(concepto.Cantidad));

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
                        FechaPortal = fecha,
                        RFCReceptor = RFCReceptor ?? string.Empty,
                        TipoFactura = "Mercaderia"

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
                        FechaPortal = fecha,
                        RFCReceptor = RFCReceptor ?? string.Empty,
                        TipoFactura = "Mercaderia"

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
                    FechaPortal = fecha,
                    RFCReceptor = RFCReceptor ?? string.Empty,
                    TipoFactura = "Mercaderia"

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
                FechaPortal = fecha,
                RFCReceptor = RFCReceptor ?? string.Empty,
                TipoFactura = "Mercaderia"

            };

            return factura;

        }



        public factura CargarFacturaFactorajeV4(int proveedorId, int cuentaId, string xmlFilePath, string pdfFilePath)
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
            var serializer = new XmlSerializer(typeof(ComprobanteVersion4));

            var xmlFileStream = new FileStream(xmlFilePath, FileMode.Open);


            var comprobante = (ComprobanteVersion4)serializer.Deserialize(xmlFileStream);

            var proveedor = new proveedore();
            var RFC = "";
            var RFCReceptor = "";
            var Serie = "";
            var Folio = "";
            var SubTotal = "";
            var Fecha = "";
            var Total = "";

            if (comprobante.Version33 == "4.0")
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
            var cadenaBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(File.ReadAllText(xmlFilePath)));
            // Validación ante el sat

            var configuraciones = _db.configuraciones.ToList();
            var cfdiToken = configuraciones.Single(c => c.Clave == "cfdi.token").Valor;
            var cfdiPassword = configuraciones.Single(c => c.Clave == "cfdi.password").Valor;
            var cfdiUser = configuraciones.Single(c => c.Clave == "cfdi.user").Valor;
            var cfdiCuenta = configuraciones.Single(c => c.Clave == "cfdi.cuenta").Valor;
            System.Net.ServicePointManager.SecurityProtocol =
            System.Net.SecurityProtocolType.Tls12;
            var url = "https://lb04.cfdinova.com.mx/KTimbradoService/webresources/TimbradoService/request";
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "POST";
            httpRequest.Accept = "application/json";
            httpRequest.ContentType = "application/json";
            var data = @"{

            ""credentials"": {
            ""usuario"":'" + cfdiUser + @"',
            ""token"":'" + cfdiToken + @"',
            ""password"":'" + cfdiPassword + @"',
           ""cuenta"": '" + cfdiCuenta + @"'
                               },

            ""issuer"": {
            ""rfc"": '" + RFCReceptor + @"',
            ""business"": ""TESTFD""
                                },

             ""document"": {
             ""format"": ""xml"",
             ""type"": ""cfdiv4"",
             ""operation"":""validation"",
             ""content"": '" + cadenaBase64 + @"' 
} }";
            var stringified = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<dynamic>(data));


            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(stringified);
            }

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var rt = streamReader.ReadToEnd();


                dynamic jsonObj = JsonConvert.DeserializeObject(rt);
                string story = (string)jsonObj["result"];



                string storys = (string)jsonObj["result"];

                if (storys == "INVALID")
                {
                    string message = (string)jsonObj["error_details"][0]["message"];
                    xmlFileStream.Close();
                    xmlFileStream.Dispose();
                    throw new BusinessException(message);
                }
                else
                {
                    string estado = (string)jsonObj["efos"]["estado"];
                    if (storys == "VALID" && estado == "Inexistente")
                    {
                        string estatus = (string)jsonObj["efos"]["estatus"];
                        xmlFileStream.Close();
                        xmlFileStream.Dispose();
                        throw new BusinessException(estatus);
                    }

                }


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
                FechaPortal = fecha,
                RFCReceptor = RFCReceptor ?? string.Empty,
                TipoFactura = "Mercaderia"

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
