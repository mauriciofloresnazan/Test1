using System;
using System.Collections.Generic;


namespace Ppgz.CitaWrapper
{
    public class PreCita
    {
        public DateTime Fecha;

         public string Centro;

        public int Cantidad;

        public int ProveedorId;

        public string UsuarioId;

        public List<Asn> Asns;

        public List<int> HorarioRielesIds;
    }
}
