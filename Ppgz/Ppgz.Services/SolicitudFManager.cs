using Ppgz.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ppgz.Services
{
    public class SolicitudFManager
    {
        private readonly Entities _db = new Entities();

        public List<localsolicitud> GetSolicitudesFactoraje()
        {
            var result = new List<localsolicitud>();
            var ls = _db.solicitudesfactoraje.Join(_db.proveedores,
                                                        sf => sf.IdProveedor,
                                                        p => p.Id,
                                                        (sf, p) => new { sf, p })
                                                        .Join(_db.proveedoresfactoraje,
                                                                n => n.sf.IdProveedor,
                                                                pf => pf.idProveedore,
                                                                (n, pf) => new
                                                                {
                                                                    n.sf.idSolicitudesFactoraje,
                                                                    n.p.NumeroProveedor,
                                                                    n.p.Nombre1,
                                                                    pf.Porcentaje,
                                                                    n.sf.NDocumentos,
                                                                    n.sf.DescuentoPP,
                                                                    n.sf.Descuentos,
                                                                    n.sf.EstatusFactoraje
                                                                }).ToList();
            var estatus = _db.estatusfactoraje.ToList();
            foreach(var item in ls)
            {
                string s = estatus.Where(e => e.idEstatusFactoraje == item.EstatusFactoraje && e.Activo == 1).FirstOrDefault().Nombre.ToString();
                localsolicitud element = new localsolicitud
                {
                    Id = item.idSolicitudesFactoraje,
                    Numero = item.NumeroProveedor,
                    Proveedor = item.Nombre1,
                    Tasa = item.Porcentaje,
                    NoDocumentos = item.NDocumentos,
                    DescuentoPP = item.DescuentoPP,
                    Descuentos = item.Descuentos,
                    Estatus = item.EstatusFactoraje,
                    EstatusNombre = s
                };
                result.Add(element);
            }
            return result;
        }

        public class localsolicitud
        {
            public int Id { get; set; }
            public string Numero { get; set; }
            public string Proveedor { get; set; }
            public int Tasa { get; set; }
            public int NoDocumentos { get; set; }
            public int DescuentoPP { get; set; }
            public int Descuentos { get; set; }
            public int Estatus { get; set; }
            public string EstatusNombre { get; set; }
        }
    }
}
