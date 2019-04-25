using Ppgz.Repository;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
                                                        (sf, p) => new
                                                        { 
                                                            sf.idSolicitudesFactoraje,
                                                            sf.IdProveedor,
                                                            p.NumeroProveedor,
                                                            p.Nombre1,
                                                            sf.NDocumentos,
                                                            sf.DescuentoPP,
                                                            sf.Descuentos,
                                                            sf.EstatusFactoraje,
															sf.MontoAFacturar,
															sf.MontoOriginal,
															sf.FechaSolicitud,
                                                            sf.DiasPP,
                                                            sf.NumeroGenerado,
                                                            sf.Sociedad
                                                        }).ToList();                                                        
                                                        /*.Join(_db.proveedoresfactoraje,
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
                                                                }).ToList();*/           

            var estatus = _db.estatusfactoraje.ToList();
            var proveedoresfactoraje = _db.proveedoresfactoraje.ToList();
            
            foreach(var item in ls)
            {
                string s = estatus.Where(e => e.idEstatusFactoraje == item.EstatusFactoraje && e.Activo == 1).FirstOrDefault().Nombre.ToString();
                //proveedorfactoraje pf = proveedoresfactoraje.Find(x => x.idProveedore == item.IdProveedor);
                //int Porcentaje = (pf != null) ? pf.Porcentaje : 0;
                float Tasa = 0;

                var fmanager = new FacturaFManager();
                List<facturasfactoraje> lfacturas = fmanager.GetFacturasBySolicitud(item.idSolicitudesFactoraje);
                if (lfacturas.Count() > 0)
                {
                    foreach(var f in lfacturas)
                    {
                        Tasa += f.Porcentaje;
                    }
                    Tasa = Tasa / lfacturas.Count();
                }

                localsolicitud element = new localsolicitud
                {
                    Id = item.idSolicitudesFactoraje,
                    Numero = item.NumeroProveedor,
                    Proveedor = item.Nombre1,
                    Tasa = Tasa,
                    NoDocumentos = item.NDocumentos,
                    DescuentoPP = item.DescuentoPP,
                    Descuentos = item.Descuentos,
                    Estatus = item.EstatusFactoraje,
                    EstatusNombre = s,
                    Selected = false,
					Monto = item.MontoOriginal,
					Total = item.MontoAFacturar,
					Fecha = item.FechaSolicitud,
					IdProveedor = item.IdProveedor,
                    DiasPP = item.DiasPP,
                    NumeroGenerado = item.NumeroGenerado.ToString(),
                    Sociedad = item.Sociedad
                };
                result.Add(element);
            }
            return result;
        }
        
        public solicitudesfactoraje GetSolicitudById(int id)
        {
            var result = _db.solicitudesfactoraje.Where(s => s.idSolicitudesFactoraje == id).FirstOrDefault();
            return result;
        }       

        public List<solicitudesfactoraje> GetSolicitudesByEstatus(int id)
        {
            var result = _db.solicitudesfactoraje.Where(s => s.EstatusFactoraje == id).ToList();
            return result;
        }

        public int InsSolicitud(solicitudesfactoraje model)
		{
			var result = _db.solicitudesfactoraje.Add(model);
			_db.SaveChanges();

			return model.idSolicitudesFactoraje;
		}

        public int UpdateSolicitud(solicitudesfactoraje model)
        {
            var result = GetSolicitudById(model.idSolicitudesFactoraje);
            if(result != null)
            {
                result.DescuentoPP = model.DescuentoPP;
                result.Descuentos = model.Descuentos;
                result.MontoAFacturar = model.MontoAFacturar;
                result.MontoOriginal = model.MontoOriginal;

                _db.Entry(result).State = EntityState.Modified;
                _db.SaveChanges();
                return result.idSolicitudesFactoraje;
            }
            else
            {
                return 0;
            }            
        }

        public bool UpdateEstatusSolicitud(int solicitudId, int estatus)
        {
            bool result = false;
            try
            {
                var solicitudfactoraje = _db.solicitudesfactoraje.Where(sf => sf.idSolicitudesFactoraje == solicitudId).FirstOrDefault();
                if (solicitudfactoraje != null)
                {
                    int solEstatus = solicitudfactoraje.EstatusFactoraje;
                    solicitudfactoraje.EstatusFactoraje = estatus;
                    _db.Entry(solicitudfactoraje).State = EntityState.Modified;
                    _db.SaveChanges();

                    var descuentos = _db.descuentosfactoraje.Where(df => df.idSolicitudesFactoraje == solicitudfactoraje.idSolicitudesFactoraje && df.EstatusFactoraje == solEstatus).ToList();
                    foreach(var item in descuentos)
                    {
                        item.EstatusFactoraje = estatus;
                        _db.Entry(item).State = EntityState.Modified;
                        _db.SaveChanges();
                    }

                    var facturas = _db.facturasfactoraje.Where(ff => ff.idSolicitudesFactoraje == solicitudfactoraje.idSolicitudesFactoraje && ff.FacturaEstatus == solEstatus).ToList();
                    foreach (var item in facturas)
                    {
                        item.FacturaEstatus = estatus;
                        _db.Entry(item).State = EntityState.Modified;
                        _db.SaveChanges();
                    }

                    
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return result;
        }

    }

    public class localsolicitud
    {
        public int Id { get; set; }
        public string Numero { get; set; }
        public string Proveedor { get; set; }
		public int IdProveedor { get; set; }
        public float Tasa { get; set; }
        public int NoDocumentos { get; set; }
        public double DescuentoPP { get; set; }
        public double Descuentos { get; set; }
        public int Estatus { get; set; }
        public string EstatusNombre { get; set; }
        public bool Selected { get; set; }
		public double Monto { get; set; }
		public DateTime Fecha { get; set; }
		public double Total { get; set; }
        public int DiasPP { get; set; }
        public string NumeroGenerado { get; set; }
        public string Sociedad { get; set; }
    }
}
