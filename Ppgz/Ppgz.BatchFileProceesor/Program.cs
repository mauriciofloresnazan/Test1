using System;
using System.IO;
using System.Threading;

namespace Ppgz.BatchFileProceesor
{
    class Program
    {
        static void Main(string[] args)
        {
            var crWatcher = new FileSystemWatcher
            {
                // TODO MOVER AL ARCHIVO DE CONFIGURACION DEL LA 
                // APLCIACION WEB Y PASARLO COMO ARGUMENTO
                Path = @"C:\temp\Implus\FtpDev\Inbox",
                Filter = "cr_*.pdf",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
                | NotifyFilters.Size | NotifyFilters.Security
            };

            crWatcher.Changed += crWatcher_Changed;
            crWatcher.EnableRaisingEvents = true;
        }

        static void crWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Archivo: {0}, Actividad: {1}", e.Name, e.ChangeType);
            MoverCr(e.FullPath);

        }
        static void MoverCr(string path)
        {
            try
            {
                if (!File.Exists(path)) return;

                var fileName = Path.GetFileName(path);

                if (fileName == null) return;

                var newPath = Path.Combine
                    (@"C:\temp\Implus\FtpDev\Crs", fileName);

                if (File.Exists(newPath))
                {
                    File.Delete(newPath);
                    Thread.Sleep(100);
                }
                File.Move(path, newPath);

                Console.WriteLine("{0} fue movido a {1}.", path, newPath);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error en el proceso: {0}", e);
            }
        }
    }
}
