#nullable disable
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Billetera
{
    /// <summary>Panel con esquinas redondeadas (las "tarjetas" del diseño).</summary>
    public class PanelRedondeado : Panel
    {
        private Color _relleno = Color.White;
        private Color _borde = Tema.Borde;

        public Color Relleno { get { return _relleno; } set { _relleno = value; Invalidate(); } }
        public Color ColorBorde { get { return _borde; } set { _borde = value; Invalidate(); } }
        public int Radio { get; set; }
        public bool Circulos { get; set; }

        public PanelRedondeado()
        {
            Radio = 18;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        // El fondo se pinta dentro de OnPaint (con el color real del padre), así no quedan restos de dibujos anteriores.
        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.Clear(Tema.FondoDe(this));
            if (Width < 4 || Height < 4) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath p = Tema.Redondeado(r, Radio))
            {
                using (SolidBrush b = new SolidBrush(_relleno)) { g.FillPath(b, p); }
                if (Circulos)
                {
                    g.SetClip(p, CombineMode.Intersect);
                    using (SolidBrush c1 = new SolidBrush(Color.FromArgb(38, 255, 255, 255)))
                    { g.FillEllipse(c1, Width - 230, -100, 320, 320); }
                    using (SolidBrush c2 = new SolidBrush(Color.FromArgb(80, 20, 140, 125)))
                    { g.FillEllipse(c2, Width - 360, Height - 55, 130, 130); }
                    g.ResetClip();
                }
                if (!_borde.IsEmpty)
                {
                    using (Pen pen = new Pen(_borde)) { g.DrawPath(pen, p); }
                }
            }
        }
    }

    /// <summary>Botón plano con esquinas redondeadas.</summary>
    public class BotonTema : Button
    {
        private bool _hover;
        public Color ColorFondo { get; set; }
        public Color ColorTexto { get; set; }
        public Color ColorBorde { get; set; }
        public int Radio { get; set; }

        public BotonTema(string texto)
        {
            Text = texto;
            ColorFondo = Tema.Verde;
            ColorTexto = Color.White;
            ColorBorde = Color.Empty;
            Radio = 12;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            Font = Tema.Fuente(10.5f, FontStyle.Bold);
            Size = new Size(180, 44);
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Tema.FondoDe(this));
            if (Width < 4 || Height < 4) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
            Color fondo = ColorFondo;
            if (_hover && fondo.A > 0) fondo = Tema.Aclarar(fondo, 14);
            if (!Enabled) fondo = Color.FromArgb(fondo.A, 190, 200, 195);
            using (GraphicsPath p = Tema.Redondeado(r, Radio))
            {
                if (fondo.A > 0)
                {
                    using (SolidBrush b = new SolidBrush(fondo)) { g.FillPath(b, p); }
                }
                if (!ColorBorde.IsEmpty)
                {
                    using (Pen pen = new Pen(ColorBorde)) { g.DrawPath(pen, p); }
                }
            }
            TextRenderer.DrawText(g, Text, Font, new Rectangle(0, 0, Width, Height), ColorTexto,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }

    /// <summary>Botón del menú lateral.</summary>
    public class BotonNav : Button
    {
        private bool _activo;
        private bool _hover;
        public bool Activo { get { return _activo; } set { _activo = value; Invalidate(); } }

        public BotonNav(string texto)
        {
            Text = texto;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            TabStop = false;
            Font = Tema.Fuente(10.5f, FontStyle.Bold);
            Size = new Size(190, 46);
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Tema.FondoDe(this));
            if (Width < 4 || Height < 4) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
            if (_activo || _hover)
            {
                Color c = _activo ? Tema.Lima : Color.FromArgb(32, 255, 255, 255);
                using (GraphicsPath p = Tema.Redondeado(r, 14))
                using (SolidBrush b = new SolidBrush(c))
                { g.FillPath(b, p); }
            }
            Color texto = _activo ? Tema.Verde : Color.FromArgb(225, 240, 230);
            TextRenderer.DrawText(g, Text, Font, new Rectangle(18, 0, Width - 18, Height), texto,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
    }

    /// <summary>Barra de progreso redondeada. Valor en porcentaje (puede pasar de 100).</summary>
    public class BarraProgreso : Control
    {
        public decimal Valor { get; set; }
        public Color ColorBarra { get; set; }
        public Color ColorPista { get; set; }

        public BarraProgreso()
        {
            ColorBarra = Tema.VerdeClaro;
            ColorPista = Color.FromArgb(222, 232, 226);
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Height = 10;
        }

        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.Clear(Tema.FondoDe(this));
            if (Width < 6 || Height < 4) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int radio = (Height - 1) / 2;
            Rectangle pista = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath p = Tema.Redondeado(pista, radio))
            using (SolidBrush b = new SolidBrush(ColorPista))
            { g.FillPath(b, p); }

            decimal v = Math.Max(0m, Math.Min(100m, Valor));
            int ancho = (int)((Width - 1) * v / 100m);
            if (v > 0 && ancho < Height) ancho = Height;
            if (ancho > 0)
            {
                Rectangle barra = new Rectangle(0, 0, Math.Min(ancho, Width - 1), Height - 1);
                using (GraphicsPath p2 = Tema.Redondeado(barra, radio))
                using (SolidBrush b2 = new SolidBrush(ColorBarra))
                { g.FillPath(b2, p2); }
            }
        }
    }

    /// <summary>Campo de texto con borde redondeado.</summary>
    public class CampoTexto : PanelRedondeado
    {
        public TextBox Caja { get; private set; }

        public CampoTexto(bool clave)
        {
            Radio = 12;
            Relleno = Color.White;
            ColorBorde = Tema.Borde;
            Size = new Size(300, 46);

            Caja = new TextBox();
            Caja.BorderStyle = BorderStyle.None;
            Caja.Font = Tema.Fuente(11f);
            Caja.ForeColor = Tema.Texto;
            Caja.BackColor = Color.White;
            Caja.UseSystemPasswordChar = clave;
            Controls.Add(Caja);
            Caja.Enter += delegate { ColorBorde = Tema.VerdeClaro; };
            Caja.Leave += delegate { ColorBorde = Tema.Borde; };
            Click += delegate { Caja.Focus(); };
            Resize += delegate { Centrar(); };
            Centrar();
        }

        public void Centrar()
        {
            Caja.Left = 14;
            Caja.Width = Math.Max(10, Width - 28);
            Caja.Top = Math.Max(0, (Height - Caja.Height) / 2);
        }

        public string Texto
        {
            get { return Caja.Text; }
            set { Caja.Text = value; }
        }
    }

}
