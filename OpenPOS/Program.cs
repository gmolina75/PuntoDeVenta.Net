using OpenPOS.Presentacion;
using OpenPOS.Presentacion.AsistenteInstalacion;
using OpenPOS.Presentacion.Compras;
using OpenPOS.Presentacion.Menu;
using OpenPOS.Sunat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
namespace OpenPOS
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var frm = new LOGIN();
            frm.FormClosed += Frm_FormClosed;
            frm.ShowDialog();
            Application.Run();

        }

        private static void Frm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.ExitThread();
            Application.Exit();
        }
    }
}
