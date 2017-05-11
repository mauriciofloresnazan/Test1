using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        

        /// <summary>
        /// Basado en las reglas de negocio calcula las fechas permitidas para una orden de compra
        /// este componente sera consumido por el modulo de citas para mostrar las fechas disponibles 
        /// para las ordenes mejorando la usabilidad
        /// </summary>
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

        /// <summary>
        /// Basado en las Reglas:
        /// #7.	Cada espacio de tiempo en el riel es equivalente a una hora y una capacidad de 600 pares por hora.
        /// #8.	La tolerancia en márgenes de pares por hora se maneja en base a “0.16” de diferencia y se 
        /// calculara con la siguiente formula:(Nº de pares / 600)= Cantidad de espacios necesarios
        /// #9.	Si el decimal de “Cantidad de espacios necesarios” es menor a 0.17 se redondea hacia abajo, 
        /// si es igual o mayor se redondea hacia arriba.
        ///  #10.	El monto total de pares a entregar en la cita se divide entre todos los 
        /// espacios  de tiempo disponibles, por lo cual el margen de tolerancia por hora en la cantidad de 
        /// pares a recibir, también se divide entre todos los espacios ocupados por la cita. Ejemplo: 
        /// Un proveedor tiene cita para 3300 pares, según el cálculo designado a asignación de rieles 
        /// necesitaría 5.5 Rieles y la regla de negocio Nº 8 indica que si el decimal es menor a 0.17 
        /// se redondea hacia abajo, necesitando para esa cita 6 Rieles, cada uno de esos rieles 
        /// procesara 550 pares por hora para un total de 3300.
        /// </summary>
        public static int GetCantidadRieles(int totalPares)
        {
            var paresPorHora = Convert.ToInt32(GetConfiguraciones()
                .Single(c => c.Clave == "warehouse.platform-rail.max-pair.hour").Valor);
            var tolerancia = Convert.ToDecimal(GetConfiguraciones()
                .Single(c => c.Clave == "warehouse.platform-rail.max-pair-hour.tolerance").Valor, CultureInfo.InvariantCulture);


            if (totalPares <= paresPorHora)
            {
                return 1;
            }

            var resultado = (totalPares / Convert.ToDecimal(paresPorHora));

            var rielesRequeridos = decimal.ToInt32((resultado % 1) > tolerancia ? Math.Ceiling(resultado) : Math.Floor(resultado));

            return rielesRequeridos;
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
            // Validado en la regla 12 
            return true;
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


            var semanaCita = CultureInfo
                .GetCultureInfo(EspMexicoCultureName)
                .Calendar
                .GetWeekOfYear(fechaCita, CalendarWeekRule.FirstDay, fechaCita.DayOfWeek);

            var semanaOrden = CultureInfo
                .GetCultureInfo(EspMexicoCultureName)
                .Calendar
                .GetWeekOfYear(fechaEntregaOrden, CalendarWeekRule.FirstDay, fechaEntregaOrden.DayOfWeek);

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



            var semanaCita = CultureInfo
               .GetCultureInfo(EspMexicoCultureName)
               .Calendar
               .GetWeekOfYear(fechaCita, CalendarWeekRule.FirstDay, fechaCita.DayOfWeek);

            var semanaEnCurso = CultureInfo
                .GetCultureInfo(EspMexicoCultureName)
                .Calendar
                .GetWeekOfYear(DateTime.Today, CalendarWeekRule.FirstDay, DateTime.Today.DayOfWeek);


            // TODO pasar a tabla de configuraciones
            return semanaCita >= semanaEnCurso && semanaCita <= semanaEnCurso + 2;
        }

        /// <summary>
        /// 5.	El proveedor solo tiene las horas comprendidas de 7:am a 8pm de lunes a viernes para seleccionar 
        /// el horario de entrega
        /// </summary>
        public static bool Regla5()
        {
            // La configuracion de los rieles contempla la validaión de la cita.
           return true;
        }

        /// <summary>
        /// /6.	La totalidad de la entrega debe hacerse el mismo día, no podrá seleccionarse en la misma cita una entrega a las 8pm y una a las 7am del siguiente día, por ejemplo. 
        /// </summary>
        public static bool Regla6()
        {
            // El modelo de datos contempla una fecha unica por cita.
            return true;
        }

        /// <summary>
        /// 7.	Cada espacio de tiempo en el riel es equivalente a una hora y una capacidad de 600 pares por hora.
        /// </summary>
        public static bool Regla7()
        {
            // Valida de acuerdo a la siguiente regla y la estructura de datos
            return true;
        }
        
        /// <summary>
        /// 8.	La tolerancia en márgenes de pares por hora se maneja en base a “0.16” de diferencia y se calculara con la siguiente formula: 
        /// (Nº de pares / 600)= Cantidad de espacios necesarios
        /// </summary>
        public static bool Regla8(int cantidadPares, int cantidadRieles)
        {
            var result =  GetCantidadRieles(cantidadPares);

            return result == cantidadRieles;
        }

        /// <summary>
        /// 9.	Si el decimal de “Cantidad de espacios necesarios” es menor a 0.17 se redondea hacia abajo, si es igual o mayor se redondea hacia arriba.
        /// </summary>
        public static bool Regla9()
        {
            // Validado con la regla 8
            return true;
        }

        /// <summary>
        /// 10.	El monto total de pares a entregar en la cita se divide entre todos los espacios de tiempo disponibles, por lo cual el margen de tolerancia por hora 
        /// en la cantidad de pares a recibir, también se divide entre todos los espacios ocupados por la cita. Ejemplo:
        /// Un proveedor tiene cita para 3300 pares, según el cálculo designado a asignación de rieles necesitaría 5.5 Rieles y la regla de negocio Nº 8 indica que 
        /// si el decimal es menor a 0.17 se redondea hacia abajo, necesitando para esa cita 6 Rieles, cada uno de esos rieles procesara 550 pares por hora para un 
        /// total de 3300.
        /// </summary>
        public static bool Regla10()
        {
            // Validado con la regla 8
            return true;
        }

        /// <summary>
        /// 11.	Un riel, al tener una cita comprometida ya no se mostrará como disponible para otro proveedor aun cuando los pares a recibir sean menores a su 
        /// capacidad (600).
        /// </summary>
        public static bool Regla11()
        {
            // Validado con la estructura de datos
            return true;
        }


        /// <summary>
        /// 12.	En el caso de las entregas parciales, para garantizar que el proveedor no entregue más de lo requerido en la orden de compra, 
        /// correspondiente a una entrega posterior a la primera, se consultará la orden de compra para verificar las parcialidades entregadas.
        /// </summary>
        public static bool Regla12(int cantidad, int cantidadPedido, int cantidadEntregado, int cantidadCitasFuturas)
        {
            return cantidad <= (cantidadPedido - (cantidadEntregado + cantidadCitasFuturas));
        }

        /// <summary>
        /// 13.	Se establece un límite configurable de capacidad (actualmente 27000) diaria de pares a las 
        /// entregas destinadas a los almacenes CD01 y CD06, 
        /// el monto de cada almacén no puede exceder dicha cantidad
        /// </summary>
        /// <param name="almacen"></param>
        /// <param name="cantidadAcumulada">Cantidad en citas agendadas para el dia</param>
        /// <param name="cantidad">Cantidad a incluir</param>
        /// <returns></returns>
        public static bool Regla13(string almacen, int cantidadAcumulada, int cantidad)
        {
            var cantidadDiariaLimite = Convert.ToInt32(GetConfiguraciones()
                .Single(c => c.Clave == "warehouse.max-pairs.per-day").Valor); 
            var almacenes = GetConfiguraciones()
                .Single(c => c.Clave == "warehouse.limited-warehouses-per-day").Valor.Split(',');

            if (!almacenes.Contains(almacen))
            {
                return true;
            }

            return (cantidadAcumulada + cantidad) <= cantidadDiariaLimite;
        }
        /// <summary>
        /// / 14.	Establecer un límite semanal configurable a los almacenes CD06 (actualmente 90000) 
        /// y CD01, el cual debe ser administrador a través del portal
        /// </summary>
        /// <param name="almacen"></param>
        /// <param name="cantidadAcumulada">Cantidad en citas agendadas para la semana</param>
        /// <param name="cantidad">Cantidad a incluir</param>
        /// <returns></returns>
        public static bool Regla14(string almacen, int cantidadAcumulada, int cantidad)
        {
            var cantidadDiariaLimite = Convert.ToInt32(GetConfiguraciones()
                .Single(c => c.Clave == "warehouse.max-pairs.per-week").Valor); ;
            var almacenes = GetConfiguraciones()
                .Single(c => c.Clave == "warehouse.limited-warehouses-per-week").Valor.Split(',');

            if (!almacenes.Contains(almacen))
            {
                return true;
            }

            return (cantidadAcumulada + cantidad) <= cantidadDiariaLimite; 
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
        /// 16.	Para facilitar la logística en el CEDIS y para una fácil identificación de la cita en la 
        /// pantalla administrativa de  las citas del día, se restringe al proveedor a seleccionar 
        /// múltiples órdenes de compra destinadas a un mismo almacén. 
        /// De necesitar entregar el mismo día órdenes de compra destinadas a distintos almacenes, el proveedor 
        /// deberá generar una  cita por cada almacén destino.
        /// </summary>
        public static bool ValidarRegla16(string[] almacenesPorItem)
        {
            var almacen1 = almacenesPorItem[0];

            return almacenesPorItem.All(almacen => almacen == almacen1);
        }

        /// <summary>
        /// 20.	Los días disponibles para agendar cita son los siguientes: Lunes, Martes, Miércoles, Jueves y Viernes.
        /// Pudiendo activar el día Sábado según haga falta.
        /// </summary>
        public static bool Regla20(DateTime fechaCita)
        {
            return GetDiasOperativos().Contains(fechaCita.DayOfWeek);
        }

        /// <summary>
        /// •	El Proveedor tiene hasta las 12pm del día anterior a la cita para cancelarla.
        /// </summary>
        /// <returns></returns>
        public static bool PuedeCancelarCita(DateTime fechaCita)
        {
            if (DateTime.Today.Date >= fechaCita.Date)
            {
                return false;
            }

            if (DateTime.Today.AddDays(1).Date == fechaCita.Date)
            {
                if (DateTime.Now.Hour > 12)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool PuedeEditarCita(DateTime fechaCita)
        {
            if (DateTime.Today.Date >= fechaCita.Date)
            {
                return false;
            }

            if (DateTime.Today.AddDays(1).Date == fechaCita.Date)
            {
                if (DateTime.Now.Hour > 17)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
