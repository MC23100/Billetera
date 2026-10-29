#nullable disable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Billetera
{
    /// <summary>Pantalla 2: Inicio / "Tu panorama".</summary>
    public class VistaInicio : VistaBase
    {
        protected override void Construir()
        {
            DateTime hoy = DateTime.Today;
            DateTime mes = new DateTime(hoy.Year, hoy.Month, 1);

            Resumen act = Datos.ResumenMes(mes);
            Resumen prev = Datos.ResumenMes(mes.AddMonths(-1));
            decimal saldo = Datos.SaldoAl(null);
            decimal saldoPrev = Datos.SaldoAl(mes);            // saldo al cierre del mes anterior
            decimal? varSaldo = Calculos.Variacion(saldo, saldoPrev);
            List<MovimientoItem> recientes = Datos.Movimientos(null, null, 0, 3);

            int dias = Calculos.DiasTranscurridos(hoy);
            int diasMes = DateTime.DaysInMonth(hoy.Year, hoy.Month);
            decimal promedio = Calculos.PromedioDiario(act.Gastos, dias);
            decimal proyeccion = Calculos.ProyeccionMes(act.Gastos, dias, diasMes);
            decimal ahorro = Calculos.Porcentaje(act.Neto, act.Ingresos);   // % de los ingresos que te sobra

            Encabezado("Hola, " + Tema.PrimerNombre(Sesion.Nombre), "Tu panorama", "+  Nuevo movimiento", NuevoMovimiento);

            // ----- Tarjeta de saldo -----
            PanelRedondeado hero = new PanelRedondeado();
            hero.Bounds = new Rectangle(30, 96, 830, 140);
            hero.Relleno = Tema.Verde;
            hero.ColorBorde = Color.Empty;
            hero.Radio = 24;
            hero.Circulos = true;
            Color claro = Color.FromArgb(190, 225, 205);
            hero.Controls.Add(Tema.Etiqueta("Saldo disponible", 10.5f, claro, FontStyle.Regular, 28, 18));
            hero.Controls.Add(Tema.Etiqueta(Tema.Dinero2(saldo), 32f, Color.White, FontStyle.Bold, 24, 40));
            if (varSaldo.HasValue)
            {
                string flecha = varSaldo.Value >= 0 ? "▲ " : "▼ ";
                PanelRedondeado chip = Tema.Chip(flecha + Tema.TextoVariacion(varSaldo).Replace(" vs. mes anterior", ""), Tema.Lima, Tema.Verde, 28, 102);
                hero.Controls.Add(chip);
                hero.Controls.Add(Tema.Etiqueta("vs. mes anterior", 9.5f, claro, FontStyle.Regular, chip.Right + 10, 105));
            }
            else
            {
                hero.Controls.Add(Tema.Etiqueta("Sin datos del mes anterior", 9.5f, claro, FontStyle.Regular, 28, 105));
            }
            Controls.Add(hero);

            // ----- Ingresos y gastos -----
            Controls.Add(TarjetaTotal("Ingresos", act.Ingresos, Tema.VerdeClaro, Calculos.Variacion(act.Ingresos, prev.Ingresos), true, 30, 252));
            Controls.Add(TarjetaTotal("Gastos", act.Gastos, Tema.Rojo, Calculos.Variacion(act.Gastos, prev.Gastos), false, 455, 252));

            // ----- Flujo neto -----
            PanelRedondeado fl = new PanelRedondeado();
            fl.Bounds = new Rectangle(30, 376, 830, 112);
            fl.Controls.Add(Tema.Etiqueta("Flujo neto", 10f, Tema.Gris, FontStyle.Regular, 24, 14));
            fl.Controls.Add(Tema.Etiqueta(Tema.Signo(act.Neto), 22f, act.Neto >= 0 ? Tema.VerdeClaro : Tema.Rojo, FontStyle.Bold, 22, 34));
            fl.Controls.Add(Tema.Etiqueta("Este mes (ingresos − gastos)", 9f, Tema.Gris, FontStyle.Regular, 24, 80));
            fl.Controls.Add(Tema.Etiqueta("Gasto proyectado del mes", 10f, Tema.Gris, FontStyle.Regular, 300, 14));
            fl.Controls.Add(Tema.Etiqueta(Tema.Dinero(proyeccion), 18f, Tema.Texto, FontStyle.Bold, 298, 36));
            fl.Controls.Add(Tema.Etiqueta("Promedio diario " + Tema.Dinero(promedio) + "  ·  día " + dias + " de " + diasMes, 9f, Tema.Gris, FontStyle.Regular, 300, 80));
            fl.Controls.Add(Tema.Etiqueta("Ahorro del mes", 10f, Tema.Gris, FontStyle.Regular, 590, 14));
            fl.Controls.Add(Tema.Etiqueta(ahorro.ToString("0") + "%", 22f, ahorro >= 0 ? Tema.VerdeClaro : Tema.Rojo, FontStyle.Bold, 588, 34));
            fl.Controls.Add(Tema.Etiqueta("de tus ingresos", 9f, Tema.Gris, FontStyle.Regular, 590, 80));
            Controls.Add(fl);

            // ----- Movimientos recientes -----
            Controls.Add(Tema.Etiqueta("Movimientos recientes", 14f, Tema.Texto, FontStyle.Bold, 30, 506));
            Controls.Add(Tema.Enlace("Ver todos", Tema.VerdeClaro, new Rectangle(740, 510, 120, 22), ContentAlignment.MiddleRight,
                delegate { if (Navegar != null) Navegar(1); }));

            if (recientes.Count == 0)
            {
                PanelRedondeado vacio = new PanelRedondeado();
                vacio.Bounds = new Rectangle(30, 546, 830, 60);
                vacio.Controls.Add(Tema.EtiquetaCaja("Aún no tienes movimientos. Registra el primero con el botón «Nuevo movimiento».", 10f, Tema.Gris,
                    FontStyle.Regular, new Rectangle(0, 0, 830, 60), ContentAlignment.MiddleCenter));
                Controls.Add(vacio);
            }
            for (int i = 0; i < recientes.Count; i++)
            {
                MovimientoItem m = recientes[i];
                int y = 546 + i * 58;
                PanelRedondeado fila = new PanelRedondeado();
                fila.Bounds = new Rectangle(30, y, 830, 52);
                fila.Radio = 14;
                Color col = Tema.ColorHex(m.Color);
                fila.Controls.Add(Tema.Icono(m.Categoria.Substring(0, 1), col, 12, 8, 36));
                string titulo = string.IsNullOrWhiteSpace(m.Nota) ? m.Categoria : m.Nota;
                fila.Controls.Add(Tema.EtiquetaCaja(titulo, 10.5f, Tema.Texto, FontStyle.Bold, new Rectangle(58, 5, 520, 22), ContentAlignment.MiddleLeft));
                fila.Controls.Add(Tema.EtiquetaCaja(m.Categoria + "  ·  " + Tema.FechaCorta(m.Fecha), 9f, Tema.Gris, FontStyle.Regular, new Rectangle(58, 27, 520, 18), ContentAlignment.MiddleLeft));
                bool gasto = m.Tipo == "gasto";
                fila.Controls.Add(Tema.EtiquetaCaja((gasto ? "−" : "+") + Tema.Dinero2(m.Monto), 11f, gasto ? Tema.Texto : Tema.VerdeClaro, FontStyle.Bold,
                    new Rectangle(600, 0, 214, 52), ContentAlignment.MiddleRight));
                Controls.Add(fila);
            }
        }

        private PanelRedondeado TarjetaTotal(string titulo, decimal monto, Color punto, decimal? variacion, bool subirEsBueno, int x, int y)
        {
            PanelRedondeado p = new PanelRedondeado();
            p.Bounds = new Rectangle(x, y, 405, 104);
            PanelRedondeado dot = new PanelRedondeado();
            dot.Bounds = new Rectangle(22, 20, 10, 10);
            dot.Radio = 5;
            dot.Relleno = punto;
            dot.ColorBorde = Color.Empty;
            p.Controls.Add(dot);
            p.Controls.Add(Tema.Etiqueta(titulo, 10f, Tema.Gris, FontStyle.Regular, 38, 13));
            p.Controls.Add(Tema.Etiqueta(Tema.Dinero(monto), 22f, Tema.Texto, FontStyle.Bold, 20, 36));
            Color c = Tema.Gris;
            if (variacion.HasValue)
                c = ((variacion.Value >= 0) == subirEsBueno) ? Tema.VerdeClaro : Tema.Rojo;
            p.Controls.Add(Tema.Etiqueta(Tema.TextoVariacion(variacion), 9.5f, c, FontStyle.Bold, 22, 78));
            return p;
        }

        private void NuevoMovimiento(object sender, EventArgs e)
        {
            using (FrmNuevoMovimiento f = new FrmNuevoMovimiento())
            {
                if (f.ShowDialog(this) == DialogResult.OK) ActualizarLuego();
            }
        }
    }
}
