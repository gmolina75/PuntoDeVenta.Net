using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using OpenPOS.Logging;

namespace OpenPOS.CONEXION
{
    /// <summary>
    /// Ejecuta un script T-SQL dividido en lotes por el separador GO,
    /// sin depender de la utilidad externa sqlcmd.
    /// </summary>
    public static class SqlScriptRunner
    {
        private static readonly Regex SeparadorGo =
            new Regex(@"^\s*GO\s*(\d+)?\s*;?\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static readonly Regex RutaDatos =
            new Regex(@"(?<=FILENAME\s*=\s*N')[^']+(?=')", RegexOptions.IgnoreCase);

        public static void Ejecutar(string cadenaConexion, string script)
        {
            Ejecutar(cadenaConexion, script, null);
        }

        /// <summary>
        /// Ejecuta un script T-SQL lote a lote. Si se indica <paramref name="progreso"/>,
        /// se invoca tras cada lote con (número de lote, total de lotes, primera línea).
        /// </summary>
        public static void Ejecutar(string cadenaConexion, string script, Action<int, int, string> progreso)
        {
            if (string.IsNullOrWhiteSpace(script))
            {
                return;
            }

            List<string> lotes = Dividir(script);

            int total = 0;
            foreach (string lote in lotes)
            {
                if (!string.IsNullOrWhiteSpace(lote))
                {
                    total++;
                }
            }

            using (var conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                int indice = 0;
                foreach (string lote in lotes)
                {
                    if (string.IsNullOrWhiteSpace(lote))
                    {
                        continue;
                    }

                    using (var comando = new SqlCommand(lote, conexion))
                    {
                        comando.CommandTimeout = 0;
                        comando.ExecuteNonQuery();
                    }

                    indice++;
                    if (progreso != null)
                    {
                        progreso(indice, total, PrimeraLinea(lote));
                    }
                }
            }
        }

        private static string PrimeraLinea(string lote)
        {
            if (string.IsNullOrEmpty(lote))
            {
                return string.Empty;
            }

            int fin = lote.IndexOf('\n');
            string linea = fin < 0 ? lote : lote.Substring(0, fin);
            linea = linea.Trim();
            return linea.Length > 90 ? linea.Substring(0, 90) : linea;
        }

        /// <summary>
        /// Divide el script en lotes. Las líneas que son únicamente "GO" (opcionalmente
        /// con un número de repeticiones) actúan como separador.
        /// </summary>
        public static List<string> Dividir(string script)
        {
            var lotes = new List<string>();
            script = script.Replace("\r\n", "\n").Replace("\r", "\n");
            string normalizado = SeparadorGo.Replace(script, "\u0000");

            foreach (string lote in normalizado.Split('\u0000'))
            {
                lotes.Add(lote.Trim());
            }
            return lotes;
        }

        private static readonly Regex IdentificadorValido =
            new Regex(@"^[A-Za-z0-9_]+$");

        /// <summary>
        /// Valida un identificador (p. ej. nombre de base de datos) y lo devuelve
        /// entre corchetes para su uso seguro en T-SQL.
        /// </summary>
        public static string IdentificadorSeguro(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre) || !IdentificadorValido.IsMatch(nombre.Trim()))
            {
                throw new ArgumentException(
                    "El nombre de la base de datos solo puede contener letras, números y guiones bajos.", "nombre");
            }
            return "[" + nombre.Trim() + "]";
        }

        /// <summary>
        /// Prepara el script para una base de datos destino, reemplazando el nombre
        /// por defecto y, opcionalmente, el directorio de los archivos .mdf/.ldf.
        /// </summary>
        public static string Preparar(string script, string nombreBaseDeDatos, string carpetaDatos = null)
        {
            if (!string.IsNullOrEmpty(nombreBaseDeDatos))
            {
                script = script.Replace("BASEADACURSO", nombreBaseDeDatos);
                script = script.Replace("BASEADA", nombreBaseDeDatos);
            }

            if (!string.IsNullOrEmpty(carpetaDatos))
            {
                carpetaDatos = carpetaDatos.TrimEnd('\\', '/') + "\\";
                script = RutaDatos.Replace(script, m =>
                {
                    string nombreArchivo = System.IO.Path.GetFileName(m.Value);
                    return carpetaDatos + nombreArchivo;
                });
            }

            return script;
        }
    }
}
