using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Ppgz.Repository;

namespace Ppgz.Web.Infrastructure.Nazan
{
    public class MensajesInstitucionalesManager
    {
        private readonly Entities _db = new Entities();


        public enum EnviadoA
        {
            TODOS,
            MERCADERIA,
            SERVICIO
        }
        internal static string GetEnviadoAString(EnviadoA enviadoA)
        {
            switch (enviadoA)
            {
                case EnviadoA.MERCADERIA:
                    return "MERCADERIA";
                case EnviadoA.SERVICIO:
                    return "SERVICIO";
                case EnviadoA.TODOS:
                    return "TODOS";
            }
            return null;
        }
        public static EnviadoA GetEnviadoAByString(string enviadoA)
        {
            switch (enviadoA)
            {
                case "MERCADERIA":
                    return EnviadoA.MERCADERIA;
                case "SERVICIO":
                    return EnviadoA.SERVICIO;
                case "TODOS":
                    return EnviadoA.TODOS;
            }
            //TODO CARLOS Y JUAN DELGADO
            throw new Exception("");

        }
        public List<mensaje> FindAll()
        {
            return _db.mensajes.ToList();
        }

        public mensaje Find(int id)
        {
            return _db.mensajes
                .FirstOrDefault(m => m.Id == id);
        }

        public void CreateTexto(string titulo, string contenido,
            DateTime fechaPublicacion, DateTime fechaCaducidad, EnviadoA enviadoA)
        {
            var mensaje = new mensaje()
            {
                Titulo = titulo.Trim(),
                Contenido = contenido.Trim(),
                FechaCaducidad = fechaCaducidad,
                FechaPublicacion = fechaPublicacion,
                EnviadoA = GetEnviadoAString(enviadoA)
            };


            _db.mensajes.Add(mensaje);
            _db.SaveChanges();

        }
        public void CreateArchivo(string titulo, string archivo,
            DateTime fechaPublicacion, DateTime fechaCaducidad, EnviadoA enviadoA)
        {
            var mensaje = new mensaje()
            {
                Titulo = titulo.Trim(),
                Archivo = archivo.Trim(),
                FechaCaducidad = fechaCaducidad,
                FechaPublicacion = fechaPublicacion,
                EnviadoA = GetEnviadoAString(enviadoA)
            };

            _db.mensajes.Add(mensaje);
            _db.SaveChanges();
        }

        public void UpdateTexto(int id, string titulo, string contenido,
            DateTime fechaPublicacion, DateTime fechaCaducidad, EnviadoA enviadoA)
        {
            if(string.IsNullOrWhiteSpace(contenido.Trim()))
            {
                // TODO CARLOS Y  JUAN DELGADO
                throw new Exception("");
            }

            var mensaje = Find(id);
            if (mensaje == null)
            {
                // TODO CARLOS Y  JUAN DELGADO
                throw new Exception("");
            }

            mensaje.Titulo = titulo.Trim();
            mensaje.FechaPublicacion = fechaPublicacion;
            mensaje.FechaCaducidad = fechaCaducidad;
            mensaje.EnviadoA = GetEnviadoAString(enviadoA);
            mensaje.Contenido = contenido;
            _db.Entry(mensaje).State = EntityState.Modified;
            _db.SaveChanges();
        }

        public void UpdateArchivo(int id, string titulo, string archivo,
            DateTime fechaPublicacion, DateTime fechaCaducidad, EnviadoA enviadoA)
        {
            if (string.IsNullOrWhiteSpace(archivo.Trim()))
            {
                // TODO CARLOS Y  JUAN DELGADO
                throw new Exception("");
            }

            var mensaje = Find(id);
            if (mensaje == null)
            {
                // TODO CARLOS Y  JUAN DELGADO
                throw new Exception("");
            }

            mensaje.Titulo = titulo.Trim();
            mensaje.FechaPublicacion = fechaPublicacion;
            mensaje.FechaCaducidad = fechaCaducidad;
            mensaje.EnviadoA = GetEnviadoAString(enviadoA);
            mensaje.Archivo = archivo;
            _db.Entry(mensaje).State = EntityState.Modified;
            _db.SaveChanges();
        }
        public void Remove(int id)
        {
            var mensaje = Find(id);
            if (mensaje == null)
            {
                // TODO CARLOS Y  JUAN DELGADO
                throw new Exception("");
            }

            _db.mensajes.Remove(mensaje);
            _db.SaveChanges();
        }

    }
}