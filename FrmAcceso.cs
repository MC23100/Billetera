#nullable disable
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Billetera
{
    /// <summary>Pantalla 1: Iniciar sesión / Crear cuenta.</summary>
    public class FrmAcceso : Form
    {
        private Panel pnlLogin, pnlRegistro;
        private BotonTema tabLogin, tabRegistro, btnEntrar, btnCrear;
        private CampoTexto txtCorreo, txtClave;
        private CampoTexto txtRegNombre, txtRegCorreo, txtRegClave, txtRegSaldo, txtRegIngreso;
        private Label lblErrorLogin, lblErrorReg;

        public FrmAcceso()
        {
            Text = "Billetera – Acceso";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Tema.Fondo;
            ClientSize = new Size(900, 640);

            ConstruirLateral();
            ConstruirPestanas();
            ConstruirLogin();
            ConstruirRegistro();
            MostrarPestana(true);
        }

        // ---------- Panel verde de la izquierda ----------
        private void ConstruirLateral()
        {
            Panel izq = new Panel();
            izq.Bounds = new Rectangle(0, 0, 400, 640);
            izq.BackColor = Tema.Verde;
            izq.Paint += delegate (object s, PaintEventArgs e)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (SolidBrush b1 = new SolidBrush(Color.FromArgb(34, 255, 255, 255)))
                { e.Graphics.FillEllipse(b1, 170, -140, 400, 400); }
                using (SolidBrush b2 = new SolidBrush(Color.FromArgb(70, 20, 140, 125)))
                { e.Graphics.FillEllipse(b2, -110, 470, 320, 320); }
            };

            izq.Controls.Add(Tema.Etiqueta("Mi Billetera", 20f, Color.White, FontStyle.Bold, 40, 56));

            izq.Controls.Add(Tema.EtiquetaCaja("Tu dinero, claro y bajo control.", 26f, Color.White, FontStyle.Bold,
                new Rectangle(40, 190, 320, 130), ContentAlignment.TopLeft));
            izq.Controls.Add(Tema.EtiquetaCaja("Registra tus ingresos y gastos, vigila tus presupuestos y cumple tus metas de ahorro.", 11f,
                Color.FromArgb(200, 230, 212), FontStyle.Regular, new Rectangle(40, 330, 310, 70), ContentAlignment.TopLeft));

            string[] puntos = { "Saldo y flujo neto al instante", "Presupuestos por categoría", "Metas de ahorro con progreso" };
            for (int i = 0; i < puntos.Length; i++)
            {
                izq.Controls.Add(Tema.Etiqueta("●", 8f, Tema.Lima, FontStyle.Regular, 42, 433 + i * 32));
                izq.Controls.Add(Tema.Etiqueta(puntos[i], 10.5f, Color.White, FontStyle.Regular, 64, 430 + i * 32));
            }
            Controls.Add(izq);
        }

        // ---------- Pestañas ----------
        private void ConstruirPestanas()
        {
            PanelRedondeado pill = new PanelRedondeado();
            pill.Bounds = new Rectangle(450, 50, 400, 50);
            pill.Radio = 14;
            tabLogin = new BotonTema("Iniciar sesión");
            tabLogin.Bounds = new Rectangle(4, 4, 194, 42);
            tabLogin.Click += delegate { MostrarPestana(true); };
            tabRegistro = new BotonTema("Crear cuenta");
            tabRegistro.Bounds = new Rectangle(202, 4, 194, 42);
            tabRegistro.Click += delegate { MostrarPestana(false); };
            pill.Controls.Add(tabLogin);
            pill.Controls.Add(tabRegistro);
            Controls.Add(pill);
        }

        private void MostrarPestana(bool login)
        {
            pnlLogin.Visible = login;
            pnlRegistro.Visible = !login;
            tabLogin.ColorFondo = login ? Tema.Verde : Color.Transparent;
            tabLogin.ColorTexto = login ? Color.White : Tema.Gris;
            tabRegistro.ColorFondo = login ? Color.Transparent : Tema.Verde;
            tabRegistro.ColorTexto = login ? Tema.Gris : Color.White;
            tabLogin.Invalidate();
            tabRegistro.Invalidate();
            AcceptButton = login ? btnEntrar : btnCrear;
            if (login) txtCorreo.Caja.Focus(); else txtRegNombre.Caja.Focus();
        }

        // ---------- Iniciar sesión ----------
        private void ConstruirLogin()
        {
            pnlLogin = new Panel();
            pnlLogin.Bounds = new Rectangle(450, 124, 400, 470);
            pnlLogin.BackColor = Tema.Fondo;

            pnlLogin.Controls.Add(Tema.Etiqueta("Bienvenido de nuevo", 20f, Tema.Texto, FontStyle.Bold, 0, 0));
            pnlLogin.Controls.Add(Tema.Etiqueta("Entra para ver tu panorama financiero", 10.5f, Tema.Gris, FontStyle.Regular, 2, 40));

            pnlLogin.Controls.Add(Tema.Etiqueta("Correo electrónico", 9.5f, Tema.Gris, FontStyle.Bold, 2, 92));
            txtCorreo = new CampoTexto(false);
            txtCorreo.Bounds = new Rectangle(0, 116, 400, 46);
            pnlLogin.Controls.Add(txtCorreo);

            pnlLogin.Controls.Add(Tema.Etiqueta("Contraseña", 9.5f, Tema.Gris, FontStyle.Bold, 2, 178));
            txtClave = new CampoTexto(true);
            txtClave.Bounds = new Rectangle(0, 202, 400, 46);
            pnlLogin.Controls.Add(txtClave);

            lblErrorLogin = Tema.EtiquetaCaja("", 9.5f, Tema.Rojo, FontStyle.Regular, new Rectangle(0, 258, 400, 44), ContentAlignment.TopLeft);
            pnlLogin.Controls.Add(lblErrorLogin);

            btnEntrar = new BotonTema("Entrar");
            btnEntrar.Bounds = new Rectangle(0, 312, 400, 52);
            btnEntrar.Font = Tema.Fuente(11.5f, FontStyle.Bold);
            btnEntrar.Radio = 14;
            btnEntrar.Click += Entrar;
            pnlLogin.Controls.Add(btnEntrar);

            pnlLogin.Controls.Add(Tema.EtiquetaCaja("Cuenta de ejemplo:  ana@correo.com  /  Ana12345", 9f, Tema.Gris, FontStyle.Regular,
                new Rectangle(0, 380, 400, 24), ContentAlignment.MiddleCenter));
            Controls.Add(pnlLogin);
        }

        private void Entrar(object sender, EventArgs e)
        {
            lblErrorLogin.Text = "";
            if (txtCorreo.Texto.Trim().Length == 0 || txtClave.Texto.Length == 0)
            {
                lblErrorLogin.Text = "Escribe tu correo y tu contraseña.";
                return;
            }
            try
            {
                if (Datos.Login(txtCorreo.Texto, txtClave.Texto))
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    lblErrorLogin.Text = "Correo o contraseña incorrectos.";
                }
            }
            catch (Exception ex)
            {
                lblErrorLogin.Text = "No se pudo conectar con MySQL. ¿Está encendido en XAMPP? (" + ex.Message + ")";
            }
        }

        // ---------- Crear cuenta ----------
        private void ConstruirRegistro()
        {
            pnlRegistro = new Panel();
            pnlRegistro.Bounds = new Rectangle(450, 124, 400, 490);
            pnlRegistro.BackColor = Tema.Fondo;

            pnlRegistro.Controls.Add(Tema.Etiqueta("Crea tu cuenta", 20f, Tema.Texto, FontStyle.Bold, 0, 0));
            pnlRegistro.Controls.Add(Tema.Etiqueta("Dinos con cuánto empiezas para calcular todo por ti", 10.5f, Tema.Gris, FontStyle.Regular, 2, 40));

            pnlRegistro.Controls.Add(Tema.Etiqueta("Nombre completo", 9.5f, Tema.Gris, FontStyle.Bold, 2, 78));
            txtRegNombre = new CampoTexto(false);
            txtRegNombre.Bounds = new Rectangle(0, 100, 400, 44);
            pnlRegistro.Controls.Add(txtRegNombre);

            pnlRegistro.Controls.Add(Tema.Etiqueta("Correo electrónico", 9.5f, Tema.Gris, FontStyle.Bold, 2, 154));
            txtRegCorreo = new CampoTexto(false);
            txtRegCorreo.Bounds = new Rectangle(0, 176, 400, 44);
            pnlRegistro.Controls.Add(txtRegCorreo);

            pnlRegistro.Controls.Add(Tema.Etiqueta("Contraseña (mínimo 6 caracteres)", 9.5f, Tema.Gris, FontStyle.Bold, 2, 230));
            txtRegClave = new CampoTexto(true);
            txtRegClave.Bounds = new Rectangle(0, 252, 400, 44);
            pnlRegistro.Controls.Add(txtRegClave);

            pnlRegistro.Controls.Add(Tema.Etiqueta("Saldo actual", 9.5f, Tema.Gris, FontStyle.Bold, 2, 306));
            txtRegSaldo = new CampoTexto(false);
            txtRegSaldo.Bounds = new Rectangle(0, 328, 192, 44);
            pnlRegistro.Controls.Add(txtRegSaldo);

            pnlRegistro.Controls.Add(Tema.Etiqueta("Ingreso mensual", 9.5f, Tema.Gris, FontStyle.Bold, 210, 306));
            txtRegIngreso = new CampoTexto(false);
            txtRegIngreso.Bounds = new Rectangle(208, 328, 192, 44);
            pnlRegistro.Controls.Add(txtRegIngreso);

            lblErrorReg = Tema.EtiquetaCaja("", 9.5f, Tema.Rojo, FontStyle.Regular, new Rectangle(0, 380, 400, 40), ContentAlignment.TopLeft);
            pnlRegistro.Controls.Add(lblErrorReg);

            btnCrear = new BotonTema("Crear cuenta");
            btnCrear.Bounds = new Rectangle(0, 428, 400, 52);
            btnCrear.Font = Tema.Fuente(11.5f, FontStyle.Bold);
            btnCrear.Radio = 14;
            btnCrear.Click += Crear;
            pnlRegistro.Controls.Add(btnCrear);
            Controls.Add(pnlRegistro);
        }

        private void Crear(object sender, EventArgs e)
        {
            lblErrorReg.Text = "";
            string nombre = txtRegNombre.Texto.Trim();
            string correo = txtRegCorreo.Texto.Trim();
            string clave = txtRegClave.Texto;
            decimal saldo, ingreso;

            if (nombre.Length < 2) { lblErrorReg.Text = "Escribe tu nombre."; return; }
            if (correo.IndexOf('@') < 1 || correo.IndexOf('.') < 0) { lblErrorReg.Text = "Escribe un correo válido."; return; }
            if (clave.Length < 6) { lblErrorReg.Text = "La contraseña debe tener al menos 6 caracteres."; return; }
            if (!Tema.TryMonto(txtRegSaldo.Texto, out saldo) || saldo < 0) { lblErrorReg.Text = "El saldo actual debe ser un número (0 o más)."; return; }
            if (!Tema.TryMonto(txtRegIngreso.Texto, out ingreso) || ingreso < 0) { lblErrorReg.Text = "El ingreso mensual debe ser un número (0 o más)."; return; }

            try
            {
                string error = Datos.Registrar(nombre, correo, clave, saldo, ingreso);
                if (error != null) { lblErrorReg.Text = error; return; }
                if (Datos.Login(correo, clave))
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            catch (Exception ex)
            {
                lblErrorReg.Text = "No se pudo crear la cuenta: " + ex.Message;
            }
        }
    }
}
