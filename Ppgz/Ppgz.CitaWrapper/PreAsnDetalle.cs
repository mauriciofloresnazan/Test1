namespace Ppgz.CitaWrapper
{
    public class PreAsnDetalle
    {
        public string NumeroPosicion;
        public string Centro;
        public string Almacen;
        public decimal Precio;
        public string UnidadMedida;

        public int CantidadPedido { get; set; }

        public int CantidadEntregada
        {
            get { return CantidadPedido - CantidadPermitidaSap; }
        }

        public int CantidadPermitidaSap;

        public int CantidadCitasFuturas { get; set; }

        public int CantidadPermitida
        {
            get {
                if (CantidadPermitidaSap - CantidadCitasFuturas < 0) {
                    return 0;
                }
                else
                {
                    return CantidadPermitidaSap - CantidadCitasFuturas;
                }
                
            }
        }

        public int Cantidad { get; set; }

        public string NumeroMaterial;

        public string DescripcionMaterial;

        public string NumeroMaterial2;

        /*AGREGADO MF 20221130*/
        public int CantidadPorEntregar;
    }
}
