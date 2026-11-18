namespace OpenPOS.Presentacion.Configuraciones
{
    partial class ConfiguracionSri
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblCertificado = new System.Windows.Forms.Label();
            this.txtCertificado = new System.Windows.Forms.TextBox();
            this.btnCargarCertificado = new System.Windows.Forms.Button();
            this.lblCertInfo = new System.Windows.Forms.Label();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.Location = new System.Drawing.Point(20, 20);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(250, 30);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Configuración SRI - Facturación Electrónica";
            // 
            // lblCertificado
            // 
            this.lblCertificado.AutoSize = true;
            this.lblCertificado.Location = new System.Drawing.Point(20, 70);
            this.lblCertificado.Name = "lblCertificado";
            this.lblCertificado.Size = new System.Drawing.Size(120, 20);
            this.lblCertificado.TabIndex = 1;
            this.lblCertificado.Text = "Certificado (.pfx):";
            // 
            // txtCertificado
            // 
            this.txtCertificado.Location = new System.Drawing.Point(20, 95);
            this.txtCertificado.Name = "txtCertificado";
            this.txtCertificado.Size = new System.Drawing.Size(350, 26);
            this.txtCertificado.TabIndex = 2;
            // 
            // btnCargarCertificado
            // 
            this.btnCargarCertificado.Location = new System.Drawing.Point(20, 130);
            this.btnCargarCertificado.Name = "btnCargarCertificado";
            this.btnCargarCertificado.Size = new System.Drawing.Size(120, 30);
            this.btnCargarCertificado.TabIndex = 3;
            this.btnCargarCertificado.Text = "Cargar...";
            this.btnCargarCertificado.Click += new System.EventHandler(this.btnCargarCertificado_Click);
            // 
            // lblContrasena
            // 
            this.lblContrasena = new System.Windows.Forms.Label();
            this.lblContrasena.AutoSize = true;
            this.lblContrasena.Location = new System.Drawing.Point(20, 210);
            this.lblContrasena.Name = "lblContrasena";
            this.lblContrasena.Size = new System.Drawing.Size(95, 20);
            this.lblContrasena.TabIndex = 4;
            this.lblContrasena.Text = "Contraseña:";
            // 
            // txtContrasena
            // 
            this.txtContrasena.Location = new System.Drawing.Point(20, 235);
            this.txtContrasena.Name = "txtContrasena";
            this.txtContrasena.PasswordChar = '*';
            this.txtContrasena.Size = new System.Drawing.Size(350, 26);
            this.txtContrasena.TabIndex = 5;
            // 
            // lblCertInfo
            // 
            this.lblCertInfo.AutoSize = true;
            this.lblCertInfo.Location = new System.Drawing.Point(20, 170);
            this.lblCertInfo.Name = "lblCertInfo";
            this.lblCertInfo.Size = new System.Drawing.Size(0, 20);
            this.lblCertInfo.TabIndex = 6;
            // 
            // btnGuardar
            // 
            this.btnGuardar.Location = new System.Drawing.Point(20, 280);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(120, 30);
            this.btnGuardar.TabIndex = 7;
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // ConfiguracionSri
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.lblCertInfo);
            this.Controls.Add(this.txtContrasena);
            this.Controls.Add(this.lblContrasena);
            this.Controls.Add(this.btnCargarCertificado);
            this.Controls.Add(this.txtCertificado);
            this.Controls.Add(this.lblCertificado);
            this.Controls.Add(this.lblTitulo);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "ConfiguracionSri";
            this.Size = new System.Drawing.Size(500, 320);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblCertificado;
        private System.Windows.Forms.TextBox txtCertificado;
        private System.Windows.Forms.Label lblContrasena;
        private System.Windows.Forms.TextBox txtContrasena;
        private System.Windows.Forms.Button btnCargarCertificado;
        private System.Windows.Forms.Label lblCertInfo;
        private System.Windows.Forms.Button btnGuardar;
    }
}