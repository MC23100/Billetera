#nullable disable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Billetera
{
    /// <summary>Pantalla 5: perfil del usuario y metas de ahorro.</summary>
    public class VistaMetas : VistaBase
    {
        private static readonly Color[] Colores = { Tema.VerdeClaro, Tema.Azul, Tema.Ambar, Color.FromArgb(147, 51, 234) };

        protected override void Construir()
        {
            DateTime hoy = DateTime.Today;
            DateTime mes = new DateTime(hoy.Year, hoy.Month, 1);
            Resumen act = Datos.ResumenMes(mes);
            decimal ahorroMes = act.Neto;                 // lo que sobra este mes
            List<MetaItem> metas = Datos.Metas();

            Encabezado("Tu cuenta", "Metas y perfil", "+  Nueva meta", NuevaMeta);

            // ----- Perfil -----
            PanelRedondeado perfil = new PanelRedondeado();
            perfil.Bounds = new Rectangle(30, 96, 830, 124);
            perfil.Relleno = Tema.Verde;
            perfil.ColorBorde = Color.Empty;
            perfil.Radio = 24;
            perfil.Circulos = true;

            PanelRedondeado avatar = new PanelRedondeado();
            avatar.Bounds = new Rectangle(24, 24, 76, 76);
            avatar.Radio = 38;
            avatar.Relleno = Tema.Lima;
            avatar.ColorBorde = Color.Empty;
            avatar.Controls.Add(Tema.EtiquetaCaja(Tema.Iniciales(Sesion.Nombre), 20f, Tema.Verde, FontStyle.Regular,
                new Rectangle(0, 0, 76, 76), ContentAlignment.MiddleCenter));
            perfil.Controls.Add(avatar);

            perfil.Controls.Add(Tema.Etiqueta(Sesion.Nombre, 18f, Color.White, FontStyle.Bold, 118, 20));
            perfil.Controls.Add(Tema.Etiqueta(Sesion.Correo, 10f, Color.FromArgb(190, 225, 205), FontStyle.Regular, 120, 54));
            PanelRedondeado c1 = Tema.Chip("Ingreso mensual: " + Tema.Dinero(Sesion.IngresoMensual) + " " + Sesion.Moneda, Color.FromArgb(40, 255, 255, 255), Color.White, 120, 82);
            perfil.Controls.Add(c1);
            PanelRedondeado c2 = Tema.Chip("Saldo inicial: " + Tema.Dinero(Sesion.SaldoInicial), Color.FromArgb(40, 255, 255, 255), Color.White, c1.Right + 8, 82);
            perfil.Controls.Add(c2);

            BotonTema editar = new BotonTema("Editar ingreso");
            editar.Bounds = new Rectangle(830 - 24 - 150, 40, 150, 42);
            editar.ColorFondo = Tema.Lima;
            editar.ColorTexto = Tema.Verde;
            editar.Click += EditarIngreso;
            perfil.Controls.Add(editar);
            Controls.Add(perfil);

            // ----- Título de metas -----
            Label titulo = Tema.Etiqueta("Mis metas", 15f, Tema.Texto, FontStyle.Bold, 30, 236);
            Controls.Add(titulo);
            Controls.Add(Tema.Chip(metas.Count + (metas.Count == 1 ? " activa" : " activas"), Tema.Menta, Tema.VerdeClaro, 30 + titulo.PreferredWidth + 10, 241));

            if (metas.Count == 0)
            {
                PanelRedondeado vacio = new PanelRedondeado();
                vacio.Bounds = new Rectangle(30, 280, 830, 100);
                vacio.Controls.Add(Tema.EtiquetaCaja("Aún no tienes metas. Crea la primera con el botón «Nueva meta».", 10.5f, Tema.Gris,
                    FontStyle.Regular, new Rectangle(0, 0, 830, 100), ContentAlignment.MiddleCenter));
                Controls.Add(vacio);
                return;
            }

            Panel contenedor = new Panel();
            contenedor.Bounds = new Rectangle(30, 280, 830, 452);
            contenedor.AutoScroll = true;
            contenedor.BackColor = Tema.Fondo;
            for (int i = 0; i < metas.Count; i++)
            {
                PanelRedondeado card = TarjetaMeta(metas[i], Colores[i % Colores.Length], hoy, ahorroMes);
                card.Location = new Point((i % 2) * 414, (i / 2) * 192);
                contenedor.Controls.Add(card);
            }
            Controls.Add(contenedor);
        }

        private PanelRedondeado TarjetaMeta(MetaItem m, Color color, DateTime hoy, decimal ahorroMes)
        {
            decimal falta = Math.Max(0m, m.Objetivo - m.Actual);
            decimal pct = Calculos.Porcentaje(m.Actual, m.Objetivo);
            bool lograda = falta == 0;

            PanelRedondeado card = new PanelRedondeado();
            card.Size = new Size(396, 178);
            card.Radio = 16;

            card.Controls.Add(Tema.EtiquetaCaja(m.Nombre, 11.5f, Tema.Texto, FontStyle.Bold, new Rectangle(16, 12, 260, 24), ContentAlignment.MiddleLeft));
            PanelRedondeado chip = lograda
                ? Tema.Chip("¡Lograda!", Tema.Lima, Tema.Verde, 0, 13)
                : Tema.Chip(pct.ToString("0") + "%", Tema.Menta, Tema.VerdeClaro, 0, 13);
            chip.Left = 396 - 16 - chip.Width;
            card.Controls.Add(chip);

            card.Controls.Add(Tema.EtiquetaCaja(Tema.Dinero(m.Actual) + " de " + Tema.Dinero(m.Objetivo), 9.5f, Tema.Gris, FontStyle.Regular,
                new Rectangle(16, 38, 364, 20), ContentAlignment.MiddleLeft));

            BarraProgreso barra = new BarraProgreso();
            barra.Bounds = new Rectangle(16, 64, 364, 10);
            barra.Valor = pct;
            barra.ColorBarra = color;
            card.Controls.Add(barra);

            string l3 = lograda ? "¡Meta cumplida!" : "Faltan " + Tema.Dinero(falta);
            if (m.FechaLimite.HasValue) l3 += "  ·  límite " + m.FechaLimite.Value.ToString("d MMM yyyy", Tema.Es);
            card.Controls.Add(Tema.EtiquetaCaja(l3, 9.5f, Tema.Texto, FontStyle.Bold, new Rectangle(16, 84, 364, 20), ContentAlignment.MiddleLeft));

            string l4 = "";
            if (!lograda)
            {
                if (m.FechaLimite.HasValue)
                {
                    int meses = Calculos.MesesHasta(hoy, m.FechaLimite.Value);
                    if (meses <= 0) l4 = "La fecha límite ya pasó.";
                    else l4 = "Ahorra " + Tema.Dinero(Calculos.CuotaMensual(falta, meses)) + " al mes para llegar a tiempo.";
                }
                if (ahorroMes > 0)
                {
                    int mp = Calculos.MesesParaMeta(falta, ahorroMes);
                    l4 += (l4.Length > 0 ? " " : "") + "Con lo que te sobra este mes (" + Tema.Dinero(ahorroMes) + ") la logras en ~" + mp + (mp == 1 ? " mes." : " meses.");
                }
            }
            card.Controls.Add(Tema.EtiquetaCaja(l4, 9f, Tema.Gris, FontStyle.Regular, new Rectangle(16, 106, 364, 34), ContentAlignment.TopLeft));

            MetaItem meta = m;
            BotonTema aportar = new BotonTema("Aportar");
            aportar.Bounds = new Rectangle(396 - 16 - 84 - 8 - 84, 142, 84, 28);
            aportar.Font = Tema.Fuente(9f, FontStyle.Bold);
            aportar.Radio = 9;
            aportar.Enabled = !lograda;
            aportar.Click += delegate { Aportar(meta); };
            card.Controls.Add(aportar);

            BotonTema eliminar = new BotonTema("Eliminar");
            eliminar.Bounds = new Rectangle(396 - 16 - 84, 142, 84, 28);
            eliminar.Font = Tema.Fuente(9f, FontStyle.Bold);
            eliminar.Radio = 9;
            eliminar.ColorFondo = Tema.RojoClaro;
            eliminar.ColorTexto = Tema.Rojo;
            eliminar.Click += delegate { Eliminar(meta); };
            card.Controls.Add(eliminar);
            return card;
        }

        private void NuevaMeta(object sender, EventArgs e)
        {
            using (FrmNuevaMeta f = new FrmNuevaMeta())
            {
                if (f.ShowDialog(this) == DialogResult.OK) ActualizarLuego();
            }
        }

        private void Aportar(MetaItem m)
        {
            decimal falta = m.Objetivo - m.Actual;
            using (FrmMonto f = new FrmMonto("Aportar a la meta", "¿Cuánto vas a aportar a «" + m.Nombre + "»? (faltan " + Tema.Dinero(falta) + ")", 0m, "Aportar", false, falta))
            {
                if (f.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    Datos.AportarMeta(m.Id, f.Valor);
                    ActualizarLuego();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo guardar el aporte: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void Eliminar(MetaItem m)
        {
            if (MessageBox.Show("¿Eliminar la meta «" + m.Nombre + "»?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            try
            {
                Datos.EliminarMeta(m.Id);
                ActualizarLuego();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo eliminar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditarIngreso(object sender, EventArgs e)
        {
            using (FrmMonto f = new FrmMonto("Ingreso mensual", "Tu ganancia mensual aproximada", Sesion.IngresoMensual, "Guardar", true, 0m))
            {
                if (f.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    Datos.ActualizarIngresoMensual(f.Valor);
                    ActualizarLuego();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo guardar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
