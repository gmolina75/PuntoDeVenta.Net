using OpenPOS.Datos;
using OpenPOS.Logica;
using Sunat.Logica;
using OpenPOS.Presentacion.CAJA;
using OpenPOS.Presentacion.Configuraciones;
using OpenPOS.Presentacion.Gastos_varios;
using OpenPOS.Presentacion.HistorialVentas;
using OpenPOS.Presentacion.Ingresos_varios;
using OpenPOS.Presentacion.Menu;
using OpenPOS.Presentacion.REPORTES.Impresion_de_comprobantes;
using OpenPOS.Sunat;
using OpenPOS.Sunat.Ecuador;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using OpenPOS.UI;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenPOS.Reportes;

namespace OpenPOS.Presentacion.Ventas
{
    public partial class Puntodeventa : UserControl
    {
        public Puntodeventa()
        {
            InitializeComponent();
        }
        System.Windows.Forms.Panel panelProductos = new System.Windows.Forms.Panel();
        int idproducto;
        string sevendePor;
        string ventagenerada = "NUEVA";
        int idventa;
        decimal cantidad;
        string Tipobusqueda;
        decimal total;
        decimal vuelto = 0;
        decimal efectivo_calculado = 0;
        decimal restante = 0;
        decimal credito = 0;
        string Tipopago;
        int idcliente = 0;
        string Comprobante = "-";
        int idcomprobante;
        string conexionsunat;
        string moneda;
        int iddettallev;
        string Impresora;
        string Totalletras;
        string Envioinmediato;
        private void btncerrar_Click(object sender, EventArgs e)
        {
            Dispose();

        }
        private void eliminarVentasimcompletas()
        {
            var funcion = new Dventas();
            funcion.eliminarVentasIncomp();

        }
        private void Puntodeventa_Load(object sender, EventArgs e)
        {
            Theme.ChromeDark(panel1);
            Theme.Card(PanelTotales);
            Theme.Card(panelCobro);
            Theme.Card(panelClienteFactura);
            Theme.CanvasDark(this);
            Theme.Card(panelBienvenida);
            Theme.Apply(dgDetalleventa);
            Theme.Apply(dgProductos);
            Theme.Apply(txtbuscar);
            Theme.Apply(txtcantidad);
            Theme.Apply(txtefectivo);
            Theme.Apply(txttarjeta);
            Theme.Apply(txtcredito);
            Theme.Apply(txtreferencia);
            Theme.Apply(lblTotalref);
            Theme.Apply(lbltotal);
            Theme.Apply(lblvuelto);
            Theme.Apply(lblCliente);
            Theme.Apply(lblComprobante);
            foreach (Control c in this.Controls) AplicarBotones(c);
            AplicarBotones(this);
            AgregarBotonFacturaElectronica();
            AgregarBotonGuiaRemision();
            AgregarBotonRetencion();
            eliminarVentasimcompletas();
            ValidarTiposBusqueda();
            ObtenerComprobanteDefecto();
            ValidarAperturacaja();
        }

        private void AgregarBotonFacturaElectronica()
        {
            var btnFe = new Button();
            btnFe.Text = "Factura Electrónica (SRI)";
            btnFe.FlatStyle = FlatStyle.Flat;
            btnFe.UseVisualStyleBackColor = false;
            btnFe.Size = new Size(160, 34);
            btnFe.Location = new Point(20, 20);
            Theme.ButtonStyler.Apply(btnFe, Theme.ButtonRole.Primary);
            btnFe.Click += BtnFacturaElectronica_Click;
            PanelOperaciones.Controls.Add(btnFe);
            btnFe.BringToFront();
        }

        private async void BtnFacturaElectronica_Click(object sender, EventArgs e)
        {
            try
            {
                var dt = dgDetalleventa.DataSource as DataTable;
                if (dt == null || dt.Rows.Count == 0)
                {
                    DialogService.Show("No hay detalle de venta", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string codigoCol = ObtenerNombreColumna(dt, "Codigo", "codigo", "CodigoProducto", "idproducto");
                string descCol = ObtenerNombreColumna(dt, "Descripcion", "descripcion", "Detalle", "producto");
                string cantCol = ObtenerNombreColumna(dt, "Cantidad", "cantidad", "cant");
                string precioCol = ObtenerNombreColumna(dt, "PrecioUnitario", "precioUnitario", "precio_unitario", "precio");
                string totalCol = ObtenerNombreColumna(dt, "Importe", "importe", "total", "precioTotal", "monto");

                var factura = new Sunat.Ecuador.FacturaElectronica
                {
                    Ruc = "",
                    PuntoEmision = "001",
                    Secuencial = Properties.Settings.Default.UltimaFacturaSecuencial + 1,
                    FechaEmision = DateTime.Now,
                    RazonSocialComprador = lblCliente.Text,
                    IdentificacionComprador = "",
                    DireccionEstablecimiento = "",
                    TotalSinImpuestos = total,
                    Total = total
                };

                foreach (DataRow fila in dt.Rows)
                {
                    factura.Detalles.Add(new Sunat.Ecuador.DetalleFactura
                    {
                        Codigo = fila[codigoCol].ToString(),
                        CodigoAuxiliar = "",
                        Descripcion = fila[descCol].ToString(),
                        Cantidad = Convert.ToDecimal(fila[cantCol]),
                        PrecioUnitario = Convert.ToDecimal(fila[precioCol]),
                        PrecioTotal = Convert.ToDecimal(fila[totalCol]),
                        Descuento = 0
                    });
                }

                string xml = Sunat.Ecuador.GeneradorXml.GenerarFactura(factura);
                string ruta = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"factura_{idventa}.xml");
                System.IO.File.WriteAllText(ruta, xml);
                Properties.Settings.Default.UltimaFacturaSecuencial = factura.Secuencial;
                Properties.Settings.Default.Save();

                var sriConfig = ObtenerConfiguracionSri();
                if (sriConfig.CertificadoPath != null && sriConfig.Contrasena != null)
                {
                    var result = await AutorizarFactura(xml, sriConfig);
                    if (result.Autorizado)
                    {
                        System.IO.File.WriteAllText(ruta.Replace(".xml", "_autorizada.xml"), result.XmlRespuesta);
                        DialogService.Show($"Factura electrónica AUTORIZADA: {result.ClaveAcceso}", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        DialogService.Show($"Autorización fallida: {result.Mensaje}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    DialogService.Show($"Factura electrónica generada (sin autorizar). Configure certificado SRI primero.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                DialogService.Show($"Error al generar factura electrónica: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ObtenerNombreColumna(DataTable dt, params string[] intentos)
        {
            foreach (string col in intentos)
            {
                if (dt.Columns.Contains(col)) return col;
            }
            return dt.Columns[0].ColumnName;
        }

        // Métodos SRI 2.0.0 para autorización automática
        private SRIConfiguracion ObtenerConfiguracionSri()
        {
            return new SRIConfiguracion
            {
                CertificadoPath = Properties.Settings.Default.SriCertificadoPath,
                Contrasena = Properties.Settings.Default.SriContrasena
            };
        }

        private async Task<AutorizacionResult> AutorizarFactura(string xml, SRIConfiguracion config)
        {
            return await AutorizadorSri.AutorizarAsync(xml, config.CertificadoPath, config.Contrasena);
        }

        private class SRIConfiguracion
        {
            public string CertificadoPath { get; set; }
            public string Contrasena { get; set; }
        }

        // Botón Guía de Remisión integrado
        private void AgregarBotonGuiaRemision()
        {
            var btn = new Button();
            btn.Text = "Guía Remisión (SRI)";
            btn.FlatStyle = FlatStyle.Flat;
            btn.UseVisualStyleBackColor = false;
            btn.Size = new Size(160, 34);
            btn.Location = new Point(190, 20);
            Theme.ButtonStyler.Apply(btn, Theme.ButtonRole.Secondary);
            btn.Click += BtnGuiaRemision_Click;
            PanelOperaciones.Controls.Add(btn);
        }

        private async void BtnGuiaRemision_Click(object sender, EventArgs e)
        {
            try
            {
                var dt = dgDetalleventa.DataSource as DataTable;
                if (dt == null || dt.Rows.Count == 0)
                {
                    DialogService.Show("No hay detalle de venta para generar guía", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string codigoCol = ObtenerNombreColumna(dt, "Codigo", "codigo", "CodigoProducto", "idproducto");
                string descCol = ObtenerNombreColumna(dt, "Descripcion", "descripcion", "Detalle", "producto");
                string cantCol = ObtenerNombreColumna(dt, "Cantidad", "cantidad", "cant");
                string unidadCol = ObtenerNombreColumna(dt, "Unidad", "unidad", "unidadmedida", "um");

                var guia = new GuiaRemision
                {
                    Ruc = "",
                    PuntoEmision = "001",
                    Secuencial = Properties.Settings.Default.UltimaFacturaSecuencial + 1,
                    FechaEmision = DateTime.Now,
                    RazonSocialComprador = lblCliente.Text,
                    IdentificacionComprador = "",
                    DireccionDestino = "",
                    MotivoTraslado = ""
                };

                foreach (DataRow fila in dt.Rows)
                {
                    guia.Detalles.Add(new DetalleGuia
                    {
                        Codigo = fila[codigoCol].ToString(),
                        Descripcion = fila[descCol].ToString(),
                        Cantidad = Convert.ToDecimal(fila[cantCol]),
                        UnidadMedida = dt.Columns.Contains(unidadCol) ? fila[unidadCol].ToString() : "UN"
                    });
                }

                string xml = GeneradorXml.GenerarGuiaRemision(guia);
                string ruta = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"guia_{idventa}.xml");
                System.IO.File.WriteAllText(ruta, xml);

                var sriConfig = ObtenerConfiguracionSri();
                if (sriConfig.CertificadoPath != null && sriConfig.Contrasena != null)
                {
                    var result = await AutorizarFactura(xml, sriConfig);
                    if (result.Autorizado)
                    {
                        System.IO.File.WriteAllText(ruta.Replace(".xml", "_autorizada.xml"), result.XmlRespuesta);
                        DialogService.Show($"Guía de Remisión AUTORIZADA: {result.ClaveAcceso}", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        DialogService.Show($"Autorización fallida: {result.Mensaje}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    DialogService.Show($"Guía de Remisión generada (sin autorizar). Configure certificado SRI primero.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                DialogService.Show($"Error al generar guía de remisión: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Botón Retención integrado
        private void AgregarBotonRetencion()
        {
            var btn = new Button();
            btn.Text = "Retención (SRI)";
            btn.FlatStyle = FlatStyle.Flat;
            btn.UseVisualStyleBackColor = false;
            btn.Size = new Size(160, 34);
            btn.Location = new Point(360, 20);
            Theme.ButtonStyler.Apply(btn, Theme.ButtonRole.Warning);
            btn.Click += BtnRetencion_Click;
            PanelOperaciones.Controls.Add(btn);
        }

        private async void BtnRetencion_Click(object sender, EventArgs e)
        {
            try
            {
                var dt = dgDetalleventa.DataSource as DataTable;
                if (dt == null || dt.Rows.Count == 0)
                {
                    DialogService.Show("No hay detalle de venta para generar retención", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string codigoCol = ObtenerNombreColumna(dt, "Codigo", "codigo", "CodigoProducto", "idproducto");
                string descCol = ObtenerNombreColumna(dt, "Descripcion", "descripcion", "Detalle", "producto");
                string totalCol = ObtenerNombreColumna(dt, "Importe", "importe", "total", "precioTotal", "monto");

                var retencion = new Retencion
                {
                    Ruc = "",
                    PuntoEmision = "001",
                    Secuencial = Properties.Settings.Default.UltimaFacturaSecuencial + 1,
                    FechaEmision = DateTime.Now,
                    RazonSocialComprador = lblCliente.Text,
                    IdentificacionComprador = ""
                };

                decimal totalRetener = total * 0.12m;
                retencion.Detalles.Add(new DetalleRetencion
                {
                    Codigo = "001",
                    Descripcion = "Retención de IESS",
                    Porcentaje = 12.00m,
                    BaseImponible = total,
                    ValorRetenido = totalRetener
                });

                string xml = GeneradorXml.GenerarRetencion(retencion);
                string ruta = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"retencion_{idventa}.xml");
                System.IO.File.WriteAllText(ruta, xml);

                var sriConfig = ObtenerConfiguracionSri();
                if (sriConfig.CertificadoPath != null && sriConfig.Contrasena != null)
                {
                    var result = await AutorizarFactura(xml, sriConfig);
                    if (result.Autorizado)
                    {
                        System.IO.File.WriteAllText(ruta.Replace(".xml", "_autorizada.xml"), result.XmlRespuesta);
                        DialogService.Show($"Retención AUTORIZADA: {result.ClaveAcceso}", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        DialogService.Show($"Autorización fallida: {result.Mensaje}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    DialogService.Show($"Retención generada (sin autorizar). Configure certificado SRI primero.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                DialogService.Show($"Error al generar retención: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AplicarBotones(Control root)
        {
            if (root == null) return;
            Button b = root as Button;
            if (b != null) Theme.ButtonStyler.Apply(b, Theme.ButtonRole.Auto);
            foreach (Control child in root.Controls) AplicarBotones(child);
        }
        private void ValidarAperturacaja()
        {
            string resultado = null;
            var funcion = new Dmovicaja();
            funcion.MostrarCierresdeCaja(ref resultado);
            if (resultado == "Nuevo*****")
            {
                aperturaCaja();
            }
        }
        private void aperturaCaja()
        {
            var ctl = new APERTURA_DE_CAJA();
            ctl.Size = new Size(Width, Height + Height / 2);
            this.Controls.Add(ctl);
            ctl.BringToFront();
        }
        private void ObtenerComprobanteDefecto()
        {
            var funcion = new Dserializacion();
            var dt = new DataTable();
            funcion.ObtenerComprobanteDefecto(ref dt);
            // Si no hay comprobante configurado aún, se usa una factura por defecto
            // en lugar de caerse con Rows[0] sobre una tabla vacía.
            Comprobante = dt != null && dt.Rows.Count > 0 ? UtilesSeguros.Celda(dt, 0, 0, "FACTURA") : "FACTURA";
            lblComprobante.Text = Comprobante;
            idcomprobante = dt != null && dt.Rows.Count > 0 ? UtilesSeguros.Entero(UtilesSeguros.Celda(dt, 0, 1), 1) : 1;
            Envioinmediato = dt != null && dt.Rows.Count > 0 ? UtilesSeguros.Celda(dt, 0, 2) : "";
            dibujarComprobantes();
            Theme.Apply(dgDetalleventa);
        }
        private void dibujarComprobantes()
        {
            Panelcomrpobantes.Controls.Clear();
            try
            {
                var funcion = new Dserializacion();
                var dt = new DataTable();
                funcion.mostrarSerializacion(ref dt);

                foreach (DataRow rdr in dt.Rows)
                {
                    Button b = new Button();
                    b.Text = rdr["tipodoc"].ToString();
                    b.Name = rdr["Id_serializacion"].ToString();
                    b.Tag = rdr["Envioinmediato"].ToString();
                    b.Size = new System.Drawing.Size(150, 60);
                    b.BackColor = Color.FromArgb(70, 70, 71);
                    b.Font = new Font("Segoe UI", 12);
                    b.FlatStyle = FlatStyle.Flat;
                    b.ForeColor = Color.WhiteSmoke;
                    Panelcomrpobantes.Controls.Add(b);
                    if (b.Text == Comprobante)
                    {
                        b.Visible = false;
                    }
                    b.Click += miEvento;
                }
                CONEXION.CONEXIONMAESTRA.cerrar();
            }
            catch (Exception ex)
            {
                OpenPOS.Logging.Logger.ShowError(ex);
            }
        }

        private void miEvento(object sender, EventArgs e)
        {
            idcomprobante = Convert.ToInt32(((Button)sender).Name);
            Envioinmediato = Convert.ToString(((Button)sender).Tag);
            Comprobante = ((Button)sender).Text;
            lblComprobante.Text = Comprobante;
            dibujarComprobantes();
            identificarTipopago();
            validarPedidodeCliente();

        }
        private void validarPedidodeCliente()
        {


            if (Comprobante == "FACTURA" && Tipopago == "CREDITO")
            {
                panelClienteFactura.Visible = false;
            }
            if (Comprobante == "FACTURA" && Tipopago == "EFECTIVO")
            {
                panelClienteFactura.Visible = true;
                lblCliente.Text = "Cliente: (Obligatorio)";
                lblCliente.ForeColor = Color.Crimson;

            }
            else if (Comprobante != "FACTURA" && Tipopago == "EFECTIVO")
            {
                panelClienteFactura.Visible = true;
                lblCliente.Text = "Cliente: (Opcional)";
                lblCliente.ForeColor = Color.DimGray;

            }

            if (Comprobante == "FACTURA")
            {
                panelClienteFactura.Visible = true;
                lblCliente.Text = "Cliente: (Obligatorio)";
                lblCliente.ForeColor = Color.Crimson;

            }
            else if (Comprobante != "FACTURA" && Tipopago == "TARJETA")
            {
                panelClienteFactura.Visible = true;
                lblCliente.Text = "Cliente: (Opcional)";
                lblCliente.ForeColor = Color.DimGray;
            }
            else if (Comprobante != "FACTURA" && Tipopago != "CREDITO")
            {
                panelClienteFactura.Visible = true;
                lblCliente.Text = "Cliente: (Opcional)";
                lblCliente.ForeColor = Color.DimGray;
            }


        }
        private void ValidarTiposBusqueda()
        {
            var funcion = new Dempresa();
            var dt = new DataTable();
            funcion.mostrarTipobusqueda(ref dt);
            // Si la empresa no está configurada aún, se usan valores por defecto
            // (búsqueda por teclado) en lugar de caerse con Rows[0].
            Tipobusqueda = dt != null && dt.Rows.Count > 0 ? UtilesSeguros.Celda(dt, 0, 0, "TECLADO") : "TECLADO";
            conexionsunat = dt != null && dt.Rows.Count > 0 ? UtilesSeguros.Celda(dt, 0, 1) : "";
            moneda = dt != null && dt.Rows.Count > 0 ? UtilesSeguros.Celda(dt, 0, 2) : "";
            if (Tipobusqueda == "TECLADO")
            {
                btnlectora.BackColor = Color.WhiteSmoke;
                btnteclado.BackColor = Color.LightGreen;
                txtbuscar.Clear();
                txtbuscar.Focus();
            }
            else
            {
                btnlectora.BackColor = Color.LightGreen;
                btnteclado.BackColor = Color.WhiteSmoke;
                txtbuscar.Focus();
                txtbuscar.Clear();

            }
        }
        private void txtbuscar_TextChanged(object sender, EventArgs e)
        {
            Validarbusquedas();
        }
        private void Validarbusquedas()
        {
            if (txtbuscar.Text == "")
            {
                ocultarPanelprod();
            }
            else if (txtbuscar.Text != "")
            {
                mostrarPanelprod();
            }
        }
        private void mostrarPanelprod()
        {
            Buscarproductos();
            panelProductos.Size = new System.Drawing.Size(600, 186);
            panelProductos.BackColor = Color.White;
            panelProductos.Location = new Point(txtbuscar.Location.X, txtbuscar.Location.Y + 22);
            panelProductos.Visible = true;
            dgProductos.Visible = true;
            dgProductos.Dock = DockStyle.Fill;
            dgProductos.BackgroundColor = Color.White;
            panelProductos.Controls.Add(dgProductos);
            this.Controls.Add(panelProductos);
            panelProductos.BringToFront();
        }
        private void ocultarPanelprod()
        {
            panelProductos.Visible = false;
            dgProductos.Visible = false;
        }
        private void Buscarproductos()
        {
            var dt = new DataTable();
            var funcion = new Dproductos();
            funcion.buscarProductos(ref dt, txtbuscar.Text);
            dgProductos.DataSource = dt;
            dgProductos.Columns[0].Visible = false;
            dgProductos.Columns[2].Visible = false;
            dgProductos.Columns[4].Visible = false;
        }

        private void txtcantidad_KeyPress(object sender, KeyPressEventArgs e)
        {
            Bases.Separador_de_Numeros(txtcantidad, e);
        }

        private void dgProductos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            idproducto = Convert.ToInt32(dgProductos.SelectedCells[0].Value);
            txtbuscar.Clear();
            dgProductos.Visible = false;
            InsertarVenta();
        }
        private void validarCantidad()
        {
            if (string.IsNullOrEmpty(txtcantidad.Text)) { txtcantidad.Text = "1"; }
            cantidad = Convert.ToDecimal(txtcantidad.Text);
        }
        private void InsertarVenta()
        {
            validarCantidad();
            if (ventagenerada == "NUEVA")
            {
                var funcion = new Dventas();
                funcion.insertarVenta(ref idventa);
                ventagenerada = "GENERADA";
                panelBienvenida.Visible = false;
                PanelTotales.Visible = true;
            }
            if (ventagenerada == "GENERADA")
            {
                InsertarDv();
            }
        }
        private void InsertarDv()
        {
            var funcion = new Ddetalleventas();
            var parametros = new LdetalleVenta();
            parametros.idventa = idventa;
            parametros.Id_producto = idproducto;
            parametros.cantidad = cantidad;
            funcion.insertarDetalleVenta(parametros);
            mostrarDv();
            txtcantidad.Text = "1";
        }
        private void mostrarDv()
        {
            var funcion = new Ddetalleventas();
            var dt = new DataTable();
            var parametros = new LdetalleVenta();
            parametros.idventa = idventa;
            funcion.MostrarDetalleVenta(ref dt, parametros);
            dgDetalleventa.DataSource = dt;
            Bases.Multilinea(ref dgDetalleventa);
            dgDetalleventa.Columns[8].Visible = false;
            dgDetalleventa.Columns[9].Visible = false;
            dgDetalleventa.Columns[10].Visible = false;
            dgDetalleventa.Columns[11].Visible = false;
            dgDetalleventa.Columns[12].Visible = false;
            dgDetalleventa.Columns[13].Visible = false;
            dgDetalleventa.Columns[14].Visible = false;
            dgDetalleventa.Columns[15].Visible = false;
            dgDetalleventa.Columns[16].Visible = false;

            sumar();
        }
        private void sumar()
        {
            try
            {
                decimal totalInterno = 0;
                foreach (DataGridViewRow fila in dgDetalleventa.Rows)
                {
                    totalInterno += Convert.ToDecimal(fila.Cells["Importe"].Value);
                    lbltotal.Text = moneda + " " + Convert.ToString(totalInterno);
                    total = totalInterno;
                }
            }
            catch (Exception ex)
            {
                OpenPOS.Logging.Logger.ShowError(ex);
            }
        }

        private void btnlectora_Click(object sender, EventArgs e)
        {
            ModoLectora();
        }
        public void ModoLectora()
        {
            ocultarPanelprod();
            Tipobusqueda = "LECTORA";
            btnlectora.BackColor = Color.LightGreen;
            btnteclado.BackColor = Color.WhiteSmoke;
            txtbuscar.Clear();
            txtbuscar.Focus();
        }
        private void Modoteclado()
        {
            ocultarPanelprod();
            Tipobusqueda = "TECLADO";
            btnteclado.BackColor = Color.LightGreen;
            btnlectora.BackColor = Color.WhiteSmoke;
            txtbuscar.Clear();
            txtbuscar.Focus();
        }

        private void btnteclado_Click(object sender, EventArgs e)
        {
            Modoteclado();
        }
        private void btncobrar_Click(object sender, EventArgs e)
        {
            Cobrar();
        }
        private void Cobrar()
        {
            panelCobro.Location = new Point((Width - panelCobro.Width) / 2, (Height - panelCobro.Height) / 2);
            panelCobro.Visible = true;
            PanelTotales.Visible = false;
            PanelC1.Visible = false;
            dgDetalleventa.Visible = false;
            PanelOperaciones.Visible = false;
            lblTotalref.Text = lbltotal.Text;
            txtefectivo.Text = total.ToString();
            txttarjeta.Clear();
            txtcredito.Clear();
            txtreferencia.Clear();
            lblvuelto.Text = "0";
            idcliente = 0;
            ObtenerComprobanteDefecto();
        }
        private void btnCerrrar_Click(object sender, EventArgs e)
        {
            panelCobro.Visible = false;
            PanelTotales.Visible = true;
            PanelC1.Visible = true;
            dgDetalleventa.Visible = true;
            PanelOperaciones.Visible = true;
        }

        private void btnguardar_Click(object sender, EventArgs e)
        {
            Procesar();
        }
        private void Procesar()
        {
            if (restante == 0)
            {
                identificarTipopago();
                Validartexbox();
                validarTipopago();
            }
            else
            {
                DialogService.Show("El restante debe ser 0", "Datos incorrectos", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private void validarTipopago()
        {
            int contadorreferencia = 0;
            contadorreferencia = txtreferencia.Text.Length;
            if ((Tipopago == "TARJETA" || Tipopago == "MIXTO") && txtreferencia.Text == "")
            {
                DialogService.Show("Ingrese los últimos 4 dígitos de la tarjeta", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if ((Tipopago == "TARJETA" || Tipopago == "MIXTO") && contadorreferencia < 4)
            {
                DialogService.Show("Ingrese los últimos 4 dígitos de la tarjeta", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if ((Tipopago == "TARJETA" || Tipopago == "MIXTO") && contadorreferencia > 4)
            {
                DialogService.Show("Ingrese los últimos 4 dígitos de la tarjeta", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (Tipopago == "EFECTIVO" && vuelto >= 0)
                {
                    Procesarventa();
                }
                else if (Tipopago == "EFECTIVO" && vuelto < 0)
                {
                    DialogService.Show("El vuelto no puede ser menor al total pagado", "Datos incorrectos", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                if (Tipopago == "TARJETA")
                {
                    Procesarventa();
                }


                if (Tipopago == "MIXTO")
                {
                    Procesarventa();
                }
            }
        }

        private void ValidarClienteStandar()
        {
            if (idcliente == 0)
            {
                MostrarclienteStandar();
            }
        }
        void MostrarclienteStandar()
        {
            var funcion = new Dclientes();
            funcion.mostrarClienteStandar(ref idcliente);
        }
        private void Procesarventa()
        {

            if (Comprobante == "FACTURA" && idcliente == 0 && Tipopago != "CREDITO")
            {
                DialogService.Show("Seleccione un Cliente, para Facturas es Obligatorio", "Datos incorrectos", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else if (Comprobante == "FACTURA" && idcliente != 0)
            {
                Terminarventa();
            }

            else if (Comprobante != "FACTURA" && Tipopago != "CREDITO")
            {
                ValidarClienteStandar();
                Terminarventa();
            }
            else if (Comprobante != "FACTURA" && Tipopago == "CREDITO")
            {
                if (idcliente == 0)
                {
                    Terminarventa();
                }
                else
                {
                    DialogService.Show("Seleccione un Cliente", "Datos incorrectos", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

            }


        }
        void Terminarventa()
        {
            confirmarVenta();
            if (conexionsunat == "SI" && Comprobante != "TICKET")
            {
                if (Envioinmediato == "SI")
                {
                    EmitirFacturaContado();
                }

            }
            imprimir_directo();
            Ventanueva();

        }

        void convertirTotalaletras()
        {
            try
            {
                int numero = Convert.ToInt32(Math.Floor(Convert.ToDecimal(total)));
                var totalString = CONEXION.total_en_letras.Num2Text(numero);
                string[] a = total.ToString().Split('.');
                string txttotaldecimal;
                txttotaldecimal = a[1];
                Totalletras = totalString + " CON " + txttotaldecimal + "/100 ";
            }
            catch (Exception ex)
            {
                OpenPOS.Logging.Logger.ShowError(ex);
            }

        }
        private void MostrarImpresora()
        {
            var funcion = new Dcaja();
            var dt = new DataTable();
            funcion.MostrarImprTicket(ref dt);
            // Si no hay impresora configurada, se deja vacía sin lanzar excepción.
            Impresora = dt != null && dt.Rows.Count > 0 ? UtilesSeguros.Celda(dt, 0, 0) : "";
        }
        private void MostrarTicket()
        {
            var dt = new DataTable();
            var funcion = new Dticket();
            var parametros = new Lticket();
            parametros.Idventa = idventa;
            parametros.Totalletras = Totalletras;
            funcion.mostrarTicket(ref dt, parametros);
            var rpt = new Ticket_report();
            rpt.table1.DataSource = dt;
            rpt.DataSource = dt;
            reportViewer1.Report = rpt;
            reportViewer1.RefreshReport();
        }
        private void imprimir_directo()
        {
            MostrarTicket();
            MostrarImpresora();
            try
            {
                ReportPainter.PrintTicket(reportViewer1.Report, Impresora);
            }
            catch (Exception ex)
            {
                OpenPOS.Logging.Logger.ShowError(ex);
            }
        }
        private void Ventanueva()
        {
            panelCobro.Visible = false;
            txtbuscar.Clear();
            idventa = 0;
            ventagenerada = "NUEVA";
            PanelTotales.Visible = true;
            PanelC1.Visible = true;
            dgDetalleventa.Visible = true;
            PanelOperaciones.Visible = true;
            lbltotal.Text = "0";
            txtbuscarcliente.Clear();

            mostrarDv();
        }
        private void EmitirFacturaContado()
        {
            var parametrosVentas = new Lventas();
            var funcionVentas = new Dventas();
            var dtventas = new DataTable();
            parametrosVentas.idventa = idventa;
            funcionVentas.mostrarVentasXidventa(ref dtventas, parametrosVentas);

            foreach (DataRow dataventas in dtventas.Rows)
            {
                parametrosVentas.idventa = Convert.ToInt32(dataventas["idventa"]);
                parametrosVentas.Serie = dataventas["Serie"].ToString();
                parametrosVentas.Correlativo = dataventas["Correlativo"].ToString();
                parametrosVentas.fecha_venta = Convert.ToDateTime(dataventas["fecha_venta"]);
                parametrosVentas.Fecha_de_pago = Convert.ToDateTime(dataventas["Fecha_de_pago"]);
                parametrosVentas.CodigoComprobante = dataventas["Codigo"].ToString();
                parametrosVentas.contadorProductos = Convert.ToInt32(dataventas["ContadorProductos"]);
                parametrosVentas.EmpresaRUCemisor = dataventas["Ruc"].ToString();
                parametrosVentas.EmpresaRazonsocialEmisora = dataventas["RazonSocial"].ToString();
                parametrosVentas.Ubigeo = dataventas["Ubigeo"].ToString();
                var parametrosUbigeos = new Lcodigosubigeos();
                ObtenerUbigeos(parametrosVentas.Ubigeo, parametrosUbigeos);
                parametrosVentas.DptoempresaEmisora = parametrosUbigeos.Departamento;
                parametrosVentas.ProvempresaEmisora = parametrosUbigeos.Provincia;
                parametrosVentas.DistmpresaEmisora = parametrosUbigeos.Distrito;
                parametrosVentas.DireccionEmpresaEmisora = dataventas["DireccionFiscal"].ToString();
                parametrosVentas.EmpresaRUCcliente = dataventas["NroDoc"].ToString();
                parametrosVentas.EmpresaRazonsocialCliente = dataventas["Nombre"].ToString();
                parametrosVentas.DireccionCliente = dataventas["Direccion"].ToString();
                parametrosVentas.TotalIgv = Convert.ToDecimal(dataventas["TotalIgv"]);
                parametrosVentas.TotSubtotal = Convert.ToDecimal(dataventas["TotSubtotal"]);
                parametrosVentas.Monto_total = Convert.ToDecimal(dataventas["Monto_total"]);
                parametrosVentas.Porcentaje_IGV = Convert.ToDouble(dataventas["Porcentaje_IGV"]);

            }
            var funciondeDv = new Ddetalleventas();
            var parametrosDv = new LdetalleVenta();
            var dtDv = new DataTable();
            parametrosDv.idventa = parametrosVentas.idventa;
            funciondeDv.MostrarDetalleVenta(ref dtDv, parametrosDv);
            var detalle = new List<LdetalleVenta>();
            foreach (DataRow items in dtDv.Rows)
            {
                var datadv = new LdetalleVenta();
                datadv.Unidad_de_medida = items["Unidad_de_medida"].ToString();
                datadv.cantidad = Convert.ToDecimal(items["Cant"]);
                datadv.Total_a_pagar = Convert.ToDecimal(items["Importe"]);
                datadv.preciounitario = Convert.ToDecimal(items["P_Unit"]);
                datadv.Descripcion = items["Producto"].ToString();
                datadv.Codigo = items["Codigo"].ToString();
                datadv.CodigoProdSunat = items["CodProdSunat"].ToString();
                detalle.Add(datadv);
            }
            int estado = 0;
            var funcion = new EmitirComprobante();
            estado = funcion.EmitirFacturasContado(parametrosVentas, detalle);
            if (estado == 1)
            {
                editarEstadoSunatVenta();
            }
        }
        private void editarEstadoSunatVenta()
        {
            var funcion = new Dventas();
            var parametros = new Lventas();
            parametros.idventa = idventa;
            funcion.editarEstadoSunatVenta(parametros);
        }
        private void ObtenerUbigeos(string ubigeo, Lcodigosubigeos parametros)
        {
            var dt = new DataTable();
            var funcion = new Dcodigosubigeo();
            var parametrosUbigeo = new Lcodigosubigeos();
            parametrosUbigeo.Ubigeo = ubigeo;
            funcion.ObtenerUbicaionXubigeo(ref dt, parametrosUbigeo);
            // Si el ubigeo no existe, se dejan vacíos en vez de explotar con Rows[0].
            parametros.Departamento = dt != null && dt.Rows.Count > 0 ? UtilesSeguros.Celda(dt, 0, 0) : "";
            parametros.Provincia = dt != null && dt.Rows.Count > 0 ? UtilesSeguros.Celda(dt, 0, 1) : "";
            parametros.Distrito = dt != null && dt.Rows.Count > 0 ? UtilesSeguros.Celda(dt, 0, 2) : "";

        }
        void confirmarVenta()
        {
            var funcion = new Dventas();
            var parametros = new Lventas();
            parametros.idventa = idventa;
            parametros.idclientev = idcliente;
            parametros.Monto_total = total;
            parametros.Tipo_de_pago = Tipopago;
            parametros.Saldo = vuelto;
            parametros.Pago_con = Convert.ToDecimal(txtefectivo.Text);
            parametros.Referencia_tarjeta = txtreferencia.Text;
            parametros.Vuelto = vuelto;
            parametros.Efectivo = efectivo_calculado;
            parametros.Credito = Convert.ToDecimal(txtcredito.Text);
            parametros.Tarjeta = Convert.ToDecimal(txttarjeta.Text);
            parametros.Idcomprobante = idcomprobante;
            funcion.confirmarVenta(parametros);
            convertirTotalaletras();
            DisminuirStock();
        }
        private void DisminuirStock()
        {
            var funcion = new Dproductos();
            var parametros = new Lproductos();

            var funciondeDv = new Ddetalleventas();
            var parametrosDv = new LdetalleVenta();
            var dtDv = new DataTable();
            parametrosDv.idventa = idventa;
            funciondeDv.MostrarDetalleVenta(ref dtDv, parametrosDv);
            foreach (DataRow items in dtDv.Rows)
            {
                string UsaInventarios = items["Usa_inventarios"].ToString();
                if (UsaInventarios == "SI")
                {
                    int idproducto = Convert.ToInt32(items["Id_producto"]);
                    double cantidad = Convert.ToDouble(items["Cant"]);
                    parametros.Id_Producto1 = idproducto;
                    parametros.Cantidad = cantidad;
                    funcion.disminuirStock(parametros);
                }
            }
        }

        void Validartexbox()
        {
            if (txtefectivo.Text == "")
            {
                txtefectivo.Text = "0";
            }
            if (txtcredito.Text == "")
            {
                txtcredito.Text = "0";
            }
            if (txttarjeta.Text == "")
            {
                txttarjeta.Text = "0";
            }
            if (lblvuelto.Text == "")
            {
                lblvuelto.Text = "0";
            }
        }

        private void identificarTipopago()
        {
            int indicadorEfectivo = 4;
            int indicadorCredito = 2;
            int indicadorTarjeta = 3;

            // validacion para evitar valores vacios
            if (txtefectivo.Text == "")
            {
                txtefectivo.Text = "0";
            }
            if (txttarjeta.Text == "")
            {
                txttarjeta.Text = "0";
            }
            if (txtcredito.Text == "")
            {
                txtcredito.Text = "0";
            }
            //validacion de .
            if (txtefectivo.Text == ".")
            {
                txtefectivo.Text = "0";
            }
            if (txttarjeta.Text == ".")
            {
                txttarjeta.Text = "0";
            }
            if (txtcredito.Text == ".")
            {
                txtcredito.Text = "0";
            }
            //validacion de 0
            if (txtefectivo.Text == "0")
            {
                indicadorEfectivo = 0;
            }
            if (txttarjeta.Text == "0")
            {
                indicadorTarjeta = 0;
            }
            if (txtcredito.Text == "0")
            {
                indicadorCredito = 0;
            }
            //calculo de indicador
            int calculo_identificacion = indicadorCredito + indicadorEfectivo + indicadorTarjeta;
            //consulta al identificador
            if (calculo_identificacion == 4)
            {
                Tipopago = "EFECTIVO";
            }
            if (calculo_identificacion == 2)
            {
                Tipopago = "CREDITO";
            }
            if (calculo_identificacion == 3)
            {
                Tipopago = "TARJETA";
            }
            if (calculo_identificacion > 4)
            {
                Tipopago = "MIXTO";
            }

        }
        private void txtefectivo_TextChanged(object sender, EventArgs e)
        {
            calcular_restante();
        }
        void calcular_restante()
        {
            try
            {
                decimal efectivo = 0;
                decimal tarjeta = 0;

                if (txtefectivo.Text == "")
                {
                    efectivo = 0;
                }
                else
                {
                    efectivo = Convert.ToDecimal(txtefectivo.Text);
                }
                if (txtcredito.Text == "")
                {
                    credito = 0;
                }
                else
                {
                    credito = Convert.ToDecimal(txtcredito.Text);
                }
                if (txttarjeta.Text == "")
                {
                    tarjeta = 0;
                }
                else
                {
                    tarjeta = Convert.ToDecimal(txttarjeta.Text);
                }

                if (txtefectivo.Text == "0.00")
                {
                    efectivo = 0;
                }
                if (txtcredito.Text == "0.00")
                {
                    credito = 0;
                }
                if (txttarjeta.Text == "0.00")
                {
                    tarjeta = 0;

                }

                if (txtefectivo.Text == ".")
                {
                    efectivo = 0;
                }
                if (txtcredito.Text == ".")
                {
                    tarjeta = 0;
                }
                if (txttarjeta.Text == ".")
                {
                    credito = 0;
                }

                try
                {
                    if (efectivo > total)
                    {
                        efectivo_calculado = efectivo - (total + credito + tarjeta);
                        if (efectivo_calculado < 0)
                        {
                            vuelto = 0;
                            lblvuelto.Text = "0";
                            txtrestante.Text = Convert.ToString(efectivo_calculado);
                            restante = efectivo_calculado;
                        }
                        else
                        {
                            vuelto = efectivo - (total - credito - tarjeta);
                            lblvuelto.Text = Convert.ToString(vuelto);
                            restante = efectivo - (total + credito + tarjeta + efectivo_calculado);
                            txtrestante.Text = Convert.ToString(restante);
                            txtrestante.Text = decimal.Parse(txtrestante.Text).ToString("##0.00");
                        }

                    }
                    else
                    {
                        vuelto = 0;
                        lblvuelto.Text = "0";
                        efectivo_calculado = efectivo;
                        restante = total - efectivo_calculado - credito - tarjeta;
                        txtrestante.Text = Convert.ToString(restante);
                        txtrestante.Text = decimal.Parse(txtrestante.Text).ToString("##0.00");
                    }
                }
                catch (Exception ex)
                {
                    OpenPOS.Logging.Logger.ShowError(ex);
                }

            }
            catch (Exception ex)
            {
                OpenPOS.Logging.Logger.ShowError(ex);
            }
        }

        private void txttarjeta_TextChanged(object sender, EventArgs e)
        {
            calcular_restante();
        }

        private void txtcredito_TextChanged(object sender, EventArgs e)
        {
            calcular_restante();
        }

        private void btnAgregarcliente_Click(object sender, EventArgs e)
        {
            var ctl = new Agregarcliente();
            ctl.Dock = DockStyle.Fill;
            panelCobro.Controls.Add(ctl);
            ctl.BringToFront();
        }

        private void txtbuscarcliente_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtbuscarcliente.Text))
            {
                buscarclientes();
            }
            else
            {
                dgClientes.Visible = false;
            }


        }
        private void buscarclientes()
        {
            DataTable dt = new DataTable();
            Obtener_datos.buscar_clientes(ref dt, txtbuscarcliente.Text);
            dgClientes.DataSource = dt;
            dgClientes.Visible = true;
            dgClientes.Size = new Size(288, 78);
            dgClientes.Location = new Point(panelClienteFactura.Location.X, panelClienteFactura.Location.Y + 80);
            dgClientes.Columns[0].Visible = false;
            dgClientes.Columns[2].Visible = false;
            dgClientes.Columns[3].Visible = false;
            dgClientes.Columns[4].Visible = false;
            dgClientes.Columns[5].Visible = false;
            dgClientes.Columns[6].Visible = false;
            dgClientes.BringToFront();

        }

        private void dgClientes_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            idcliente = Convert.ToInt32(dgClientes.SelectedCells[0].Value);
            txtbuscarcliente.Text = dgClientes.SelectedCells[1].Value.ToString();
            dgClientes.Visible = false;
        }

        private void txtefectivo_KeyPress(object sender, KeyPressEventArgs e)
        {
            Bases.Separador_de_Numeros(txtefectivo, e);
        }

        private void txttarjeta_KeyPress(object sender, KeyPressEventArgs e)
        {
            Bases.Separador_de_Numeros(txttarjeta, e);

        }

        private void txtcredito_KeyPress(object sender, KeyPressEventArgs e)
        {
            Bases.Separador_de_Numeros(txtcredito, e);

        }

        private void txtreferencia_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar))
            {
                e.Handled = false;
            }
            else if (Char.IsControl(e.KeyChar))
            {
                e.Handled = false;
            }
            else if (Char.IsSeparator(e.KeyChar))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void txtbuscarcliente_Click(object sender, EventArgs e)
        {
            txtbuscarcliente.SelectAll();
        }
        private void ObtenerdatosDV()
        {
            iddettallev = Convert.ToInt32(dgDetalleventa.SelectedCells[9].Value.ToString());
        }
        private void SumarCantDv()
        {
            var funcion = new Ddetalleventas();
            var parametros = new LdetalleVenta();
            parametros.iddetalle_venta = iddettallev;
            funcion.SumarCantDv(parametros);
            mostrarDv();
        }
        private void EliminarDv()
        {
            var funcion = new Ddetalleventas();
            var parametros = new LdetalleVenta();
            parametros.iddetalle_venta = iddettallev;
            funcion.EliminarDv(parametros);
            mostrarDv();
        }
        private void dgDetalleventa_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            ObtenerdatosDV();
            if (e.ColumnIndex == this.dgDetalleventa.Columns["S"].Index)
            {
                SumarCantDv();
            }
            if (e.ColumnIndex == this.dgDetalleventa.Columns["R"].Index)
            {
                RestarCantDv();
                EliminarVentas();
            }
            if (e.ColumnIndex == this.dgDetalleventa.Columns["EL"].Index)
            {
                EliminarDv();
                EliminarVentas();
            }

        }
        private void RestarCantDv()
        {
            var funcion = new Ddetalleventas();
            var parametros = new LdetalleVenta();
            parametros.iddetalle_venta = iddettallev;
            parametros.idventa = idventa;
            funcion.RestarCantDv(parametros);
            mostrarDv();
        }
        private void EliminarVentas()
        {
            if (dgDetalleventa.Rows.Count == 0)
            {
                var funcion = new Dventas();
                var parametros = new Lventas();
                parametros.idventa = idventa;
                funcion.eliminarVenta(parametros);
                ventagenerada = "NUEVA";
                idventa = 0;
            }

        }

        private void btnCaja_Click(object sender, EventArgs e)
        {
            var ctl = new Cierrecaja();
            ctl.Size = new Size(Width, Height);
            this.Controls.Add(ctl);
            ctl.BringToFront();
        }

        private void btndevoluciones_Click(object sender, EventArgs e)
        {
            var ctl = new HistorialVentasForm();
            ctl.Size = new Size(Width, Height);
            this.Controls.Add(ctl);
            ctl.BringToFront();
        }

        private void btnMovcaja_Click(object sender, EventArgs e)
        {
            var ctl = new Listado_gastos_ingresos();
            ctl.Size = new Size(Width, Height);
            this.Controls.Add(ctl);
            ctl.BringToFront();
        }

        private void btningreso_Click(object sender, EventArgs e)
        {
            var ctl = new IngresosVarios();
            ctl.Size = new Size(Width, Height);
            this.Controls.Add(ctl);
            ctl.BringToFront();
        }

        private void btnsalida_Click(object sender, EventArgs e)
        {
            var ctl = new Gastos();
            ctl.Size = new Size(Width, Height);
            this.Controls.Add(ctl);
            ctl.BringToFront();
        }

        private void btneliminar_Click(object sender, EventArgs e)
        {
            if (dgDetalleventa.RowCount > 0)
            {
                var r = DialogService.Show("¿Realmente desea eliminar esta Venta?", "Eliminando registros", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (r == DialogResultCustom.OK)
                {
                    var funcion = new Dventas();
                    var parametros = new Lventas();
                    parametros.idventa = idventa;
                    funcion.eliminarVenta(parametros);
                    Ventanueva();
                }
            }
        }

        private void btnmayoreo_Click(object sender, EventArgs e)
        {
            aplicarPreciomayoreo();
        }
        private void aplicarPreciomayoreo()
        {
            if (dgDetalleventa.Rows.Count > 0)
            {
                LdetalleVenta parametros = new LdetalleVenta();
                Editar_datos funcion = new Editar_datos();
                parametros.Id_producto = idproducto;
                parametros.iddetalle_venta = iddettallev;
                if (funcion.aplicar_precio_mayoreo(parametros) == true)
                {
                    mostrarDv();
                }
            }

        }
    }
}
