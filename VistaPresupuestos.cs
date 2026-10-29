#nullable disable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Billetera
{
    /// <summary>Pantalla 4: presupuestos del mes por categoría.</summary>
    public class VistaPresupuestos : VistaBase
    {
        protected override void Construir()
        {
            DateTime hoy = DateTime.Today;
            DateTime mes = new DateTime(hoy.Year, hoy.Month, 1);
            List<PresupuestoItem> items = Datos.Presupuestos(mes);

            Encabezado(Tema.NombreMes(mes), "Presupuestos", "+  Nuevo presupuesto", NuevoPresupuesto);

            if (items.Count == 0)
            {
                PanelRedondeado vacio = new PanelRedondeado();
                vacio.Bounds = new Rectangle(30, 96, 830, 120);
                vacio.Controls.Add(Tema.EtiquetaCaja("Aún no tienes presupuestos este mes.\nCrea uno con el botón «Nuevo presupuesto».", 11f, Tema.Gris,
                    FontStyle.Regular, new Rectangle(0, 0, 830, 120), ContentAlignment.MiddleCenter));
                Controls.Add(vacio);
                return;
            }

            // ----- Totales -----
            decimal limiteTotal = 0, gastadoTotal = 0;
            foreach (PresupuestoItem it in items) { limiteTotal += it.Limite; gastadoTotal += it.Gastado; }
            decimal restante = Calculos.Restante(limiteTotal, gastadoTotal);
            decimal pctTotal = Calculos.Porcentaje(gastadoTotal, limiteTotal);
            string estadoTotal = Calculos.Estado(gastadoTotal, limiteTotal);
            int dias = Calculos.DiasTranscurridos(hoy);
            int diasMes = DateTime.DaysInMonth(hoy.Year, hoy.Month);
            decimal promedio = Calculos.PromedioDiario(gastadoTotal, dias);
            decimal proyeccion = Calculos.ProyeccionMes(gastadoTotal, dias, diasMes);

            PanelRedondeado hero = new PanelRedondeado();
            hero.Bounds = new Rectangle(30, 92, 830, 150);
            hero.Relleno = Tema.Verde;
            hero.ColorBorde = Color.Empty;
            hero.Radio = 24;
            hero.Circulos = true;
            Color claro = Color.FromArgb(190, 225, 205);
            hero.Controls.Add(Tema.Etiqueta("Presupuesto de " + Tema.NombreMes(mes), 10.5f, claro, FontStyle.Regular, 24, 14));
            string textoRest = restante >= 0 ? Tema.Dinero(restante) + " restantes" : Tema.Dinero(-restante) + " excedidos";
            hero.Controls.Add(Tema.Etiqueta(textoRest, 24f, Color.White, FontStyle.Bold, 20, 34));

            Color chipFondo = Tema.Lima, chipTexto = Tema.Verde;
            if (estadoTotal == "Cerca del límite") { chipFondo = Tema.AmbarClaro; chipTexto = Tema.Ambar; }
            if (estadoTotal == "Excedido") { chipFondo = Tema.RojoClaro; chipTexto = Tema.Rojo; }
            PanelRedondeado chip = Tema.Chip(estadoTotal.ToUpper(), chipFondo, chipTexto, 0, 18);
            chip.Left = 830 - 24 - chip.Width;
            hero.Controls.Add(chip);

            BarraProgreso barra = new BarraProgreso();
            barra.Bounds = new Rectangle(24, 88, 782, 12);
            barra.Valor = pctTotal;
            barra.ColorBarra = Tema.Lima;
            barra.ColorPista = Color.FromArgb(70, 255, 255, 255);
            hero.Controls.Add(barra);

            hero.Controls.Add(Tema.Etiqueta(Tema.Dinero(gastadoTotal) + " gastados", 9.5f, claro, FontStyle.Regular, 24, 104));
            hero.Controls.Add(Tema.EtiquetaCaja("de " + Tema.Dinero(limiteTotal), 9.5f, claro, FontStyle.Regular, new Rectangle(606, 104, 200, 20), ContentAlignment.MiddleRight));
            hero.Controls.Add(Tema.Etiqueta("Quedan " + Calculos.DiasRestantesMes(hoy) + " días  ·  promedio diario " + Tema.Dinero(promedio) +
                                            "  ·  proyección de cierre " + Tema.Dinero(proyeccion), 9f, claro, FontStyle.Regular, 24, 126));
            Controls.Add(hero);

            // ----- Consejo -----
            PresupuestoItem peor = null;
            foreach (PresupuestoItem it in items)
            {
                if (it.Gastado > it.Limite && (peor == null || it.Gastado - it.Limite > peor.Gastado - peor.Limite)) peor = it;
            }
            string consejo;
            if (peor != null)
            {
                decimal exceso = peor.Gastado - peor.Limite;
                int semanas = Calculos.SemanasRestantes(hoy);
                consejo = "Superaste " + peor.Categoria + " por " + Tema.Dinero(exceso) + ". Reduce " +
                          Tema.Dinero(Math.Ceiling(exceso / semanas)) + " por semana en otras categorías para compensarlo.";
            }
            else if (pctTotal >= 90)
            {
                consejo = "Vas muy cerca de tu límite total. Cuida los gastos de los próximos " + Calculos.DiasRestantesMes(hoy) + " días.";
            }
            else
            {
                consejo = "¡Vas bien! Tus gastos están dentro del plan este mes.";
            }
            PanelRedondeado tip = new PanelRedondeado();
            tip.Bounds = new Rectangle(30, 254, 830, 46);
            tip.Relleno = Color.FromArgb(236, 250, 214);
            tip.ColorBorde = Color.FromArgb(214, 238, 170);
            tip.Radio = 14;
            tip.Controls.Add(Tema.EtiquetaCaja("Consejo:", 9.5f, Tema.Verde, FontStyle.Bold, new Rectangle(16, 0, 70, 46), ContentAlignment.MiddleLeft));
            tip.Controls.Add(Tema.EtiquetaCaja(consejo, 9.5f, Tema.Texto, FontStyle.Regular, new Rectangle(86, 0, 730, 46), ContentAlignment.MiddleLeft));
            Controls.Add(tip);

            // ----- Tarjetas por categoría -----
            Controls.Add(Tema.Etiqueta("Por categoría", 14f, Tema.Texto, FontStyle.Bold, 30, 312));
            Panel contenedor = new Panel();
            contenedor.Bounds = new Rectangle(30, 348, 830, 384);
            contenedor.AutoScroll = true;
            contenedor.BackColor = Tema.Fondo;
            for (int i = 0; i < items.Count; i++)
            {
                PanelRedondeado card = TarjetaCategoria(items[i]);
                card.Location = new Point((i % 2) * 414, (i / 2) * 96);
                contenedor.Controls.Add(card);
            }
            Controls.Add(contenedor);
        }

        private PanelRedondeado TarjetaCategoria(PresupuestoItem it)
        {
            decimal pct = Calculos.Porcentaje(it.Gastado, it.Limite);
            string estado = Calculos.Estado(it.Gastado, it.Limite);
            Color colorEstado = Tema.VerdeClaro, fondoChip = Tema.Menta;
            if (estado == "Cerca del límite") { colorEstado = Tema.Ambar; fondoChip = Tema.AmbarClaro; }
            if (estado == "Excedido") { colorEstado = Tema.Rojo; fondoChip = Tema.RojoClaro; }

            PanelRedondeado card = new PanelRedondeado();
            card.Size = new Size(396, 86);
            card.Radio = 16;
            if (estado == "Excedido") card.ColorBorde = Color.FromArgb(240, 170, 170);

            card.Controls.Add(Tema.Icono(it.Categoria.Substring(0, 1), Tema.ColorHex(it.Color), 14, 12, 38));
            card.Controls.Add(Tema.EtiquetaCaja(it.Categoria, 10.5f, Tema.Texto, FontStyle.Bold, new Rectangle(62, 10, 190, 22), ContentAlignment.MiddleLeft));
            card.Controls.Add(Tema.EtiquetaCaja(Tema.Dinero(it.Gastado) + " de " + Tema.Dinero(it.Limite), 9f, Tema.Gris, FontStyle.Regular, new Rectangle(62, 32, 200, 18), ContentAlignment.MiddleLeft));

            PanelRedondeado chip = Tema.Chip(pct.ToString("0") + "%", fondoChip, colorEstado, 0, 12);
            chip.Left = 396 - 14 - chip.Width;
            card.Controls.Add(chip);

            PresupuestoItem item = it;
            card.Controls.Add(Tema.Enlace("Editar", Tema.VerdeClaro, new Rectangle(226, 38, 76, 18), ContentAlignment.MiddleRight,
                delegate { Ajustar(item); }));
            card.Controls.Add(Tema.Enlace("Eliminar", Tema.Rojo, new Rectangle(306, 38, 76, 18), ContentAlignment.MiddleRight,
                delegate { Eliminar(item); }));

            BarraProgreso barra = new BarraProgreso();
            barra.Bounds = new Rectangle(14, 64, 368, 10);
            barra.Valor = pct;
            barra.ColorBarra = colorEstado;
            card.Controls.Add(barra);
            return card;
        }

        private void NuevoPresupuesto(object sender, EventArgs e)
        {
            using (FrmNuevoPresupuesto f = new FrmNuevoPresupuesto(null))
            {
                if (f.ShowDialog(this) == DialogResult.OK) ActualizarLuego();
            }
        }

        private void Ajustar(PresupuestoItem it)
        {
            using (FrmNuevoPresupuesto f = new FrmNuevoPresupuesto(it))
            {
                if (f.ShowDialog(this) == DialogResult.OK) ActualizarLuego();
            }
        }

        private void Eliminar(PresupuestoItem it)
        {
            if (MessageBox.Show("¿Eliminar el presupuesto de " + it.Categoria + "?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            try
            {
                Datos.EliminarPresupuesto(it.Id);
                ActualizarLuego();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo eliminar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
