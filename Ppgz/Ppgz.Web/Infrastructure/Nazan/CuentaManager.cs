using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Ppgz.Repository;
using Ppgz.Web.Areas.Nazan;
using Ppgz.Web.Models;

namespace Ppgz.Web.Infrastructure.Nazan
{
    public class CuentaManager
    {
        

        public enum Tipo
        {
            MERCADERIA,
            SERVICIO
        }

        internal  static string GetTipoString(Tipo tipo)
        {
            switch (tipo)
            {
                case Tipo.MERCADERIA:
                    return "MERCADERIA";
                case Tipo.SERVICIO:
                    return "SERVICIO";
            }
            return null;
        }


        public static Tipo GetTipoByString(string tipo)
        {
            switch (tipo)
            {
                case "MERCADERIA":
                    return Tipo.MERCADERIA;
                case "SERVICIO":
                    return Tipo.SERVICIO;
            }
            //TODO CARLOS Y JUAN DELGADO
            throw new Exception("");
      
        }

        private readonly Entities _db = new Entities();

        public cuenta FindByNombre(string nombreProveedor)
        {
            return _db.cuentas.FirstOrDefault(c => c.NombreProveedor == nombreProveedor);
        }

        public void Create(string nombreProveedor, Tipo tipo, string responsableLogin,
            string reponsableNombre, string reponsableApellido, string responsableCargo, 
            string responsableEmail, string responableTelefono, string responsablePassword
            )
        {
            nombreProveedor = nombreProveedor.Trim();

            var cuenta = FindByNombre(nombreProveedor);

            if (cuenta != null)
            {
                //TODO CARLOS CAMPOS
                throw new Exception(Errores.CuentaNombreProveedorExistente);
            }
            var applicationUserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            if (applicationUserManager.FindByName(responsableLogin) != null)
            {
                // TODO CARLOS Y JUAN DELGADO
                throw new Exception(Errores.UsuarioNazanLoginExistente);
            }

            var perfilProveedorManager = new PerfilProveedorManager();

            var usuarioProveedorManager = new UsuarioProveedorManager();
            usuarioProveedorManager.Create(
                responsableLogin,
                reponsableNombre,
                reponsableApellido,
                responsableEmail,
                responsablePassword,
                perfilProveedorManager.GetMaestroByUsuarioTipo(tipo).Id
                );

            var usuarioMaestro = usuarioProveedorManager.FindMaestroByLogin(responsableLogin);

            cuenta = new cuenta
            {
                CodigoProveedor = Guid.NewGuid().ToString("D"),
                NombreProveedor = nombreProveedor,
                Activo = true,
                Tipo = GetTipoString(tipo),
                ResponsableUsuarioId = usuarioMaestro.Id

            };

            //cuenta.aspnetusers.Add(usuarioMaestro);
 
            _db.cuentas.Add(cuenta);
            _db.SaveChanges();

            usuarioMaestro.cuentas.Add(cuenta);
            _db.SaveChanges();

       
        }

        public List<cuenta> FinAll()
        {
            return _db.cuentas.ToList();
        }
        public cuenta Find(int id)
        {
            return _db.cuentas.Find(id);
        }
    }
}
