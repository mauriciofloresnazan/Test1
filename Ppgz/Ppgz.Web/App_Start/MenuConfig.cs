﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI.HtmlControls;
using Microsoft.Ajax.Utilities;

namespace Ppgz.Web
{
    public static class MenuConfig
    {
        //PARA INCLUIR EN EL MENÚ, SEGUIR LOS PASOS:
        public static string[] TiposLista()
        {
            string[] tipos = 
            {
                "NAZAN",
                "MERCADERIA",
                "SERVICIO"
            };
            return tipos;
        }
        public static List<string> GetMenuFuncionalidad(this IPrincipal user)
        {
            List<string> resultMenu = new List<string>();
            string[] tipos = TiposLista();
            foreach (var t in tipos)
            {
                List<string> rolesDelUsuario = LimpiaRolesXMenu(GetRoles(user, t));
                string[] menulist;
                switch (t)
                {
                    case "NAZAN":
                        menulist = MenuNazan();
                        break;
                    case "MERCADERIA":
                        menulist = MenuMercaderia();
                        break;
                    case "SERVICIO":
                        menulist = MenuServicio();
                        break;
                    default:
                        menulist = MenuNazan();
                        break;
                }

                foreach (var role in rolesDelUsuario)
                {
                    foreach (var item in menulist)
                    {
                        if (role == t)
                        {
                            resultMenu.Add(item.Split('|')[0] + "|" + item.Split('|')[1] + "|" + item.Split('|')[2] + "|" + item.Split('|')[3] + "|" + item.Split('|')[4] + "|" + item.Split('|')[5]);
                        }
                        if (item.Split('|')[0].Length <= 0) continue;
                        if (role.Contains(item.Split('|')[0]))
                        {
                            resultMenu.Add(item.Split('|')[0] + "|" + item.Split('|')[1] + "|" + item.Split('|')[2] + "|" + item.Split('|')[3] + "|" + item.Split('|')[4] + "|" + item.Split('|')[5]);
                        }
                    }
                }
            }
            return resultMenu;
        }
        private static List<string> LimpiaRolesXMenu(List<string> listaMenu)
        {
            List<string> resultList = new List<string>();
            int cant = 0;
            foreach (var item in listaMenu)
            {
                if (cant == 0)
                {
                    resultList.Add(item.Contains('-') ? item.Split('-')[1] : item);
                }
                else
                {
                    if (item.Contains('-'))
                    {
                        if (item.Split('-')[1] != listaMenu[cant - 1].Split('-')[1])
                        {
                            resultList.Add(item.Split('-')[1]);
                        }
                    }
                }
                cant++;
            }
            return resultList;
        }
        //1) PARA INCLUIR EN EL MENÚ PRINCIPAL, PRIMERO INCLUIR EN EL MENÚ SEGÚN TIPO (NAZAN, SERVICIO O MERCADERIA) 
        //   EL ÍTEM A AGREGAR,
        //      LOS CAMPOS VAN EN ESTE ORDEN, SEPARADOS POR PIPE (|):
        //          1- NOMBRE DEL PERFIL A VALIDAR (COLOCAR SOLAMENTE EL NOMBRE COMO TAL DEL ROL, SIN NAZAN NI LISTAR O MODIFICAR)
        //          2- NOMBRE QUE SE MOSTRARÁ EN EL MENÚ
        //          3- NOMBRE DEL ACTION NAME
        //          4- NOMBRE DEL CONTROLLER NAME
        //          5- NOMBRE DE LA IMAGEN A COLOCAR DENTRO DEL CLASS
        //          6- NOMBRE DEL TIPO DE AREA QUE CORRESPONDE
        public static string[] MenuNazan()
        {
            string[] menuLista =
            {
                "|Gestión de Proveedor|||fa fa-address-book-o|Nazan",
                "|Administración de Usuarios Proveedor|||fa fa-address-card|Nazan",
                "|Administración de Perfiles Proveedor|||fa fa-address-card|Nazan",
                "|Órdenes de Compra|||fa fa-address-book|Nazan",
                "|Cuentas por Pagar|||fa fa-calculator|Nazan",
                "|Administración de Facturas|||fa fa-calculator|Nazan",
                "ADMINISTRAMENSAJESINSTITUCIONALES|Administrar Mensajes Insitucionales|Index|AdministrarMensajesInstitucionales|fa fa-envelope-open|Nazan",
                "|Control de Citas|||fa fa-calendar|Nazan",
                "|Reportes|||fa fa-file-pdf-o|Nazan",
                "ADMINISTRARUSUARIOSNAZAN|Administración de Usuarios Nazan|Index|AdministrarUsuariosNazan|fa fa fa-users|Nazan",
                "ADMINISTRARPERFILESNAZAN|Administración de Perfiles Nazan|Index|AdministrarPerfilesNazan|fa fa fa-road|Nazan",
                "ADMINISTRARPROVEEDORESNAZAN|Administración de Proveedoes Nazan|Index|AdministrarProveedores|fa fa-address-book-o|Nazan"
            };
            return menuLista;
        }
        public static string[] MenuMercaderia()
        {
            string[] menuLista =
            {
                "|Gestión de Proveedor|||fa fa-address-book-o|Mercaderia",
                "ORDENESCOMPRA|Órdenes de Compra|Index|OrdenesCompra|fa fa-address-book|Mercaderia",
                "|Control de Citas|||fa fa-calendar|Mercaderia",
                "|Comprobante de Recibo|||fa fa-file-pdf-o|Mercaderia",
                "|Impresión de Etiquetas|||fa fa-file-pdf-o|Mercaderia",
                "|Cuentas por Pagar|||fa fa-calculator|Mercaderia",
                "|Administración de Facturas|||fa fa-calculator|Mercaderia",
                "MENSAJESINSTITUCIONALES|Mensajes Institucionales|Index|MensajesInstitucionales|fa fa-envelope-open|Mercaderia",
                "|Administración de Usuarios|Index|AdministrarUsuarios|fa fa fa-users|Mercaderia",
                "ADMINISTRARPERFILES|Administración de Perfiles|Index|AdministrarPerfiles|fa fa fa-road|Mercaderia",
                "ADMINISTRARUSUARIOS|Autenticación de Usuarios Proveedor|||fa fa-address-book-o|Mercaderia",
                "|Reportes|||fa fa-file-pdf-o|Mercaderia"
            };
            return menuLista;
        }
        public static string[] MenuServicio()
        {
            string[] menuLista =
            {
                "|Gestión de Proveedor|||fa fa-address-book-o|Servicio",
                "ORDENESCOMPRA|Órdenes de Compra|Index|OrdenesCompra|fa fa-address-book|Servicio",
                "|Cuentas por Pagar|||fa fa-calculator|Servicio",
                "|Administración de Facturas|||fa fa-calculator|Servicio",
                "MENSAJESINSTITUCIONALES|Mensajes Insitucionales|Index|MensajesInstitucionales|fa fa-envelope-open|Servicio",
                "|Administración de Usuarios Proveedor|||fa fa-address-card|Servicio",
                "ADMINISTRARPERFILES|Administración de Perfiles|Index|AdministrarPerfiles|fa fa fa-users|Servicio",
                "ADMINISTRARUSUARIOS|Autenticación de Usuarios|Index|AdministrarUsuarios|fa fa fa-road|Servicio",
                "|Reportes|||fa fa-file-pdf-o|Servicio"
            };
            return menuLista;
        }
        public static List<string> GetMenuPaginaActual(string nombreControllerActual, string areaActual)
        {
            List<string> menuInternolist = new List<string>();
            switch (areaActual.ToUpper())
            {
                case "NAZAN":
                    menuInternolist = MenuInternoNazan(nombreControllerActual);
                    break;
                case "MERCADERIA":
                    menuInternolist = MenuInternoMercaderia(nombreControllerActual);
                    break;
                case "SERVICIO":
                    menuInternolist = MenuInternoServicio(nombreControllerActual);
                    break;
                default:
                    menuInternolist = MenuInternoNazan(nombreControllerActual);
                    break;
            }
            return menuInternolist;
        }
        //1) PARA INCLUIR EN EL MENÚ INTERNO, PRIMERO AGREGAR EN EL CASE DENTRO DEL MENÚ INTERNO SEGÚN TIPO 
        //   (NAZAN, SERVICIO O MERCADERIA) EL NOMBRE DEL CONTROLADOR A AGREGARLE EL MENÚ INTERNO
        //2) LUEGO, INCLUIR EN EL MENÚ INTERNO EL ÍTEM A AGREGAR,
        //      LOS CAMPOS VAN EN ESTE ORDEN, SEPARADOS POR PIPE (|):
        //          1- NOMBRE DEL PERFIL A VALIDAR (COLOCAR SOLAMENTE EL NOMBRE COMO TAL DEL ROL, SIN NAZAN NI LISTAR O MODIFICAR)
        //          2- NOMBRE QUE SE MOSTRARÁ EN EL MENÚ
        //          3- NOMBRE DEL ACTION NAME
        //          4- NOMBRE DEL CONTROLLER NAME
        //          5- NOMBRE DE LA IMAGEN A COLOCAR DENTRO DEL CLASS
        //          6- NOMBRE DEL TIPO DE AREA QUE CORRESPONDE
        private static List<string> MenuInternoServicio(string nombreControllerActual)
        {
            List<string> menu;
            switch (nombreControllerActual)
            {
                case "MensajesInstitucionales":
                    menu = new List<string>
                    {
                        "MENSAJESINSTITUCIONALES|Insitucionales|Index|MensajesInstitucionales|fa fa-envelope-open|Servicio",
                        "MENSAJESINSTITUCIONALES|Rechazados|Index|MensajesInstitucionales|fa fa-envelope-open|Servicio",
                        "MENSAJESINSTITUCIONALES|Descartados|Index|MensajesInstitucionales|fa fa-envelope-open|Servicio"
                    };
                    break;
                default:
                    menu = new List<string> { "MENSAJESINSTITUCIONALES|" + nombreControllerActual + "|Index|" + nombreControllerActual + "|fa fa-envelope-open|Servicio" };
                    break;
            }
            return menu;
        }
        private static List<string> MenuInternoMercaderia(string nombreControllerActual)
        {
            List<string> menu;
            switch (nombreControllerActual)
            {
                case "MensajesInstitucionales":
                    menu = new List<string>
                    {
                        "MENSAJESINSTITUCIONALES|Recibidos|Index|MensajesInstitucionales|fa fa-envelope-open|Mercaderia",
                        "MENSAJESINSTITUCIONALES|Rechazados|Index|MensajesInstitucionales|fa fa-envelope-open|Mercaderia",
                        "MENSAJESINSTITUCIONALES|Descartados|Index|MensajesInstitucionales|fa fa-envelope-open|Mercaderia"
                    };
                    break;
                default:
                    menu = new List<string> { "MENSAJESINSTITUCIONALES|" + nombreControllerActual + "|Index|" + nombreControllerActual + "|fa fa-envelope-open|Mercaderia" };
                    break;
            }
            return menu;
        }
        private static List<string> MenuInternoNazan(string nombreControllerActual)
        {
            List<string> menu;
            switch (nombreControllerActual)
            {
                case "AdministrarMensajesInstitucionales":
                    menu = new List<string>
                    {
                        "ADMINISTRAMENSAJESINSTITUCIONALES|Recibidos|Index|AdministrarMensajesInstitucionales|fa fa-envelope-open|Nazan",
                        "ADMINISTRAMENSAJESINSTITUCIONALES|Rechazados|Index|AdministrarMensajesInstitucionales|fa fa-envelope-open|Nazan",
                        "ADMINISTRAMENSAJESINSTITUCIONALES|Descartados|Index|AdministrarMensajesInstitucionales|fa fa-envelope-open|Nazan"
                    };
                    break;
                case "AdministrarProveedores":
                    menu = new List<string>
                    {
                        "ADMINISTRARPERFILESNAZAN|Nuevo|Index|AdministrarMensajesInstitucionales|fa fa-envelope-open|Nazan",
                        "ADMINISTRARPERFILESNAZAN|Proveedor|Index|AdministrarMensajesInstitucionales|fa fa-envelope-open|Nazan"
                    };
                    break;
                case "AdministrarUsuariosNazan":
                    menu = new List<string>
                    {
                        "ADMINISTRARUSUARIOS|Nuevo|Index|AdministrarMensajesInstitucionales|fa fa-envelope-open|Nazan"
                    };
                    break;
                default:
                    menu = new List<string> { "MENSAJESINSTITUCIONALES|" + nombreControllerActual + "|Index|" + nombreControllerActual + "|fa fa-envelope-open|Nazan" };
                    break;
            }
            return menu;
        }
        public static string TieneMenuInterno(string nombreControllerActual, string areaActual)
        {
            string respuesta;
            List<string> menuList = GetMenuPaginaActual(nombreControllerActual, areaActual);
            if (menuList.Count > 1)
            {
                respuesta = "S|";
            }
            else
            {
                respuesta = "N|";
            }
            return respuesta;
        }
        private static List<string> GetRoles(this IPrincipal user, string tipo)
        {
            List<string> listaRoles = new List<string>();
            Startup listasStartup = new Startup();
            if (tipo == "NAZAN")
            {
                Dictionary<string,string> rolesNazan = listasStartup.GetRolesNazan();
                foreach (KeyValuePair<string, string> role in rolesNazan)
                {
                    if (user.IsInRole(role.Key))
                    {
                        listaRoles.Add(role.Key);
                    }
                }
            }
            else if (tipo == "MERCADERIA")
            {
                Dictionary<string, string> rolesMercaderia = listasStartup.GetRolesMercaderia();
                foreach (KeyValuePair<string, string> role in rolesMercaderia)
                {
                    if (user.IsInRole(role.Key))
                    {
                        listaRoles.Add(role.Key);
                    }
                }
            }
            else if (tipo == "SERVICIO")
            {
                Dictionary<string, string> rolesServicio = listasStartup.GetRolesServicio();
                foreach (KeyValuePair<string, string> role in rolesServicio)
                {
                    if (user.IsInRole(role.Key))
                    {
                        listaRoles.Add(role.Key);
                    }
                }
            }
            return listaRoles;
        }
    }
}