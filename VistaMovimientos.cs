#nullable disable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Billetera
{
    /// <summary>Pantalla 3: lista de movimientos con filtros.</summary>
    public class VistaMovimientos : VistaBase
    {
        private ComboBox cboMes;
        private ComboBox cboTipo;
        private ComboBox cboCategoria;
        private DataGridView dgv;
        private Label lblTotales;
        private bool cargando;

        public VistaMovimientos()
        {
            LimpiarAlActualizar = false;      // los filtros se crean una sola vez
            Encabezado("Historial", "Movimientos", "+  Nuevo movimiento", NuevoMovimiento);

            // ----- Filtros -----
            Controls.Add(Tema.Etiqueta("Mes", 9f, Tema.Gris, FontStyle.Bold, 32, 92));
            cboMes = CrearCombo(30, 114, 210);
            DateTime hoy = DateTime.Today;
            DateTime mes = new DateTime(hoy.Year, hoy.Month, 1);
            for (int i = 0; i < 12; i++)
            {
                MesItem it = new MesItem();
                it.Fecha = mes.AddMonths(-i);
                cboMes.Items.Add(it);
            }
            cboMes.SelectedIndex = 0;

            Controls.Add(Tema.Etiqueta("Tipo", 9f, Tema.Gris, FontStyle.Bold, 262, 92));
            cboTipo = CrearCombo(260, 114, 170);
            cboTipo.Items.Add("Todos");
            cboTipo.Items.Add("Gastos");
            cboTipo.Items.Add("Ingresos");
            cboTipo.SelectedIndex = 0;

            Controls.Add(Tema.Etiqueta("Categoría", 9f, Tema.Gris, FontStyle.Bold, 452, 92));
            cboCategoria = CrearCombo(450, 114, 230);

            // ----- Tabla -----
            PanelRedondeado tarjeta = new PanelRedondeado();
            tarjeta.Bounds = new Rectangle(30, 164, 830, 486);
            tarjeta.Radio = 16;
            dgv = new DataGridView();
            Tema.EstilizarGrid(dgv);
            dgv.Bounds = new Rectangle(8, 8, 814, 470);
            dgv.Columns.Add("fecha", "Fecha");
            dgv.Columns.Add("categoria", "Categoría");
            dgv.Columns.Add("nota", "Nota");
            dgv.Columns.Add("tipo", "Tipo");
            dgv.Columns.Add("monto", "Monto");
            dgv.Columns[0].Width = 120;
            dgv.Columns[1].Width = 160;
            dgv.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgv.Columns[3].Width = 90;
            dgv.Columns[4].Width = 150;
            dgv.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgv.Columns[4].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            tarjeta.Controls.Add(dgv);
            Controls.Add(tarjeta);

            // ----- Pie -----
            lblTotales = Tema.EtiquetaCaja("", 9.5f, Tema.Texto, FontStyle.Bold, new Rectangle(30, 660, 612, 44), ContentAlignment.MiddleLeft);
            Controls.Add(lblTotales);
            BotonTema eliminar = new BotonTema("Eliminar seleccionado");
            eliminar.Bounds = new Rectangle(650, 660, 210, 42);
            eliminar.ColorFondo = Tema.RojoClaro;
            eliminar.ColorTexto = Tema.Rojo;
            eliminar.Click += EliminarSeleccionado;
            Controls.Add(eliminar);

            cboMes.SelectedIndexChanged += CambioFiltro;
            cboTipo.SelectedIndexChanged += CambioFiltro;
            cboCategoria.SelectedIndexChanged += CambioFiltro;
        }

        private ComboBox CrearCombo(int x, int y, int ancho)
        {
            ComboBox c = new ComboBox();
            c.DropDownStyle = ComboBoxStyle.DropDownList;
            c.Font = Tema.Fuente(10.5f);
            c.Location = new Point(x, y);
            c.Width = ancho;
            Controls.Add(c);
            return c;
        }

        private void CambioFiltro(object sender, EventArgs e)
        {
            if (!cargando) Cargar();
        }

        protected override void Construir()
        {
            // Recargar categorías (por si cambiaron) conservando la selección
            cargando = true;
            int idSel = 0;
            CategoriaItem actual = cboCategoria.SelectedItem as CategoriaItem;
            if (actual != null) idSel = actual.Id;
            cboCategoria.Items.Clear();
            CategoriaItem todas = new CategoriaItem();
            todas.Id = 0;
            todas.Nombre = "Todas";
            cboCategoria.Items.Add(todas);
            int indice = 0;
            foreach (CategoriaItem c in Datos.Categorias(null))
            {
                int pos = cboCategoria.Items.Add(c);
                if (c.Id == idSel) indice = pos;
            }
            cboCategoria.SelectedIndex = indice;
            cargando = false;
            Cargar();
        }

        private void Cargar()
        {
            MesItem mes = (MesItem)cboMes.SelectedItem;
            string tipo = null;
            if (cboTipo.SelectedIndex == 1) tipo = "gasto";
            if (cboTipo.SelectedIndex == 2) tipo = "ingreso";
            CategoriaItem cat = cboCategoria.SelectedItem as CategoriaItem;
            int idCat = (cat == null) ? 0 : cat.Id;

            List<MovimientoItem> lista = Datos.Movimientos(mes.Fecha, tipo, idCat, 0);
            dgv.Rows.Clear();
            decimal ing = 0, gas = 0;
            foreach (MovimientoItem m in lista)
            {
                bool gasto = m.Tipo == "gasto";
                if (gasto) gas += m.Monto; else ing += m.Monto;
                int idx = dgv.Rows.Add(
                    m.Fecha.ToString("dd MMM yyyy", Tema.Es),
                    m.Categoria,
                    m.Nota,
                    gasto ? "Gasto" : "Ingreso",
                    (gasto ? "−" : "+") + Tema.Dinero2(m.Monto));
                dgv.Rows[idx].Tag = m.Id;
                dgv.Rows[idx].Cells[4].Style.ForeColor = gasto ? Tema.Rojo : Tema.VerdeClaro;
                dgv.Rows[idx].Cells[4].Style.Font = Tema.Fuente(10f, FontStyle.Bold);
            }
            dgv.ClearSelection();
            lblTotales.Text = lista.Count + " movimiento(s)   ·   Ingresos: " + Tema.Dinero(ing) +
                              "   ·   Gastos: " + Tema.Dinero(gas) + "   ·   Neto: " + Tema.Signo(Calculos.FlujoNeto(ing, gas));
        }

        private void NuevoMovimiento(object sender, EventArgs e)
        {
            using (FrmNuevoMovimiento f = new FrmNuevoMovimiento())
            {
                if (f.ShowDialog(this) == DialogResult.OK) Cargar();
            }
        }

        private void EliminarSeleccionado(object sender, EventArgs e)
        {
            if (dgv.CurrentRow == null || dgv.CurrentRow.Tag == null)
            {
                MessageBox.Show("Selecciona un movimiento de la tabla.", "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show("¿Eliminar este movimiento?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            try
            {
                Datos.EliminarMovimiento((int)dgv.CurrentRow.Tag);
                Cargar();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo eliminar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
