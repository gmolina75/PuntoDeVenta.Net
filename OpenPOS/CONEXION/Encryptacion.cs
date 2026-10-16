using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace OpenPOS.CONEXION
{
    class AES
    {
        // Sal fija histórica del algoritmo; se conserva para poder descifrar
        // archivos cifrados por versiones anteriores (ConnectionString.xml).
        private static readonly byte[] SalFija = { 0x0, 0x1, 0x2, 0x1C, 0x1D, 0x1E, 0x3, 0x4, 0x5, 0xF, 0x20, 0x21, 0xAD, 0xAF, 0xA4 };

        private static byte[] Encrypt(byte[] clearData, byte[] Key, byte[] IV)
        {
            using (Rijndael alg = Rijndael.Create())
            using (MemoryStream ms = new MemoryStream())
            {
                alg.Key = Key;
                alg.IV = IV;
                using (CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(clearData, 0, clearData.Length);
                    cs.FlushFinalBlock();
                }
                return ms.ToArray();
            }
        }

        public string Encrypt(string Data, string Password, int Bits)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(Data);
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, SalFija);
            try
            {
                if (Bits == 128) return Convert.ToBase64String(Encrypt(clearBytes, pdb.GetBytes(16), pdb.GetBytes(16)));
                if (Bits == 192) return Convert.ToBase64String(Encrypt(clearBytes, pdb.GetBytes(24), pdb.GetBytes(16)));
                if (Bits == 256) return Convert.ToBase64String(Encrypt(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16)));
                throw new ArgumentOutOfRangeException("Bits", "Tamaño de clave no soportado: " + Bits);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "No se pudo cifrar el dato. Verifique la clave de cifrado configurada (OPENPOS_CRYPTO_KEY / openpos.key).", ex);
            }
        }

        private static byte[] Decrypt(byte[] cipherData, byte[] Key, byte[] IV)
        {
            using (Rijndael alg = Rijndael.Create())
            using (MemoryStream ms = new MemoryStream())
            {
                alg.Key = Key;
                alg.IV = IV;
                using (CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherData, 0, cipherData.Length);
                    cs.FlushFinalBlock();
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Descifra <paramref name="Data"/> con <paramref name="Password"/>.
        /// Lanza <see cref="InvalidOperationException"/> con un mensaje claro si la
        /// clave no coincide o el dato está corrupto; nunca devuelve un valor de
        /// relleno silencioso.
        /// </summary>
        public string Decrypt(string Data, string Password, int Bits)
        {
            byte[] cipherBytes;
            try
            {
                cipherBytes = Convert.FromBase64String(Data);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException("El texto cifrado no es Base64 válido: el archivo está corrupto.", ex);
            }

            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, SalFija);
            byte[] decryptedData;
            try
            {
                if (Bits == 128) decryptedData = Decrypt(cipherBytes, pdb.GetBytes(16), pdb.GetBytes(16));
                else if (Bits == 192) decryptedData = Decrypt(cipherBytes, pdb.GetBytes(24), pdb.GetBytes(16));
                else if (Bits == 256) decryptedData = Decrypt(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));
                else throw new ArgumentOutOfRangeException("Bits", "Tamaño de clave no soportado: " + Bits);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "No se pudo descifrar el dato: la clave de cifrado no coincide o el archivo está corrupto.", ex);
            }
            return Encoding.Unicode.GetString(decryptedData);
        }
    }
}
