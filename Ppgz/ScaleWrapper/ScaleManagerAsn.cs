using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using log4net;
using Ppgz.Repository;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Data;
using System.IO;
using MySql.Data.MySqlClient;

namespace ScaleWrapper
{
    public class ScaleManagerAsn
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
                            .FirstOrDefault(asn => asn.OrdenNumeroDocumento == documento & asn.CitaId == cita.Id);

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
                    var db = new Entities();
                    var proveedor = cita.proveedore;
                    var pvr = Convert.ToInt32(proveedor.NumeroProveedor);
                    var dat = db.EnviarDatos.Where(op=> op.Id_Proveedor==pvr).ToList();
                    InsertarDetailsContenedor(command, cita, dat);
                    // Attempt to commit the transaction.
                    transaction.Commit();
                    //command.Transaction.Commit();
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

        public void RegistrarMINE(cita cita)
        {

            try
            {
                var proveedor = cita.proveedore;
                var pvr = Convert.ToInt32(proveedor.NumeroProveedor);

                //GetData_Mysql_to_InsertScale.- Obtenemos los valores de la DB MySQL para armar los insert a SCALE
                // con el SP sp_GetInsertToScale se le pasa el parametro CitaId

                //Variable para concatenar el insert de la cabecera
                
                 var InsertHeader = new StringBuilder();
                 InsertHeader.AppendLine("");
                
                //string InsertHeader = @"  INSERT INTO download_receipt_header (interface_record_id, 
                //        interface_action_code, interface_condition, warehouse, erp_order_num, receipt_id, receipt_id_type, receipt_type, 
                //        receipt_date, ship_from, ship_from_address1, ship_from_address2, ship_from_city, ship_from_state, ship_from_country, 
                //        ship_from_postal_code, ship_from_name, ship_from_email_address, ship_from_phone_num, ship_from_fax_num, source_id, 
                //        source_name, source_address1, source_address2, source_city, source_state, source_postal_code, source_country, 
                //        source_phone_num, source_fax_num, source_email_address, user_def1, user_def2, user_def3, user_def4, USER_DEF5,
                //        user_def6, user_def7, user_def8,user_stamp,date_time_stamp, arrived_date_time) 
                //        SELECT * FROM (
                //        VALUES ";

                //Variable para concatenar los valores de la cabecera
                
                 var ValuesHeader = new StringBuilder();
                 ValuesHeader.AppendLine(@"");
                 
                //string ValuesHeader = "";

                //Variable para concatenar el insert de los detalles
                
                 var InsertDetails = new StringBuilder();
                 InsertDetails.AppendLine("");
                 
                //string InsertDetails = @"  INSERT INTO DOWNLOAD_RECEIPT_DETAIL 
                //        (INTERFACE_RECORD_ID,Interface_link_id,warehouse,
                //        INTERFACE_ACTION_CODE,INTERFACE_CONDITION,ERP_ORDER_LINE_NUM,item,
                //        ITEM_NET_PRICE,user_def5,TOTAL_QTY,QUANTITY_UM,DATE_TIME_STAMP) 
                //        SELECT * FROM ( 
                //        VALUES ";

                //Variable para concatenar los valores delos detalles
                
                 var ValuesDetails = new StringBuilder();
                 ValuesDetails.AppendLine(@"");
                 
                //string ValuesDetails = "";

                //Variable que nos ayuda a guardar de manera temporal el registro anterior del cursor de la iteracion 
                //almacena el numero de pedido (OrdenNumeroDocumento de la tabla asn)
                //ejemplo  4502134238
                string documentoAnterior = "";

                //var parameters = new List<MySqlParameter>()
                //{
                //  new MySqlParameter("cita",cita.Id)
                //};
                ////Db.StoreProcedure(parameters, "sp_GetInsertToScale");
                //DataSet ds_HeaderandDetail=Db.GetDataSet(parameters, "sp_GetInsertToScale");
                DataSet ds_HeaderandDetail = Db.GetDataReader("SET SQL_SAFE_UPDATES=0; call sp_GetInsertToScale(" + cita.Id.ToString() + "," + pvr.ToString() + ");");
                if (ds_HeaderandDetail.Tables[0].Rows.Count > 0)
                    {
                        //Nos ayuda a saber si es el primer registro
                        bool IsFirstRegistro = true;
                        //Recorremos los registos del query string QueryMaster
                        foreach (DataRow dr in ds_HeaderandDetail.Tables[0].Rows)
                        {
                            if (IsFirstRegistro)
                            {
                                //ValuesHeader = ValuesHeader + dr[1].ToString();
                                ValuesHeader.AppendLine(dr[1].ToString());
                                ValuesDetails.AppendLine(dr[2].ToString());
                                IsFirstRegistro = false;
                            }
                            else
                            {
                                if (dr[0].ToString().Equals(documentoAnterior))
                                {
                                    //ValuesDetails = ValuesDetails + dr[2].ToString();
                                    ValuesDetails.AppendLine( dr[2].ToString());
                                }
                                else
                                {
                                    //ValuesHeader = ValuesHeader + dr[1].ToString();
                                    ValuesHeader.AppendLine(dr[1].ToString());
                                    //ValuesDetails = ValuesDetails + dr[2].ToString();
                                    ValuesDetails.AppendLine(dr[2].ToString());
                                }
                            }
                            documentoAnterior = dr[0].ToString();
                        }

                    //ValuesHeader = ValuesHeader.TrimEnd(',');                    
                    //ValuesDetails = ValuesDetails.TrimEnd(',');
                    

                    ////InsertHeader = InsertHeader + ValuesHeader + @") AS a (interface_record_id, 
                    ////    interface_action_code, interface_condition, warehouse, erp_order_num, receipt_id, receipt_id_type, receipt_type, 
                    ////    receipt_date, ship_from, ship_from_address1, ship_from_address2, ship_from_city, ship_from_state, ship_from_country, 
                    ////    ship_from_postal_code, ship_from_name, ship_from_email_address, ship_from_phone_num, ship_from_fax_num, source_id, 
                    ////    source_name, source_address1, source_address2, source_city, source_state, source_postal_code, source_country, 
                    ////    source_phone_num, source_fax_num, source_email_address, user_def1, user_def2, user_def3, user_def4, USER_DEF5,
                    ////    user_def6, user_def7, user_def8,user_stamp,date_time_stamp, arrived_date_time);";
                    //InsertHeader.AppendLine(@"  INSERT INTO download_receipt_header(interface_record_id,
                    //    interface_action_code, interface_condition, warehouse, erp_order_num, receipt_id, receipt_id_type, receipt_type,
                    //    receipt_date, ship_from, ship_from_address1, ship_from_address2, ship_from_city, ship_from_state, ship_from_country,
                    //    ship_from_postal_code, ship_from_name, ship_from_email_address, ship_from_phone_num, ship_from_fax_num, source_id,
                    //    source_name, source_address1, source_address2, source_city, source_state, source_postal_code, source_country,
                    //    source_phone_num, source_fax_num, source_email_address, user_def1, user_def2, user_def3, user_def4, USER_DEF5,
                    //    user_def6, user_def7, user_def8, user_stamp, date_time_stamp, arrived_date_time)
                    //    SELECT * FROM(
                    //    VALUES " + ValuesHeader.ToString() + @") AS a (interface_record_id, 
                    //        interface_action_code, interface_condition, warehouse, erp_order_num, receipt_id, receipt_id_type, receipt_type, 
                    //        receipt_date, ship_from, ship_from_address1, ship_from_address2, ship_from_city, ship_from_state, ship_from_country, 
                    //        ship_from_postal_code, ship_from_name, ship_from_email_address, ship_from_phone_num, ship_from_fax_num, source_id, 
                    //        source_name, source_address1, source_address2, source_city, source_state, source_postal_code, source_country, 
                    //        source_phone_num, source_fax_num, source_email_address, user_def1, user_def2, user_def3, user_def4, USER_DEF5,
                    //        user_def6, user_def7, user_def8,user_stamp,date_time_stamp, arrived_date_time);");
                    InsertHeader.AppendLine(ValuesHeader.ToString());
                    ////InsertDetails = InsertDetails + ValuesDetails + @") AS b (INTERFACE_RECORD_ID,Interface_link_id,warehouse,
                    ////    INTERFACE_ACTION_CODE,INTERFACE_CONDITION,ERP_ORDER_LINE_NUM,item,
                    ////    ITEM_NET_PRICE,user_def5,TOTAL_QTY,QUANTITY_UM,DATE_TIME_STAMP);";
                    //InsertDetails.AppendLine(@"  INSERT INTO DOWNLOAD_RECEIPT_DETAIL 
                    //(INTERFACE_RECORD_ID,Interface_link_id,warehouse,
                    //INTERFACE_ACTION_CODE,INTERFACE_CONDITION,ERP_ORDER_LINE_NUM,item,
                    //ITEM_NET_PRICE,user_def5,TOTAL_QTY,QUANTITY_UM,DATE_TIME_STAMP) 
                    //SELECT * FROM ( 
                    //VALUES " + ValuesDetails.ToString() + @") AS b (INTERFACE_RECORD_ID,Interface_link_id,warehouse,
                    //    INTERFACE_ACTION_CODE,INTERFACE_CONDITION,ERP_ORDER_LINE_NUM,item,
                    //    ITEM_NET_PRICE,user_def5,TOTAL_QTY,QUANTITY_UM,DATE_TIME_STAMP);");
                    InsertDetails.AppendLine(ValuesDetails.ToString());
                    }

                
                 var U_Etiquetas = new StringBuilder();
                 U_Etiquetas.AppendLine(@"");
                 
                //string U_Etiquetas = "";
                if (ds_HeaderandDetail.Tables[1].Rows.Count > 0)
                {
                    string fnAnt = "";
                    string recibo = "";
                    foreach (DataRow dr in ds_HeaderandDetail.Tables[1].Rows)
                    {
                        if (fnAnt.Equals(dr[0].ToString()))
                        {
                            U_Etiquetas.AppendLine(" UPDATE gnzn_asn_etiquetas SET recibo='" + recibo + "' WHERE id_eti=" + dr[1].ToString() + "; ");
                        }
                        else
                        {
                            var r = DbScale.GetDataTable(dr[0].ToString());
                            if (r.Rows.Count == 1)
                            {
                                var row = r.Rows[0];
                                var re = row.ItemArray;
                                var resiosos = row[0];
                                recibo = Convert.ToString(re[0]);
                                U_Etiquetas.AppendLine(" UPDATE gnzn_asn_etiquetas SET recibo='" + Convert.ToString(re[0]) + "' WHERE id_eti=" + dr[1].ToString() + "; ");
                                //U_Etiquetas = U_Etiquetas + "UPDATE gnzn_asn_etiquetas SET recibo='" + Convert.ToString(re[0]) + "' WHERE id_eti=" + dr[1].ToString() + "; ";
                            }
                        }
                        fnAnt = dr[0].ToString();
                    }

                }

                if (!string.IsNullOrEmpty(U_Etiquetas.ToString()))
                    Db.Update(U_Etiquetas.ToString());

                var db = new Entities();
                //var dat = db.EnviarDatos.Where(op => op.Id_Proveedor == pvr).ToList();
                
                 var contenedores = new StringBuilder();
                 contenedores.AppendLine(GetInsertarDetailsContenedor( cita));                 
                //string contenedores =GetInsertarDetailsContenedor(cita);

                SqlConnection connection = new SqlConnection(ConnectionString);
                try
                {    
                    connection.Open();
                    SqlCommand command = new SqlCommand();
                    command.Connection = connection;
                    command.CommandTimeout = 0;
                    //command.Transaction = transaction;
                    command.CommandText = InsertHeader.ToString() + "  " + InsertDetails.ToString() + "  " + contenedores.ToString() + @" 
                                            BEGIN TRAN
                                            UPDATE DOWNLOAD_RECEIPT_HEADER SET INTERFACE_CONDITION = 'Ready' WHERE USER_DEF8 = " + cita.Id + @" AND INTERFACE_CONDITION = 'Po_Pr_Proc'
                                            COMMIT TRAN";
                    command.ExecuteNonQuery();
                }
                catch (Exception ed)
                {
                    ErrorAppLog.Error(string.Format("Commit Exception Type: {0}", ed.GetType()));
                    ErrorAppLog.Error(string.Format("  Message: {0}", ed.Message));
                }
                finally
                {
                    ////commit
                    /////rollback
                    connection.Close();
                }

            }
            catch (Exception ex)
            {
                ErrorAppLog.Error(string.Format("Commit Exception Type: {0}", ex.GetType()));
                ErrorAppLog.Error(string.Format("  Message: {0}", ex.Message));

                // Attempt to roll back the transaction.
                try
                {
                    //transaction.Rollback();
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

        private string  GetInsertarDetailsContenedor(cita cita)
        {
            var db = new Entities();
            var proveedor = cita.proveedore;
            var pvr = Convert.ToInt32(proveedor.NumeroProveedor);

            DataSet ds=Db.GetDataReader(@"SET SQL_SAFE_UPDATES=0;
  UPDATE gnzn_asn_etiquetas e
                SET e.pares = (
                    SELECT sum(pares) FROM gnzn_tmp_asn_revision_datos rv WHERE rv.Id_Proveedor = e.Id_Proveedor
                                            AND rv.Edo_Rev = 0  AND rv.Caja = e.caja GROUP BY rv.Caja
                                )
                WHERE e.Id_Proveedor = " + pvr + " AND e.cita = "+ cita.Id + @";
     
     UPDATE gnzn_tmp_asn_revision_datos SET Edo_Rev=1 WHERE Edo_Rev=0 AND Id_Proveedor=" + pvr + " AND CitaId=" + cita.Id + @";

SELECT concat('
INSERT INTO DOWNLOAD_RECEIPT_CONTAINER (INTERFACE_RECORD_ID, INTERFACE_LINK_ID,INTERFACE_ACTION_CODE, INTERFACE_CONDITION, container_id, container_type, DATE_TIME_STAMP, REASON_CODE, QUANTITY, TMP_ERP_ORDER_LINE_NUM)
select 
''',IFNULL(INTERFACE_RECORD_ID,''),''',''',IFNULL(INTERFACE_LINK_ID,''),''',''',IFNULL(INTERFACE_ACTION_CODE,''),
''',''',IFNULL(INTERFACE_CONDITION,''),''',''',IFNULL(container_id,''),''',''',IFNULL(container_type,''),''',''',IFNULL(DATE_FORMAT(DATE_TIME_STAMP,'%Y%c%d'),''),
''',''',IFNULL(REASON_CODE,''),''',',IFNULL(convert(QUANTITY,CHAR),'0'),',',IFNULL(convert(TMP_ERP_ORDER_LINE_NUM,CHAR),'0'),' go ')

 FROM gnzn_datos_asn_contendores WHERE CitaId=" + cita.Id+";");

            var query = new StringBuilder();
            

            var valores = new StringBuilder();
            //string valores = "";
            if(ds.Tables[0].Rows.Count > 0)
            {                
                foreach(DataRow dr in ds.Tables[0].Rows)
                {
                    
                    //valores = valores + dr[0].ToString();                    
                    valores.AppendLine(dr[0].ToString());
                }
            }
            //valores = valores.ToString().TrimEnd(',');
            //valores.Remove(valores.Length -1, 1);
            query.AppendLine(valores.ToString());
            //query.AppendLine(@"INSERT INTO DOWNLOAD_RECEIPT_CONTAINER 
            //       (INTERFACE_RECORD_ID, 
            //        INTERFACE_LINK_ID,
            //        INTERFACE_ACTION_CODE, 
            //        INTERFACE_CONDITION, 
            //        container_id, 
            //        container_type, 
            //       DATE_TIME_STAMP, 
            //       REASON_CODE, 
            //       QUANTITY, 
            //      TMP_ERP_ORDER_LINE_NUM) 
            //        SELECT * FROM (
            //    VALUES" + valores.ToString() + @")AS c (INTERFACE_RECORD_ID, 
            //        INTERFACE_LINK_ID,
            //        INTERFACE_ACTION_CODE, 
            //        INTERFACE_CONDITION, 
            //        container_id, 
            //        container_type, 
            //       DATE_TIME_STAMP, 
            //       REASON_CODE, 
            //       QUANTITY, 
            //      TMP_ERP_ORDER_LINE_NUM);");
            return query.ToString();
        }

        private void InsertarDetailsContenedor(SqlCommand command, cita cita, List<EnviarDatos> asns)
        {
            var db = new Entities();
            var proveedor = cita.proveedore;
            var pvr = Convert.ToInt32(proveedor.NumeroProveedor);
            var oo = db.RevisarDatos.Where(pp => pp.Edo_Rev == 0 && pp.Id_Proveedor==pvr).ToList();
            var pedidos = db.EnviarEtiquetas.Where(rp => rp.Id_Proveedor == pvr && rp.cita==cita.Id).ToList();

            foreach (var etiq in oo)
            {
                var preAsn = pedidos.FirstOrDefault(or => or.caja == etiq.Caja);
                var dato = oo.Where(x => x.Caja == etiq.Caja).Sum(x => x.Pares);
                preAsn.pares = dato;
                db.Entry(etiq).State = EntityState.Modified;
                db.SaveChanges();
            }

           (from r in db.RevisarDatos
             where r.Edo_Rev == 0 && r.Id_Proveedor == pvr
             select r).ToList()
           .ForEach(b => b.CitaId = cita.Id);

            (from r in db.RevisarDatos
             where r.Edo_Rev == 0 && r.Id_Proveedor==pvr
             select r).ToList()
           .ForEach(b => b.Edo_Rev = 1);
            db.SaveChanges();
            var parameters = new List<SqlParameter>();
            var dat = db.EnviarDatos.Where(op =>op.CitaId==cita.Id).ToList();
            for (var index = 0; index < dat.Count; index++)
            {
                 var asn = dat[index];
                //string connectionString = "Data Source=172.25.4.43;Initial Catalog=ILS;User ID=wsp;Password=wsp@2017;";
                //SqlConnection connectiones = new SqlConnection(@connectionString);
                SqlConnection connectiones = new SqlConnection(ConnectionString);
                string query = @"INSERT INTO DOWNLOAD_RECEIPT_CONTAINER 
                   (INTERFACE_RECORD_ID, 
                    INTERFACE_LINK_ID,
                    INTERFACE_ACTION_CODE, 
                    INTERFACE_CONDITION, 
                    container_id, 
                    container_type, 
                   DATE_TIME_STAMP, 
                   REASON_CODE, 
                   QUANTITY, 
                  TMP_ERP_ORDER_LINE_NUM) 
            VALUES('" + asn.INTERFACE_RECORD_ID + @"',
                             '" + asn.INTERFACE_LINK_ID + @"',
                             '" + asn.INTERFACE_ACTION_CODE + @"',
                             '" + asn.INTERFACE_CONDITION + @"',
                             '" + asn.container_id + @"',
                             '" + asn.container_type + @"',
                             '" + asn.DATE_TIME_STAMP.ToString("yyyyMMdd") + @"',
                             '" + asn.REASON_CODE + @"',
                             '" + Convert.ToInt32(asn.QUANTITY) + @"',
                             '" + Convert.ToInt32(asn.TMP_ERP_ORDER_LINE_NUM) + @"'); ";
                SqlCommand commando = new SqlCommand(query, connectiones);
                try
                {
                    connectiones.Close();
                    connectiones.Open();
                    commando.ExecuteNonQuery();

                }
                catch
                {

                }
                finally
                {
                    connectiones.Close();
                }
            }

        }


        internal string InsertarHeader(SqlCommand command, cita cita, string almacenScale, string numeroOrden,
            string tiendaOrigen, string tiendaDestino, string numeroOrdenSurtido, string inOut, int i, int citaId)
        {
            var proveedor = cita.proveedore;

            var id = string.Format("{0}{1}{2}", "ASN_", DateTime.Now.ToString("yyyyMMddHHmmss"), i);
            var sourcename = string.Format("{0} {1} {2} {3}",
                                                proveedor.Nombre1,
                                                proveedor.Nombre2,
                                                proveedor.Nombre3,
                                                proveedor.Nombre4);
            /*/
             * 
             * Agregar horario de cita mediante escaneo de los horarios de los rieles
             * */

            var db = new Entities();
            var proveedores = cita.proveedore;
            var pvr = Convert.ToInt32(proveedores.NumeroProveedor);
            var oo = db.EnviarDatos.Where(pp => pp.container_type != null && pp.Id_Proveedor == pvr && pp.CitaId==citaId).ToList();
            var horarios = cita.horariorieles;
            /*
            DataSet dt = Db.GetDataSet(sql:@"SELECT 
                            TIME_FORMAT(TIME(STR_TO_DATE(h.HoraDesde, '%h:%i %p')), '%H') 'HORA',
                            TIME_FORMAT(TIME(STR_TO_DATE(h.HoraDesde, '%h:%i %p')), '%i') 'MINUTOS',
                            TIME_FORMAT(TIME(STR_TO_DATE(h.HoraDesde, '%h:%i %p')), '%p') 'PM/AM'
                            FROM citas c
                            JOIN horariorieles hr ON hr.CitaId = c.Id
                            JOIN horarios h ON h.Id = hr.HorarioId
                            WHERE c.Id IN(" + citaId + @"); 
                        SELECT 
id,Carga,Caja,Factura,Pedido,Tienda,Ean,pares,Material,Id_Proveedor,Edo_Rev,CitaId,Estilo,Color,Cuenta
                            FROM gnzn_tmp_asn_revision_datos WHERE Id_Proveedor = " + pvr + @"
                            AND Edo_Rev = 0 OR CitaId = "+ citaId + @" AND Pedido = '"+ numeroOrden + @"'
                            GROUP BY Pedido ORDER BY id  limit 1; ");
            foreach(DataRow dr in dt.Tables[0].Rows)
            {
                cita.FechaCita = cita.FechaCita.AddHours(Int32.Parse(dr["HORA"].ToString()));
                cita.FechaCita = cita.FechaCita.AddMinutes(Int32.Parse(dr["MINUTOS"].ToString()));
            }
            */
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
            string ordencomprastring = "";
            if (cita.Almacen.ToUpper() == "CROSS DOCK")
            {
                user_def4 = "'" + tiendaDestino + "'";
                user_def6 = "'" + numeroOrdenSurtido + "'";
                ordencomprastring = "Cross Dock";
            }
            else
            {
                user_def4 = "NULL";
                user_def6 = "NULL";
                ordencomprastring = "Orden de Compra";

            }

            int insd = inOut == "1" ? 1 : 0;
            //string ordencomprastring = cita.Almacen == "Cross Dock" ? "Cross Dock" : "Orden de Compra";
            var spc1 = db.RevisarDatos.Where(oooo => oooo.Id_Proveedor == pvr && oooo.Edo_Rev == 0||oooo.CitaId==citaId).GroupBy(x => x.Pedido).Select(x => x.FirstOrDefault());

            #region Mio insert scale
            //foreach (DataRow dr2 in dt.Tables[1].Rows)
            //{
            //    #region download_receipt_header_MINE
            //    var sql2 = string.Format(@"

            //    INSERT INTO download_receipt_header 
            //                (interface_record_id, 
            //                 interface_action_code, 
            //                 interface_condition, 
            //                 warehouse, 
            //                 erp_order_num, 
            //                 receipt_id, 
            //                 receipt_id_type, 
            //                 receipt_type, 
            //                 receipt_date, 
            //                 ship_from, 
            //                 ship_from_address1, 
            //                 ship_from_address2, 
            //                 ship_from_city, 
            //                 ship_from_state, 
            //                 ship_from_country, 
            //                 ship_from_postal_code, 
            //                 ship_from_name, 
            //                 ship_from_email_address, 
            //                 ship_from_phone_num, 
            //                 ship_from_fax_num, 
            //                 source_id, 
            //                 source_name, 
            //                 source_address1, 
            //                 source_address2, 
            //                 source_city, 
            //                 source_state, 
            //                 source_postal_code, 
            //                 source_country, 
            //                 source_phone_num, 
            //                 source_fax_num, 
            //                 source_email_address, 
            //                 user_def1, 
            //                 user_def2, 
            //                 user_def3, 
            //                 user_def4, 
            //                 USER_DEF5,
            //                 user_def6, 
            //                 user_def7, 
            //                 user_def8,
            //                 user_stamp,
            //                 date_time_stamp, 
            //                 arrived_date_time) 
            //    VALUES      ('" + id + @"', 
            //                 'Save', 
            //                 'Ready', 
            //                 '" + almacenScale + @"', 
            //                 '" + numeroOrden + @"',  
            //                 dbo.GNZN_Fn_Folio_Recibo('" + numeroOrden + @"', '" + cita.Id + @"'),
            //                 '" + ordencomprastring + @"',  
            //                 '" + ordencomprastring + @"',
            //                 '" + cita.FechaCita.ToString("yyyy-MM-ddTHH:mm:ss") + @"',  
            //                 '" + proveedor.NumeroProveedor + @"',  
            //                 '" + proveedor.Calle + @"',  
            //                 '" + proveedor.Direccion + @"',  
            //                 '" + proveedor.Poblacion + @"',  
            //                 '" + proveedor.EstadoNombre + @"',  
            //                 'MEXICO', 
            //                 '" + proveedor.CodigoPostal + @"', 
            //                 '" + sourcename.Substring(0, Math.Min(sourcename.Length, 50)) + @"',  
            //                 '" + proveedor.Correo + @"',  
            //                 '" + proveedor.NumeroTelefono + @"',  
            //                 '', 
            //                 '" + proveedor.NumeroProveedor + @"',  
            //                 '" + sourcename.Substring(0, Math.Min(sourcename.Length, 50)) + @"',
            //                 '" + proveedor.Calle + @"',  
            //                 '" + proveedor.Direccion + @"',  
            //                 '" + proveedor.Poblacion + @"',  
            //                 '" + proveedor.EstadoNombre + @"',
            //                 '" + proveedor.CodigoPostal + @"',   
            //                 'MEXICO', 
            //                 '" + proveedor.NumeroTelefono + @"',  
            //                 '', 
            //                 '" + proveedor.Correo + @"',
            //                 '" + cita.FechaCita.ToString("yyyyMMdd") + @"',  
            //                 '" + proveedor.OrganizacionCompra + @"',  
            //                 '" + tiendaOrigen + @"',  
            //                 " + user_def4 + @",         
            //                 '" + dr2["Factura"].ToString() + @"',        
            //                 " + user_def6 + @",  
            //                 " + insd + @",  
            //                 '" + cita.Id + @"', 
            //                 '" + cita.CantidadTotal + @"', 
            //                 GETDATE(), 
            //                 '" + cita.FechaCita.ToString("yyyy-MM-ddTHH:mm:ss") + @"');");
            //    #endregion
            //    command.CommandText = sql2;
            //}
            #endregion

            #region comentado por le query que trae las dos tablas
            db.Database.CommandTimeout = 0;
            foreach (var fac in spc1)
            {
                if (fac.Pedido == numeroOrden)
                {

                    #region download_receipt_header
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
                             USER_DEF5,
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
                             '" + fac.Factura + @"',        
                             " + user_def6 + @",  
                             " + insd + @",  
                             '" + cita.Id + @"', 
                             '" + cita.CantidadTotal + @"', 
                             GETDATE(), 
                             '" + cita.FechaCita.ToString("yyyy-MM-ddTHH:mm:ss") + @"');");
                    #endregion
                    command.CommandText = sql;

                }
            }
            #endregion


            //var parameters = new List<MySqlParameter>()
            //    {
            //        new MySqlParameter("IdCita", citaId),
            //        new MySqlParameter("ProveedorId", pvr),
            //        new MySqlParameter("numeroOrden", numeroOrden),
            //        new MySqlParameter
            //        {
            //            ParameterName = "idEtiqueta",
            //            Direction = ParameterDirection.Output,
            //            MySqlDbType = MySqlDbType.Int64
            //        }
            //    };

            ////DataSet idEtiqueta=Db.GetDataSet(parameters, "sp_InsertEtiquetasfromAsnRevDat");
            //Dictionary<string, string> idEtiqueta = Db.ExecuteProcedureOutput(parameters,"sp_InsertEtiquetasfromAsnRevDat");


            var borrarEti = db.EnviarEtiquetas.Where(m => m.cita == citaId).FirstOrDefault();

            if (borrarEti == null)
            {
                var db11 = new Entities();
                var proveedores1 = cita.proveedore;
                var pvres = Convert.ToInt32(proveedor.NumeroProveedor);
                var spc = db.RevisarDatos.Where(oooo => oooo.Id_Proveedor == pvres && oooo.Edo_Rev == 0).GroupBy(x => x.Caja).Select(x => x.FirstOrDefault());
                db.Database.CommandTimeout = 0;
                foreach (var eti in spc)
                {
                    var archivo = new EnviarEtiquetas();
                    archivo.cita = citaId;
                    archivo.recibo = eti.Pedido;
                    archivo.carga = eti.Carga;
                    archivo.factura = eti.Factura;
                    archivo.tienda = eti.Tienda;
                    archivo.caja = eti.Caja;
                    archivo.pares = eti.Pares;
                    archivo.Id_Proveedor = eti.Id_Proveedor;
                    archivo.estilo = Convert.ToString(eti.Estilo);
                    archivo.color = eti.Color;
                    db.EnviarEtiquetas.Add(archivo);

                }

                db.SaveChanges();


            }
            else
            {

                if (borrarEti.cita != citaId)
                {
                    var spc = db.RevisarDatos.Where(oooo => oooo.Edo_Rev == 0 || oooo.CitaId == citaId).GroupBy(x => x.Caja).Select(x => x.FirstOrDefault());
                    foreach (var eti in spc)
                    {
                        var archivo = new EnviarEtiquetas();
                        archivo.cita = citaId;
                        archivo.recibo = eti.Pedido;
                        archivo.carga = eti.Carga;
                        archivo.factura = eti.Factura;
                        archivo.tienda = eti.Tienda;
                        archivo.caja = eti.Caja;
                        archivo.pares = eti.Pares;
                        archivo.Id_Proveedor = eti.Id_Proveedor;
                        archivo.estilo = Convert.ToString(eti.Estilo);
                        archivo.color = eti.Color;
                        db.EnviarEtiquetas.Add(archivo);

                    }
                    db.SaveChanges();

                }
            }


            ////if (idEtiqueta.Tables[0].Rows.Count > 0)
            //if (idEtiqueta.Values.Count > 0)
            //{
            //    var r = DbScale.GetDataTable("select  dbo.GNZN_Fn_Folio_Recibo(" + numeroOrden + ", " + citaId + ")");
            //    if (r.Rows.Count == 1)
            //    {
            //        var row = r.Rows[0];
            //        var re = row.ItemArray;
            //        var resiosos = row[0];
            //        Db.Update("UPDATE gnzn_asn_etiquetas SET recibo='"+ Convert.ToString(re[0]) + "' WHERE id_eti="+ idEtiqueta["idEtiqueta"] + "; ");
            //    }
            //}
            var modif = db.EnviarEtiquetas.Where(oooo => oooo.cita == citaId).GroupBy(x => x.caja).Select(x => x.FirstOrDefault());
            db.Database.CommandTimeout = 0;
            foreach (var eti in modif)
            {

                if (eti.recibo == numeroOrden)
                {
                    var r = DbScale.GetDataTable("select  dbo.GNZN_Fn_Folio_Recibo(" + numeroOrden + ", " + citaId + ")");
                    if (r.Rows.Count == 1)
                    {
                        var row = r.Rows[0];
                        var re = row.ItemArray;
                        var resiosos = row[0];

                        eti.recibo = Convert.ToString(re[0]);
                        db.Entry(eti).State = EntityState.Modified;
                    }
                }
            }
            db.SaveChanges();

            foreach (var lin in oo)
            {
                if (Convert.ToInt64(lin.Pedido) == Convert.ToInt64(numeroOrden))
                {
                    lin.INTERFACE_LINK_ID = id;
                    db.Entry(lin).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }

            //int regAfectados = Db.Update("update gnzn_datos_asn_contendores set  INTERFACE_LINK_ID =concat('ASN_', DATE_FORMAT(NOW(), '%Y%m%d%H%i%s') ,'1') where  (container_type IS NOT NULL OR container_type <>'') AND Id_Proveedor=" + pvr + " AND CitaId=" + citaId + " AND Pedido=" + numeroOrden + ";");

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
            try
            {
                //begin tRAN
                //conexionSQL.Oope()
                command.ExecuteNonQuery();
                //conexion.Close();
                
            }
            catch(Exception ed)
            {
                string MsgExecInsertDOWNLOAD_RECEIPT_DETAIL = ed.Message;
            }
            finally
            {
                ////commit
                /////rollback
            }

        }

        public void Cancelar(int citaId)
        {

            if (citaId < 1)
            {
                ErrorAppLog.Error(string.Format("Cita # {0} incorrecta", citaId));
                return;
            }

            const string sql = @"
                 UPDATE DOWNLOAD_RECEIPT_HEADER
              SET INTERFACE_CONDITION =	'Ready',
	          INTERFACE_ACTION_CODE = 'Delete'
              where USER_DEF8=@citaId and INTERFACE_ACTION_CODE <>	'Delete'";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@citaId", citaId)
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
                new SqlParameter("@ARRIVED_DATE_TIME", cita.FechaCita),
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

        private void escribearchivo(string lines)
        {
            string namearchivo = "LOG_MAU_TEMP.txt";
            string RutaSitio = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); //System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/");
            string PathGeneric = Path.Combine(RutaSitio + "\\" + namearchivo);
            StreamWriter ws = new StreamWriter(PathGeneric, true);
            ws.WriteLine(lines);
            ws.Flush();
            ws.Close();
            //System.IO.File.AppendAllLines(Path.Combine(PathGeneric), lines);
        }

        
    }
}