using System;
using System.Collections.Generic;
using System.Linq;

namespace Ppgz.CitaWrapper
{
    public static class RulesManager
    {
        public static List<DateTime> GetFechasPermitidas(DateTime sapFechaEntrega, bool esProveedorEspecial)
        {
            var fechasPermitidas = new List<DateTime>();

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

                day = day.AddDays(1);
            }


            return fechasPermitidas;
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
    }
}
