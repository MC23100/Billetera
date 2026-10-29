#nullable disable
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace Billetera
{
    /// <summary>Colores, fuentes y funciones de apoyo (tomados del diseño de Figma).</summary>
    public static class Tema
    {
        public static readonly Color Fondo = Color.FromArgb(240, 247, 242);
        public static readonly Color Verde = Color.FromArgb(12, 92, 56);
        public static readonly Color VerdeClaro = Color.FromArgb(18, 128, 78);
        public static readonly Color Lima = Color.FromArgb(163, 240, 90);
        public static readonly Color Menta = Color.FromArgb(224, 241, 230);
        public static readonly Color Texto = Color.FromArgb(16, 40, 28);
        public static readonly Color Gris = Color.FromArgb(100, 116, 108);
        public static readonly Color Borde = Color.FromArgb(218, 229, 222);
        public static readonly Color Rojo = Color.FromArgb(214, 69, 69);
        public static readonly Color RojoClaro = Color.FromArgb(253, 232, 232);
        public static readonly Color Ambar = Color.FromArgb(217, 119, 6);
        public static readonly Color AmbarClaro = Color.FromArgb(254, 243, 199);
        public static readonly Color Azul = Color.FromArgb(37, 99, 235);

        public static readonly CultureInfo Us = new CultureInfo("en-US");
        public static readonly CultureInfo Es = new CultureInfo("es-MX");

        public static Font Fuente(float tam, FontStyle estilo = FontStyle.Regular)
        {
            return new Font("Segoe UI", tam, estilo);
        }

        // ---------- Formato de dinero ----------
        /// <summary>$1,245.80 (siempre 2 decimales)</summary>
        public static string Dinero2(decimal v)
        {
            return (v < 0 ? "-$" : "$") + Math.Abs(v).ToString("N2", Us);
        }

        /// <summary>$34,800 (sin decimales si no los tiene)</summary>
        public static string Dinero(decimal v)
        {
            string fmt = (v == Math.Truncate(v)) ? "N0" : "N2";
            return (v < 0 ? "-$" : "$") + Math.Abs(v).ToString(fmt, Us);
        }

        /// <summary>+$13,370 o −$500</summary>
        public static string Signo(decimal v)
        {
            string fmt = (v == Math.Truncate(v)) ? "N0" : "N2";
            return (v >= 0 ? "+$" : "−$") + Math.Abs(v).ToString(fmt, Us);
        }

        public static string TextoVariacion(decimal? v)
        {
            if (!v.HasValue) return "Sin datos del mes anterior";
            return (v.Value >= 0 ? "+" : "−") + Math.Abs(v.Value).ToString("0.#", Us) + "% vs. mes anterior";
        }

        /// <summary>Convierte "1,245.80" o "$1245.8" en decimal. Usa punto como separador decimal.</summary>
        public static bool TryMonto(string texto, out decimal valor)
        {
            valor = 0;
            if (string.IsNullOrWhiteSpace(texto)) return false;
            string t = texto.Replace("$", "").Replace(",", "").Replace(" ", "");
            return decimal.TryParse(t, NumberStyles.Number, CultureInfo.InvariantCulture, out valor);
        }

        // ---------- Textos y fechas ----------
        public static string Capitalizar(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public static string NombreMes(DateTime f)
        {
            return Capitalizar(f.ToString("MMMM yyyy", Es));
        }

        public static string FechaCorta(DateTime f)
        {
            if (f.Date == DateTime.Today) return "Hoy";
            if (f.Date == DateTime.Today.AddDays(-1)) return "Ayer";
            return f.ToString("d MMM", Es);
        }

        public static string Iniciales(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return "?";
            string[] partes = nombre.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string r = partes[0].Substring(0, 1);
            if (partes.Length > 1) r += partes[1].Substring(0, 1);
            return r.ToUpper();
        }

        public static string PrimerNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return "";
            return nombre.Trim().Split(' ')[0];
        }

        public static Color ColorHex(string hex)
        {
            try { return ColorTranslator.FromHtml(hex); }
            catch { return VerdeClaro; }
        }

        public static Color Aclarar(Color c, int n)
        {
            return Color.FromArgb(c.A, Math.Min(255, c.R + n), Math.Min(255, c.G + n), Math.Min(255, c.B + n));
        }

        // ---------- Dibujo ----------
        /// <summary>Color real que hay detrás de un control (así cada control pinta su propio fondo y no se ven "fantasmas").</summary>
        public static Color FondoDe(Control c)
        {
            Control p = c.Parent;
            while (p != null)
            {
                PanelRedondeado tarjeta = p as PanelRedondeado;
                if (tarjeta != null) return Mezclar(FondoDe(tarjeta), tarjeta.Relleno);
                if (p.BackColor.A == 255) return p.BackColor;
                p = p.Parent;
            }
            return Fondo;
        }

        /// <summary>Pone un color (con transparencia) encima de otro y devuelve el color resultante.</summary>
        private static Color Mezclar(Color abajo, Color arriba)
        {
            double a = arriba.A / 255.0;
            return Color.FromArgb(255,
                (int)Math.Round(arriba.R * a + abajo.R * (1 - a)),
                (int)Math.Round(arriba.G * a + abajo.G * (1 - a)),
                (int)Math.Round(arriba.B * a + abajo.B * (1 - a)));
        }

        public static GraphicsPath Redondeado(Rectangle r, int radio)
        {
            int d = Math.Min(radio * 2, Math.Min(r.Width, r.Height));
            if (d < 1) d = 1;
            GraphicsPath p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        // ---------- Fábrica de controles ----------
        public static Label Etiqueta(string texto, float tam, Color color, FontStyle estilo, int x, int y)
        {
            Label l = new Label();
            l.Text = texto;
            l.Font = Fuente(tam, estilo);
            l.ForeColor = color;
            l.BackColor = Color.Transparent;
            l.AutoSize = true;
            l.Location = new Point(x, y);
            return l;
        }

        public static Label EtiquetaCaja(string texto, float tam, Color color, FontStyle estilo, Rectangle r, ContentAlignment alinear)
        {
            Label l = new Label();
            l.Text = texto;
            l.Font = Fuente(tam, estilo);
            l.ForeColor = color;
            l.BackColor = Color.Transparent;
            l.AutoSize = false;
            l.Bounds = r;
            l.TextAlign = alinear;
            return l;
        }

        public static PanelRedondeado Chip(string texto, Color fondo, Color colorTexto, int x, int y)
        {
            Font f = Fuente(8.5f, FontStyle.Bold);
            Size t = TextRenderer.MeasureText(texto, f);
            PanelRedondeado p = new PanelRedondeado();
            p.Relleno = fondo;
            p.ColorBorde = Color.Empty;
            p.Radio = 11;
            p.Bounds = new Rectangle(x, y, t.Width + 18, 24);
            p.Controls.Add(EtiquetaCaja(texto, 8.5f, colorTexto, FontStyle.Bold, new Rectangle(0, 0, p.Width, p.Height), ContentAlignment.MiddleCenter));
            return p;
        }

        public static PanelRedondeado Icono(string letra, Color color, int x, int y, int d)
        {
            PanelRedondeado c = new PanelRedondeado();
            c.Bounds = new Rectangle(x, y, d, d);
            c.Radio = d / 2;
            c.Relleno = Color.FromArgb(45, color);
            c.ColorBorde = Color.Empty;
            c.Controls.Add(EtiquetaCaja(letra, d >= 50 ? 18f : 12f, color, FontStyle.Bold, new Rectangle(0, 0, d, d), ContentAlignment.MiddleCenter));
            return c;
        }

        public static Label Enlace(string texto, Color color, Rectangle r, ContentAlignment alinear, EventHandler alClick)
        {
            Label l = EtiquetaCaja(texto, 9.5f, color, FontStyle.Bold, r, alinear);
            l.Cursor = Cursors.Hand;
            l.Click += alClick;
            return l;
        }

        public static void ConfigurarDialogo(Form f, string titulo, int ancho, int alto)
        {
            f.Text = titulo;
            f.FormBorderStyle = FormBorderStyle.FixedDialog;
            f.MaximizeBox = false;
            f.MinimizeBox = false;
            f.ShowInTaskbar = false;
            f.StartPosition = FormStartPosition.CenterParent;
            f.BackColor = Fondo;
            f.ClientSize = new Size(ancho, alto);
        }

        public static void EstilizarGrid(DataGridView g)
        {
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.AllowUserToResizeRows = false;
            g.ReadOnly = true;
            g.RowHeadersVisible = false;
            g.BackgroundColor = Color.White;
            g.BorderStyle = BorderStyle.None;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.GridColor = Color.FromArgb(232, 240, 235);
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersHeight = 38;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.MultiSelect = false;
            g.RowTemplate.Height = 36;

            DataGridViewCellStyle enc = new DataGridViewCellStyle();
            enc.BackColor = Verde;
            enc.ForeColor = Color.White;
            enc.Font = Fuente(9.5f, FontStyle.Bold);
            enc.SelectionBackColor = Verde;
            enc.SelectionForeColor = Color.White;
            enc.Alignment = DataGridViewContentAlignment.MiddleLeft;
            enc.Padding = new Padding(8, 0, 0, 0);
            g.ColumnHeadersDefaultCellStyle = enc;

            DataGridViewCellStyle fila = new DataGridViewCellStyle();
            fila.BackColor = Color.White;
            fila.ForeColor = Texto;
            fila.Font = Fuente(10f);
            fila.SelectionBackColor = Menta;
            fila.SelectionForeColor = Texto;
            fila.Padding = new Padding(8, 0, 0, 0);
            g.DefaultCellStyle = fila;

            DataGridViewCellStyle alt = new DataGridViewCellStyle();
            alt.BackColor = Color.FromArgb(248, 252, 249);
            g.AlternatingRowsDefaultCellStyle = alt;
        }
    }
}
