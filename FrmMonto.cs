#nullable disable
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Billetera
{
    /// <summary>Diálogo sencillo para pedir un monto (aportar a una meta, editar el ingreso mensual).</summary>
    public class FrmMonto : Form
    {
        public decimal Valor { get; private set; }

        private CampoTexto txt;
        private Label lblError;
        private bool permitirCero;
        private decimal maximo;

        public FrmMonto(string titulo, string etiqueta, decimal inicial, string textoBoton, bool permitirCero, decimal maximo)
        {
            this.permitirCero = permitirCero;
            this.maximo = maximo;
            Tema.ConfigurarDialogo(this, titulo, 420, 270);

            Controls.Add(Tema.Etiqueta(titulo, 16f, Tema.Texto, FontStyle.Bold, 24, 20));
            Controls.Add(Tema.EtiquetaCaja(etiqueta, 9.5f, Tema.Gris, FontStyle.Regular, new Rectangle(24, 58, 372, 38), ContentAlignment.TopLeft));

            txt = new CampoTexto(false);
            txt.Bounds = new Rectangle(24, 104, 372, 46);
            if (inicial > 0) txt.Texto = inicial.ToString("0.##", Tema.Us);
            Controls.Add(txt);

            lblError = Tema.EtiquetaCaja("", 9f, Tema.Rojo, FontStyle.Regular, new Rectangle(24, 156, 372, 36), ContentAlignment.TopLeft);
            Controls.Add(lblError);

            BotonTema cancelar = new BotonTema("Cancelar");
            cancelar.Bounds = new Rectangle(24, 204, 150, 44);
            cancelar.ColorFondo = Tema.Menta;
            cancelar.ColorTexto = Tema.Verde;
            cancelar.DialogResult = DialogResult.Cancel;
            Controls.Add(cancelar);

            BotonTema guardar = new BotonTema(textoBoton);
            guardar.Bounds = new Rectangle(186, 204, 210, 44);
            guardar.Click += Guardar;
            Controls.Add(guardar);

            AcceptButton = guardar;
            CancelButton = cancelar;
            Shown += delegate { txt.Caja.Focus(); txt.Caja.SelectAll(); };
        }

        private void Guardar(object sender, EventArgs e)
        {
            decimal v;
            if (!Tema.TryMonto(txt.Texto, out v) || v < 0 || (v == 0 && !permitirCero))
            {
                lblError.Text = permitirCero ? "Escribe un monto válido (solo números)." : "Escribe un monto mayor a 0 (solo números).";
                return;
            }
            if (maximo > 0 && v > maximo)
            {
                lblError.Text = "El monto no puede ser mayor a " + Tema.Dinero(maximo) + ".";
                return;
            }
            Valor = v;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
