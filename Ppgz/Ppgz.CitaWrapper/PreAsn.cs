using System;
using System.Collections.Generic;
using System.Linq;

namespace Ppgz.CitaWrapper
{
    public class PreAsn
    {
        
        public string NumeroProveedor;
        public String NumeroDocumento;
        public String Sociedad;
        public DateTime FechaEntrega;
        public List<DateTime> FechasPermitidas;
        public bool EsCrossDock;
        public string Tienda;
        public string TiendaOrigen;
        public string InOut;
        public string NumeroOrdenSurtido;
        public string Centro;

        /*AGREGADO MF 20221130*/
        public bool Autorizada;
        /* ***************** */
        public ICollection<PreAsnDetalle> Detalles { get; set; }

        public int TotalPedido
        {
            get
            {
                return Detalles.AsEnumerable().Sum(x => x.CantidadPedido);
            }
        }
        public int TotalPermitido
        {
            get
            {
                return Detalles == null ? 0 : Detalles.AsEnumerable().Sum(x => x.CantidadPermitida > 0 ? x.CantidadPermitida : 0 );
            }
        }

        public int TotalCantidad
        {
            get
            {
                return Detalles.AsEnumerable().Sum(x => x.Cantidad > 0 ? x.Cantidad : 0);
            }
        }


    }
}
