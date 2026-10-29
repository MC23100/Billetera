#nullable disable
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Billetera
{
    /// <summary>Ventana principal: menú lateral + las 4 pantallas internas.</summary>
    public class FrmPrincipal : Form
    {
        public bool CerrarSesion { get; private set; }

        private BotonNav[] botones;
        private VistaBase[] vistas;
        private Panel contenido;

        public FrmPrincipal()
        {
            Text = "Billetera";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Tema.Fondo;
            DoubleBuffered = true;

            // Si la pantalla es baja (ej. laptops de 768 px) la ventana se achica y solo el área de contenido
            // (a la derecha) muestra una barra de desplazamiento. El menú lateral queda siempre fijo.
            int alto = Math.Min(740, Screen.PrimaryScreen.WorkingArea.Height - 50);
            int ancho = 1120 + (alto < 740 ? SystemInformation.VerticalScrollBarWidth : 0);
            ClientSize = new Size(ancho, alto);

            // ----- Menú lateral -----
            Panel lateral = new Panel();
            lateral.Bounds = new Rectangle(0, 0, 230, alto);
            lateral.BackColor = Tema.Verde;

            lateral.Controls.Add(Tema.Etiqueta("Mi Billetera", 15f, Color.White, FontStyle.Bold, 24, 30));
            lateral.Controls.Add(Tema.Etiqueta("MENÚ", 8.5f, Color.FromArgb(150, 200, 175), FontStyle.Bold, 26, 104));

            string[] nombres = { "Inicio", "Movimientos", "Presupuestos", "Metas y perfil" };
            botones = new BotonNav[nombres.Length];
            for (int i = 0; i < nombres.Length; i++)
            {
                int idx = i;
                BotonNav b = new BotonNav(nombres[i]);
                b.Bounds = new Rectangle(20, 128 + i * 54, 190, 46);
                b.Click += delegate { Mostrar(idx); };
                botones[i] = b;
                lateral.Controls.Add(b);
            }

            // Tarjeta de usuario
            PanelRedondeado usuario = new PanelRedondeado();
            usuario.Bounds = new Rectangle(16, alto - 144, 198, 66);   // pegado abajo
            usuario.Relleno = Color.FromArgb(30, 255, 255, 255);
            usuario.ColorBorde = Color.Empty;
            usuario.Radio = 16;
            PanelRedondeado av = new PanelRedondeado();
            av.Bounds = new Rectangle(12, 14, 38, 38);
            av.Radio = 19;
            av.Relleno = Tema.Lima;
            av.ColorBorde = Color.Empty;
            av.Controls.Add(Tema.EtiquetaCaja(Tema.Iniciales(Sesion.Nombre), 11f, Tema.Verde, FontStyle.Bold, new Rectangle(0, 0, 38, 38), ContentAlignment.MiddleCenter));
            usuario.Controls.Add(av);
            usuario.Controls.Add(Tema.EtiquetaCaja(Sesion.Nombre, 9.5f, Color.White, FontStyle.Bold, new Rectangle(58, 12, 134, 20), ContentAlignment.MiddleLeft));
            usuario.Controls.Add(Tema.EtiquetaCaja(Sesion.Correo, 8f, Color.FromArgb(190, 225, 205), FontStyle.Regular, new Rectangle(58, 32, 134, 18), ContentAlignment.MiddleLeft));
            lateral.Controls.Add(usuario);

            BotonTema salir = new BotonTema("Cerrar sesión");
            salir.Bounds = new Rectangle(16, alto - 64, 198, 42);
            salir.ColorFondo = Color.FromArgb(34, 255, 255, 255);
            salir.ColorTexto = Color.White;
            salir.Font = Tema.Fuente(10f, FontStyle.Bold);
            salir.Click += delegate { CerrarSesion = true; Close(); };
            lateral.Controls.Add(salir);
            Controls.Add(lateral);

            // ----- Contenido -----
            contenido = new Panel();
            contenido.Bounds = new Rectangle(230, 0, ancho - 230, alto);
            contenido.BackColor = Tema.Fondo;
            contenido.AutoScroll = true;
            Controls.Add(contenido);

            vistas = new VistaBase[] { new VistaInicio(), new VistaMovimientos(), new VistaPresupuestos(), new VistaMetas() };
            foreach (VistaBase v in vistas)
            {
                v.Navegar = Mostrar;
                contenido.Controls.Add(v);
            }
            Mostrar(0);
        }

        private void Mostrar(int indice)
        {
            for (int i = 0; i < botones.Length; i++) botones[i].Activo = (i == indice);
            for (int i = 0; i < vistas.Length; i++) vistas[i].Visible = (i == indice);   // solo se ve una pantalla a la vez
            vistas[indice].Actualizar();
        }
    }
}
