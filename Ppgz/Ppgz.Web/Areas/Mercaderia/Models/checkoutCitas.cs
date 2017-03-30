using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json;
using Ppgz.Repository;

namespace Ppgz.Web.Areas.Mercaderia
{
    public class checkoutCitas
    {


        public class datosProveedor
        {

            public string idProveedor { get; set; }
            public string fecha { get; set; }

            public datosProveedor()
            {
            }

            public datosProveedor(string prmidProveedor, string prmFecha)
            {
                idProveedor = prmidProveedor; 
                fecha = prmFecha;

            }

        }




        // This will be serialized into a JSON Address object
        private class Tallas
        {
            public string Talla { get; set; }
            public string Cantidad { get; set; }

            public Tallas(string talla, string cantidad)
            {
                Talla = talla; Cantidad = cantidad;
            }
        }

        // This will be serialized into a JSON Contact object
        private class Items
        {
            public string Item { get; set; }
            public List<Tallas> Tallas { get; set; }

            public Items(string prmItem, List<Tallas> prmTalla)
            {
                Item = prmItem; Tallas = new List<Tallas>(prmTalla);
            }

        }

      
        //Creo Valores Unicos Editables
        private class valEdit<Val>
        {
            public string Id { get; set; }

            public valEdit() { }

            public valEdit(string key)
            {
                this.Id = key;
            }
        }

        //Creo el Equivalente a KeyValuePar de .NET, pero Editable
        public class KeyVal<Key, Val>
        {
            public string Id { get; set; }
            public object Valores { get; set; }

            public KeyVal() { }

            public KeyVal(string key, object val)
            {
                this.Id = key;
                this.Valores = val;
            }
        }

        public class KeyVal2<Key, Val>
        {
            public string Id { get; set; }
            public List<ordencompradetalle> Valores { get; set; }

            public KeyVal2() { }

            public KeyVal2(string key, List<ordencompradetalle> val)
            {
                this.Id = key;
                this.Valores = val;
            }
        }

        //Clase para Mantener la Lista de Ordenes
        public static class ListaDeOrdenes
        {
            private static List<KeyVal2<string, List<ordencompradetalle>>> _list;
            private static string numOrdenTemp = "";
            private static string proveedorId = "";
            private static string Fecha = "";

            static ListaDeOrdenes()
            {
                //inicilizo la lista
                _list = new List<KeyVal2<string, List<ordencompradetalle>>>();
            }

            public static void Clear()
            {
                _list = new List<KeyVal2<string, List<ordencompradetalle>>>();
                
            }

            public static string IdProveedor(string valor, string opc)
            {

                string res = "";

                if (opc == "set")
                {
                    proveedorId = valor;
                    
                }

                if (opc == "get")
                {

                    res = proveedorId;
                }

                return res;
            }


            public static string FechaOrden(string valor, string opc)
            {

                string res = "";

                if (opc == "set")
                {
                    Fecha = valor;

                }

                if (opc == "get")
                {

                    res = Fecha;
                }

                return res;


            }

            public static string getsetOrdenTemp(string orden,int opcion = 0)
            {

                //*********************
                // valores para opcion
                //*********************
                // 0 = Agregar y Validar que la Orden no Existe, si la Orden Existe Regresa 1, de lo contrario 0
                // 1 = Obtener el Numero de la Orden
                //*********************
               
                string resultado = "";

                if (opcion == 0)
                {
                    numOrdenTemp = orden;

                    Boolean existeOrden = false;

                    if (_list.Count > 0)
                    {
                        foreach (var ordenesTemp in _list)
                        {
                            if (ordenesTemp.Id == orden) //ORDEN A BUSCAR
                            {
                                existeOrden = true;
                                resultado = "1";
                                numOrdenTemp = "";
                                break;
                            }
                        }
                    };


                    if (existeOrden == false)
                    {
                        resultado = "0";
                  
                    };

                }
                else if (opcion == 1)
                {

                    resultado = numOrdenTemp;

                }


                return resultado;

            }

            public static void Agregar(string orden, List<ordencompradetalle> ordenes)
            {

                Boolean existeOrden = false;

                if (_list.Count > 0)
                {
                    foreach (var ordenesTemp in _list)
                    {
                        if (ordenesTemp.Id == orden) //ORDEN A BUSCAR
                        {
                            existeOrden = true;
                        }
                    }
                };


                if (existeOrden == false)
                {
                    //
                    // Guardo los registros en la lista
                    //
                    _list.Add(new KeyVal2<string, List<ordencompradetalle>>(orden, ordenes));

                };

            }

            public static Int64 countElementosEnLista()
            {

                //Cuento los Elementos en la Lista
                Int64 varElementos = _list.Count();
                return varElementos;

            }

            public static ArrayList getElementosEnLista()
            {
                //Recorro todos los elementos Principales de la Lista
                ArrayList varValores = new ArrayList();

                foreach (var value in _list)
                {
                    varValores.Add(value);
                }

                return varValores;
            }

            public static ArrayList getElementosEnLista(int indice)
            {
                //Obtengo el Valor de Algun elemento Principal de la Lista
                ArrayList varValores = new ArrayList();

                varValores.Add(_list[indice]);

                return varValores;
            }

            public static Hashtable getListaDeOrdenes()
            {

                Hashtable hashtable = new Hashtable();

                Int16 contador = 0;

                if (_list.Count > 0)
                {
                    foreach (var ordenesTemp in _list)
                    {
                        hashtable[contador] = ordenesTemp.Id;
                        contador += 1;
                    }
                };

                return hashtable;


            }

            public static List<ordencompradetalle> convertirHashTable(Hashtable tbl)
            {


                

                //DictionaryEntry de = new DictionaryEntry("detalle", tbl["detalle"]);

                List<ordencompradetalle> resultado = new List<ordencompradetalle>((List<ordencompradetalle>)tbl["detalle"]);

                //resultado.Add(new List<ordencompradetalle>(tbl["detalle"]));

                return resultado;

            }

            public static List<ordencompradetalle> getListaDeItems(string Orden)
            {


                Hashtable hashtable = new Hashtable();

                Int16 contador = 0;


                List<ordencompradetalle> ItemsOrdenes = new List<ordencompradetalle>();//Aqui cargo los Items de la Orden

                string valItems = "";
                string valTalla = "";
                string valCantidad = "";

                if (_list.Count > 0)
                {
                    foreach (var ordenesTemp in _list)
                    {

                        if (ordenesTemp.Id == Orden)//ORDEN A BUSCAR
                        {

                            //List<ordencompradetalle> ItemsEnOrden = new List<ordencompradetalle>();
                            List<ordencompradetalle> resultado = new List<ordencompradetalle>((List<ordencompradetalle>)ordenesTemp.Valores);
                            ItemsOrdenes.AddRange(resultado);
                        }

                    }
                };


                //var json = JsonConvert.SerializeObject(ItemsOrdenes);

                return ItemsOrdenes;

            }



            //********************************
            //ACTUALIZO ELEMENTOS DE LA LISTA
            //********************************
            public static int updElementoEnLista(string Orden, string Item, string OldValue, string NewValue)
            {

                /**************************************
                METODO PARA ACTUALIZAR LA CANTIDAD
                 DE LA TALLA, EXISTENTE EN UN ITEMS
                 PARA UN NUMERO DE ORDEN
                ***************************************/

                int Resultado = 0;
                /**************************************
                AQUI SE RECORRE LA LISTA CON LAS ORDENES
                ***************************************/
                foreach (var ordenes in _list)
                {
                    if (ordenes.Id == Orden)//ORDEN A BUSCAR
                    {

                        //List<ordencompradetalle> ItemsEnOrden = new List<ordencompradetalle>();
                        List<ordencompradetalle> resultado = new List<ordencompradetalle>((List<ordencompradetalle>)ordenes.Valores);
                        //ItemsOrdenes.AddRange(resultado);

                        foreach (var item in resultado)
                        {
                            if (item.NumeroMaterial == Item)
                            {
                                item.CantidadComprometida = Convert.ToDecimal(NewValue);
                                Resultado = 1;
                                break;
                            }

                        }


                    }
                }

                return Resultado;

            }

        }



    }
}