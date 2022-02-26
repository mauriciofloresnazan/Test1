using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Ppgz.Repository;
using SapWrapper;


namespace Ppgz.Services
{
    public class ProveedorManager
    {
        private readonly Entities _db = new Entities();

        public List<proveedore> FindByCuentaId(int cuentaId)
        {
            var proveedores = _db.proveedores.Where(p => p.CuentaId == cuentaId).ToList();

            foreach (proveedore proveedor in proveedores)
            {
                proveedor.Sociedades = JsonConvert.DeserializeObject<SociedadesProv[]>(proveedor.Sociedad);
            }

            return proveedores;
        }

        public proveedore Find(int id)
        {
            return _db.proveedores.Find(id);
        }

        public proveedore Find(int id, int cuentaId)
        {
            return _db.proveedores.FirstOrDefault(p=> p.Id == id && p.CuentaId == cuentaId);
        }

        public proveedore FindByNumeroProveedor(string numeroProveedor)
        {
            return _db.proveedores.FirstOrDefault(p => p.NumeroProveedor == numeroProveedor);
        }

        public proveedore FindProveedorEnSap(string numeroProveedor)
        {

            var sapproveedorManager = new SapProveedorManager();
            var result = sapproveedorManager.GetProveedor(numeroProveedor);
            //var sociedades = "{\"Sociedades\":[1001,2000,2323]}";
            //dynamic sociedadesObj = new ExpandoObject();
            //SortedDictionary<string, string, string> sociedadesObj =  new SortedDictionary<string, bool>();


            var db = new Entities();
            var _configuraciones = db.configuraciones.ToList();

            ArrayList sociedades = new ArrayList();
            if (result == null)
            {
                throw new BusinessException(CommonMensajesResource.ERRO_Sap_ProveedorCodigo);
            }
            else
            {
                
                foreach (DataRow dtRow in result.Rows){
                    SociedadesProv sociedad = new SociedadesProv();
                    sociedad.Activa = false;
                    try {
                        sociedad.Nombre = _configuraciones.Single(c => c.Clave == "sociedades.nombre." + dtRow["BUKRS"].ToString()).Valor;
                    }
                    catch
                    {
                        sociedad.Nombre = "Nombre no configurado";
                    }
                    
                    //sociedad.Nombre = "Nazan";
                    sociedad.Sociedad = dtRow["BUKRS"].ToString();
                    sociedades.Add(sociedad);
                    //sociedadesObj[dtRow["BUKRS"].ToString()] = false;
                    // Add some elements to the dictionary. 
                    //sociedadesObj.Add(dtRow["BUKRS"].ToString(), false);
                }

            }


            // Serializing object to json data  
            //JavaScriptSerializer js = new JavaScriptSerializer();
            //string jsonData = js.Serialize(sociedades);
            string jsonData = JsonConvert.SerializeObject(sociedades);




            var proveedor = new proveedore
            {
                NumeroProveedor = result.Rows[0]["LIFNR"].ToString(),
                ClavePais = result.Rows[0]["LAND1"].ToString(),
                Nombre1 = result.Rows[0]["NAME1"].ToString(),
                Nombre2 = result.Rows[0]["NAME2"].ToString(),
                Nombre3 = result.Rows[0]["NAME3"].ToString(),
                Nombre4 = result.Rows[0]["NAME4"].ToString(),
                Poblacion = result.Rows[0]["ORT01"].ToString(),
                Distrito = result.Rows[0]["ORT02"].ToString(),
                Apartado = result.Rows[0]["PFACH"].ToString(),
                CodigoApartado = result.Rows[0]["PSTL2"].ToString(),
                CodigoPostal = result.Rows[0]["PSTLZ"].ToString(),
                Region = result.Rows[0]["REGIO"].ToString(),
                Calle = result.Rows[0]["STRAS"].ToString(),
                Direccion = result.Rows[0]["ADRNR"].ToString(),
                //Sociedad = result.Rows[0]["BUKRS"].ToString(),
                Sociedad = jsonData, 
                OrganizacionCompra = result.Rows[0]["EKORG"].ToString(),
                ClaveMoned = result.Rows[0]["WAERS"].ToString(),
                VendedorResponsable = result.Rows[0]["VERKF"].ToString(),
                NumeroTelefono = result.Rows[0]["TELF1"].ToString(),
                CondicionPago = result.Rows[0]["ZTERM"].ToString(),
                IncoTerminos1 = result.Rows[0]["INCO1"].ToString(),
                IncoTerminos2 = result.Rows[0]["INCO2"].ToString(),
                GrupoCompras = result.Rows[0]["EKGRP"].ToString(),
                DenominacionGrupo = result.Rows[0]["EKNAM"].ToString(),
                TelefonoGrupoCompra = result.Rows[0]["EKTEL"].ToString(),
                TelefonoPrefijo = result.Rows[0]["TEL_NUMER"].ToString(),
                TelefonoExtension = result.Rows[0]["TEL_EXTENS"].ToString(),
                Correo = result.Rows[0]["SMTP_ADDR"].ToString(),
                Rfc = result.Rows[0]["STCD1"].ToString(),
                EstadoNombre = result.Rows[0]["BEZEI"].ToString()
            };

            return proveedor;
      



        }

        public void Eliminar(int id)
        {
            var proveedor = Find(id);

            if (proveedor == null)
            {
                throw new BusinessException(CommonMensajesResource.ERROR_Proveedor_Id);
            }
            
            foreach (var cita in proveedor.citas)
            {
                cita.asns.ToList().ForEach(asn => _db.asns.Remove(asn));


                foreach (var horarioRiel in cita.horariorieles.ToList())
                {
                    horarioRiel.CitaId = null;
                    horarioRiel.Disponibilidad = true;
                    _db.Entry(horarioRiel).State = EntityState.Modified;
                }

                cita.crs.ToList().ForEach(cr => _db.crs.Remove(cr));
   
            }
           
            proveedor.citas.ToList().ForEach(c => _db.citas.Remove(c));
            proveedor.facturas.ToList().ForEach(f => _db.facturas.Remove(f));
            proveedor.ordencompras.ToList().ForEach(oc => _db.ordencompras.Remove(oc));
            proveedor.niveleseervicios.ToList().ForEach(ns => _db.niveleseervicios.Remove(ns));
            //proveedor.etiquetas.ToList().ForEach(e => _db.etiquetas.Remove(e));

            _db.Entry(proveedor).State = EntityState.Deleted;
            _db.SaveChanges();
        }
        
    }
}