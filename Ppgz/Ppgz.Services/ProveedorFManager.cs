using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Ppgz.Repository;
using SapWrapper;

namespace Ppgz.Services
{
    public class ProveedorFManager
    {
        private readonly Entities _db = new Entities();

        public List<localPF> GetProveedoresFactoraje()
        {            
            var result = _db.proveedoresfactoraje.Join(_db.proveedores, pf => pf.idProveedore, p => p.Id, (pf, p) => new { p.Id, pf.DiaDePago, p.Nombre1, p.NumeroProveedor, pf.Porcentaje, p.Rfc }).ToList();
            List<localPF> listresult = new List<localPF>();

            foreach(var item in result)
            {
                localPF element = new localPF
                {
                    IdProveedor = item.Id,
                    DiaDePago = item.DiaDePago,
                    Nombre = item.Nombre1,
                    Numero = item.NumeroProveedor,
                    Porcentaje = item.Porcentaje,
                    Rfc = item.Rfc
                };
                listresult.Add(element);
            }

            return listresult;
        }

        public proveedorfactoraje GetProveedorById(int id)
        {
            return _db.proveedoresfactoraje.FirstOrDefault(p => p.idProveedore == id);
        }

        public bool UpdateProveedorFactoraje(int id, int diadepago, int porcentaje)
        {
            bool result = false;

            var proveedorf = GetProveedorById(id);
            if (proveedorf != null)
            {
                proveedorf.DiaDePago = diadepago;
                proveedorf.Porcentaje = porcentaje;
                
                _db.Entry(proveedorf).State = EntityState.Modified;
                _db.SaveChanges();
                result = true;
            }

            return result;
        }

        public bool DeleteProveedorFactoraje(int id)
        {
            bool result = false;
            var proveedorf = GetProveedorById(id);

            if (proveedorf != null)
            {
                /*SolicitudesF, FacturasF, DescuentosF (Relacionados con Proveedor)
                var solicitudesf = _db.solicitudesfactoraje.Where(s => s.IdProveedor == id).ToList();

                var facturasf = new List<facturasfactoraje>();
                var descuentosf = new List<descuentofactoraje>();
                foreach (var solicitud in solicitudesf)
                {
                    var lfacturasf = _db.facturasfactoraje.Where(f => f.idSolicitudesFactoraje == solicitud.idSolicitudesFactoraje).ToList();
                    foreach(var factura in lfacturasf)
                    {
                        facturasf.Add(factura);
                    }

                    var ldescuentosf = _db.descuentosfactoraje.Where(d => d.idSolicitudesFactoraje == solicitud.idSolicitudesFactoraje).ToList();
                    foreach (var descuento in ldescuentosf)
                    {
                        descuentosf.Add(descuento);
                    }
                }

                foreach(var descuento in descuentosf)
                {
                    _db.Entry(descuento).State = EntityState.Deleted;
                    _db.SaveChanges();
                }

                foreach (var factura in facturasf)
                {
                    _db.Entry(factura).State = EntityState.Deleted;
                    _db.SaveChanges();
                }

                foreach(var solicitud in solicitudesf)
                {
                    _db.Entry(solicitud).State = EntityState.Deleted;
                    _db.SaveChanges();
                }
                /*SolicitudesF, FacturasF, DescuentosF (Relacionados con Proveedor)*/

                _db.Entry(proveedorf).State = EntityState.Deleted;
                _db.SaveChanges();
                result = true;
            }

            return result;
        }

        public class localPF
        {
            public int IdProveedor { get; set; }
            public int DiaDePago { get; set; }
            public string Nombre { get; set; }
            public string Numero { get; set; }
            public int Porcentaje { get; set; }
            public string Rfc { get; set; }
        }
    }
}
