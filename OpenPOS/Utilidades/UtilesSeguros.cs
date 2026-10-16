using System;
using System.Data;
using System.Drawing;
using System.IO;

namespace OpenPOS
{
    /// <summary>
    /// Utilidades defensivas para lecturas de base de datos y campos de formulario.
    /// Evitan cierres/saltos por valores nulos, DBNull o índices fuera de rango
    /// sin alterar la información real de las tablas.
    /// </summary>
    public static class UtilesSeguros
    {
        /// <summary>
        /// Devuelve el valor de una celda como texto sin lanzar excepciones.
        /// Si la tabla, fila o columna no existe, o el valor es nulo/DBNull,
        /// devuelve <paramref name="defecto"/> ("" por omisión).
        /// </summary>
        public static string Celda(DataTable dt, int fila, int columna, string defecto = "")
        {
            try
            {
                if (dt == null || fila < 0 || fila >= dt.Rows.Count) return defecto;
                if (columna < 0 || columna >= dt.Columns.Count) return defecto;
                object v = dt.Rows[fila][columna];
                return v == null || v == DBNull.Value ? defecto : Convert.ToString(v);
            }
            catch
            {
                return defecto;
            }
        }

        /// <summary>
        /// Devuelve el contenido binario (byte[]) de una celda, o null si no es un
        /// array de bytes válido (por ejemplo imágenes guardadas como nulas/DBNull).
        /// </summary>
        public static byte[] Bytes(DataTable dt, int fila, int columna)
        {
            try
            {
                if (dt == null || fila < 0 || fila >= dt.Rows.Count) return null;
                if (columna < 0 || columna >= dt.Columns.Count) return null;
                object v = dt.Rows[fila][columna];
                return v is byte[] ? (byte[])v : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Convierte bytes a imagen. Devuelve null si los bytes no forman una imagen
        /// (evita que Image.FromStream explote con buffers corruptos o vacíos).
        /// </summary>
        public static Image Imagen(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return null;
            try
            {
                using (var ms = new MemoryStream(bytes))
                {
                    return Image.FromStream(ms);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Convierte un texto a entero de forma segura. Si el texto es vacío, nulo o
        /// no numérico devuelve <paramref name="defecto"/> (0 por omisión).
        /// </summary>
        public static int Entero(string texto, int defecto = 0)
        {
            int n;
            return int.TryParse((texto ?? string.Empty).Trim(), out n) ? n : defecto;
        }
    }
}