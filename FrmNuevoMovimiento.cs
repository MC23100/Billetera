#nullable disable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Billetera
{
    /// <summary>Pantalla "Nuevo movimiento": gasto o ingreso, monto, categoría, fecha y nota.</summary>
    public class FrmNuevoMovimiento : Form
    {
        private string tipo = "gasto";
        private BotonTema btnGasto, btnIngreso;
        private Label lblPregunta, lblError;
        private CampoTexto txtMonto, txtNota;
        private ComboBox cboCategoria;
        private DateTimePicker dtpFecha;

        public FrmNuevoMovimiento()
        {
            Tema.ConfigurarDialogo(this, "Nuevo movimiento", 440, 650);

            Controls.Add(Tema.Etiqueta("Nuevo movimiento", 16f, Tema.Texto, FontStyle.Bold, 24, 18));

            // ----- Gasto / Ingreso -----
            PanelRedondeado pill = new PanelRedondeado();
            pill.Bounds = new Rectangle(24, 62, 392, 50);
            pill.Radio = 14;
            btnGasto = new BotonTema("Gasto");
            btnGasto.Bounds = new Rectangle(4, 4, 190, 42);
            btnGasto.Click += delegate { CambiarTipo("gasto"); };
            btnIngreso = new BotonTema("Ingreso");
            btnIngreso.Bounds = new Rectangle(198, 4, 190, 42);
            btnIngreso.Click += delegate { CambiarTipo("ingreso"); };
            pill.Controls.Add(btnGasto);
            pill.Controls.Add(btnIngreso);
            Controls.Add(pill);

            // ----- Monto -----
            lblPregunta = Tema.EtiquetaCaja("¿Cuánto gastaste?", 10.5f, Tema.Gris, FontStyle.Regular, new Rectangle(24, 126, 392, 22), ContentAlignment.MiddleCenter);
            Controls.Add(lblPregunta);
            txtMonto = new CampoTexto(false);
            txtMonto.Bounds = new Rectangle(24, 152, 392, 70);
            txtMonto.Radio = 16;
            txtMonto.Caja.Font = Tema.Fuente(24f, FontStyle.Bold);
            txtMonto.Caja.TextAlign = HorizontalAlignment.Center;
            txtMonto.Centrar();
            Controls.Add(txtMonto);

            // ----- Categoría -----
            Controls.Add(Tema.Etiqueta("Categoría", 9.5f, Tema.Gris, FontStyle.Bold, 26, 238));
            cboCategoria = new ComboBox();
            cboCategoria.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCategoria.Font = Tema.Fuente(11f);
            cboCategoria.Bounds = new Rectangle(24, 262, 392, 30);
            Controls.Add(cboCategoria);

            // ----- Fecha -----
            Controls.Add(Tema.Etiqueta("Fecha", 9.5f, Tema.Gris, FontStyle.Bold, 26, 308));
            dtpFecha = new DateTimePicker();
            dtpFecha.Format = DateTimePickerFormat.Custom;
            dtpFecha.CustomFormat = "dd/MM/yyyy";
            dtpFecha.Font = Tema.Fuente(11f);
            dtpFecha.Bounds = new Rectangle(24, 332, 392, 30);
            dtpFecha.Value = DateTime.Today;
            Controls.Add(dtpFecha);

            // ----- Nota -----
            Controls.Add(Tema.Etiqueta("Nota (opcional)", 9.5f, Tema.Gris, FontStyle.Bold, 26, 378));
            txtNota = new CampoTexto(false);
            txtNota.Bounds = new Rectangle(24, 402, 392, 46);
            txtNota.Caja.MaxLength = 150;
            Controls.Add(txtNota);

            lblError = Tema.EtiquetaCaja("", 9.5f, Tema.Rojo, FontStyle.Regular, new Rectangle(24, 460, 392, 40), ContentAlignment.TopLeft);
            Controls.Add(lblError);

            BotonTema guardar = new BotonTema("Guardar movimiento");
            guardar.Bounds = new Rectangle(24, 512, 392, 54);
            guardar.Font = Tema.Fuente(11.5f, FontStyle.Bold);
            guardar.Radio = 14;
            guardar.Click += Guardar;
            Controls.Add(guardar);

            BotonTema cancelar = new BotonTema("Cancelar");
            cancelar.Bounds = new Rectangle(24, 576, 392, 44);
            cancelar.ColorFondo = Color.Transparent;
            cancelar.ColorTexto = Tema.Gris;
            cancelar.DialogResult = DialogResult.Cancel;
            Controls.Add(cancelar);

            AcceptButton = guardar;
            CancelButton = cancelar;
            CambiarTipo("gasto");
            Shown += delegate { txtMonto.Caja.Focus(); };
        }

        private void CambiarTipo(string nuevo)
        {
            tipo = nuevo;
            bool gasto = tipo == "gasto";
            btnGasto.ColorFondo = gasto ? Tema.Verde : Color.Transparent;
            btnGasto.ColorTexto = gasto ? Color.White : Tema.Gris;
            btnIngreso.ColorFondo = gasto ? Color.Transparent : Tema.Verde;
            btnIngreso.ColorTexto = gasto ? Tema.Gris : Color.White;
            btnGasto.Invalidate();
            btnIngreso.Invalidate();
            lblPregunta.Text = gasto ? "¿Cuánto gastaste?" : "¿Cuánto ingresó?";

            cboCategoria.Items.Clear();
            try
            {
                foreach (CategoriaItem c in Datos.Categorias(tipo)) cboCategoria.Items.Add(c);
                if (cboCategoria.Items.Count > 0) cboCategoria.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                lblError.Text = "No se pudieron cargar las categorías: " + ex.Message;
            }
        }

        private void Guardar(object sender, EventArgs e)
        {
            lblError.Text = "";
            decimal monto;
            if (!Tema.TryMonto(txtMonto.Texto, out monto) || monto <= 0)
            {
                lblError.Text = "Escribe un monto mayor a 0 (usa punto para los centavos).";
                return;
            }
            CategoriaItem cat = cboCategoria.SelectedItem as CategoriaItem;
            if (cat == null)
            {
                lblError.Text = "Elige una categoría.";
                return;
            }
            try
            {
                if (tipo == "gasto")
                {
                    decimal saldo = Datos.SaldoAl(null);
                    if (monto > saldo)
                    {
                        lblError.Text = "No puedes gastar más de tu saldo disponible (" + Tema.Dinero2(saldo) + ").";
                        return;
                    }
                }
                Datos.InsertarMovimiento(tipo, cat.Id, monto, dtpFecha.Value, txtNota.Texto.Trim());
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
