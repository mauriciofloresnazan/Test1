using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ppgz.CitaWrapper.Entities;

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
