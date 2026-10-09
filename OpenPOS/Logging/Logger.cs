using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace OpenPOS.Logging
{
    /// <summary>
    /// Registro de eventos de la aplicación en archivos de texto diarios.
    /// Los logs se guardan en %LOCALAPPDATA%\OpenPOS\logs. Si no hay permisos,
    /// se usa el directorio de la aplicación como respaldo.
    /// </summary>
    public static class Logger
    {
        private static readonly object _sync = new object();
        private static readonly string _logDir;

        static Logger()
        {
            try
            {
                string baseDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "OpenPOS", "logs");
                Directory.CreateDirectory(baseDir);
                _logDir = baseDir;
            }
            catch
            {
                try { _logDir = AppDomain.CurrentDomain.BaseDirectory; }
                catch { _logDir = "."; }
            }
        }

        public static string LogDirectory { get { return _logDir; } }

        public static void Info(string mensaje) { Escribir("INFO", mensaje); }

        public static void Warn(string mensaje) { Escribir("WARN", mensaje); }

        public static void Error(string mensaje) { Escribir("ERROR", mensaje); }

        public static void Error(Exception ex, string mensaje = null)
        {
            if (ex == null)
            {
                Escribir("ERROR", mensaje);
                return;
            }

            string texto = string.IsNullOrEmpty(mensaje) ? ex.Message : mensaje + " | " + ex.Message;
            Escribir("ERROR", texto + Environment.NewLine + ex);
        }

        /// <summary>
        /// Registra la excepción y muestra un mensaje amigable al usuario,
        /// sin exponer la traza de la pila.
        /// </summary>
        public static void ShowError(Exception ex, string mensajeUsuario = null)
        {
            Error(ex);
            string texto = string.IsNullOrEmpty(mensajeUsuario)
                ? "Ocurrió un error inesperado. El detalle quedó registrado en el archivo de logs."
                : mensajeUsuario;
            try
            {
                MessageBox.Show(texto, "openPOS", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                // sin interfaz disponible
            }
        }

        private static void Escribir(string nivel, string mensaje)
        {
            try
            {
                lock (_sync)
                {
                    string archivo = Path.Combine(_logDir, "openpos-" + DateTime.Now.ToString("yyyyMMdd") + ".log");
                    string linea = string.Format("{0:yyyy-MM-dd HH:mm:ss.fff} [{1}] {2}{3}",
                        DateTime.Now, nivel, mensaje, Environment.NewLine);
                    File.AppendAllText(archivo, linea, Encoding.UTF8);
                }
            }
            catch
            {
                // nunca lanzar desde el logger
            }
        }
    }
}
