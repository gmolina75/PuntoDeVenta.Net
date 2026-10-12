using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace OpenPOS.Presentacion.Asistenteinstalacion
{
    /// <summary>
    /// Datos capturados por el asistente de instalación del servidor.
    /// </summary>
    internal sealed class DatosInstalacion
    {
        public string Instancia;
        public string BaseDatos;
        public string Usuario;
        public string Contrasena;
    }

    /// <summary>
    /// Cuadro de diálogo que solicita al técnico los datos de instalación
    /// (instancia, base de datos y credenciales de SQL Server) en lugar de
    /// tenerlos predefinidos en el código.
    /// </summary>
    internal sealed class DatosInstalacionDialog : Form
    {
        private static readonly Color Ink = Color.FromArgb(11, 16, 32);
        private static readonly Color Header = Color.FromArgb(15, 22, 44);
        private static readonly Color Txt = Color.FromArgb(230, 236, 247);
        private static readonly Color Muted = Color.FromArgb(143, 160, 196);

        private static readonly Regex IdentificadorValido = new Regex(@"^[A-Za-z0-9_]+$");

        private readonly TextBox _instancia;
        private readonly TextBox _baseDatos;
        private readonly TextBox _usuario;
        private readonly TextBox _contrasena;

        public DatosInstalacion Resultado { get; private set; }

        public DatosInstalacionDialog(DatosInstalacion inicial)
        {
            if (inicial == null) inicial = new DatosInstalacion();

            Text = "Datos de instalación · openPOS";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = Ink;
            ClientSize = new Size(486, 500);
            Font = new Font("Segoe UI", 10f);

            var header = new Panel { Dock = DockStyle.Top, Height = 78, BackColor = Header };
            header.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 6, BackColor = UI.Theme.Accent });
            header.Controls.Add(new Label
            {
                Text = "Datos de instalación",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(28, 16),
                BackColor = Color.Transparent
            });
            header.Controls.Add(new Label
            {
                Text = "Instalación y configuración del servidor de base de datos",
                ForeColor = Muted,
                Font = new Font("Segoe UI", 9.5f),
                AutoSize = true,
                Location = new Point(30, 46),
                BackColor = Color.Transparent
            });

            _instancia = Campo("Nombre de la instancia de SQL Server", inicial.Instancia, 108, false);
            _baseDatos = Campo("Nombre de la base de datos", inicial.BaseDatos, 178, false);
            _usuario = Campo("Usuario de SQL Server", inicial.Usuario, 248, false);
            _contrasena = Campo("Contraseña de SQL Server", inicial.Contrasena, 318, true);

            Controls.Add(new Label
            {
                Text = "El nombre de la base de datos es editable (por defecto «openpos»).\r\n" +
                       "La contraseña se usa como clave del usuario «sa» al instalar SQL Server.",
                ForeColor = Muted,
                Font = new Font("Segoe UI", 8.5f),
                AutoSize = true,
                Location = new Point(28, 374),
                BackColor = Color.Transparent
            });

            var aceptar = new Button
            {
                Text = "Comenzar instalación",
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = UI.Theme.Primary,
                Size = new Size(200, 36),
                Location = new Point(ClientSize.Width - 200 - 28, 448),
                Cursor = Cursors.Hand
            };
            aceptar.FlatAppearance.BorderSize = 0;
            aceptar.Click += (s, e) => Confirmar();

            var cancelar = new Button
            {
                Text = "Cancelar",
                FlatStyle = FlatStyle.Flat,
                ForeColor = Txt,
                BackColor = Color.FromArgb(30, 40, 68),
                Size = new Size(110, 36),
                Location = new Point(aceptar.Left - 110 - 10, 448),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            cancelar.FlatAppearance.BorderColor = Color.FromArgb(60, 78, 120);

            Controls.Add(header);
            Controls.Add(aceptar);
            Controls.Add(cancelar);

            AcceptButton = aceptar;
            CancelButton = cancelar;
        }

        private TextBox Campo(string etiqueta, string valor, int y, bool esPassword)
        {
            Controls.Add(new Label
            {
                Text = etiqueta,
                ForeColor = Muted,
                Font = new Font("Segoe UI", 9f),
                AutoSize = true,
                Location = new Point(30, y),
                BackColor = Color.Transparent
            });

            var txt = new TextBox
            {
                Text = valor ?? string.Empty,
                Location = new Point(30, y + 22),
                Size = new Size(ClientSize.Width - 60, 28),
                Font = new Font("Segoe UI", 10.5f),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 247, 252),
                ForeColor = Color.FromArgb(20, 26, 44)
            };
            if (esPassword)
            {
                txt.UseSystemPasswordChar = true;
            }
            Controls.Add(txt);
            return txt;
        }

        private void Confirmar()
        {
            string instancia = (_instancia.Text ?? string.Empty).Trim();
            string baseDatos = (_baseDatos.Text ?? string.Empty).Trim();
            string usuario = (_usuario.Text ?? string.Empty).Trim();
            string contrasena = _contrasena.Text ?? string.Empty;

            if (instancia.Length == 0)
            {
                Avisar("Indica el nombre de la instancia de SQL Server.", _instancia);
                return;
            }
            if (baseDatos.Length == 0 || !IdentificadorValido.IsMatch(baseDatos))
            {
                Avisar("El nombre de la base de datos solo puede contener letras, números y guiones bajos.", _baseDatos);
                return;
            }
            if (usuario.Length == 0 || !IdentificadorValido.IsMatch(usuario))
            {
                Avisar("El usuario de SQL Server solo puede contener letras, números y guiones bajos.", _usuario);
                return;
            }
            if (contrasena.Length == 0)
            {
                Avisar("Indica la contraseña de SQL Server.", _contrasena);
                return;
            }

            Resultado = new DatosInstalacion
            {
                Instancia = instancia,
                BaseDatos = baseDatos,
                Usuario = usuario,
                Contrasena = contrasena
            };
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Avisar(string mensaje, Control foco)
        {
            MessageBox.Show(this, mensaje, "openPOS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            if (foco != null) foco.Focus();
        }
    }
}
