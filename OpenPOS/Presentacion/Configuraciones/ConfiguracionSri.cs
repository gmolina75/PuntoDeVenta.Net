using System;
using System.Windows.Forms;

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
                MessageBox.Show("Seleccione un certificado válido");
                return;
            }
            // Guardar configuración en app.config o base de datos
            MessageBox.Show("Configuración guardada");
        }
    }
}