using System.Data;
using Ppgz.Repository;

namespace Ppgz.Repository
{
    public static class ConsultasDB
    {
        public static DataSet GetFacturasxFecha(string FechaIni,string FechaFin, string Proveedor,string Status)
        {
            string query = @"SELECT 
                            p.Rfc as rfc
                            ,concat(p.Nombre1,' ' ,p.Nombre2,' '  ,p.Nombre3,' '  ,p.Nombre4 ) AS Nombre 
                            ,f.Serie,f.Folio,f.EstatusOriginal,f.NumeroGenerado
                            ,f.Uuid,f.Fecha ,f.FechaPortal ,f.Total
                            ,f.Estatus,f.Comentario, f.RFCReceptor,
                            f.Procesado,f.XmlRuta,f.PdfRuta
                            FROM facturas f
                            JOIN proveedores p ON p.Id = f.proveedor_id
                            WHERE Fecha >='" + FechaIni + "' AND Fecha < '" + FechaFin + "'";
                            
            if(!Proveedor.Equals("T"))
                query = query + " AND f.proveedor_id = " + Proveedor;
            
            if(!Status.Equals("T"))
                query = query + " AND f.Estatus = '" + Status + "'";

            query = query + ";";


            return Db.GetDataSet(query);
            
        }

        public static DataSet GetProveedores()
        {
            string query = @"SELECT p.Id,p.NumeroProveedor
	                            ,concat(p.Nombre1,' ' ,p.Nombre2,' ', p.Nombre3,' ', p.Nombre4 ) AS Nombre
	                            ,p.Rfc
                            FROM proveedores p
                            ORDER BY p.Nombre1";
            return Db.GetDataSet(query);
        }
    }
}
