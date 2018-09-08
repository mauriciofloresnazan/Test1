using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading;
using Ppgz.Repository;
using System.Globalization;

namespace Ppgz.BatchFileProcessor
{
    class Program
    {
        private static string _wrongFilesPath;

        private static string _crInboxPath;
        private static string _crPath;

        private static readonly Entities Db = new Entities();
        
        static void Main()
        {
            _wrongFilesPath = Db.configuraciones.Single(c => c.Clave == "batchfile.wrongfilespath").Valor;

            _crInboxPath = Db.configuraciones.Single(c => c.Clave == "batchfile.crinboxpath").Valor;
            _crPath = Db.configuraciones.Single(c => c.Clave == "batchfile.crpath").Valor;

        
            var crWatcher = new FileSystemWatcher
            {
                // TODO MOVER AL ARCHIVO DE CONFIGURACION DEL LA 
                // APLCIACION WEB Y PASARLO COMO ARGUMENTO
                Path = _crInboxPath,
                Filter = "*.*"
            };

            crWatcher.Changed += crWatcher_Changed;
            crWatcher.EnableRaisingEvents = true;

            
            Console.ReadLine();
        }

        static void crWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (Path.GetExtension(e.FullPath) != ".pdf")
                {
                    throw new Exception("Archivo incorrecto");
                }

                var archivoSinExtension = Path.GetFileNameWithoutExtension(e.FullPath);

                if (archivoSinExtension == null)
                {
                    return;
                }

                var split = archivoSinExtension.Split('_');
                var datenow = DateTime.Now;
                Console.WriteLine("Archivo: {0}, Actividad: {1}", e.Name, e.ChangeType);

                if (split.Length < 3)
                {
                    var citaId = Convert.ToInt32(archivoSinExtension.Split('_')[1]);                                        
                    MoverCr(e.FullPath, datenow, citaId);
                }
                else
                {
                    var proveedor = split[1];
                    var fecha = split[2];                    

                    MoverCrForaneo(e.FullPath, proveedor, fecha);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
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
                    (_wrongFilesPath, fileName);

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
        static void MoverCr(string path, DateTime fecha, int citaId)
        {

            if (!File.Exists(path)) return;

            var fileName = Path.GetFileName(path);

            if (fileName == null) return;

            if (Db.citas.Find(citaId) == null)
            {
                throw new Exception("Cita incorrecta");
            }

            var crRottPath = new DirectoryInfo(_crPath);

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

            var cr = Db.crs.FirstOrDefault(c => c.CitaId == citaId);
            if (cr == null)
            {
                cr = new cr
                {
                    CitaId = citaId,
                    Fecha = fecha,
                    ArchivoCR = newPath
                };
                Db.Entry(cr).State = EntityState.Added;
            }
            else
            {
                cr.Fecha = fecha;
                cr.ArchivoCR = newPath;
                Db.Entry(cr).State = EntityState.Modified;
            }

            Db.SaveChanges();


        }

        static void MoverCrForaneo(string path, string proveedor, string fechapath)
        {
            string day = fechapath.Substring(0, 2);
            string month = fechapath.Substring(2, 2);
            string year = fechapath.Substring(4, 4);

            if (!File.Exists(path)) return;

            var fileName = Path.GetFileName(path);

            if (fileName == null) return;

            var crRottPath = new DirectoryInfo(_crPath);

            var yearPath = crRottPath.GetDirectories(year).Length == 0 ?
                crRottPath.CreateSubdirectory(year) :
                crRottPath.GetDirectories(year)[0];

            var monthPath = yearPath.GetDirectories(month).Length == 0 ?
                yearPath.CreateSubdirectory(month) :
                yearPath.GetDirectories(month)[0];


            var newPath = Path.Combine
                (monthPath.FullName, fileName);

            if (File.Exists(newPath))
            {
                File.Delete(newPath);
                Thread.Sleep(100);
            }

            WaitForFile(path);
            File.Move(path, newPath);

            string sfecha = day + "/" + month + "/" + year;
            DateTime fecha = DateTime.ParseExact(sfecha, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            
            var crf = new crforaneo{                
                Fecha = fecha,
                ArchivoCR = newPath,
                Proveedor = proveedor
            };

            Db.Entry(crf).State = EntityState.Added;
            Db.SaveChanges();
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
