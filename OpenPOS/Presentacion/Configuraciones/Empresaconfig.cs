using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.IO;
using OpenPOS.Datos;
using Sunat.Logica;
using OpenPOS.Presentacion.Menu;

namespace OpenPOS.Presentacion.Configuraciones
{
    public partial class Empresaconfig : UserControl
    {
        public Empresaconfig()
        {
            InitializeComponent();
        }

        private void btncerrar_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        // Carga inicial: primero los ubigeos disponibles, luego los datos de la empresa.
        // Cada paso es defensivo para que la pantalla jamás se caiga por datos faltantes.
        private void Empresaconfig_Load(object sender, EventArgs e)
        {
            try
            {
                Cargarubigeos();
                mostrarEmpresa();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar la configuración de la empresa:\n" + ex.Message,
                                "Empresaconfig", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        // -- campos de estado interno (evitan consultar la BD en cada acción) --
        int PorcImpuesto;
        string Trabajasconimpuestos;
        string Modobusqueda;
        string Conectarsunat;
        string Servidor;
        string Codigoubigeo;

        /// <summary>
        /// Llena los combos de Departamento, Provincia y Distrito con el listado de ubigeos.
        /// Si la consulta devuelve una tabla sin las columnas esperadas (o vacía),
        /// deja los combos sin datos en vez de lanzar una excepción.
        /// </summary>
        private void Cargarubigeos()
        {
            try
            {
                var funcion = new Dcodigosubigeo();
                var dt = new DataTable();
                funcion.mostrarCodigosubigeo(ref dt);

                // Solo se enlaza una columna si realmente existe en el resultado.
                if (dt.Columns.Contains("Departamento")) txtdepartamento.DisplayMember = "Departamento";
                if (dt.Columns.Contains("Provincia")) txtprovincia.DisplayMember = "Provincia";
                if (dt.Columns.Contains("Distrito")) txtdistrito.DisplayMember = "Distrito";

                // Asignar DataSource aunque esté vacío: el combo mostrará vacío, sin error.
                txtdepartamento.Sorted = false;
                txtdepartamento.DataSource = dt;
                txtprovincia.DataSource = dt;
                txtdistrito.DataSource = dt;
                txtdepartamento.SelectedIndex = -1;
                txtprovincia.SelectedIndex = -1;
                txtdistrito.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                OpenPOS.Logging.Logger.ShowError(ex);
            }
        }

        /// <summary>
        /// A partir del código de ubigeo rellena los campos de departamento/provincia/distrito.
        /// Si no hay coincidencia (por ejemplo, el ubigeo guardado ya no existe), los deja vacíos
        /// en lugar de explotar con Rows[0].
        /// </summary>
        private void ObtenerUbicaionXubigeo()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Codigoubigeo)) return;

                var funcion = new Dcodigosubigeo();
                var parametros = new Lcodigosubigeos();
                var dt = new DataTable();
                parametros.Ubigeo = Codigoubigeo;
                funcion.ObtenerUbicaionXubigeo(ref dt, parametros);

                if (dt != null && dt.Rows.Count > 0)
                {
                    txtdepartamento.Text = UtilesSeguros.Celda(dt, 0, 0);
                    txtprovincia.Text = UtilesSeguros.Celda(dt, 0, 1);
                    txtdistrito.Text = UtilesSeguros.Celda(dt, 0, 2);
                }
                else
                {
                    txtdepartamento.Text = "";
                    txtprovincia.Text = "";
                    txtdistrito.Text = "";
                }
            }
            catch (Exception ex)
            {
                OpenPOS.Logging.Logger.ShowError(ex);
            }
        }

        /// <summary>
        /// Recalcula el código de ubigeo según el texto actual de los tres combos.
        /// </summary>
        private void ObtenercodigoUbigeo()
        {
            var funcion = new Dcodigosubigeo();
            var parametros = new Lcodigosubigeos();
            parametros.Departamento = txtdepartamento.Text;
            parametros.Provincia = txtprovincia.Text;
            parametros.Distrito = txtdistrito.Text;
            funcion.ObtenercodigoUbigeo(ref Codigoubigeo, parametros);
        }

        /// <summary>
        /// Pinta los campos con los datos de la empresa registrada.
        /// Defensivo: si la tabla está vacía (empresa aún no registrada) muestra valores
        /// por defecto en vez de un IndexOutOfRange; el logo nulo/corrupto cae al logo oficial.
        /// </summary>
        private void mostrarEmpresa()
        {
            var funcion = new Dempresa();
            var dt = new DataTable();
            funcion.mostrar_empresa(ref dt);

            // Sin registro de empresa todavía: la carnada en blanco no debe tumbar la pantalla.
            if (dt == null || dt.Rows.Count == 0)
            {
                SiNoHayEmpresa();
                return;
            }

            txtrazonsocial.Text = UtilesSeguros.Celda(dt, 0, 15);
            txtmoneda.Text = UtilesSeguros.Celda(dt, 0, 4);
            Trabajasconimpuestos = UtilesSeguros.Celda(dt, 0, 5);
            txtporcentaje.Text = UtilesSeguros.Celda(dt, 0, 3);
            txtimpuesto.Text = UtilesSeguros.Celda(dt, 0, 2);
            Modobusqueda = UtilesSeguros.Celda(dt, 0, 6);
            txtRuta.Text = UtilesSeguros.Celda(dt, 0, 7);
            Conectarsunat = UtilesSeguros.Celda(dt, 0, 18);
            txtdireccionfiscal.Text = UtilesSeguros.Celda(dt, 0, 16);
            Codigoubigeo = UtilesSeguros.Celda(dt, 0, 17);
            txtRuc.Text = UtilesSeguros.Celda(dt, 0, 14);
            txtCertificado.Text = UtilesSeguros.Celda(dt, 0, 20);
            txtContraseñaCert.Text = UtilesSeguros.Celda(dt, 0, 21);
            txtUsuarioSunat.Text = UtilesSeguros.Celda(dt, 0, 22);
            txtContraseñaSunat.Text = UtilesSeguros.Celda(dt, 0, 23);
            Servidor = UtilesSeguros.Celda(dt, 0, 19);

            // -- logo de la empresa: si la BD no trae una imagen válida se usa el logo oficial --
            Image logo = UtilesSeguros.Imagen(UtilesSeguros.Bytes(dt, 0, 1));
            ImagenEmpresa.BackgroundImage = null;
            ImagenEmpresa.Image = logo ?? Properties.Resources.openposlogo;
            ImagenEmpresa.SizeMode = PictureBoxSizeMode.Zoom;

            // -- indicadores de impuestos --
            if (Trabajasconimpuestos == "SI")
            {
                si.Checked = true;
                PanelImpuesto.Visible = true;
            }
            else
            {
                no.Checked = true;
                PanelImpuesto.Visible = false;
            }

            // -- modo de búsqueda de productos --
            if (Modobusqueda == "LECTORA")
            {
                txtlectora.Checked = true;
                txtteclado.Checked = false;
            }
            else
            {
                txtlectora.Checked = false;
                txtteclado.Checked = true;
            }

            // -- conexión SUNAT --
            if (Conectarsunat == "SI")
            {
                checConectarsunat.Checked = true;
                panelSunat.Visible = true;
            }
            else
            {
                checConectarsunat.Checked = false;
                panelSunat.Visible = false;
            }

            // -- servidor SUNAT: distingue pruebas (beta) de producción --
            if (Servidor != null && Servidor.Contains("beta"))
            {
                cbxServidor.Text = "Pruebas";
            }
            else
            {
                cbxServidor.Text = "Produccion";
            }

            ObtenerUbicaionXubigeo();
        }

        /// <summary>
        /// Estado inicial cuando no existe empresa registrada: campos en blanco y logo oficial.
        /// </summary>
        private void SiNoHayEmpresa()
        {
            txtrazonsocial.Text = "";
            txtmoneda.Text = "";
            txtporcentaje.Text = "";
            txtimpuesto.Text = "";
            txtRuta.Text = "";
            txtRuc.Text = "";
            txtdireccionfiscal.Text = "";
            txtdepartamento.Text = "";
            txtprovincia.Text = "";
            txtdistrito.Text = "";
            txtCertificado.Text = "";
            txtContraseñaCert.Text = "";
            txtUsuarioSunat.Text = "";
            txtContraseñaSunat.Text = "";
            ImagenEmpresa.Image = Properties.Resources.openposlogo;
            ImagenEmpresa.SizeMode = PictureBoxSizeMode.Zoom;
            no.Checked = true;
            PanelImpuesto.Visible = false;
            txtlectora.Checked = false;
            txtteclado.Checked = true;
        }

        /// <summary>
        /// Guarda los datos generales de la empresa. Valida antes de persistir:
        /// razón social obligatoria, porcentaje numérico cuando se trabaja con impuestos.
        /// </summary>
        private void btnguardar_Click(object sender, EventArgs e)
        {
            // Validación mínima: no se puede guardar una empresa sin razón social.
            if (string.IsNullOrWhiteSpace(txtrazonsocial.Text))
            {
                MessageBox.Show("El campo Razón Social es obligatorio.", "Validación",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // Si se trabaja con impuestos el porcentaje debe ser numérico.
            if (si.Checked && !EsPorcentajeValido())
            {
                MessageBox.Show("El porcentaje de impuesto debe ser numérico (0-100).", "Validación",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            try
            {
                Editarempresa();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo guardar los cambios:\n" + ex.Message, "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Verifica que el porcentaje ingresado sea numérico y esté entre 0 y 100.
        /// </summary>
        private bool EsPorcentajeValido()
        {
            int p;
            if (!int.TryParse(txtporcentaje.Text.Trim(), out p)) return false;
            return p >= 0 && p <= 100;
        }

        /// <summary>
        /// Persiste los cambios de la empresa. El logo se envía solo si hay imagen válida.
        /// </summary>
        private void Editarempresa()
        {
            ValidarConexionsunat();
            ValidarImpuestos();

            var funcion = new Dempresa();
            var parametros = new Lempresa();
            parametros.RazonSocial = txtrazonsocial.Text.Trim();
            parametros.Impuesto = txtimpuesto.Text.Trim();
            parametros.Porcentaje_impuesto = PorcImpuesto;
            parametros.SimboloMoneda = txtmoneda.Text.Trim();
            parametros.Trabajas_con_impuestos = Trabajasconimpuestos;
            parametros.Modo_de_busqueda = Modobusqueda;
            parametros.Carpeta_para_copias_de_seguridad = txtRuta.Text.Trim();
            parametros.Correo_para_envio_de_reportes = "-";
            parametros.ConectarSunat = Conectarsunat;

            // El logo se convierte a bytes solo si hay imagen; si no existe se omite el dato.
            if (ImagenEmpresa.Image != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ImagenEmpresa.Image.Save(ms, ImagenEmpresa.Image.RawFormat);
                    parametros.Logo = ms.ToArray();
                }
            }

            funcion.EditarEmpresa(parametros);
            MessageBox.Show("Cambios guardados", "Guardando Cambios",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
            Dispose();
        }

        /// <summary>
        /// Decide si se guarda la conexión SUNAT (SI/NO) y valida esa sección cuando esté activada.
        /// </summary>
        private void ValidarConexionsunat()
        {
            if (checConectarsunat.Checked)
            {
                Conectarsunat = "SI";
                Validarsunat();
            }
            else
            {
                Conectarsunat = "NO";
            }
        }

        /// <summary>
        /// Calcula el porcentaje de impuesto. Si se trabaja sin impuestos, deja 0.
        /// Usa parseo seguro para no caerse con texto vacío.
        /// </summary>
        private void ValidarImpuestos()
        {
            if (si.Checked)
            {
                PorcImpuesto = UtilesSeguros.Entero(txtporcentaje.Text, 0);
                Trabajasconimpuestos = "SI";
            }
            else if (no.Checked)
            {
                PorcImpuesto = 0;
                Trabajasconimpuestos = "NO";
            }
        }

        /// <summary>
        /// Guarda los datos de conexión SUNAT (RUC, dirección, ubigeo, servidor y certificado).
        /// </summary>
        private void Validarsunat()
        {
            ObtenercodigoUbigeo();
            Validarservidor();

            var funcion = new Dempresa();
            var parametros = new Lempresa();
            parametros.Ruc = txtRuc.Text.Trim();
            parametros.DireccionFiscal = txtdireccionfiscal.Text.Trim();
            parametros.Ubigeo = Codigoubigeo;
            parametros.Servidor = Servidor;
            parametros.CarpetaCertificado = txtCertificado.Text.Trim();
            parametros.Passcertificado = txtContraseñaCert.Text;
            parametros.UserSecundario = txtUsuarioSunat.Text.Trim();
            parametros.PassSecundario = txtContraseñaSunat.Text;
            parametros.CodMoneda = txtCodigoMoneda.Text.Trim();
            funcion.editarSunatEmpresa(parametros);
        }

        /// <summary>
        /// Traduce la opción "Pruebas/Producción" a la URL oficial del servicio de SUNAT.
        /// </summary>
        private void Validarservidor()
        {
            if (cbxServidor.Text == "Pruebas")
            {
                Servidor = "https://e-beta.sunat.gob.pe/ol-ti-itcpfegem-beta/billService";
            }
            else
            {
                Servidor = "https://e-factura.sunat.gob.pe/ol-ti-itcpfegem/billService";
            }
        }

        private void si_CheckedChanged(object sender, EventArgs e)
        {
            PanelImpuesto.Visible = true;
        }

        private void no_CheckedChanged(object sender, EventArgs e)
        {
            PanelImpuesto.Visible = false;
        }

        private void txtlectora_CheckedChanged(object sender, EventArgs e)
        {
            if (txtlectora.Checked)
            {
                txtteclado.Checked = false;
                Modobusqueda = "LECTORA";
            }
            else
            {
                txtteclado.Checked = true;
                Modobusqueda = "TECLADO";
            }
        }

        private void txtteclado_CheckedChanged(object sender, EventArgs e)
        {
            if (txtteclado.Checked)
            {
                txtlectora.Checked = false;
            }
            else
            {
                txtlectora.Checked = true;
            }
        }

        /// <summary>
        /// Permite elegir un nuevo logo para la empresa desde archivo.
        /// </summary>
        private void lbleditarLogo_Click(object sender, EventArgs e)
        {
            dlg.InitialDirectory = "";
            dlg.Filter = "Imagenes|*.jpg;*.png;*.bmp";
            dlg.FilterIndex = 2;
            dlg.Title = "Cargador de Imagenes OpenPOS";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ImagenEmpresa.BackgroundImage = null;
                    ImagenEmpresa.Image = new Bitmap(dlg.FileName);
                    ImagenEmpresa.SizeMode = PictureBoxSizeMode.Zoom;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo cargar la imagen:\n" + ex.Message, "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        private void Label9_Click(object sender, EventArgs e)
        {
            Obtener_ruta();
        }

        /// <summary>
        /// Selecciona la carpeta de respaldos. Rechaza la unidad C: para evitar
        /// problemas de permisos al guardar copias de la base de datos.
        /// </summary>
        private void Obtener_ruta()
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                string ruta = folderBrowserDialog1.SelectedPath;
                if (ruta.Contains(@"C:\"))
                {
                    MessageBox.Show("Seleccione un disco diferente al C", "Ruta Invalida",
                                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    txtRuta.Text = ruta;
                }
            }
        }

        private void ToolStripButton22_Click(object sender, EventArgs e)
        {
            Obtener_ruta();
        }

        private void checConectarsunat_CheckedChanged(object sender, EventArgs e)
        {
            panelSunat.Visible = checConectarsunat.Checked;
        }

        private void label3_Click(object sender, EventArgs e)
        {
            Obtenerutacertificado();
        }

        /// <summary>
        /// Selecciona el archivo .pfx del certificado digital SUNAT.
        /// </summary>
        private void Obtenerutacertificado()
        {
            var dalg = new OpenFileDialog();
            dalg.InitialDirectory = "";
            dalg.Filter = "Certificado Sunat|*.pfx";
            dalg.FilterIndex = 2;
            dalg.Title = "Cargador de certificado digital";
            if (dalg.ShowDialog() == DialogResult.OK)
            {
                txtCertificado.Text = Path.GetFullPath(dalg.FileName);
            }
        }

        private void txtCertificado_Click(object sender, EventArgs e)
        {
            Obtenerutacertificado();
        }
    }
}