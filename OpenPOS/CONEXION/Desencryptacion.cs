using System;
using System.Xml;
using OpenPOS.Logging;

namespace OpenPOS.CONEXION
{
    public class Desencryptacion
    {
        static private AES aes = new AES();
        static public string CnString;
        static string dbcnString;

        /// <summary>
        /// Clave maestra de cifrado. Ya no está incrustada en el código:
        /// se resuelve mediante <see cref="KeyProvider"/> (entorno, configuración
        /// o archivo protegido con DPAPI).
        /// </summary>
        public static string appPwdUnique
        {
            get { return KeyProvider.MasterKey; }
        }

        /// <summary>
        /// Lee y descifra la cadena de conexión de ConnectionString.xml.
        /// Si el archivo todavía usa la clave heredada (pública en el código
        /// fuente histórico), genera una clave local nueva protegida con DPAPI
        /// y re-cifra el archivo, cerrando esa vulnerabilidad en el equipo.
        /// </summary>
        public static object checkServer()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("ConnectionString.xml");
            XmlElement root = doc.DocumentElement;
            dbcnString = root.Attributes[0].Value;

            string plano;
            if (TryDecrypt(dbcnString, appPwdUnique, out plano))
            {
                CnString = plano;
                if (KeyProvider.UsandoClaveHeredada)
                {
                    MigrarAClaveLocal(doc, root, plano);
                }
                return CnString;
            }

            if (!KeyProvider.UsandoClaveHeredada)
            {
                // Instalación antigua cifrada con la clave heredada: migrar.
                try
                {
                    Logger.Warn("ConnectionString.xml está cifrado con la clave heredada; se migrará a una clave local nueva.");
                    CnString = (string)aes.Decrypt(dbcnString, KeyProvider.LegacyKey, 256);
                    MigrarAClaveLocal(doc, root, CnString);
                    return CnString;
                }
                catch (Exception ex)
                {
                    try { Logger.Warn("Migración de clave heredada no aplicada: " + ex.Message); } catch { }
                }
            }

            throw new InvalidOperationException(
                "No se pudo descifrar ConnectionString.xml: la clave de cifrado no coincide o el archivo está corrupto. " +
                "Configure la variable OPENPOS_CRYPTO_KEY o restaure el archivo openpos.key.");
        }

        private static bool TryDecrypt(string cifrado, string clave, out string plano)
        {
            try
            {
                plano = (string)aes.Decrypt(cifrado, clave, 256);
                return true;
            }
            catch
            {
                plano = null;
                return false;
            }
        }

        private static void MigrarAClaveLocal(XmlDocument doc, XmlElement root, string plano)
        {
            KeyProvider.GenerarYGuardar();
            KeyProvider.Reset();
            root.Attributes[0].Value = aes.Encrypt(plano, appPwdUnique, 256);
            doc.Save("ConnectionString.xml");
            try { Logger.Info("ConnectionString.xml migrado a una clave de cifrado local (openpos.key)."); } catch { }
        }
    }
}
