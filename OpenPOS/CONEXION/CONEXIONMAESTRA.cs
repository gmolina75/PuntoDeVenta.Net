using System;
using System.Data;
using System.Data.SqlClient;

namespace OpenPOS.CONEXION
{
    class CONEXIONMAESTRA
    {
        private static readonly Lazy<string> cadenaLazy = new Lazy<string>(ObtenerCadena);
        private static SqlConnection _conectar;

        public static string conexion
        {
            get { return cadenaLazy.Value; }
        }

        public static SqlConnection conectar
        {
            get
            {
                if (_conectar == null)
                {
                    _conectar = new SqlConnection(conexion);
                }
                return _conectar;
            }
        }

        private static string ObtenerCadena()
        {
            try
            {
                return Convert.ToString(Desencryptacion.checkServer());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "No se pudo obtener la cadena de conexión. Verifique ConnectionString.xml y la clave de cifrado " +
                    "(OPENPOS_CRYPTO_KEY / appSetting 'OpenPOS:CryptoKey' / openpos.key).", ex);
            }
        }

        public static void abrir()
        {
            if (conectar.State == ConnectionState.Broken || conectar.State == ConnectionState.Connecting)
            {
                var vieja = conectar;
                _conectar = new SqlConnection(conexion);
                vieja.Dispose();
            }
            if (conectar.State == ConnectionState.Closed)
            {
                conectar.Open();
            }
        }

        public static void cerrar()
        {
            if (conectar.State == ConnectionState.Open)
            {
                conectar.Close();
            }
        }
    }
}
