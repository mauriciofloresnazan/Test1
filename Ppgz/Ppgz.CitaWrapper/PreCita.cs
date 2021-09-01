using System;
using System.Collections.Generic;


namespace Ppgz.CitaWrapper
{
    public class PreCita
    {
        public DateTime Fecha;

        public DateTime FechaCreacion;

         public string Centro;

        public string Sociedad;

        public int Cantidad;

        public int ProveedorId;

        public string UsuarioId;

        public string TipoCita;

        public List<Asn> Asns;

        public List<int> HorarioRielesIds;
    }
}
