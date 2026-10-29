#nullable disable
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Billetera
{
    /// <summary>Crear una nueva meta de ahorro.</summary>
    public class FrmNuevaMeta : Form
    {
        private CampoTexto txtNombre, txtObjetivo, txtAhorrado;
        private DateTimePicker dtpLimite;
        private Label lblError;

        public FrmNuevaMeta()
        {
            Tema.ConfigurarDialogo(this, "Nueva meta", 420, 520);
            Controls.Add(Tema.Etiqueta("Nueva meta de ahorro", 16f, Tema.Texto, FontStyle.Bold, 24, 20));

            Controls.Add(Tema.Etiqueta("Nombre de la meta", 9.5f, Tema.Gris, FontStyle.Bold, 26, 62));
            txtNombre = new CampoTexto(false);
            txtNombre.Bounds = new Rectangle(24, 86, 372, 46);
            txtNombre.Caja.MaxLength = 60;
            Controls.Add(txtNombre);

            Controls.Add(Tema.Etiqueta("Monto objetivo", 9.5f, Tema.Gris, FontStyle.Bold, 26, 146));
            txtObjetivo = new CampoTexto(false);
            txtObjetivo.Bounds = new Rectangle(24, 170, 372, 46);
            Controls.Add(txtObjetivo);

            Controls.Add(Tema.Etiqueta("Ya tengo ahorrado", 9.5f, Tema.Gris, FontStyle.Bold, 26, 230));
            txtAhorrado = new CampoTexto(false);
            txtAhorrado.Bounds = new Rectangle(24, 254, 372, 46);
            txtAhorrado.Texto = "0";
            Controls.Add(txtAhorrado);

            Controls.Add(Tema.Etiqueta("Fecha límite (opcional: marca la casilla)", 9.5f, Tema.Gris, FontStyle.Bold, 26, 314));
            dtpLimite = new DateTimePicker();
            dtpLimite.Format = DateTimePickerFormat.Custom;
            dtpLimite.CustomFormat = "dd/MM/yyyy";
            dtpLimite.ShowCheckBox = true;
            dtpLimite.Checked = false;
            dtpLimite.Font = Tema.Fuente(11f);
            dtpLimite.Value = DateTime.Today.AddMonths(6);
            dtpLimite.Bounds = new Rectangle(24, 338, 372, 30);
            Controls.Add(dtpLimite);

            lblError = Tema.EtiquetaCaja("", 9.5f, Tema.Rojo, FontStyle.Regular, new Rectangle(24, 378, 372, 40), ContentAlignment.TopLeft);
            Controls.Add(lblError);

            BotonTema cancelar = new BotonTema("Cancelar");
            cancelar.Bounds = new Rectangle(24, 440, 150, 46);
            cancelar.ColorFondo = Tema.Menta;
            cancelar.ColorTexto = Tema.Verde;
            cancelar.DialogResult = DialogResult.Cancel;
            Controls.Add(cancelar);

            BotonTema guardar = new BotonTema("Crear meta");
            guardar.Bounds = new Rectangle(186, 440, 210, 46);
            guardar.Click += Guardar;
            Controls.Add(guardar);
            AcceptButton = guardar;
            CancelButton = cancelar;
        }

        private void Guardar(object sender, EventArgs e)
        {
            lblError.Text = "";
            decimal objetivo, ahorrado;
            if (txtNombre.Texto.Trim().Length == 0) { lblError.Text = "Escribe un nombre para la meta."; return; }
            if (!Tema.TryMonto(txtObjetivo.Texto, out objetivo) || objetivo <= 0) { lblError.Text = "El monto objetivo debe ser mayor a 0."; return; }
            if (!Tema.TryMonto(txtAhorrado.Texto, out ahorrado) || ahorrado < 0) { lblError.Text = "Lo ahorrado debe ser 0 o más."; return; }
            if (ahorrado > objetivo) { lblError.Text = "Lo ahorrado no puede ser mayor que el objetivo."; return; }

            DateTime? limite = null;
            if (dtpLimite.Checked)
            {
                if (dtpLimite.Value.Date <= DateTime.Today) { lblError.Text = "La fecha límite debe ser futura."; return; }
                limite = dtpLimite.Value.Date;
            }
            try
            {
                Datos.CrearMeta(txtNombre.Texto, objetivo, ahorrado, limite);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                lblError.Text = "No se pudo guardar: " + ex.Message;
            }
        }
    }
}
