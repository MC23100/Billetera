#nullable disable
using System;

namespace Billetera
{
    /// <summary>Todos los cálculos matemáticos de la app, en un solo lugar.</summary>
    public static class Calculos
    {
        // Flujo neto = ingresos - gastos
        public static decimal FlujoNeto(decimal ingresos, decimal gastos)
        {
            return ingresos - gastos;
        }

        // Porcentaje = parte / total * 100  (redondeado a entero)
        public static decimal Porcentaje(decimal parte, decimal total)
        {
            if (total <= 0) return 0;
            return Math.Round(parte / total * 100m, 0, MidpointRounding.AwayFromZero);
        }

        // Lo que queda de un presupuesto
        public static decimal Restante(decimal limite, decimal gastado)
        {
            return limite - gastado;
        }

        // Estado de un presupuesto según lo gastado
        public static string Estado(decimal gastado, decimal limite)
        {
            if (gastado > limite) return "Excedido";
            if (gastado >= limite * 0.9m) return "Cerca del límite";
            return "En ruta";
        }

        public static int DiasTranscurridos(DateTime hoy)
        {
            return hoy.Day;
        }

        // Promedio diario = gastado / días transcurridos
        public static decimal PromedioDiario(decimal gastado, int dias)
        {
            if (dias <= 0) return 0;
            return Math.Round(gastado / dias, 2);
        }

        // Proyección de cierre de mes = promedio diario * días del mes
        public static decimal ProyeccionMes(decimal gastado, int diasTranscurridos, int diasDelMes)
        {
            return Math.Round(PromedioDiario(gastado, diasTranscurridos) * diasDelMes, 2);
        }

        // Cambio porcentual = (actual - anterior) / |anterior| * 100  (null si no hay dato anterior)
        public static decimal? Variacion(decimal actual, decimal anterior)
        {
            if (anterior == 0) return null;
            return Math.Round((actual - anterior) / Math.Abs(anterior) * 100m, 1);
        }

        public static int DiasRestantesMes(DateTime hoy)
        {
            return DateTime.DaysInMonth(hoy.Year, hoy.Month) - hoy.Day;
        }

        public static int SemanasRestantes(DateTime hoy)
        {
            int dias = DiasRestantesMes(hoy);
            return Math.Max(1, (int)Math.Ceiling(dias / 7.0));
        }

        // Meses completos entre dos fechas (mínimo 1 si la fecha límite es futura)
        public static int MesesHasta(DateTime desde, DateTime hasta)
        {
            int meses = (hasta.Year - desde.Year) * 12 + hasta.Month - desde.Month;
            if (hasta.Day < desde.Day) meses--;
            if (meses < 1 && hasta.Date > desde.Date) meses = 1;
            return Math.Max(0, meses);
        }

        // Cuánto hay que ahorrar al mes para llegar a la meta a tiempo
        public static decimal CuotaMensual(decimal falta, int meses)
        {
            if (meses <= 0) return falta;
            return Math.Ceiling(falta / meses);
        }

        // En cuántos meses se llega a la meta con un ahorro mensual dado
        public static int MesesParaMeta(decimal falta, decimal ahorroMensual)
        {
            if (ahorroMensual <= 0) return 0;
            return (int)Math.Ceiling(falta / ahorroMensual);
        }
    }
}
