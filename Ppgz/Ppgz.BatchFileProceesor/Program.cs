using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Ppgz.Repository;

namespace Ppgz.BatchFileProceesor
{
    class Program
    {
        private const string WrongFilesPath = @"C:\temp\Implus\FtpDev\WrongFiles";
        private const string CrInboxPath = @"C:\temp\Implus\FtpDev\Inbox";
        private const string CrPath = @"C:\temp\Implus\FtpDev\Crs";
        private const string CrFilter = "cr_*.pdf";


        static void Main()
        {
            var crWatcher = new FileSystemWatcher
            {
                // TODO MOVER AL ARCHIVO DE CONFIGURACION DEL LA 
                // APLCIACION WEB Y PASARLO COMO ARGUMENTO
                Path = CrInboxPath,
                Filter = CrFilter,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
                | NotifyFilters.Size | NotifyFilters.Security
            };

            crWatcher.Changed += crWatcher_Changed;
            crWatcher.EnableRaisingEvents = true;
            Console.ReadLine();
        }

        static void crWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {

                var codigo = e.Name.Split('_')[1];
                var fecha = DateTime.ParseExact(
                    e.Name.Split('_')[2].ToLower().Replace(".pdf", string.Empty),
                    "ddMMyyyy",
                    CultureInfo.InvariantCulture);


                Console.WriteLine("Archivo: {0}, Actividad: {1}", e.Name, e.ChangeType);
                MoverCr(e.FullPath, fecha, codigo);
            }
            catch (Exception)
            {
                MoverError(e.FullPath);

            }

        }
        static void MoverError(string path)
        {
            try
            {
                if (!File.Exists(path)) return;

                var fileName = Path.GetFileName(path);

                if (fileName == null) return;

                var newPath = Path.Combine
                    (WrongFilesPath, fileName);

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
        static void MoverCr(string path, DateTime fecha, string codigo)
        {
            try
            {
                if (!File.Exists(path)) return;

                var fileName = Path.GetFileName(path);

                if (fileName == null) return;

                var crRottPath = new DirectoryInfo(CrPath);

                var yearPath = crRottPath.GetDirectories(fecha.Year.ToString()).Length == 0 ?
                    crRottPath.CreateSubdirectory(fecha.Year.ToString()) :
                    crRottPath.GetDirectories(fecha.Year.ToString())[0];

                var monthPath = yearPath.GetDirectories(fecha.Month.ToString()).Length == 0 ?
                    yearPath.CreateSubdirectory(fecha.Month.ToString()) :
                    yearPath.GetDirectories(fecha.Month.ToString())[0];


                var newPath = Path.Combine
                    (monthPath.FullName, fileName);


                if (File.Exists(newPath))
                {
                    File.Delete(newPath);
                    Thread.Sleep(100);
                }


                WaitForFile(path);

                File.Move(path, newPath);

                var cr = new cr
                {
                    Codigo = codigo,
                    Fecha = fecha,
                    ArchivoCR = newPath
                };


                var db = new Entities();
                db.crs.Add(cr);
                db.SaveChanges();

                //Console.WriteLine("{0} fue movido a {1}.", path, newPath);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error en el proceso: {0}", e);
            }
        }
        // TODO
        static void WaitForFile(string fullPath)
        {
            int numTries = 0;
            while (true)
            {
                ++numTries;
                try
                {

                    using (FileStream fs = new FileStream(fullPath,
                        FileMode.Open, FileAccess.ReadWrite,
                        FileShare.None, 100))
                    {
                        fs.ReadByte();

                        break;
                    }
                }
                catch (Exception)
                {
                    if (numTries > 10)
                    {
                        return;
                    }

                    Thread.Sleep(500);
                }
            }
        }

    }
}
