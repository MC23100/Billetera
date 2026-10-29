#nullable disable
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Billetera
{
    /// <summary>Base de las 4 pantallas internas (Inicio, Movimientos, Presupuestos, Metas y perfil).</summary>
    public abstract class VistaBase : UserControl
    {
        public const int Ancho = 890;

        /// <summary>La ventana principal asigna esto para que una vista pueda saltar a otra (0=Inicio, 1=Movimientos, 2=Presupuestos, 3=Metas).</summary>
        public Action<int> Navegar;

        protected bool LimpiarAlActualizar = true;

        protected VistaBase()
        {
            BackColor = Tema.Fondo;
            Size = new Size(Ancho, 740);      // tamaño fijo; la ventana principal pone la barra de desplazamiento si hace falta
            DoubleBuffered = true;
        }

        protected abstract void Construir();

        public void Actualizar()
        {
            SuspendLayout();          // evita parpadeo mientras se redibuja la pantalla
            try
            {
                if (LimpiarAlActualizar) Limpiar();
                Construir();
            }
            catch (Exception ex)
            {
                if (LimpiarAlActualizar)
                {
                    Limpiar();
                    Controls.Add(Tema.EtiquetaCaja("No se pudieron cargar los datos: " + ex.Message, 10.5f, Tema.Rojo,
                        FontStyle.Regular, new Rectangle(30, 30, 820, 80), ContentAlignment.TopLeft));
                }
                else
                {
                    MessageBox.Show("No se pudieron cargar los datos: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                ResumeLayout(true);
            }
        }

        /// <summary>Actualiza cuando termine el evento actual (evita destruir un botón mientras se está pulsando).</summary>
        protected void ActualizarLuego()
        {
            if (IsHandleCreated) BeginInvoke(new Action(Actualizar)); else Actualizar();
        }

        protected void Limpiar()
        {
            while (Controls.Count > 0)
            {
                Control c = Controls[0];
                Controls.RemoveAt(0);
                c.Dispose();
            }
        }

        /// <summary>Título de la pantalla y botón principal a la derecha.</summary>
        protected void Encabezado(string subtitulo, string titulo, string textoBoton, EventHandler alClick)
        {
            Controls.Add(Tema.Etiqueta(subtitulo, 11f, Tema.Gris, FontStyle.Regular, 30, 22));
            Controls.Add(Tema.Etiqueta(titulo, 22f, Tema.Texto, FontStyle.Bold, 30, 44));
            if (textoBoton != null)
            {
                BotonTema b = new BotonTema(textoBoton);
                b.Bounds = new Rectangle(Ancho - 30 - 210, 34, 210, 44);
                b.Click += alClick;
                Controls.Add(b);
            }
        }
    }
}
