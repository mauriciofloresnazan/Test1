namespace Ppgz.CitaWrapper
{
    public class PreAsnDetalle
    {
        public string NumeroPosicion;
        public string Centro;
        public string Almacen;

        public int CantidadPedido { get; set; }

        public int CantidadEntregada
        {
            get { return CantidadPedido - CantidadPermitidaSap; }
        }

        public int CantidadPermitidaSap;

        public int CantidadCitasFuturas { get; set; }

        public int CantidadPermitida
        {
            get { return CantidadPermitidaSap - CantidadCitasFuturas; }
        }

        public int Cantidad { get; set; }

        public string NumeroMaterial;

        public string DescripcionMaterial;


    }
}
