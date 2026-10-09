using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Management;
using System.Data.SqlClient;
using System.Xml;
using System.Diagnostics;

namespace OpenPOS.Presentacion.Asistenteinstalacion
{
    public partial class Instalacionservidor : Form
    {
        public Instalacionservidor()
        {
            InitializeComponent();
        }
        string nombre_del_equipo_usuario;

        private void Instalacionservidor_Load(object sender, EventArgs e)
        {
            centrarPaneles();
            Reemplazar();
            comprobar_si_ya_hay_servidor_instalado_SQL_EXPRESS();
            Conectar();
        }
        private void Conectar()
        {
            if (btnInstalarServidor.Visible == true)
            {
                comprobar_si_ya_hay_servidor_instalado_SQL_NORMAL();

            }
        }
        private void Reemplazar()
        {
            //Solo modificar este campo
            txtCrear_procedimientos.Text = txtCrear_procedimientos.Text.Replace("BASEADACURSO", TXTbasededatos.Text);
            //********

            txtEliminarBase.Text = txtEliminarBase.Text.Replace("BASEADACURSO", TXTbasededatos.Text);
            txtCrearUsuarioDb.Text = txtCrearUsuarioDb.Text.Replace("ada369", txtusuario.Text);
            txtCrearUsuarioDb.Text = txtCrearUsuarioDb.Text.Replace("BASEADA", TXTbasededatos.Text);
            txtCrearUsuarioDb.Text = txtCrearUsuarioDb.Text.Replace("softwarereal", lblcontraseña.Text);
            //Adjuntando al texbox que contiene los procedimientos almacenados
            txtCrear_procedimientos.Text = txtCrear_procedimientos.Text + Environment.NewLine + txtCrearUsuarioDb.Text;
        }
        private void centrarPaneles()
        {
            nombre_del_equipo_usuario = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            Panel2.Location = new Point((Width - Panel2.Width) / 2, (Height - Panel2.Height) / 2);
            Cursor = Cursors.WaitCursor;
            Panel4.Visible = false;
            Panel4.Dock = DockStyle.None;
        }


        private void comprobar_si_ya_hay_servidor_instalado_SQL_EXPRESS()
        {
            txtservidor.Text = @".\" + lblnombredeservicio.Text;
            ejecutar_scryt_ELIMINARBase_comprobacion_de_inicio();
            ejecutar_scryt_crearBase_comprobacion_De_inicio();
        }

        private void comprobar_si_ya_hay_servidor_instalado_SQL_NORMAL()
        {
            txtservidor.Text = ".";
            ejecutar_scryt_ELIMINARBase_comprobacion_de_inicio();
            ejecutar_scryt_crearBase_comprobacion_De_inicio();
        }



        private CONEXION.AES aes = new CONEXION.AES();
        private void ejecutar_scryt_crearBase_comprobacion_De_inicio()
        {
            var cnn = new SqlConnection("Server=" + txtservidor.Text + "; " + "database=master; integrated security=yes");
            string s = "CREATE DATABASE " + CONEXION.SqlScriptRunner.IdentificadorSeguro(TXTbasededatos.Text);
            var cmd = new SqlCommand(s, cnn);
            try
            {
                cnn.Open();
                cmd.ExecuteNonQuery();
                SavetoXML(aes.Encrypt("Data Source=" + txtservidor.Text + ";Initial Catalog=" + TXTbasededatos.Text + ";Integrated Security=True", CONEXION.Desencryptacion.appPwdUnique, int.Parse("256")));
                ejecutar_scryt_crearProcedimientos_almacenados_y_tablas();
                Panel4.Visible = true;
                Panel4.Dock = DockStyle.Fill;
                Label1.Text = @"Instancia Encontrada...
            No Cierre esta Ventana, se cerrara Automaticamente cuando este todo Listo";
                Panel6.Visible = false;
                timer4.Start();
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                Panel6.Visible = true;
                btnInstalarServidor.Visible = true;
                Panel4.Visible = false;
                Panel4.Dock = DockStyle.None;
                lblbuscador_de_servidores.Text = "De click a Instalar Servidor, luego de click a SI cuando se le pida, luego presione ACEPTAR y espere por favor ";
            }


            finally
            {
                if (cnn.State == ConnectionState.Open)
                    cnn.Close();
            }
        }
        private void ejecutar_scryt_crearBase()
        {
            var cnn = new SqlConnection("Server=" + txtservidor.Text + "; " + "database=master; integrated security=yes");
            string s = "CREATE DATABASE " + CONEXION.SqlScriptRunner.IdentificadorSeguro(TXTbasededatos.Text);
            var cmd = new SqlCommand(s, cnn);
            try
            {
                cnn.Open();
                cmd.ExecuteNonQuery();
                SavetoXML(aes.Encrypt("Data Source=" + txtservidor.Text + ";Initial Catalog=" + TXTbasededatos.Text + ";Integrated Security=True", CONEXION.Desencryptacion.appPwdUnique, int.Parse("256")));
                ejecutar_scryt_crearProcedimientos_almacenados_y_tablas();
                timer4.Start();
            }
            catch (Exception ex)
            {
                OpenPOS.Logging.Logger.Error(ex, "No se pudo crear/aplicar el esquema de la base de datos.");
            }

            finally
            {
                if (cnn.State == ConnectionState.Open)
                    cnn.Close();
            }
        }
        string ruta;
        private void ejecutar_scryt_crearProcedimientos_almacenados_y_tablas()
        {
            ruta = Path.Combine(Directory.GetCurrentDirectory(), txtnombre_scrypt.Text + ".txt");
            try
            {
                string cadena = "Server=" + txtservidor.Text + ";Database=master;Integrated Security=True;";
                string script = CONEXION.SqlScriptRunner.Preparar(txtCrear_procedimientos.Text, TXTbasededatos.Text);
                CONEXION.SqlScriptRunner.Ejecutar(cadena, script);
                OpenPOS.Logging.Logger.Info("Esquema ejecutado correctamente en la base de datos " + TXTbasededatos.Text + ".");
            }
            catch (Exception ex)
            {
                OpenPOS.Logging.Logger.Error(ex, "Error al ejecutar el script de tablas y procedimientos.");
                throw;
            }
        }
        public void SavetoXML(object dbcnString)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("ConnectionString.xml");
            XmlElement root = doc.DocumentElement;
            root.Attributes[0].Value = Convert.ToString(dbcnString);
            XmlTextWriter writer = new XmlTextWriter("ConnectionString.xml", null);
            writer.Formatting = Formatting.Indented;
            doc.Save(writer);
            writer.Close();
        }
        private void ejecutar_scryt_ELIMINARBase_comprobacion_de_inicio()
        {
            string str;
            SqlConnection myConn = new SqlConnection("Data Source=" + txtservidor.Text + ";Initial Catalog=master;Integrated Security=True");
            str = txtEliminarBase.Text;
            SqlCommand myCommand = new SqlCommand(str, myConn);
            try
            {
                myConn.Open();
                myCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                OpenPOS.Logging.Logger.Warn("Comprobación inicial de base de datos: " + ex.Message);
            }
            finally
            {
                if ((myConn.State == ConnectionState.Open))
                {
                    myConn.Close();
                }
            }
        }
        private void ejecutar_scryt_ELIMINARBase()
        {
            string str;
            SqlConnection myConn = new SqlConnection("Data Source=" + txtservidor.Text + ";Initial Catalog=master;Integrated Security=True");
            str = txtEliminarBase.Text;
            SqlCommand myCommand = new SqlCommand(str, myConn);
            try
            {
                myConn.Open();
                myCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                OpenPOS.Logging.Logger.Warn("No se pudo eliminar la base de datos previa: " + ex.Message);
            }
            finally
            {
                if ((myConn.State == ConnectionState.Open))
                {
                    myConn.Close();
                }
            }
        }
        public static int milisegundo;
        public static int segundos;

        private void timer4_Tick(object sender, EventArgs e)
        {

            timer2.Stop();
            timer3.Stop();
            milisegundo += 1;
            mil3.Text = Convert.ToString(milisegundo);

            if (milisegundo == 60)
            {
                segundos += 1;
                seg3.Text = Convert.ToString(segundos);
                milisegundo = 0;
            }

            if (segundos == 15)
            {
                timer4.Stop();

                try
                {
                    File.Delete(ruta);
                }
                catch (Exception ex)
                {

                }

                Dispose();
                Application.Restart();
            }

        }

        private void Button2_Click(object sender, EventArgs e)
        {
            try
            {


                txtArgumentosini.Text = txtArgumentosini.Text.Replace("PRUEBAFINAL22", lblnombredeservicio.Text);
                TimerCRARINI.Start();
                executa();
                timer2.Start();
                Panel4.Visible = true;
                Panel4.Dock = DockStyle.Fill;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void executa()
        {
            try
            {
                Process Pross = new Process();
                Pross.StartInfo.FileName = "SQLEXPR_x86_ENU.exe";
                Pross.StartInfo.Arguments = "/ConfigurationFile=ConfigurationFile.ini /ACTION=Install /IACCEPTSQLSERVERLICENSETERMS /SECURITYMODE=SQL /SAPWD=" + lblcontraseña.Text + " /SQLSYSADMINACCOUNTS=" + nombre_del_equipo_usuario;

                Pross.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                // El instalador de SQL Server requiere privilegios elevados: se solicita UAC
                // aunque la aplicación se ejecute como usuario estándar.
                Pross.StartInfo.UseShellExecute = true;
                Pross.StartInfo.Verb = "runas";
                Pross.Start();

                Panel4.Visible = true;
                Panel4.Dock = DockStyle.Fill;

            }
            catch (Exception ex)
            {
                OpenPOS.Logging.Logger.Error(ex, "No se pudo iniciar el instalador de SQL Server Express.");
            }
        }

        private void TimerCRARINI_Tick(object sender, EventArgs e)
        {
            string rutaPREPARAR;
            StreamWriter sw;
            rutaPREPARAR = Path.Combine(Directory.GetCurrentDirectory(), "ConfigurationFile.ini");
            rutaPREPARAR = rutaPREPARAR.Replace("ConfigurationFile.ini", @"SQLEXPR_x86_ENU\ConfigurationFile.ini");


            if (File.Exists(rutaPREPARAR) == true)
            {
                TimerCRARINI.Stop();
            }

            try
            {
                sw = File.CreateText(rutaPREPARAR);
                sw.WriteLine(txtArgumentosini.Text);
                sw.Flush();
                sw.Close();
                TimerCRARINI.Stop();
            }
            catch (Exception ex)
            {

            }
        }

        private void timer5_Tick(object sender, EventArgs e)
        {

        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            milisegundo1 += 1;
            milise.Text = Convert.ToString(milisegundo1);
            if (milisegundo1 == 60)
            {
                segundos1 += 1;
                seg.Text = Convert.ToString(segundos1);

                milisegundo1 = 0;

            }

            if (segundos1 == 60)
            {
                minutos1 += 1;

                min.Text = Convert.ToString(minutos1);
                segundos1 = 0;
            }

            if (minutos1 == 1)
            {

                ejecutar_scryt_ELIMINARBase();
                ejecutar_scryt_crearBase();

            }
        }
        public static int milisegundo1;
        public static int segundos1;
        public static int minutos1;

        private void timer2_Tick(object sender, EventArgs e)
        {
            milisegundo1 += 1;
            milise.Text = Convert.ToString(milisegundo1);
            if (milisegundo1 == 60)
            {
                segundos1 += 1;
                seg.Text = Convert.ToString(segundos1);

                milisegundo1 = 0;

            }

            if (segundos1 == 60)
            {
                minutos1 += 1;

                min.Text = Convert.ToString(minutos1);
                segundos1 = 0;
            }

            if (minutos1 == 6)
            {
                timer2.Enabled = false;

                ejecutar_scryt_ELIMINARBase();
                ejecutar_scryt_crearBase();

                timer3.Start();
            }
        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void Panel10_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
