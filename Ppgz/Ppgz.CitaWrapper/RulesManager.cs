using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using Ppgz.Repository;

namespace Ppgz.CitaWrapper
{
    public static class RulesManager
    {


        // TODO MEJORAR
        private static List<configuracione> _configuraciones;
        
        // TODO MEJORAR
        private static List<configuracione> GetConfiguraciones()
        {
            if (_configuraciones != null) return _configuraciones;
            var db = new Repository.Entities();
            _configuraciones = db.configuraciones.ToList();

            return _configuraciones;
        }
        
        internal static List<DayOfWeek> GetDiasOperativos()
        {

            var dias = GetConfiguraciones().Single(c => c.Clave == "warehouse.working-day.enabled").Valor.Split(',');

            var result = new List<DayOfWeek>();
            foreach (var dia in dias)
            {
                if (dia == "1")
                    result.Add(DayOfWeek.Sunday);
                if (dia == "2")
                    result.Add(DayOfWeek.Monday);
                if (dia == "3")
                    result.Add(DayOfWeek.Tuesday);
                if (dia == "4")
                    result.Add(DayOfWeek.Wednesday);
                if (dia == "5")
                    result.Add(DayOfWeek.Thursday);
                if (dia == "6")
                    result.Add(DayOfWeek.Friday);
                if (dia == "7")
                    result.Add(DayOfWeek.Saturday);
            }
            return result;
        }
        internal static List<DayOfWeek> GetDiasEspeciales()
        {

            var dias = GetConfiguraciones().Single(c => c.Clave == "warehouse.special-day.provider").Valor.Split(',');

            var result = new List<DayOfWeek>();
            foreach (var dia in dias)
            {
                if (dia == "1")
                    result.Add(DayOfWeek.Sunday);
                if (dia == "2")
                    result.Add(DayOfWeek.Monday);
                if (dia == "3")
                    result.Add(DayOfWeek.Tuesday);
                if (dia == "4")
                    result.Add(DayOfWeek.Wednesday);
                if (dia == "5")
                    result.Add(DayOfWeek.Thursday);
                if (dia == "6")
                    result.Add(DayOfWeek.Friday);
                if (dia == "7")
                    result.Add(DayOfWeek.Saturday);
            }
            return result;
        }
        
        public static List<DateTime> GetFechasPermitidas(DateTime sapFechaEntrega, bool esProveedorEspecial)
        {
            // Limpieza de fecha
            sapFechaEntrega = sapFechaEntrega.Date;
         
            var fechasPermitidas = new List<DateTime>();

            // TODO mejorar El proveedor solo puede agendar con un máximo tiempo que comprende la semana en curso más 2 semanas
            if (sapFechaEntrega > DateTime.Today.AddDays(30))
            {
                return fechasPermitidas;
            }

            // TODO MEJORAR obtener 2 semanas atras el dia en curso.
            var dia = sapFechaEntrega.AddDays(-30);

            while (dia < sapFechaEntrega.AddDays(30))
            {
                var fechaValida = ValidarReglasGenerales(dia);

                if (!Regla2(dia))
                    fechaValida = false;

                if (!Regla3(dia, sapFechaEntrega))
                    fechaValida = false;

                if (!Regla4(dia))
                    fechaValida = false;

                if (!Regla15(dia, esProveedorEspecial))
                    fechaValida = false;

                if (!Regla20(dia))
                    fechaValida = false;

                if (fechaValida)
                    fechasPermitidas.Add(dia);

                dia = dia.AddDays(1);

            }


            return fechasPermitidas;


            /*var fechasPermitidas = new List<DateTime>();

            // Dias de semana permitidos de acuerdo a a las reglas del negocio 
            var diasPermitidos = GetDiasDeSemana();


            // Se calcula el numero de la semana del año
            var semana = System.Globalization.CultureInfo
                .GetCultureInfo("es-MX")
                .Calendar
                .GetWeekOfYear(sapFechaEntrega, System.Globalization.CalendarWeekRule.FirstDay, sapFechaEntrega.DayOfWeek);

            // TODO MEJORAR
            // Se agregan los días del rango entre 2 semanas antes y 2 semanas despues de la fecha de entrega
            var day = sapFechaEntrega.AddDays(-30);

            while (day < sapFechaEntrega.AddDays(30))
            {
                if (diasPermitidos.Contains(day.DayOfWeek))
                {

                    var semana2 = System.Globalization.CultureInfo
                        .GetCultureInfo("es-MX")
                        .Calendar
                        .GetWeekOfYear(day, System.Globalization.CalendarWeekRule.FirstDay, day.DayOfWeek);

                    if (semana2 >= semana - 2 && semana2 <= semana + 2)
                    {

                        if (EsFechaValida(day, esProveedorEspecial))
                        {
                            fechasPermitidas.Add(day);
                        }

                    }
                }
                day = day.AddDays(1);
            }


            return fechasPermitidas;*/
        }

        private static bool EsFechaValida(DateTime dateTime, bool esProveedorEspecial)
        {
            var fecha = dateTime.Date;

            var manana = DateTime.Today.Date.AddDays(1);
            // Las fechas deben ser superiores al dia en curso
            if (fecha < manana)
            {
                return false;
            }

            // Solo tien hasta las 5pm del día en curso se poder incluir el día siguiente 
            // TODO pasar al la tabla de configuraciones
            if (fecha == manana)
            {
                if (DateTime.Now.Hour > 17)
                {
                    return false;
                }
            }

            var diaDeSemana = fecha.DayOfWeek.ToString();
            if (!esProveedorEspecial)
            {
                // TODO OBTENER DE CONFIGURACIONES
                var diasEspeciales = new[]
                {
                    "",
                };
                if (diasEspeciales.Contains(diaDeSemana))
                {
                    return false;
                }
            }

            // TODO INCLUIR SOLO LOS DIAS CONFIGURADOS PARA OPERAR
            return true;
        }















        /// <summary>
        /// Nombre del  ISO 639 del español en México
        /// </summary>
        private const string EspMexicoCultureName = "es-MX";

        /// <summary>
        /// Estas reglas no estan en el caso de uso pero al momento del desarrollo fue necesario incluirlas
        /// </summary>
        public static bool ValidarReglasGenerales(DateTime fechaCita)
        {
            // Limpieza de fecha
            var fecha = fechaCita.Date;
            var manana = DateTime.Today.Date.AddDays(1);

            if (fecha < manana)
            {
                return false;
            }
            return true;
        }


        /// <summary>
        /// 1.	El proveedor puede modificar el número de pares a entregar por talla solo hacia abajo, 
        /// no se permitirá incluir un número mayor de pares que el definido en la orden de compra
        /// </summary>
        public static bool Regla1()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 2.	El proveedor tiene hasta las 5pm del día en curso para poder seleccionar cita para el día siguiente
        /// </summary>
        public static bool Regla2(DateTime fechaCita)
        {
            // Limpieza de fecha
            fechaCita = fechaCita.Date;

            var manana = DateTime.Today.Date.AddDays(1);

            if (fechaCita != manana) return true;

            // TODO pasar la hora limite al la tabla de configuraciones
            return DateTime.Now.Hour <= 17;
        }

        /// <summary>
        /// 3.	La cita solo puede ser agendada si la fecha de entrega de la Orden de Compra está dentro de un rango 
        /// de 2 semanas antes o 2 semanas después de la fecha seleccionada
        /// </summary>
        public static bool Regla3(DateTime fechaCita, DateTime fechaEntregaOrden)
        {
            // Limpieza de fecha 
            fechaCita = fechaCita.Date;
            fechaEntregaOrden = fechaEntregaOrden.Date;


            //TODO CALCULAR CUANDO ESTOY EN LA ULTIMA DESMANA DEL AÑO


            var semanaCita = System.Globalization.CultureInfo
                .GetCultureInfo(EspMexicoCultureName)
                .Calendar
                .GetWeekOfYear(fechaCita, System.Globalization.CalendarWeekRule.FirstDay, fechaCita.DayOfWeek);

            var semanaOrden = System.Globalization.CultureInfo
                .GetCultureInfo(EspMexicoCultureName)
                .Calendar
                .GetWeekOfYear(fechaEntregaOrden, System.Globalization.CalendarWeekRule.FirstDay, fechaEntregaOrden.DayOfWeek);

            // TODO pasar a tabla de configuraciones
            return semanaCita >= semanaOrden - 2 && semanaCita <= semanaOrden + 2;
        }

        /// <summary>
        /// 4.	El proveedor solo puede agendar con un máximo tiempo que comprende la semana en curso más 2 semanas
        /// </summary>
        public static bool Regla4(DateTime fechaCita)
        {            
            // Limpieza de fecha 
            fechaCita = fechaCita.Date;

            //TODO CALCULAR CUANDO ESTOY EN LA ULTIMA DESMANA DEL AÑO



            var semanaCita = System.Globalization.CultureInfo
               .GetCultureInfo(EspMexicoCultureName)
               .Calendar
               .GetWeekOfYear(fechaCita, System.Globalization.CalendarWeekRule.FirstDay, fechaCita.DayOfWeek);

            var semanaEnCurso = System.Globalization.CultureInfo
                .GetCultureInfo(EspMexicoCultureName)
                .Calendar
                .GetWeekOfYear(DateTime.Today, System.Globalization.CalendarWeekRule.FirstDay, DateTime.Today.DayOfWeek);


            // TODO pasar a tabla de configuraciones
            return semanaCita >= semanaEnCurso && semanaCita <= semanaEnCurso + 2;
        }

        /// <summary>
        /// 5.	El proveedor solo tiene las horas comprendidas de 7:am a 8pm de lunes a viernes para seleccionar 
        /// el horario de entrega
        /// </summary>
        public static bool Regla5()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 15.	Los días miércoles solo pueden hacer cita los proveedores marcados como “proveedores especiales” 
        /// en el área administrativa, los proveedores que no tengan esa propiedad verán el 
        /// miércoles sin disponibilidad
        /// </summary>
        public static bool Regla15(DateTime fechaCita, bool esProveedorEspecial)
        {            
            // Limpieza de fecha 
            fechaCita = fechaCita.Date;

            return !GetDiasEspeciales().Contains(fechaCita.DayOfWeek) || esProveedorEspecial;
        }

        /// <summary>
        /// 16.	Para facilitar la logística en el CEDIS y para una fácil identificación de la cita en la pantalla administrativa de 
        /// las citas del día, se restringe al proveedor a seleccionar múltiples órdenes de compra destinadas a un mismo almacén. 
        /// De necesitar entregar el mismo día órdenes de compra destinadas a distintos almacenes, el proveedor deberá generar una 
        /// cita por cada almacén destino.
        /// </summary>
        public static bool ValidarRegla16()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 20.	Los días disponibles para agendar cita son los siguientes: Lunes, Martes, Miércoles, Jueves y Viernes.
        /// Pudiendo activar el día Sábado según haga falta.
        /// </summary>
        public static bool Regla20(DateTime fechaCita)
        {
            return GetDiasOperativos().Contains(fechaCita.DayOfWeek);
        }
    }
}
