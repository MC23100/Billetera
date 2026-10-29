#nullable disable
using System;

namespace Billetera
{
    public class CategoriaItem
    {
        public int Id;
        public string Nombre;
        public string Tipo;
        public string Color;
        public override string ToString() { return Nombre; }
    }

    public class MovimientoItem
    {
        public int Id;
        public string Tipo;          // "gasto" o "ingreso"
        public decimal Monto;
        public DateTime Fecha;
        public string Nota;
        public string Categoria;
        public string Color;
    }

    public class PresupuestoItem
    {
        public int Id;
        public int IdCategoria;
        public string Categoria;
        public string Color;
        public decimal Limite;
        public decimal Gastado;
    }

    public class MetaItem
    {
        public int Id;
        public string Nombre;
        public decimal Objetivo;
        public decimal Actual;
        public DateTime? FechaLimite;
    }

    public class Resumen
    {
        public decimal Ingresos;
        public decimal Gastos;
        public decimal Neto { get { return Ingresos - Gastos; } }
    }

    public class MesItem
    {
        public DateTime Fecha;
        public override string ToString() { return Tema.NombreMes(Fecha); }
    }

    /// <summary>Datos del usuario que inició sesión.</summary>
    public static class Sesion
    {
        public static int IdUsuario;
        public static string Nombre;
        public static string Correo;
        public static string Moneda;
        public static decimal SaldoInicial;
        public static decimal IngresoMensual;
    }
}
