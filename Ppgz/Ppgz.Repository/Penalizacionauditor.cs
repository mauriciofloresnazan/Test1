﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Ppgz.Repository
{
    public class Penalizacionauditor
    {
        public int id { get; set; }
        public System.DateTime FechaPenalizacion { get; set; }
        public string NumeroProveedor { get; set; }
        public string RazonSocial { get; set; }
        public string Marca { get; set; }
        public string Total { get; set; }
        public string auditor { get; set; }
        public bool procesado { get; set; }
        public string Correo { get; set; }
        public decimal Totalsum { get; set; }

        public virtual proveedore proveedore { get; set; }
    }
}