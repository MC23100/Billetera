#nullable disable
using System;
using System.Windows.Forms;

namespace Billetera
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Acceso -> ventana principal. Si cierra sesión, vuelve al acceso.
            while (true)
            {
                using (FrmAcceso acceso = new FrmAcceso())
                {
                    if (acceso.ShowDialog() != DialogResult.OK) break;
                }
                bool otraVez;
                using (FrmPrincipal principal = new FrmPrincipal())
                {
                    principal.ShowDialog();
                    otraVez = principal.CerrarSesion;
                }
                if (!otraVez) break;
            }
        }
    }
}
