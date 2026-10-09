using System;
using System.Collections.Generic;
using OpenPOS.CONEXION;

namespace OpenPOS.Tests
{
    /// <summary>
    /// Ejecutor de pruebas mínimo, sin dependencias de NuGet, para poder
    /// validar la lógica crítica en cualquier equipo con MSBuild/.NET 4.8.
    /// Devuelve un código de salida distinto de cero si alguna prueba falla.
    /// </summary>
    internal static class Program
    {
        private static int _fallos;
        private static int _ejecutadas;

        private static int Main()
        {
            Console.WriteLine("== Pruebas openPOS ==");

            Check("SqlScriptRunner.Dividir separa por GO", () =>
            {
                string script = "SELECT 1\r\nGO\r\nSELECT 2\ngo\r\nSELECT 3";
                List<string> lotes = SqlScriptRunner.Dividir(script);
                Assert(lotes.Count == 3, "Se esperaban 3 lotes, se obtuvieron " + lotes.Count);
                Assert(lotes[0] == "SELECT 1", "Lote 1 incorrecto: " + lotes[0]);
                Assert(lotes[2] == "SELECT 3", "Lote 3 incorrecto: " + lotes[2]);
            });

            Check("SqlScriptRunner.Dividir trata GO con número como un separador", () =>
            {
                int noVacios = 0;
                foreach (string lote in SqlScriptRunner.Dividir("SELECT 1\nGO 2\nSELECT 2"))
                {
                    if (lote.Length > 0) noVacios++;
                }
                Assert(noVacios == 2, "Se esperaban 2 lotes, se obtuvieron " + noVacios);
            });

            Check("SqlScriptRunner.Preparar reemplaza el nombre de la base", () =>
            {
                string resultado = SqlScriptRunner.Preparar(
                    "CREATE DATABASE BASEADACURSO; USE BASEADACURSO;", "MiBase");
                Assert(resultado == "CREATE DATABASE MiBase; USE MiBase;",
                    "Resultado inesperado: " + resultado);
            });

            Check("SqlScriptRunner.Preparar reescribe la carpeta de datos", () =>
            {
                string script = @"CREATE DATABASE X ON (FILENAME = N'C:\Viejo\BASEADACURSO.mdf')";
                string resultado = SqlScriptRunner.Preparar(script, null, @"D:\Datos");
                Assert(resultado.Contains(@"D:\Datos\BASEADACURSO.mdf"),
                    "No se reescribió la ruta: " + resultado);
            });

            Check("SqlScriptRunner.IdentificadorSeguro valida y escapa", () =>
            {
                Assert(SqlScriptRunner.IdentificadorSeguro("MiBase_1") == "[MiBase_1]",
                    "Identificador válido rechazado");
                Assert(Lanza(() => SqlScriptRunner.IdentificadorSeguro("x]; DROP TABLE y--")),
                    "Se aceptó un identificador con inyección SQL");
                Assert(Lanza(() => SqlScriptRunner.IdentificadorSeguro("con espacio")),
                    "Se aceptó un identificador con espacios");
            });

            Check("KeyProvider expone una clave no vacía", () =>
            {
                Assert(!string.IsNullOrEmpty(KeyProvider.MasterKey), "La clave maestra está vacía");
            });

            Check("Desencryptacion usa la misma clave que KeyProvider", () =>
            {
                Assert(Desencryptacion.appPwdUnique == KeyProvider.MasterKey,
                    "La clave de Desencryptacion no coincide con KeyProvider");
            });

            Check("AES cifra y descifra correctamente (ida y vuelta)", () =>
            {
                var aes = new AES();
                const string original = "Server=localhost;Database=MiBase;";
                string cifrado = aes.Encrypt(original, KeyProvider.MasterKey, 256);
                string descifrado = aes.Decrypt(cifrado, KeyProvider.MasterKey, 256);
                Assert(descifrado == original, "El descifrado no coincide con el original");
            });

            Console.WriteLine();
            Console.WriteLine(string.Format("Pruebas ejecutadas: {0}, fallos: {1}", _ejecutadas, _fallos));
            return _fallos == 0 ? 0 : 1;
        }

        private static void Check(string nombre, Action prueba)
        {
            _ejecutadas++;
            try
            {
                prueba();
                Console.WriteLine("[OK]   " + nombre);
            }
            catch (Exception ex)
            {
                _fallos++;
                Console.WriteLine("[FALLO] " + nombre + " -> " + ex.Message);
            }
        }

        private static void Assert(bool condicion, string mensaje)
        {
            if (!condicion)
            {
                throw new InvalidOperationException(mensaje);
            }
        }

        private static bool Lanza(Action accion)
        {
            try
            {
                accion();
                return false;
            }
            catch (ArgumentException)
            {
                return true;
            }
        }
    }
}
