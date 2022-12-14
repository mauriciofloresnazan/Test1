﻿using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using log4net;
using Ppgz.Repository;

namespace ScaleWrapper
{
    public class ScaleManager
    {
        public readonly ILog ErrorAppLog = LogManager.GetLogger(@"ErrorAppLog");
        private static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["Scale"].ConnectionString;

        public void Registrar(cita cita)
        {
            var entities = new Entities();
            var numerosDocumentos = cita.asns.Select(asn => asn.OrdenNumeroDocumento).Distinct();


            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                // Start a local transaction.
                transaction = connection.BeginTransaction("Insertar cita en Scale");

                // Must assign both transaction object and connection
                // to Command object for a pending local transaction
                command.Connection = connection;
                command.Transaction = transaction;

                try
                {
                    int i = 1;
                    foreach (var numeroDocumento in numerosDocumentos)
                    {
                        var documento = numeroDocumento;

                        var orden = cita.asns
                            .FirstOrDefault(asn => asn.OrdenNumeroDocumento == documento);

                        var almacenScale = entities.ScaleAlmacens.FirstOrDefault(sa => sa.Sap == orden.Centro);

                        if (almacenScale == null)
                        {
                            ErrorAppLog.Error(string.Format("El Almacén {0} no está configurado para Scale. Error en la Cita # {1},  Órden {2}", orden.Centro, cita.Id, orden.OrdenNumeroDocumento));
                            continue;
                        }

                        var id = InsertarHeader(command, cita, almacenScale.Scale, numeroDocumento, orden.TiendaOrigen, orden.Tienda, orden.NumeroOrdenSurtido, orden.InOut, i, cita.Id);

                        var asns = cita.asns.Where(asn => asn.OrdenNumeroDocumento == numeroDocumento).ToList();

                        InsertarDetails(command, id, almacenScale.Scale, asns, cita.Id);
                        i++;
                    }

                    // Attempt to commit the transaction.
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    ErrorAppLog.Error(string.Format("Commit Exception Type: {0}", ex.GetType()));
                    ErrorAppLog.Error(string.Format("  Message: {0}", ex.Message));

                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        ErrorAppLog.Error(string.Format("Rollback Exception Type: {0}", ex2.GetType()));
                        ErrorAppLog.Error(string.Format("  Message: {0}", ex2.Message));
                    }
                }
            }



        }

        internal string InsertarHeader(SqlCommand command, cita cita, string almacenScale, string numeroOrden,
            string tiendaOrigen, string tiendaDestino, string numeroOrdenSurtido, string inOut, int i, int citaId)
        {

            var proveedor = cita.proveedore;



            var id = string.Format("{0}{1}{2}", DateTime.Now.ToString("yyyyMMddHHmmss"), cita.Id, i);
            var sourcename = string.Format("{0} {1} {2} {3}",
                                                proveedor.Nombre1,
                                                proveedor.Nombre2,
                                                proveedor.Nombre3,
                                                proveedor.Nombre4);


            /*/
             * 
             * Agregar horario de cita mediante escaneo de los horarios de los rieles
             * */
            var horarios = cita.horariorieles;
            foreach (horarioriele hor in horarios)
            {
                Char delimiter = ':';
                String[] substrings = hor.horario.HoraDesde.Split(delimiter);
                String[] substrings2 = substrings[1].Split(null);

                var horacitascale = Int32.Parse(substrings[0].ToString());

                if (substrings2[1].ToString() == "pm")
                {
                    if (horacitascale != 12)
                    {
                        horacitascale = horacitascale + 12;
                    }

                }

                if (cita.FechaCita.Hour == 0)
                {

                    cita.FechaCita = cita.FechaCita.AddHours(horacitascale);
                    cita.FechaCita = cita.FechaCita.AddMinutes(Int32.Parse(substrings2[0].ToString()));

                }
                else if (cita.FechaCita.Hour > horacitascale)
                {
                    var restar = 0 - cita.FechaCita.Hour;
                    var fecha = new DateTime(cita.FechaCita.Year, cita.FechaCita.Month, cita.FechaCita.Day);
                    cita.FechaCita = fecha;
                    cita.FechaCita = cita.FechaCita.AddHours(horacitascale);
                    cita.FechaCita = cita.FechaCita.AddMinutes(Int32.Parse(substrings2[0].ToString()));

                }



            }
            //Fin


            var user_def4 = "";
            var user_def6 = "";

            if (cita.Almacen.ToUpper() == "CROSS DOCK")
            {
                user_def4 = "'" + tiendaDestino + "'";
                user_def6 = "'" + numeroOrdenSurtido + "'";
            }
            else
            {
                user_def4 = "NULL";
                user_def6 = "NULL";

            }

            int insd = inOut == "1" ? 1 : 0;
            string ordencomprastring = cita.Almacen == "Cross Dock" ? "Cross Dock" : "Orden de Compra";

            var sql = string.Format(@"

                INSERT INTO download_receipt_header 
                            (interface_record_id, 
                             interface_action_code, 
                             interface_condition, 
                             warehouse, 
                             erp_order_num, 
                             receipt_id, 
                             receipt_id_type, 
                             receipt_type, 
                             receipt_date, 
                             ship_from, 
                             ship_from_address1, 
                             ship_from_address2, 
                             ship_from_city, 
                             ship_from_state, 
                             ship_from_country, 
                             ship_from_postal_code, 
                             ship_from_name, 
                             ship_from_email_address, 
                             ship_from_phone_num, 
                             ship_from_fax_num, 
                             source_id, 
                             source_name, 
                             source_address1, 
                             source_address2, 
                             source_city, 
                             source_state, 
                             source_postal_code, 
                             source_country, 
                             source_phone_num, 
                             source_fax_num, 
                             source_email_address, 
                             user_def1, 
                             user_def2, 
                             user_def3, 
                             user_def4, 
                             user_def6, 
                             user_def7, 
                             user_def8,
                             user_stamp,
                             date_time_stamp, 
                             arrived_date_time) 
                VALUES      ('" + id + @"', 
                             'Save', 
                             'Ready', 
                             '" + almacenScale + @"', 
                             '" + numeroOrden + @"',  
                             dbo.GNZN_Fn_Folio_Recibo('" + numeroOrden + @"', '" + cita.Id + @"'),
                             '" + ordencomprastring + @"',  
                             '" + ordencomprastring + @"',
                             '" + cita.FechaCita.ToString("yyyy-MM-ddTHH:mm:ss") + @"',  
                             '" + proveedor.NumeroProveedor + @"',  
                             '" + proveedor.Calle + @"',  
                             '" + proveedor.Direccion + @"',  
                             '" + proveedor.Poblacion + @"',  
                             '" + proveedor.EstadoNombre + @"',  
                             'MEXICO', 
                             '" + proveedor.CodigoPostal + @"', 
                             '" + sourcename.Substring(0, Math.Min(sourcename.Length, 50)) + @"',  
                             '" + proveedor.Correo + @"',  
                             '" + proveedor.NumeroTelefono + @"',  
                             '', 
                             '" + proveedor.NumeroProveedor + @"',  
                             '" + sourcename.Substring(0, Math.Min(sourcename.Length, 50)) + @"',
                             '" + proveedor.Calle + @"',  
                             '" + proveedor.Direccion + @"',  
                             '" + proveedor.Poblacion + @"',  
                             '" + proveedor.EstadoNombre + @"',
                             '" + proveedor.CodigoPostal + @"',   
                             'MEXICO', 
                             '" + proveedor.NumeroTelefono + @"',  
                             '', 
                             '" + proveedor.Correo + @"',
                             '" + cita.FechaCita.ToString("yyyyMMdd") + @"',  
                             '" + proveedor.OrganizacionCompra + @"',  
                             '" + tiendaOrigen + @"',  
                             " + user_def4 + @",   
                             " + user_def6 + @",  
                             " + insd + @",  
                             '" + cita.Id + @"', 
                             '" + cita.CantidadTotal + @"', 
                             GETDATE(), 
                             '" + cita.FechaCita.ToString("yyyy-MM-ddTHH:mm:ss") + @"');");

            command.CommandText = sql;

            command.ExecuteNonQuery();

            return id;
        }

        internal void InsertarDetails(SqlCommand command, string interfaceLinkId, string almacenScale, List<asn> asns, int citaId)
        {
            var sql = new StringBuilder();

            sql.AppendLine(@"

                INSERT INTO DOWNLOAD_RECEIPT_DETAIL 
                   (INTERFACE_RECORD_ID,
                    Interface_link_id,
                    warehouse,
                    INTERFACE_ACTION_CODE,
                    INTERFACE_CONDITION,
                    ERP_ORDER_LINE_NUM,
                    item,
                    ITEM_NET_PRICE,
                    user_def5,
                    TOTAL_QTY,
                    QUANTITY_UM,
                    DATE_TIME_STAMP) 
                VALUES");

            var parameters = new List<SqlParameter>();

            for (var index = 0; index < asns.Count; index++)
            {
                //// Si es multiplo de 100 
                //if (index % 100 == 0)
                //{

                //    // si tiene valores lo inserta
                //    if (parameters.Any())
                //    {
                //        DbScale.Insert(sql.ToString(), parameters);
                //    }

                //    sql = new StringBuilder();

                //    sql.AppendLine(@"

                //    INSERT INTO DOWNLOAD_RECEIPT_DETAIL 
                //       (INTERFACE_RECORD_ID,
                //        Interface_link_id,
                //        warehouse,
                //        INTERFACE_ACTION_CODE,
                //        INTERFACE_CONDITION,
                //        ERP_ORDER_LINE_NUM,
                //        item,
                //        ITEM_NET_PRICE,
                //        user_def5,
                //        TOTAL_QTY,
                //        QUANTITY_UM,
                //        DATE_TIME_STAMP) 
                //    VALUES");
                //    parameters = new List<SqlParameter>();
                //}
                //else
                //{
                //    sql.Append(",");
                //}

                if (index > 0)
                {
                    sql.Append(",");
                }

                var asn = asns[index];
                var id = string.Format("{0}{1}{2}", DateTime.Now.ToString("yyyyMMddHHmmssfff"), asn.cita.Id, index);


                var QUANTITY_UM = "";

                if (asn.UnidadMedida == "ST")
                {
                    QUANTITY_UM = "Par";

                }
                else
                {
                    QUANTITY_UM = asn.UnidadMedida.Substring(0, 3);

                }


                sql.AppendLine("('" + id + "', '" + interfaceLinkId + "', '" + almacenScale + "', 'Save', 'Ready', '" + asn.NumeroPosicion + "', '" + asn.NumeroMaterial2 + "', '" + asn.Precio + "', '" + asn.cita.FechaCita.ToString("yyyyMMdd") + "', '" + asn.Cantidad + "', '" + QUANTITY_UM + "', GETDATE())");
            }


            command.CommandText = sql.ToString();

            // Si los parametros nos son null los recorremos
            if (parameters != null)
            {
                foreach (SqlParameter param in parameters)
                {
                    command.Parameters.Add(param);
                }
            }
            command.ExecuteNonQuery();

        }

        public void Cancelar(int citaId)
        {

            if (citaId < 1)
            {
                ErrorAppLog.Error(string.Format("Cita # {0} incorrecta", citaId));
                return;
            }

            const string sql = @"
                
                DELETE FROM DOWNLOAD_RECEIPT_DETAIL WHERE INTERFACE_LINK_ID IN(SELECT INTERFACE_RECORD_ID FROM DOWNLOAD_RECEIPT_HEADER WHERE USER_DEF8 = @CitaId)
                AND    INTERFACE_ACTION_CODE = 'Delete'

                UPDATE DOWNLOAD_RECEIPT_DETAIL
                SET    INTERFACE_CONDITION = 'Ready',
                       INTERFACE_ACTION_CODE = 'Delete'
                WHERE  INTERFACE_LINK_ID IN(SELECT INTERFACE_RECORD_ID FROM DOWNLOAD_RECEIPT_HEADER WHERE USER_DEF8 = @CitaId)
                AND    INTERFACE_ACTION_CODE <> 'Delete'

                UPDATE DOWNLOAD_RECEIPT_HEADER
                SET    INTERFACE_CONDITION = 'Ready',
                       INTERFACE_ACTION_CODE = 'Delete'
                WHERE  USER_DEF8 = @CitaId";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@CitaId", citaId)
            };

            try
            {
                DbScale.Insert(sql, parameters);
            }
            catch (Exception exception)
            {
                ErrorAppLog.Error(string.Format("Cita # {0}. {1}", citaId, exception.Message));
            }

        }

        public void ActualizarFecha(int citaId)
        {
            var entities = new Entities();

            var cita = entities.citas.Find(citaId);

            if (cita == null)
            {
                ErrorAppLog.Error(string.Format("Cita # {0} incorrecta", citaId));
                return;
            }
            var horarios = cita.horariorieles;
            foreach (horarioriele hor in horarios)
            {
                Char delimiter = ':';
                String[] substrings = hor.horario.HoraDesde.Split(delimiter);
                String[] substrings2 = substrings[1].Split(null);

                var horacitascale = Int32.Parse(substrings[0].ToString());

                if (substrings2[1].ToString() == "pm")
                {
                    if (horacitascale != 12)
                    {
                        horacitascale = horacitascale + 12;
                    }

                }

                if (cita.FechaCita.Hour == 0)
                {

                    cita.FechaCita = cita.FechaCita.AddHours(horacitascale);
                    cita.FechaCita = cita.FechaCita.AddMinutes(Int32.Parse(substrings2[0].ToString()));

                }
                else if (cita.FechaCita.Hour > horacitascale)
                {
                    var restar = 0 - cita.FechaCita.Hour;
                    var fecha = new DateTime(cita.FechaCita.Year, cita.FechaCita.Month, cita.FechaCita.Day);
                    cita.FechaCita = fecha;
                    cita.FechaCita = cita.FechaCita.AddHours(horacitascale);
                    cita.FechaCita = cita.FechaCita.AddMinutes(Int32.Parse(substrings2[0].ToString()));

                }



            }
            const string sql = @"

                UPDATE DOWNLOAD_RECEIPT_DETAIL
                SET    USER_DEF5 = @user_def5,
	                   INTERFACE_CONDITION = 'Ready'
                WHERE  INTERFACE_LINK_ID IN(SELECT INTERFACE_RECORD_ID FROM DOWNLOAD_RECEIPT_HEADER WHERE USER_DEF8 = @CitaId)

                UPDATE DOWNLOAD_RECEIPT_HEADER
                SET    RECEIPT_DATE = @RECEIPT_DATE, 
	                   ARRIVED_DATE_TIME = @ARRIVED_DATE_TIME,
	                   USER_DEF1 = @USER_DEF1,
	                   INTERFACE_CONDITION = 'Ready'
                WHERE  USER_DEF8 = @CitaId";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@CitaId", citaId),
                new SqlParameter("@user_def5", cita.FechaCita.ToString("yyyyMMdd")),

                new SqlParameter("@RECEIPT_DATE", cita.FechaCita),
                new SqlParameter("@ARRIVED_DATE_TIME", cita.FechaCita.ToString("yyyy-MM-ddTHH:mm:ss")),
                new SqlParameter("@user_def1", cita.FechaCita.ToString("yyyyMMdd")),
            };

            try
            {
                DbScale.Insert(sql, parameters);
            }
            catch (Exception exception)
            {
                ErrorAppLog.Error(string.Format("Cita # {0}. {1}", citaId, exception.Message));
            }
        }



        public void ActualizarFechaEnroque(int citaId)
        {
            var entities = new Entities();

            var cita = entities.citas.Find(citaId);

            if (cita == null)
            {
                ErrorAppLog.Error(string.Format("Cita # {0} incorrecta", citaId));
                return;
            }
            var horarios = cita.horariorieles;
            foreach (horarioriele hor in horarios)
            {
                Char delimiter = ':';
                String[] substrings = hor.horario.HoraDesde.Split(delimiter);
                String[] substrings2 = substrings[1].Split(null);

                var horacitascale = Int32.Parse(substrings[0].ToString());

                if (substrings2[1].ToString() == "pm")
                {
                    if (horacitascale != 12)
                    {
                        horacitascale = horacitascale + 12;
                    }

                }

                if (cita.FechaCita.Hour == 0)
                {

                    cita.FechaCita = cita.FechaCita.AddHours(horacitascale);
                    cita.FechaCita = cita.FechaCita.AddMinutes(Int32.Parse(substrings2[0].ToString()));

                }
                else if (cita.FechaCita.Hour > horacitascale)
                {
                    var restar = 0 - cita.FechaCita.Hour;
                    var fecha = new DateTime(cita.FechaCita.Year, cita.FechaCita.Month, cita.FechaCita.Day);
                    cita.FechaCita = fecha;
                    cita.FechaCita = cita.FechaCita.AddHours(horacitascale);
                    cita.FechaCita = cita.FechaCita.AddMinutes(Int32.Parse(substrings2[0].ToString()));

                }



            }
            const string sql = @"

                UPDATE DOWNLOAD_RECEIPT_HEADER
                SET     
	                   ARRIVED_DATE_TIME = @ARRIVED_DATE_TIME,
	                   INTERFACE_CONDITION = 'Ready'
                WHERE  USER_DEF8 = @CitaId";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@CitaId", citaId),
                new SqlParameter("@ARRIVED_DATE_TIME", cita.FechaCita.ToString("yyyy-MM-ddTHH:mm:ss")),
            };

            try
            {
                DbScale.Insert(sql, parameters);
            }
            catch (Exception exception)
            {
                ErrorAppLog.Error(string.Format("Cita # {0}. {1}", citaId, exception.Message));
            }
        }




        public void ActualizarCantidad(int[] asnIds)
        {
            var entities = new Entities();

            if (!asnIds.Any())
            {
                return;
            }

            foreach (var asnId in asnIds)
            {
                var asn = entities.asns.Find(asnId);

                if (asn == null)
                {
                    ErrorAppLog.Error(string.Format("Asn # {0} incorrecto", asnId));
                    return;
                }
                const string sql = @"

                UPDATE DOWNLOAD_RECEIPT_DETAIL
                SET    TOTAL_QTY = @cantidad,
	                   INTERFACE_CONDITION = 'Ready'
                WHERE  INTERFACE_LINK_ID IN(SELECT INTERFACE_RECORD_ID FROM DOWNLOAD_RECEIPT_HEADER WHERE USER_DEF8 = @CitaId AND ERP_ORDER_NUM = @ERP_ORDER_NUM)
                AND    item = @item 
                AND    ERP_ORDER_LINE_NUM = CAST (@ERP_ORDER_LINE_NUM AS NUMERIC)

                UPDATE DOWNLOAD_RECEIPT_HEADER
                SET    INTERFACE_CONDITION = 'Ready'
                WHERE  USER_DEF8 = @CitaId
                AND    ERP_ORDER_NUM = @ERP_ORDER_NUM";

                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@CitaId", asn.cita.Id),
                    new SqlParameter("@cantidad", asn.Cantidad),
                    new SqlParameter("@ERP_ORDER_LINE_NUM", asn.NumeroPosicion),
                    new SqlParameter("@item", asn.NumeroMaterial2),
                    new SqlParameter("@ERP_ORDER_NUM", asn.OrdenNumeroDocumento),
                };

                try
                {
                    DbScale.Insert(sql, parameters);
                }
                catch (Exception exception)
                {
                    ErrorAppLog.Error(string.Format("Asn # {0}. {1}", asn.Id, exception.Message));
                }



            }


        }


        public void EliminarAsn(asn asnAeliminar)
        {

            const string sql = @"

                UPDATE DOWNLOAD_RECEIPT_DETAIL
                SET    INTERFACE_CONDITION = 'Ready',
                       INTERFACE_ACTION_CODE = 'Delete'
                WHERE  INTERFACE_LINK_ID IN(SELECT INTERFACE_RECORD_ID FROM DOWNLOAD_RECEIPT_HEADER WHERE USER_DEF8 = @CitaId AND ERP_ORDER_NUM = @ERP_ORDER_NUM)
                AND    item = @item 
                AND    ERP_ORDER_LINE_NUM = CAST (@ERP_ORDER_LINE_NUM AS NUMERIC)

                UPDATE DOWNLOAD_RECEIPT_HEADER
                SET    INTERFACE_CONDITION = 'Ready'
                WHERE  USER_DEF8 = @CitaId
                AND    ERP_ORDER_NUM = @ERP_ORDER_NUM";

            var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@CitaId", asnAeliminar.cita.Id),
                    new SqlParameter("@cantidad", asnAeliminar.Cantidad),
                    new SqlParameter("@ERP_ORDER_LINE_NUM", asnAeliminar.NumeroPosicion),
                    new SqlParameter("@item", asnAeliminar.NumeroMaterial2),
                    new SqlParameter("@ERP_ORDER_NUM", asnAeliminar.OrdenNumeroDocumento),
                };

            try
            {
                DbScale.Insert(sql, parameters);
            }
            catch (Exception exception)
            {
                ErrorAppLog.Error(string.Format("Elimiacion Asn # {0} CitaId= {2} :. {1}", asnAeliminar.Id, exception.Message, asnAeliminar.cita.Id));
            }
        }


    }
}