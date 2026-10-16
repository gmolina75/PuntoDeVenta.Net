using System;
using System.Security.Cryptography;

namespace OpenPOS.Logica
{
    /// <summary>
    /// Hash de contraseñas con PBKDF2 (HMAC-SHA256, 100.000 iteraciones y sal
    /// aleatoria por contraseña). El valor almacenado tiene el formato
    /// "P1$&lt;sal&gt;$&lt;hash&gt;" en Base64 y cabe en varchar(50), así que no
    /// requiere cambios en el esquema de la base de datos. Es unidireccional:
    /// no existe función para recuperar la contraseña original.
    /// </summary>
    public static class PasswordHasher
    {
        private const string Prefijo = "P1$";
        private const int Iteraciones = 100000;
        private const int TamanoSal = 12;
        private const int TamanoHash = 20;

        public static string Hash(string password)
        {
            if (password == null) throw new ArgumentNullException("password");
            byte[] sal = new byte[TamanoSal];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(sal);
            }
            byte[] hash = Calcular(password, sal, Iteraciones);
            return Prefijo + Convert.ToBase64String(sal) + "$" + Convert.ToBase64String(hash);
        }

        public static bool EsHash(string valor)
        {
            return !string.IsNullOrEmpty(valor) && valor.StartsWith(Prefijo, StringComparison.Ordinal);
        }

        public static bool Verificar(string password, string almacenado)
        {
            if (password == null || !EsHash(almacenado)) return false;
            string[] partes = almacenado.Split('$');
            if (partes.Length != 3) return false;
            byte[] sal, esperado;
            try
            {
                sal = Convert.FromBase64String(partes[1]);
                esperado = Convert.FromBase64String(partes[2]);
            }
            catch (FormatException)
            {
                return false;
            }
            byte[] actual = Calcular(password, sal, Iteraciones);
            return IgualEnTiempoConstante(actual, esperado);
        }

        private static byte[] Calcular(string password, byte[] sal, int iteraciones)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, sal, iteraciones, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(TamanoHash);
            }
        }

        private static bool IgualEnTiempoConstante(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int diferencia = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diferencia |= a[i] ^ b[i];
            }
            return diferencia == 0;
        }
    }
}
