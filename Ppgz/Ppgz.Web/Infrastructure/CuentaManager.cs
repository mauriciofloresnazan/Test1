using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Ppgz.Repository;
using Ppgz.Web.Areas.Nazan;
using Ppgz.Web.Infrastructure.Proveedor;
using Ppgz.Web.Models;

namespace Ppgz.Web.Infrastructure
{
    public class CuentaManager
    {
        private readonly Entities _db = new Entities();

        public enum Tipo
        {
            Mercaderia,
            Servicio
        }

        internal static string GetTipoString(Tipo tipo)
        {
            switch (tipo)
            {
                case Tipo.Mercaderia:
                    return "MERCADERIA";
                case Tipo.Servicio:
                    return "SERVICIO";
            }
            return null;
        }

        public static Tipo GetTipoByString(string tipo)
        {
            switch (tipo)
            {
                case "MERCADERIA":
                    return Tipo.Mercaderia;
                case "SERVICIO":
                    return Tipo.Servicio;
            }
            throw new BusinessException(MensajesResource.ERROR_CuentaManager_GetTipoByString);
        }

        public cuenta FindByNombre(string nombreProveedor)
        {
            return _db.cuentas.FirstOrDefault(c => c.NombreProveedor == nombreProveedor);
        }

        public cuenta FindByResponsableUsuarioId(string id)
        {
            return _db.cuentas.FirstOrDefault(c => c.ResponsableUsuarioId == id);
        }

        public List<cuenta> FinAll()
        {
            return _db.cuentas.ToList();
        }
        public cuenta Find(int id)
        {
            return _db.cuentas.Find(id);
        }

        /// <summary>
        /// Crea la cuenta del prveedor y registra el usuario maestro
        /// </summary>
        /// <param name="nombreProveedor"></param>
        /// <param name="tipo"></param>
        /// <param name="responsableLogin"></param>
        /// <param name="reponsableNombre"></param>
        /// <param name="reponsableApellido"></param>
        /// <param name="responsableCargo"></param>
        /// <param name="responsableEmail"></param>
        /// <param name="responableTelefono"></param>
        /// <param name="responsablePassword"></param>
        public void Crear(string nombreProveedor, Tipo tipo, string responsableLogin,
            string reponsableNombre, string reponsableApellido, string responsableCargo,
            string responsableEmail, string responableTelefono, string responsablePassword)
        {
            nombreProveedor = nombreProveedor.Trim();

            var cuenta = FindByNombre(nombreProveedor);

            // Valida si existe una cuenta con el mismo nombre
            if (cuenta != null)
            {
                throw new BusinessException(MensajesResource.ERROR_CuentaManager_Crear_NombreProveedorExistente);
            }

            var applicationUserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
          
            if (applicationUserManager.FindByName(responsableLogin) != null)
            {
                throw new BusinessException(MensajesResource.ERROR_CuentaManager_Crear_LoginExistente);
            }

            var perfilProveedorManager = new PerfilProveedorManager();

            var usuarioProveedorManager = new UsuarioProveedorManager();
            usuarioProveedorManager.Crear(
                responsableLogin,
                reponsableNombre,
                reponsableApellido,
                responsableEmail,
                responsablePassword,
                perfilProveedorManager.GetMaestroByUsuarioTipo(tipo).Id, 
                // TODO MOVER A LA CLASE USUARIOPROVEEDORMANAGER PARA QUE LO RETORNE
                "PROVEEDOR-MAESTRO"
                );

            var usuarioMaestro = usuarioProveedorManager.FindMaestroByLogin(responsableLogin);

            cuenta = new cuenta
            {
                // TODO CAMBIAR 
                CodigoProveedor = Guid.NewGuid().ToString("D"),
                NombreProveedor = nombreProveedor,
                Activo = true,
                Tipo = GetTipoString(tipo),
                ResponsableUsuarioId = usuarioMaestro.Id
            };

            _db.cuentas.Add(cuenta);
            _db.SaveChanges();

            // TODO PASAR LOS QUERIES A UN ARCHIVO DE RECURSO
            const string sql = @"
                        INSERT INTO  cuentasusuarios (UsuarioId, CuentaId)
                        VALUES ({0},{1})";
            _db.Database.ExecuteSqlCommand(sql, usuarioMaestro.Id, cuenta.Id);
            _db.SaveChanges();
        }

        public void AsociarUsuarioEnCuenta(string usuarioId, int cuentaId)
        {
            // TODO PASAR LOS QUERIES A UN ARCHIVO DE RECURSO
            const string sql = @"
                        INSERT INTO  cuentasusuarios (UsuarioId, CuentaId)
                        VALUES ({0},{1})";
            _db.Database.ExecuteSqlCommand(sql, usuarioId, cuentaId);
            _db.SaveChanges();
        }

        public void DesAsociarUsuarioEnCuenta(string usuarioId)
        {
            // TODO PASAR LOS QUERIES A UN ARCHIVO DE RECURSO O STORE PROCEDURE
            const string sql = @"
                        DELETE FROM cuentasusuarios 
                        WHERE  UsuarioId = {0}";
            _db.Database.ExecuteSqlCommand(sql, usuarioId);
            _db.SaveChanges();
        }

        //TODO
        public void Eliminar()
        {
            throw new NotImplementedException();
        }

    }
}
