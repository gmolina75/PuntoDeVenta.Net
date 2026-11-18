using System;
using System.Windows.Forms;
using OpenPOS.UI;

namespace OpenPOS.Presentacion.Configuraciones
{
    public partial class ConfiguracionSri : UserControl
    {
        public ConfiguracionSri()
        {
            InitializeComponent();
        }

        private void btnCargarCertificado_Click(object sender, EventArgs e)
        {
            using (var openFile = new OpenFileDialog())
            {
                openFile.Filter = "Archivo PFX (*.pfx)|*.pfx|All Files|*.*";
                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    txtCertificado.Text = openFile.FileName;
                    var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(openFile.FileName);
                    lblCertInfo.Text = $"Emisor: {cert.Issuer}, Válido hasta: {cert.NotAfter:yyyy-MM-dd}";
                }
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!System.IO.File.Exists(txtCertificado.Text))
            {
                DialogService.Show("Seleccione un certificado válido", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Properties.Settings.Default.SriCertificadoPath = txtCertificado.Text;
            Properties.Settings.Default.SriContrasena = txtContrasena.Text;
            Properties.Settings.Default.Save();
            DialogService.Show("Configuración SRI guardada exitosamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}