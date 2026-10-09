using OpenPOS.CONEXION;
using OpenPOS.Datos;
using Sunat.Logica;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace OpenPOS.Presentacion.CopiasBd
{
    public partial class GeneradorAutomatico : Form
    {
        public GeneradorAutomatico()
        {
            InitializeComponent();
        }
        int contador=10;
        string txtsoftware = "openPOS";
        string Base_De_datos = "BASEADACURSO";
        private Thread Hilo;
        private bool acaba = false;
        string lblfrecuencia;
        private void GeneradorAutomatico_Load(object sender, EventArgs e)
        {
            FormBorderStyle = FormBorderStyle.None;
            Mostrar_empresa();
            timerContador.Start();
        }
        private void GenerarCopia()
        {
            if (!string.IsNullOrEmpty(txtRuta.Text))
            {
                Hilo = new Thread(new ThreadStart(ejecucion));
                Pcargando.Visible = true;
                Hilo.Start();
                acaba = false;
                timer1.Start();

            }
            else
            {
                MessageBox.Show("Selecciona una Ruta donde Guardar las Copias de Seguridad", "Seleccione Ruta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRuta.Focus();

            }
        }
        private void ejecucion()
        {
            string miCarpeta = "Copias_de_Seguridad_de_" + txtsoftware;
            if (System.IO.Directory.Exists(txtRuta.Text + miCarpeta))
            {

            }
            else
            {
                System.IO.Directory.CreateDirectory(txtRuta.Text + miCarpeta);
            }
            string ruta_completa = txtRuta.Text + miCarpeta;
            string SubCarpeta = ruta_completa + @"\Respaldo_al_" + DateTime.Now.Day + "_" + (DateTime.Now.Month) + "_" + DateTime.Now.Year + "_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute;
            try
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(ruta_completa, SubCarpeta));

            }
            catch (Exception)
            {


            }
            try
            {
                string v_nombre_respaldo = Base_De_datos + ".bak";
                string nombreDb = CONEXION.SqlScriptRunner.IdentificadorSeguro(Base_De_datos);
                string rutaRespaldo = System.IO.Path.Combine(SubCarpeta, v_nombre_respaldo);
                CONEXIONMAESTRA.abrir();
                SqlCommand cmd = new SqlCommand("BACKUP DATABASE " + nombreDb + " TO DISK = @ruta", CONEXIONMAESTRA.conectar);
                cmd.Parameters.AddWithValue("@ruta", rutaRespaldo);
                cmd.ExecuteNonQuery();
                acaba = true;
            }
            catch (Exception ex)
            {
                acaba = false;
                OpenPOS.Logging.Logger.Error(ex, "No se pudo generar la copia de seguridad.");
                MessageBox.Show("No se pudo generar la copia de seguridad. Revisa el registro de logs.",
                    "Copias de seguridad", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void Mostrar_empresa()
        {
            DataTable dt = new DataTable();
            var funcion = new Dempresa();
            funcion.mostrar_empresa(ref dt);
            foreach (DataRow row in dt.Rows)
            {
                txtRuta.Text = row["Carpeta_para_copias_de_seguridad"].ToString();
                lblfrecuencia = row["Frecuencia_de_copias"].ToString();

            }
        }
        private void timerContador_Tick(object sender, EventArgs e)
        {
            contador -= 1;
            lbltiempo.Text = contador.ToString();
            if (contador==0)
            {
                contador = 0;
                timerContador.Stop();
                GenerarCopia();

            }
        }

        private void btncancelar_Click(object sender, EventArgs e)
        {

            Application.Exit();

        }

        private void ActiveForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Dispose();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (acaba == true)
            {
                timer1.Stop();
                Pcargando.Visible = false;
                editarRespaldos();

            }
        }
        private void editarRespaldos()
        {
            Lempresa parametros = new Lempresa();
            Editar_datos funcion = new Editar_datos();
            parametros.Carpeta_para_copias_de_seguridad = txtRuta.Text;
            parametros.Ultima_fecha_de_copia_de_seguridad = DateTime.Now.ToString();
            parametros.Ultima_fecha_de_copia_date = DateTime.Now;
            parametros.Frecuencia_de_copias = Convert.ToInt32(lblfrecuencia);
            if (funcion.editarRespaldos(parametros) == true)
            {
                Application.Exit();
            }
        }

        private void GeneradorAutomatico_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}
