using ClosedXML.Excel;
using System;
using System.Data;
using System.IO;
using System.Web;

namespace Ppgz.Web.Infrastructure
{
	/// <summary> Clase para manejar distintos tipos de archivos.</summary>
	public static class FileManager
	{
		/// <summary>Exporta a un archivo excel, partiendo de un DataTable.</summary>
		/// <param name="dt">DataTable con la información a exportar.</param>
		/// <param name="nombreXls">Nombre del archivo a generar.</param>
		/// <param name="httpContext">Contexto actual del controlador.</param>
		public static void ExportExcel(DataTable dt, string nombreXls, HttpContextBase httpContext)
		{
			GC.GetTotalMemory(true);
			MemoryStream fileStream = new MemoryStream();
			XLWorkbook workBook = new XLWorkbook();
			workBook.Worksheets.Add(dt, "Resultados");
			workBook.SaveAs(fileStream, false);
			fileStream.Position = 0;
			string fileName = httpContext.Server.UrlEncode(nombreXls + ".xlsx");
			httpContext.Response.Clear();
			httpContext.Response.Buffer = true;
			httpContext.Response.AddHeader("content-disposition", "attachment; filename=" + fileName);
			httpContext.Response.ContentType = "application/vnd.ms-excel";
			httpContext.Response.BinaryWrite(fileStream.ToArray());
			httpContext.Response.End();
			GC.GetTotalMemory(true);
		}

        public static void ExportExcel(XLWorkbook workbook, string nombreXls, HttpContextBase httpContext)
        {

            GC.GetTotalMemory(true);
            var fileStream = new MemoryStream();
            workbook.SaveAs(fileStream, false);
            fileStream.Position = 0;

            var fileName = httpContext.Server.UrlEncode(nombreXls + ".xlsx");
            httpContext.Response.Clear();
            httpContext.Response.Buffer = true;
            httpContext.Response.AddHeader("content-disposition", "attachment; filename=" + fileName);
            httpContext.Response.ContentType = "application/vnd.ms-excel";
            httpContext.Response.BinaryWrite(fileStream.ToArray());
            httpContext.Response.End();
            GC.GetTotalMemory(true);
        }

    }
}