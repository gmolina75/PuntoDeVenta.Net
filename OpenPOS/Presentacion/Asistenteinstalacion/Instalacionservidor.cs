using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using OpenPOS.Logging;
using OpenPOS.UI;

namespace OpenPOS.Presentacion.Asistenteinstalacion
{
    public partial class Instalacionservidor : Form
    {
        public Instalacionservidor()
        {
            InitializeComponent();
        }

        private InstallConsole _consola;
        private readonly CONEXION.AES _aes = new CONEXION.AES();
        private string _nombreEquipoUsuario;
        private string _instanciaDefault;
        private bool _reiniciando;

        // Plantillas originales (con marcadores) capturadas al cargar. Los reemplazos
        // se aplican al ejecutar, de modo que el nombre de la base y las credenciales
        // indicadas por el técnico siempre se respetan.
        private string _plantillaProcedimientos;
        private string _plantillaUsuarioDb;
        private string _plantillaEliminar;
        private string _plantillaArgumentosIni;

        // Contexto capturado en el hilo de interfaz para usarlo en segundo plano
        // sin tocar los controles desde otro hilo.
        private sealed class Contexto
        {
            public string Servidor;
            public string NombreServicio;
            public string BaseDatos;
            public string UsuarioDb;
            public string Password;
            public string ArgumentosIni;
            public string UsuarioEquipo;
        }

        private void Instalacionservidor_Load(object sender, EventArgs e)
        {
            centrarPaneles();
            CapturarPlantillas();

            _consola = new InstallConsole { Dock = DockStyle.Fill, Visible = false };
            Controls.Add(_consola);

            lblbuscador_de_servidores.Text = "Detectando SQL Server en este equipo…";

            Task.Run(() => Detectar());
        }

        private void centrarPaneles()
        {
            _nombreEquipoUsuario = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            Panel2.Location = new Point((Width - Panel2.Width) / 2, (Height - Panel2.Height) / 2);
            Cursor = Cursors.Default;
            Panel4.Visible = false;
            Panel4.Dock = DockStyle.None;
        }

        private void CapturarPlantillas()
        {
            _plantillaProcedimientos = txtCrear_procedimientos.Text;
            _plantillaUsuarioDb = txtCrearUsuarioDb.Text;
            _plantillaEliminar = txtEliminarBase.Text;
            _plantillaArgumentosIni = txtArgumentosini.Text;
            _instanciaDefault = string.IsNullOrWhiteSpace(lblnombredeservicio.Text)
                ? "SQLEXPRESS"
                : lblnombredeservicio.Text.Trim();
        }

        private Contexto CrearContexto(DatosInstalacion datos, string servidor)
        {
            return new Contexto
            {
                Servidor = servidor,
                NombreServicio = datos.Instancia,
                BaseDatos = datos.BaseDatos,
                UsuarioDb = datos.Usuario,
                Password = datos.Contrasena,
                ArgumentosIni = _plantillaArgumentosIni.Replace("PRUEBAFINAL22", datos.Instancia),
                UsuarioEquipo = _nombreEquipoUsuario
            };
        }

        // ---- Detección y flujos -------------------------------------------

        private void Detectar()
        {
            try
            {
                string express = @"." + "\\" + _instanciaDefault;
                string servidor = null;
                if (PuedeConectar(express, 4)) servidor = express;
                else if (PuedeConectar(".", 4)) servidor = ".";

                if (servidor == null)
                {
                    MostrarInvitacion();
                    return;
                }

                DatosInstalacion datos = PedirDatos();
                if (datos == null)
                {
                    MostrarInvitacion();
                    return;
                }

                EjecutarConfiguracion(CrearContexto(datos, servidor));
            }
            catch (Exception ex)
            {
                MostrarFalloFinal(ex);
            }
        }

        private DatosInstalacion PedirDatos()
        {
            var inicial = new DatosInstalacion
            {
                Instancia = _instanciaDefault,
                BaseDatos = "openpos",
                Usuario = "openPOS",
                Contrasena = string.Empty
            };

            return Dialogo(inicial);
        }

        private DatosInstalacion Dialogo(DatosInstalacion inicial)
        {
            try
            {
                if (IsDisposed || !IsHandleCreated) return inicial;
                if (InvokeRequired)
                {
                    return (DatosInstalacion)Invoke(new Func<DatosInstalacion>(() => Dialogo(inicial)));
                }

                using (var dlg = new DatosInstalacionDialog(inicial))
                {
                    return dlg.ShowDialog(this) == DialogResult.OK ? dlg.Resultado : null;
                }
            }
            catch
            {
                return null;
            }
        }

        private void EjecutarConfiguracion(Contexto ctx)
        {
            var pasos = new[]
            {
                "Detectar la instancia de SQL Server",
                "Verificar la conexión con la instancia",
                "Eliminar la base de datos previa (si existe)",
                "Crear la base de datos " + ctx.BaseDatos,
                "Ejecutar tablas y procedimientos almacenados",
                "Guardar la configuración de conexión",
                "Finalizar"
            };

            MostrarConsola();
            _consola.Iniciar("Aplicando la configuración del servidor…");
            _consola.DefinirPasos(pasos);
            int total = pasos.Length;

            try
            {
                Marcar(0, total);
                _consola.Info("Instancia detectada: «" + ctx.Servidor + "».");
                Completar(0, total);

                Marcar(1, total);
                EsperarConexion(ctx.Servidor, 3);
                HabilitarAutenticacionMixta(ctx);
                Completar(1, total);

                AplicarEsquema(ctx, 2, 3, 4, 5, total);

                Marcar(6, total);
                Completar(6, total);
                Finalizar(ctx);
            }
            catch (Exception ex)
            {
                MostrarFalloFinal(ex);
            }
        }

        private void EjecutarInstalacion(Contexto ctx)
        {
            var pasos = new[]
            {
                "Preparar los archivos de configuración",
                "Iniciar el instalador de SQL Server Express",
                "Instalar SQL Server Express (puede tardar varios minutos)",
                "Verificar la instancia recién instalada",
                "Eliminar la base de datos previa (si existe)",
                "Crear la base de datos " + ctx.BaseDatos,
                "Ejecutar tablas y procedimientos almacenados",
                "Guardar la configuración de conexión",
                "Finalizar"
            };

            _consola.Iniciar("Instalando SQL Server Express…");
            _consola.DefinirPasos(pasos);
            int total = pasos.Length;

            try
            {
                Marcar(0, total);
                EscribirConfigurationFile(ctx);
                Completar(0, total);

                Marcar(1, total);
                Process instalador = IniciarInstalador(ctx);
                Completar(1, total);

                Marcar(2, total);
                EsperarInstalador(instalador, ctx.Servidor);
                Completar(2, total);

                Marcar(3, total);
                EsperarConexion(ctx.Servidor, 24);
                Completar(3, total);

                AplicarEsquema(ctx, 4, 5, 6, 7, total);

                Marcar(8, total);
                Completar(8, total);
                Finalizar(ctx);
            }
            catch (Exception ex)
            {
                MostrarFalloFinal(ex);
            }
        }

        private void AplicarEsquema(Contexto ctx, int iEliminar, int iCrear, int iProced, int iGuardar, int total)
        {
            Marcar(iEliminar, total);
            EliminarBase(ctx);
            Completar(iEliminar, total);

            Marcar(iCrear, total);
            CrearBase(ctx);
            Completar(iCrear, total);

            Marcar(iProced, total);
            CrearProcedimientos(ctx);
            Completar(iProced, total);

            Marcar(iGuardar, total);
            GuardarConfiguracion(ctx);
            Completar(iGuardar, total);
        }

        // ---- Pasos concretos ----------------------------------------------

        private void EscribirConfigurationFile(Contexto ctx)
        {
            string ini = Path.Combine(Directory.GetCurrentDirectory(), "ConfigurationFile.ini");
            File.WriteAllText(ini, ctx.ArgumentosIni, new UTF8Encoding(false));
            _consola.Info("Archivo de configuración escrito en «" + ini + "».");
        }

        private Process IniciarInstalador(Contexto ctx)
        {
            string exe = Path.Combine(Directory.GetCurrentDirectory(), "SQLEXPR_x86_ENU.exe");
            if (!File.Exists(exe))
            {
                throw new FileNotFoundException(
                    "No se encontró «SQLEXPR_x86_ENU.exe» en la carpeta de la aplicación. " +
                    "Copie el instalador de SQL Server Express junto a openPOS.exe y vuelva a intentarlo.");
            }

            string ini = Path.Combine(Directory.GetCurrentDirectory(), "ConfigurationFile.ini");
            var psi = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = "/ConfigurationFile=\"" + ini + "\" /ACTION=Install " +
                            "/IACCEPTSQLSERVERLICENSETERMS /SECURITYMODE=SQL " +
                            "/SAPWD=\"" + ctx.Password + "\" /SQLSYSADMINACCOUNTS=\"" + ctx.UsuarioEquipo + "\"",
                WindowStyle = ProcessWindowStyle.Normal,
                UseShellExecute = true,
                Verb = "runas"
            };

            _consola.Info("Solicitando permisos de administrador al instalador de SQL Server…");
            try
            {
                Process proc = Process.Start(psi);
                if (proc == null)
                {
                    throw new InvalidOperationException("El instalador no pudo iniciarse.");
                }
                _consola.Info("Instalador iniciado (deje que la ventana de SQL Server finalice por sí sola).");
                return proc;
            }
            catch (System.ComponentModel.Win32Exception w) when (w.NativeErrorCode == 1223)
            {
                throw new InvalidOperationException(
                    "La instalación fue cancelada: se denegaron los permisos de administrador (UAC).");
            }
        }

        private void EsperarInstalador(Process instalador, string servidor)
        {
            if (instalador == null)
            {
                throw new InvalidOperationException("No hay una instancia válida del instalador.");
            }

            var reloj = Stopwatch.StartNew();
            int aviso = 0;

            while (reloj.Elapsed.TotalMinutes < 30)
            {
                if (PuedeConectar(servidor, 3))
                {
                    _consola.Info("La instancia de SQL Server ya está disponible.");
                    return;
                }

                bool terminado = false;
                try { terminado = instalador.WaitForExit(1000); }
                catch { Thread.Sleep(1000); }

                if (terminado)
                {
                    try { _consola.Info("El instalador finalizó con código " + instalador.ExitCode + "."); }
                    catch { _consola.Info("El instalador finalizó."); }
                    return;
                }

                if (reloj.Elapsed.TotalSeconds > aviso * 30 + 30)
                {
                    aviso++;
                    _consola.Info(string.Format(
                        "Instalando… {0} min {1:00} s transcurridos. El instalador continúa trabajando.",
                        (int)reloj.Elapsed.TotalMinutes, reloj.Elapsed.Seconds));
                }
            }

            throw new InvalidOperationException(
                "La instalación superó el tiempo máximo de espera (30 minutos). Revise la ventana del instalador.");
        }

        private void EsperarConexion(string servidor, int maxIntentos)
        {
            for (int i = 1; i <= maxIntentos; i++)
            {
                if (PuedeConectar(servidor, 5))
                {
                    _consola.Info("Conexión establecida con «" + servidor + "».");
                    return;
                }

                _consola.Aviso("La instancia aún no responde; reintentando (" + i + "/" + maxIntentos + ")…");
                Thread.Sleep(5000);
            }

            throw new InvalidOperationException(
                "No se pudo conectar con la instancia «" + servidor + "» después de varios intentos.");
        }

        private void HabilitarAutenticacionMixta(Contexto ctx)
        {
            // Permite conexiones con usuario/contraseña SQL (necesario para que la
            // cadena guardada en ConnectionString.xml funcione en este equipo y en clientes).
            const string sql = "EXEC xp_instance_regwrite N'HKEY_LOCAL_MACHINE', " +
                               "N'Software\\Microsoft\\MSSQLServer\\MSSQLServer', " +
                               "N'LoginMode', REG_DWORD, 2";
            try
            {
                using (var conexion = new SqlConnection("Server=" + ctx.Servidor + ";database=master;Integrated Security=True"))
                using (var comando = new SqlCommand(sql, conexion))
                {
                    conexion.Open();
                    comando.ExecuteNonQuery();
                }
                _consola.Info("Autenticación mixta (Windows + SQL) habilitada en la instancia.");
            }
            catch (Exception ex)
            {
                _consola.Aviso(
                    "No se pudo habilitar la autenticación mixta automáticamente: " + ex.Message + ". " +
                    "Si la conexión con usuario/contraseña falla, actívala en SQL Server Management Studio " +
                    "(Propiedades del servidor → Seguridad → Modo de autenticación de SQL Server y Windows).");
            }
        }

        private void EliminarBase(Contexto ctx)
        {
            _consola.Info("Eliminando la base de datos previa (si existe)…");
            string script = CONEXION.SqlScriptRunner.Preparar(_plantillaEliminar, ctx.BaseDatos);
            using (var conexion = new SqlConnection("Data Source=" + ctx.Servidor + ";Initial Catalog=master;Integrated Security=True"))
            using (var comando = new SqlCommand(script, conexion))
            {
                try
                {
                    conexion.Open();
                    comando.ExecuteNonQuery();
                    _consola.Info("Base de datos previa eliminada.");
                }
                catch (Exception ex)
                {
                    _consola.Aviso("No se pudo eliminar la base previa: " + ex.Message);
                }
            }
        }

        private void CrearBase(Contexto ctx)
        {
            string sql = "CREATE DATABASE " + CONEXION.SqlScriptRunner.IdentificadorSeguro(ctx.BaseDatos);
            using (var conexion = new SqlConnection("Server=" + ctx.Servidor + ";database=master;integrated security=yes"))
            using (var comando = new SqlCommand(sql, conexion))
            {
                conexion.Open();
                comando.CommandTimeout = 0;
                try
                {
                    comando.ExecuteNonQuery();
                    _consola.Info("Base de datos «" + ctx.BaseDatos + "» creada.");
                }
                catch (SqlException ex) when (EsErrorYaExiste(ex))
                {
                    _consola.Info("La base de datos «" + ctx.BaseDatos + "» ya existía; se continuará.");
                }
            }
        }

        private void CrearProcedimientos(Contexto ctx)
        {
            string login = ctx.UsuarioDb;

            // Primero se crea el LOGIN/usuario de SQL Server y luego el esquema,
            // que referencia ese login mediante el marcador __LOGIN__ (el esquema
            // traía un usuario fijo "pruebas2020" que ya no existe). El marcador
            // __PASSWORD__ se sustituye por la contraseña elegida en el asistente.
            string usuario = _plantillaUsuarioDb
                .Replace("openPOS", login)
                .Replace("__PASSWORD__", ctx.Password.Replace("'", "''"));
            string esquema = _plantillaProcedimientos.Replace("__LOGIN__", login);

            string combinado = usuario + Environment.NewLine + esquema;
            string script = CONEXION.SqlScriptRunner.Preparar(combinado, ctx.BaseDatos);
            string cadena = "Server=" + ctx.Servidor + ";Database=master;Integrated Security=True;";

            try
            {
                CONEXION.SqlScriptRunner.Ejecutar(cadena, script, (indice, totalLotes, linea) =>
                {
                    if (indice == 1 || indice == totalLotes || indice % 10 == 0)
                    {
                        _consola.Info(string.Format("Ejecutando lote {0}/{1}…  {2}", indice, totalLotes, linea));
                    }
                });

                _consola.Info("Tablas y procedimientos almacenados aplicados correctamente.");
            }
            catch (SqlException ex) when (EsErrorYaExiste(ex))
            {
                _consola.Aviso("Se omitieron elementos que ya existían en el servidor: " + ex.Message);
            }
        }

        private static bool EsErrorYaExiste(Exception ex)
        {
            SqlException sql = ex as SqlException;
            if (sql != null)
            {
                foreach (SqlError err in sql.Errors)
                {
                    switch (err.Number)
                    {
                        case 15025:  // el principal ya existe (login/usuario)
                        case 15023:  // ya es miembro del rol
                        case 1801:   // la base de datos ya existe
                        case 2714:   // el objeto ya existe
                        case 2601:   // clave duplicada
                        case 15014:  // no se puede agregar el rol a sí mismo
                            return true;
                    }
                    if (err.Message.IndexOf("already exists", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }
            }

            Exception baseex = ex.InnerException ?? ex;
            return baseex != null &&
                   baseex.Message.IndexOf("already exists", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void GuardarConfiguracion(Contexto ctx)
        {
            var csb = new SqlConnectionStringBuilder
            {
                DataSource = ctx.Servidor,
                InitialCatalog = ctx.BaseDatos,
                IntegratedSecurity = false,
                UserID = ctx.UsuarioDb,
                Password = ctx.Password
            };
            SavetoXML(_aes.Encrypt(csb.ConnectionString, CONEXION.Desencryptacion.appPwdUnique, 256));
            _consola.Info("Cadena de conexión cifrada y guardada en «ConnectionString.xml».");
            _consola.Info("La conexión usará el usuario SQL «" + ctx.UsuarioDb + "» y la base «" + ctx.BaseDatos + "».");

            VerificarLoginSql(ctx);
        }

        private void VerificarLoginSql(Contexto ctx)
        {
            var csb = new SqlConnectionStringBuilder
            {
                DataSource = ctx.Servidor,
                InitialCatalog = "master",
                IntegratedSecurity = false,
                UserID = ctx.UsuarioDb,
                Password = ctx.Password,
                ConnectTimeout = 5
            };

            try
            {
                using (var conexion = new SqlConnection(csb.ConnectionString))
                {
                    conexion.Open();
                    _consola.Info("La cuenta SQL «" + ctx.UsuarioDb + "» ya puede conectarse a la instancia.");
                }
            }
            catch (Exception ex)
            {
                _consola.Aviso(
                    "La cuenta SQL «" + ctx.UsuarioDb + "» aún no puede conectarse: " + ex.Message + ". " +
                    "Si se instaló sobre una instancia ya existente, reinicia el servicio SQL Server " +
                    "(Services.msc → MSSQLServer / MSSQL$SQLEXPRESS) para activar la autenticación mixta.");
            }
        }

        public void SavetoXML(object dbcnString)
        {
            const string archivo = "ConnectionString.xml";
            string valor = Convert.ToString(dbcnString);

            var doc = new XmlDocument();
            XmlElement root;

            if (File.Exists(archivo))
            {
                doc.Load(archivo);
                root = doc.DocumentElement;
                if (root == null)
                {
                    root = doc.CreateElement("Configuracion");
                    doc.AppendChild(root);
                }
            }
            else
            {
                root = doc.CreateElement("Configuracion");
                doc.AppendChild(root);
            }

            if (root.Attributes.Count == 0)
            {
                root.SetAttribute("Cadena", valor);
            }
            else
            {
                root.Attributes[0].Value = valor;
            }

            using (var writer = new XmlTextWriter(archivo, null) { Formatting = Formatting.Indented })
            {
                doc.Save(writer);
            }
        }

        private static bool PuedeConectar(string servidor, int timeoutSegundos)
        {
            try
            {
                var csb = new SqlConnectionStringBuilder
                {
                    DataSource = servidor,
                    InitialCatalog = "master",
                    IntegratedSecurity = true,
                    ConnectTimeout = timeoutSegundos
                };
                using (var conexion = new SqlConnection(csb.ConnectionString))
                {
                    conexion.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        // ---- Presentación --------------------------------------------------

        private void MostrarConsola()
        {
            Ui(() =>
            {
                Panel2.Visible = false;
                _consola.Visible = true;
                _consola.BringToFront();
                Cursor = Cursors.Default;
            });
        }

        private void MostrarInvitacion()
        {
            Ui(() =>
            {
                if (_consola != null) _consola.Visible = false;
                Panel2.Visible = true;
                Panel9.Visible = true;
                Panel6.Visible = false;
                Panel4.Visible = false;
                btnInstalarServidor.Visible = true;
                lblbuscador_de_servidores.Text =
                    "No se detectó SQL Server en este equipo.\r\n\r\n" +
                    "Haz clic en «Instalar Servidor», acepta los permisos de administrador y espera. " +
                    "El asistente mostrará cada paso, su tiempo y el registro de actividad.";
                Cursor = Cursors.Default;
            });
        }

        private void Marcar(int i, int total)
        {
            _consola.PasoEnProceso(i);
            _consola.SetProgreso(total <= 0 ? 0 : (int)(i * 100.0 / total));
        }

        private void Completar(int i, int total)
        {
            _consola.PasoCompletado(i);
            _consola.SetProgreso(total <= 0 ? 100 : (int)((i + 1) * 100.0 / total));
        }

        private void Finalizar(Contexto ctx)
        {
            _consola.SetProgreso(100);
            _consola.Exito("Proceso completado. La base de datos «" + ctx.BaseDatos + "» está lista para usarse.");
            _consola.SetEstado("Todo listo. Reiniciando openPOS…");

            for (int i = 10; i > 0; i--)
            {
                _consola.SetEstado("Todo listo. Reiniciando openPOS en " + i + " s…");
                Thread.Sleep(1000);
            }

            _consola.Detener();
            Reiniciar();
        }

        private void MostrarFalloFinal(Exception ex)
        {
            _consola.ErrorLog(ex.Message);
            _consola.SetEstado("El proceso no pudo completarse. Revisa el registro.");
            Logger.Error(ex, "Fallo durante la instalación/configuración del servidor.");
        }

        private void Reiniciar()
        {
            Ui(() =>
            {
                if (_reiniciando) return;
                _reiniciando = true;
                try { Dispose(); } catch { }
                Application.Restart();
            });
        }

        private void Ui(Action accion)
        {
            try
            {
                if (IsDisposed || !IsHandleCreated) return;
                if (InvokeRequired) BeginInvoke(accion);
                else accion();
            }
            catch
            {
                // la instalación nunca debe caer por un acceso de interfaz
            }
        }

        // ---- Eventos del diseñador ----------------------------------------

        private void Button2_Click(object sender, EventArgs e)
        {
            DatosInstalacion datos = PedirDatos();
            if (datos == null)
            {
                return;
            }

            var ctx = CrearContexto(datos, @"." + "\\" + datos.Instancia);

            btnInstalarServidor.Visible = false;
            MostrarConsola();
            Task.Run(() => EjecutarInstalacion(ctx));
        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {
        }

        private void Panel10_Paint(object sender, PaintEventArgs e)
        {
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Stop();
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            timer3.Stop();
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            timer4.Stop();
        }

        private void TimerCRARINI_Tick(object sender, EventArgs e)
        {
            TimerCRARINI.Stop();
        }
    }
}
