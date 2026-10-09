using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using OpenPOS.Logging;

namespace OpenPOS.CONEXION
{
    /// <summary>
    /// Resuelve la clave maestra de cifrado sin depender de un valor incrustado en el
    /// código fuente. Orden de resolución:
    ///   1. Variable de entorno OPENPOS_CRYPTO_KEY.
    ///   2. appSetting "OpenPOS:CryptoKey" del archivo de configuración.
    ///   3. Archivo openpos.key protegido con DPAPI (ámbito máquina o usuario).
    ///   4. Clave heredada (compatibilidad hacia atrás) con advertimiento en el log.
    /// </summary>
    public static class KeyProvider
    {
        /// <summary>Clave heredada de instalaciones previas. No debe cambiarse.</summary>
        private const string LegacyKey = "Ada369.codigo369.BASEADA.Hola_Mundo";
        private const string EnvVar = "OPENPOS_CRYPTO_KEY";
        private const string SettingName = "OpenPOS:CryptoKey";
        private const string KeyFileName = "openpos.key";
        private const string KeyFileNameLegacy = "ada369.key";

        private static readonly Lazy<string> _masterKey = new Lazy<string>(Resolver);

        public static string MasterKey { get { return _masterKey.Value; } }

        public static bool UsandoClaveHeredada
        {
            get { return string.Equals(MasterKey, LegacyKey, StringComparison.Ordinal); }
        }

        private static string Resolver()
        {
            string clave = LeerDeEntorno();
            if (!string.IsNullOrEmpty(clave)) return clave;

            clave = LeerDeConfiguracion();
            if (!string.IsNullOrEmpty(clave)) return clave;

            clave = LeerDeArchivoProtegido();
            if (!string.IsNullOrEmpty(clave)) return clave;

            try
            {
                Logger.Warn("No se encontró una clave de cifrado configurada (OPENPOS_CRYPTO_KEY / " +
                            "appSetting 'OpenPOS:CryptoKey' / openpos.key). Usando la clave heredada.");
            }
            catch { }

            return LegacyKey;
        }

        private static string LeerDeEntorno()
        {
            try { return Environment.GetEnvironmentVariable(EnvVar); }
            catch { return null; }
        }

        private static string LeerDeConfiguracion()
        {
            try { return ConfigurationManager.AppSettings[SettingName]; }
            catch { return null; }
        }

        private static string LeerDeArchivoProtegido()
        {
            try
            {
                string ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, KeyFileName);
                if (!File.Exists(ruta))
                {
                    ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, KeyFileNameLegacy);
                }
                if (!File.Exists(ruta)) return null;

                byte[] protegido = File.ReadAllBytes(ruta);
                byte[] plano;
                try
                {
                    plano = ProtectedData.Unprotect(protegido, null, DataProtectionScope.LocalMachine);
                }
                catch
                {
                    plano = ProtectedData.Unprotect(protegido, null, DataProtectionScope.CurrentUser);
                }
                return Encoding.UTF8.GetString(plano);
            }
            catch (Exception ex)
            {
                try { Logger.Warn("No se pudo leer " + KeyFileName + ": " + ex.Message); } catch { }
                return null;
            }
        }

        /// <summary>
        /// Genera una clave aleatoria y la persiste protegida con DPAPI (ámbito máquina).
        /// </summary>
        public static void GenerarYGuardar()
        {
            string ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, KeyFileName);
            byte[] aleatoria = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(aleatoria);
            }
            string clave = Convert.ToBase64String(aleatoria);

            byte[] plano = Encoding.UTF8.GetBytes(clave);
            byte[] protegido;
            try
            {
                protegido = ProtectedData.Protect(plano, null, DataProtectionScope.LocalMachine);
            }
            catch
            {
                protegido = ProtectedData.Protect(plano, null, DataProtectionScope.CurrentUser);
            }
            File.WriteAllBytes(ruta, protegido);
            try { Logger.Info("Nueva clave de cifrado generada en " + ruta); } catch { }
        }
    }
}
