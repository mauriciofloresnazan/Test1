using System;
using System.Collections.Generic;
using System.Linq;
using Ppgz.Repository;
using Ppgz.Services;

namespace Ppgz.Web.Areas.Mercaderia
{
    public class CurrentCita
    {
        private readonly proveedore _proveedor;
        public proveedore Proveedor
        {
            get
            {
                return _proveedor;
                
            }
        }

        public DateTime? Fecha;

        public List<ordencompra> Ordenes;

        public CurrentCita(int cuentaId, int proveedorId)
        {
            var proveedorManager = new ProveedorManager();

            var proveedor = proveedorManager.Find(proveedorId);

            if (proveedor.CuentaId != cuentaId)
            {
                throw new BusinessException("Error en la cuenta del proveedor");
            }
            _proveedor = proveedor;

        }

        public void AddOrden(ordencompra orden)
        {
            if (Ordenes.Any(o => o.Id == orden.Id))
            {
                throw new BusinessException("La orden ya se encuentra incluida");
            }
            Ordenes.Add(orden);
        }

        public void RemoveOrden(string numeroDocumento)
        {
            var orden = Ordenes.FirstOrDefault(o => o.NumeroDocumento == numeroDocumento);
            if (orden != null)
            {
                Ordenes.Remove(orden);
            }
        }


        public int UpdateElementoEnLista(string numeroDocumento, string numeroMaterial, 
            int oldValue, int newValue)
        {

            /**************************************
            METODO PARA ACTUALIZAR LA CANTIDAD
             DE LA TALLA, EXISTENTE EN UN ITEMS
             PARA UN NUMERO DE ORDEN
            ***************************************/

            var resultado = 0;

            var orden = Ordenes.FirstOrDefault(o => o.NumeroDocumento == numeroDocumento);

            if (orden == null)
            {
                return resultado;
            }

            if (!orden.ordencompradetalles.Any())
            {
                return resultado;
            }

            var detalle = orden.ordencompradetalles.FirstOrDefault(d => d.NumeroMaterial == numeroMaterial);
          
            if (detalle == null)
            {
                return resultado;
            }

            detalle.CantidadComprometida = newValue;
            resultado = 1;


            /**************************************
            AQUI SE RECORRE LA LISTA CON LAS ORDENES
            ***************************************/
            /*foreach (var ordenes in _list)
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
            */
            return resultado;

        }


    }
}