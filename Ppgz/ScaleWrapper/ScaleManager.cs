using System;
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
        public static readonly ILog ErrorAppLog = LogManager.GetLogger(@"ErrorAppLog");

        public void Registrar(cita cita)
        {
            var entities = new Entities();
            var almacenScale = entities.ScaleAlmacens.FirstOrDefault(sa => sa.Sap == cita.Almacen);

            if (almacenScale == null)
            {

                ErrorAppLog.Error(string.Format("El Almacén {0} no esta configurado para Scale. Error en la Cita # {1}", cita.Almacen, cita.Id));
                return;
            }

            var numerosDocumentos = cita.asns.Select(asn=> asn.OrdenNumeroDocumento).Distinct();

            foreach (var numeroDocumento in numerosDocumentos)
            {
                var documento = numeroDocumento;

                var orden = cita.asns
                    .FirstOrDefault(asn => asn.OrdenNumeroDocumento == documento);
                

               var id = InsertarHeader(cita, almacenScale.Scale, numeroDocumento, orden.TiendaOrigen,orden.Tienda, orden.NumeroOrdenSurtido, orden.InOut);

                var asns = cita.asns.Where(asn => asn.OrdenNumeroDocumento == numeroDocumento).ToList();

                /*foreach (var asn in asns)
                {
                    InsertarDetail(id, almacenScale.Scale, asn);
                }*/
                InsertarDetails(id,almacenScale.Scale, asns);
            }
        }

        internal string InsertarHeader(cita cita, string almacenScale, string numeroOrden, 
            string tiendaOrigen, string tiendaDestino, string numeroOrdenSurtido, string inOut)
        {

            var proveedor = cita.proveedore;

            var id = string.Format("{0}{1}", DateTime.Now.ToString("yyyyMMddHHmmss"), cita.Id);
            
            var parameters = new List<SqlParameter>
            {
                new SqlParameter(
                   "@INTERFACE_RECORD_ID",id),
                    new SqlParameter("@INTERFACE_ACTION_CODE", "Save"),
                    new SqlParameter("@INTERFACE_CONDITION", "Ready"),
                    new SqlParameter("@warehouse", almacenScale),
                    new SqlParameter("@ERP_ORDER_NUM", numeroOrden),
                    //new SqlParameter("@RECEIPT_ID", string.Format("{0}-{1}", numeroOrden, cita.Id)),

                    new SqlParameter("@RECEIPT_ID_TYPE", cita.Almacen == "Cross Dock" ? "Cross Dock" : "Orden de Compra"),
                    new SqlParameter("@RECEIPT_TYPE", cita.Almacen == "Cross Dock" ? "Cross Dock" : "Orden de Compra"),

                    new SqlParameter("@RECEIPT_DATE", cita.FechaCita),
                    new SqlParameter("@Ship_From", proveedor.NumeroProveedor),
                    new SqlParameter("@SHIP_FROM_ADDRESS1", proveedor.Calle),
                    new SqlParameter("@SHIP_FROM_ADDRESS2", proveedor.Direccion),
                    new SqlParameter("@Ship_From_city", proveedor.Poblacion),
                    new SqlParameter("@SHIP_FROM_STATE", proveedor.EstadoNombre),
                    new SqlParameter("@SHIP_FROM_COUNTRY", "MEXICO"),
                    new SqlParameter("@SHIP_FROM_POSTAL_CODE", proveedor.CodigoPostal),
                    new SqlParameter("@SHIP_FROM_NAME",
                        string.Format(
                            "{0} {1} {2} {3}", 
                            proveedor.Nombre1, 
                            proveedor.Nombre2, 
                            proveedor.Nombre3, 
                            proveedor.Nombre4)),
                    new SqlParameter("@SHIP_FROM_EMAIL_ADDRESS", proveedor.Correo),
                    new SqlParameter("@SHIP_FROM_PHONE_NUM", proveedor.NumeroTelefono),
                    new SqlParameter("@SHIP_FROM_FAX_NUM", ""),
                    new SqlParameter("@Source_id",proveedor.NumeroProveedor),
                    new SqlParameter("@Source_name",
                                            string.Format(
                                                "{0} {1} {2} {3}", 
                                                proveedor.Nombre1, 
                                                proveedor.Nombre2, 
                                                proveedor.Nombre3, 
                                                proveedor.Nombre4)),
                    new SqlParameter("@SOURCE_ADDRESS1", proveedor.Calle),
                    new SqlParameter("@SOURCE_ADDRESS2", proveedor.Direccion),
                    new SqlParameter("@Source_City", proveedor.Poblacion),
                    new SqlParameter("@Source_State", proveedor.EstadoNombre),
                    new SqlParameter("@SOURCE_POSTAL_CODE", proveedor.CodigoPostal),
                    new SqlParameter("@SOURCE_COUNTRY","MEXICO"),
                    new SqlParameter("@SOURCE_PHONE_NUM", proveedor.NumeroTelefono),
                    new SqlParameter("@SOURCE_FAX_NUM", ""),
                    new SqlParameter("@SOURCE_EMAIL_ADDRESS", proveedor.Correo),
                    new SqlParameter("@user_def1", cita.FechaCita.ToString("yyyyMMdd")),
                    new SqlParameter("@user_def2", proveedor.OrganizacionCompra),
                    new SqlParameter("@user_def3", tiendaOrigen),
                    //new SqlParameter("@user_def5","foo"),
                    new SqlParameter("@user_def7", inOut == "1" ? 1 : 0),
                    new SqlParameter("@user_def8", cita.Id),
                    //new SqlParameter("@DATE_TIME_STAMP","GETDATE()"),
                    new SqlParameter("@ARRIVED_DATE_TIME",cita.FechaCita),


            };

            if (cita.Almacen.ToUpper() == "CROSS DOCK")
            {
                parameters.Add(new SqlParameter("@user_def4", tiendaDestino));
                parameters.Add(new SqlParameter("@user_def6", numeroOrdenSurtido));

            }
            else
            {
                parameters.Add(new SqlParameter("@user_def4", DBNull.Value));
                parameters.Add(new SqlParameter("@user_def6", DBNull.Value));
                
            }

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
                             --user_def5, 
                             user_def6, 
                             user_def7, 
                             user_def8, 
                             date_time_stamp, 
                             arrived_date_time) 
                VALUES      (@INTERFACE_RECORD_ID, 
                             @INTERFACE_ACTION_CODE, 
                             @INTERFACE_CONDITION, 
                             @warehouse, 
                             @ERP_ORDER_NUM, 
                             --@RECEIPT_ID, 
                             dbo.GNZN_Fn_Folio_Recibo(@ERP_ORDER_NUM),
                             @RECEIPT_ID_TYPE, 
                             @RECEIPT_TYPE, 
                             @RECEIPT_DATE, 
                             @Ship_From, 
                             @SHIP_FROM_ADDRESS1, 
                             @SHIP_FROM_ADDRESS2, 
                             @Ship_From_city, 
                             @SHIP_FROM_STATE, 
                             @SHIP_FROM_COUNTRY, 
                             @SHIP_FROM_POSTAL_CODE, 
                             @SHIP_FROM_NAME, 
                             @SHIP_FROM_EMAIL_ADDRESS, 
                             @SHIP_FROM_PHONE_NUM, 
                             @SHIP_FROM_FAX_NUM, 
                             @Source_id, 
                             @Source_name, 
                             @SOURCE_ADDRESS1, 
                             @SOURCE_ADDRESS2, 
                             @Source_City, 
                             @Source_State, 
                             @SOURCE_POSTAL_CODE, 
                             @SOURCE_COUNTRY, 
                             @SOURCE_PHONE_NUM, 
                             @SOURCE_FAX_NUM, 
                             @SOURCE_EMAIL_ADDRESS, 
                             @user_def1, 
                             @user_def2, 
                             @user_def3, 
                             @user_def4, 
                             --@user_def5, 
                             @user_def6, 
                             @user_def7, 
                             @user_def8, 
                             GETDATE(), 
                             @ARRIVED_DATE_TIME);

                                ");

            DbScale.Insert(sql, parameters);

            return id;
        }
        internal void InsertarDetail(string interfaceLinkId, string almacenScale, asn asn)
        {
            var id = string.Format("{0}{1}", DateTime.Now.ToString("yyyyMMddHHmmssfff"), asn.cita.Id);

            var parameters = new List<SqlParameter>
            {
                new SqlParameter(
                   "@INTERFACE_RECORD_ID",id),
                    new SqlParameter("@Interface_link_id", interfaceLinkId),
                    new SqlParameter("@warehouse", almacenScale),
                    new SqlParameter("@INTERFACE_ACTION_CODE", "Save"),
                    new SqlParameter("@INTERFACE_CONDITION", "Ready"),
                    new SqlParameter("@ERP_ORDER_LINE_NUM", asn.NumeroPosicion),
                    new SqlParameter("@item", asn.NumeroMaterial),
                    new SqlParameter("@ITEM_NET_PRICE", asn.Precio),
                    new SqlParameter("@user_def5",asn.cita.FechaCita.ToString("yyyyMMdd")),
                    new SqlParameter("@TOTAL_QTY", asn.CantidadPedidoSap),
                    new SqlParameter("@QUANTITY_UM", asn.UnidadMedida),
              

            };

            var sql = string.Format(@"

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
                VALUES      
                   (@INTERFACE_RECORD_ID,
                    @Interface_link_id,
                    @warehouse,
                    @INTERFACE_ACTION_CODE,
                    @INTERFACE_CONDITION,
                    @ERP_ORDER_LINE_NUM,
                    @item,
                    @ITEM_NET_PRICE,
                    @user_def5,
                    @TOTAL_QTY,
                    @QUANTITY_UM,
                    GETDATE());");

            DbScale.Insert(sql, parameters);
        }


        internal void InsertarDetails(string interfaceLinkId, string almacenScale, List<asn> asns)
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
                // Si es multiplo de 100 
                if (index % 100 == 0)
                {

                    // si tiene valores lo inserta
                    if(parameters.Any())
                    {                        
                        DbScale.Insert(sql.ToString(), parameters);
                    }

                    sql = new StringBuilder();

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
                    parameters = new List<SqlParameter>();
                }
                else
                {
                    sql.Append(",");
                }

                var asn = asns[index];
                var id = string.Format("{0}{1}{2}", DateTime.Now.ToString("yyyyMMddHHmmssfff"), asn.cita.Id, index);

                parameters.Add(new SqlParameter("@INTERFACE_RECORD_ID" + index, id));
                parameters.Add(new SqlParameter("@Interface_link_id" + index, interfaceLinkId));
                parameters.Add(new SqlParameter("@warehouse" + index, almacenScale));
                parameters.Add(new SqlParameter("@INTERFACE_ACTION_CODE" + index, "Save"));
                parameters.Add(new SqlParameter("@INTERFACE_CONDITION" + index, "Ready"));
                parameters.Add(new SqlParameter("@ERP_ORDER_LINE_NUM" + index, asn.NumeroPosicion));
                parameters.Add(new SqlParameter("@item" + index, asn.NumeroMaterial));
                parameters.Add(new SqlParameter("@ITEM_NET_PRICE" + index, asn.Precio));
                parameters.Add(new SqlParameter("@user_def5" + index, asn.cita.FechaCita.ToString("yyyyMMdd")));
                parameters.Add(new SqlParameter("@TOTAL_QTY" + index, asn.CantidadPedidoSap));
                parameters.Add(new SqlParameter("@QUANTITY_UM" + index, asn.UnidadMedida));

                sql.AppendLine(@"(@INTERFACE_RECORD_ID"+ index + @",
                    @Interface_link_id" + index + @",
                    @warehouse" + index + @",
                    @INTERFACE_ACTION_CODE" + index + @",
                    @INTERFACE_CONDITION" + index + @",
                    @ERP_ORDER_LINE_NUM" + index + @",
                    @item" + index + @",
                    @ITEM_NET_PRICE" + index + @",
                    @user_def5" + index + @",
                    @TOTAL_QTY" + index + @",
                    @QUANTITY_UM" + index + @",
                    GETDATE())");
                /*if (index + 1 < asns.Count)
                {
                    sql.Append(",");
                }*/
            }

            /*var parameters = new List<SqlParameter>
            {
                new SqlParameter(
                   "@INTERFACE_RECORD_ID",id),
                    new SqlParameter("@Interface_link_id", interfaceLinkId),
                    new SqlParameter("@warehouse", almacenScale),
                    new SqlParameter("@INTERFACE_ACTION_CODE", "Save"),
                    new SqlParameter("@INTERFACE_CONDITION", "Ready"),
                    new SqlParameter("@ERP_ORDER_LINE_NUM", asn.NumeroPosicion),
                    new SqlParameter("@item", asn.NumeroMaterial),
                    new SqlParameter("@ITEM_NET_PRICE", asn.Precio),
                    new SqlParameter("@user_def5",asn.cita.FechaCita.ToString("yyyyMMdd")),
                    new SqlParameter("@TOTAL_QTY", asn.CantidadPedidoSap),
                    new SqlParameter("@QUANTITY_UM", asn.UnidadMedida),
              

            };

            var sql = string.Format(@"

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
                VALUES      
                   (@INTERFACE_RECORD_ID,
                    @Interface_link_id,
                    @warehouse,
                    @INTERFACE_ACTION_CODE,
                    @INTERFACE_CONDITION,
                    @ERP_ORDER_LINE_NUM,
                    @item,
                    @ITEM_NET_PRICE,
                    @user_def5,
                    @TOTAL_QTY,
                    @QUANTITY_UM,
                    GETDATE());");*/

            DbScale.Insert(sql.ToString(), parameters);
        }
        
    }
}
