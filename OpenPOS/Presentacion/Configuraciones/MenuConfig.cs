using OpenPOS.UI;
using RestCsharp.Presentacion.Configuraciones;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPOS.Presentacion.Configuraciones
{
    public partial class MenuConfig : UserControl
    {
        public MenuConfig()
        {
            InitializeComponent();
            this.Load += MenuConfig_Load;
        }

        private void MenuConfig_Load(object sender, EventArgs e)
        {
            Theme.CanvasDark(this);
            Theme.Card(panel1);
            Theme.Card(Panel5);
            Theme.Card(Panel3);
            Theme.Card(panel4);
            Theme.Card(Panel38);
            Theme.Card(panel2);
            Theme.Card(Panel16);
            Theme.Card(Panel35);
            Theme.Card(Panel36);
            Theme.Card(Panel37);
            Theme.Card(Panel9);
            Theme.Card(Panel10);
            Theme.Card(Panel11);
            Theme.Card(Panel5);
            Theme.Card(Panel6);
            Theme.Card(Panel7);
            Theme.Card(panel8);
            Theme.Card(panel12);
            Theme.Card(Panel38);
            Theme.Card(Panel40);
            foreach (Control c in this.Controls) AplicarBotones(c);
            AplicarBotones(this);

            // Add SRI button to FlowLayoutPanel1
            var btnSRI = new Button { Text = "SRI / Facturación", Font = new Font(Theme.FontFamily, 10f, FontStyle.Bold), BackColor = Color.FromArgb(29,29,29), ForeColor = Color.White, Size = new Size(120, 34), Margin = new Padding(4) };
            Theme.ButtonStyler.Apply(btnSRI, Theme.ButtonRole.Secondary);
            btnSRI.Click += btnSRI_Click;
            FlowLayoutPanel1.Controls.Add(btnSRI);
        }

        private void AplicarBotones(Control root)
        {
            if (root == null) return;
            Button b = root as Button;
            if (b != null) Theme.ButtonStyler.Apply(b, Theme.ButtonRole.Auto);
            foreach (Control child in root.Controls) AplicarBotones(child);
        }

        private void btnSRI_Click(object sender, EventArgs e)
        {
            var ctl = new ConfiguracionSri();
            ctl.Dock = DockStyle.Fill;
            ctl.Size = new Size(Width, Height);
            Controls.Add(ctl);
            ctl.BringToFront();
        }

        private void btnempresa_Click(object sender, EventArgs e)
        {
            var ctl = new Empresaconfig();
            ctl.Dock = DockStyle.Fill;
            ctl.Size = new Size(Width, Height);
            Controls.Add(ctl);
            ctl.BringToFront();
        }

        private void btnusuarios_Click(object sender, EventArgs e)
        {
            var ctl = new Usuariosconfig();
            ctl.Dock = DockStyle.Fill;
            ctl.Size = new Size(Width, Height);
            Controls.Add(ctl);
            ctl.BringToFront();
        }

        private void btncajas_Click(object sender, EventArgs e)
        {
            var ctl = new Cajasconfig();
            ctl.Dock = DockStyle.Fill;
            ctl.Size = new Size(Width, Height);
            Controls.Add(ctl);
            ctl.BringToFront();
        }

        private void btnserializacion_Click(object sender, EventArgs e)
        {
            var ctl = new Serializacionconfig();
            ctl.Dock = DockStyle.Fill;
            ctl.Size = new Size(Width, Height);
            Controls.Add(ctl);
            ctl.BringToFront();
        }

        private void btnproductos_Click(object sender, EventArgs e)
        {
            var ctl = new Productosconfig();
            ctl.Dock = DockStyle.Fill;
            ctl.Size = new Size(Width, Height);
            Controls.Add(ctl);
            ctl.BringToFront();
        }

        private void btnclientes_Click(object sender, EventArgs e)
        {
            var ctl = new Clientesconfig();
            ctl.Dock = DockStyle.Fill;
            ctl.Size = new Size(Width, Height);
            Controls.Add(ctl);
            ctl.BringToFront();
        }

        private void btnproveedores_Click(object sender, EventArgs e)
        {
            var ctl = new Proveedoresconfig();
            ctl.Dock = DockStyle.Fill;
            ctl.Size = new Size(Width, Height);
            Controls.Add(ctl);
            ctl.BringToFront();
        }

        private void btndiseñador_Click(object sender, EventArgs e)
        {
            var ctl = new DisenoTicket();
            ctl.Dock = DockStyle.Fill;
            ctl.Size = new Size(Width, Height);
            Controls.Add(ctl);
            ctl.BringToFront();
        }

        private void btnimpresoras_Click(object sender, EventArgs e)
        {
            var ctl = new Impresorasconfig();
            ctl.Dock = DockStyle.Fill;
            ctl.Size = new Size(Width, Height);
            Controls.Add(ctl);
            ctl.BringToFront();
        }

        private void btnBalanza_Click(object sender, EventArgs e)
        {
            var ctl = new Impresorasconfig();
            ctl.Dock = DockStyle.Fill;
            ctl.Size = new Size(Width, Height);
            Controls.Add(ctl);
            ctl.BringToFront();
        }

        private void btncorreo_Click(object sender, EventArgs e)
        {
            var ctl = new Correoconfi();
            ctl.Dock = DockStyle.Fill;
            ctl.Size = new Size(Width, Height);
            Controls.Add(ctl);
            ctl.BringToFront();
        }

        private void btnRespaldos_Click(object sender, EventArgs e)
        {
            var ctl = new Respaldosconfig();
            ctl.Dock = DockStyle.Fill;
            ctl.Size = new Size(Width, Height);
            Controls.Add(ctl);
            ctl.BringToFront();
        }
    }
}
