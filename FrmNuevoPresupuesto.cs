#nullable disable
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Billetera
{
    /// <summary>Crear un presupuesto del mes o ajustar el límite de uno existente.</summary>
    public class FrmNuevoPresupuesto : Form
    {
        private ComboBox cboCategoria;
        private CampoTexto txtLimite;
        private Label lblError;
        private DateTime mes;

        public FrmNuevoPresupuesto(PresupuestoItem editar)
        {
            DateTime hoy = DateTime.Today;
            mes = new DateTime(hoy.Year, hoy.Month, 1);
            bool editando = editar != null;
            Tema.ConfigurarDialogo(this, editando ? "Ajustar presupuesto" : "Nuevo presupuesto", 420, 380);

            Controls.Add(Tema.Etiqueta(editando ? "Ajustar presupuesto" : "Nuevo presupuesto", 16f, Tema.Texto, FontStyle.Bold, 24, 20));
            Controls.Add(Tema.Etiqueta("Mes: " + Tema.NombreMes(mes), 10f, Tema.Gris, FontStyle.Regular, 26, 56));

            Controls.Add(Tema.Etiqueta("Categoría", 9.5f, Tema.Gris, FontStyle.Bold, 26, 92));
            cboCategoria = new ComboBox();
            cboCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCategoria.Font = Tema.Fuente(11f);
            cboCategoria.Bounds = new Rectangle(24, 116, 372, 30);
            Controls.Add(cboCategoria);

            Controls.Add(Tema.Etiqueta("Límite del mes", 9.5f, Tema.Gris, FontStyle.Bold, 26, 164));
            txtLimite = new CampoTexto(false);
            txtLimite.Bounds = new Rectangle(24, 188, 372, 46);
            Controls.Add(txtLimite);

            lblError = Tema.EtiquetaCaja("", 9.5f, Tema.Rojo, FontStyle.Regular, new Rectangle(24, 244, 372, 40), ContentAlignment.TopLeft);
            Controls.Add(lblError);

            BotonTema cancelar = new BotonTema("Cancelar");
            cancelar.Bounds = new Rectangle(24, 304, 150, 46);
            cancelar.ColorFondo = Tema.Menta;
            cancelar.ColorTexto = Tema.Verde;
            cancelar.DialogResult = DialogResult.Cancel;
            Controls.Add(cancelar);

            BotonTema guardar = new BotonTema("Guardar");
            guardar.Bounds = new Rectangle(186, 304, 210, 46);
            guardar.Click += Guardar;
            Controls.Add(guardar);
            AcceptButton = guardar;
            CancelButton = cancelar;

            try
            {
                foreach (CategoriaItem c in Datos.Categorias("gasto"))
                {
                    int pos = cboCategoria.Items.Add(c);
                    if (editando && c.Id == editar.IdCategoria) cboCategoria.SelectedIndex = pos;
                }
                if (cboCategoria.SelectedIndex < 0 && cboCategoria.Items.Count > 0) cboCategoria.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                lblError.Text = "No se pudieron cargar las categorías: " + ex.Message;
            }

            if (editando)
            {
                cboCategoria.Enabled = false;
                txtLimite.Texto = editar.Limite.ToString("0.##", Tema.Us);
            }
        }

        private void Guardar(object sender, EventArgs e)
        {
            lblError.Text = "";
            CategoriaItem cat = cboCategoria.SelectedItem as CategoriaItem;
            decimal limite;
            if (cat == null) { lblError.Text = "Elige una categoría."; return; }
            if (!Tema.TryMonto(txtLimite.Texto, out limite) || limite <= 0)
            {
                lblError.Text = "Escribe un límite mayor a 0.";
                return;
            }
            try
            {
                Datos.GuardarPresupuesto(cat.Id, mes, limite);
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
