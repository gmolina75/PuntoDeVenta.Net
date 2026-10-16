using OpenPOS.CONEXION;
using Sunat.Logica;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPOS.Datos
{
   public class DcodigosProdsunat
    {
        public void buscarCodProdSunat(ref DataTable dt, string buscador)
        {
            try
            {

                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("buscarCodProdSunat", CONEXIONMAESTRA.conectar);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@buscador", buscador);
                da.Fill(dt);
            }
            catch (Exception ex)
            {
                OpenPOS.Logging.Logger.ShowError(ex);
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }
        public void mostrarCodSunatXCod(ref DataTable dt, LcodigosSunat parametros)
        {
            try
            {

                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("mostrarCodSunatXCod", CONEXIONMAESTRA.conectar);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@codigo", parametros.codigo);
                da.Fill(dt);
            }
            catch (Exception ex)
            {
                OpenPOS.Logging.Logger.ShowError(ex);
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }
        public void InsertarcodigosProductosSunat()
        {
            try
            {
                string rutatxt = Path.GetDirectoryName(Application.ExecutablePath) + @"\catalogo.txt";
                if (!File.Exists(rutatxt))
                {
                    MessageBox.Show("No se encontró el archivo del catálogo de productos SUNAT:\n" + rutatxt);
                    return;
                }

                // Lectura desde la aplicación (cuenta del usuario) + SqlBulkCopy; evita el
                // "cannot bulk load" que ocurría porque BULK INSERT corría bajo la cuenta del
                // servicio SQL Server sin permiso para leer la carpeta de la aplicación.
                var dt = new DataTable();
                dt.Columns.Add("Codigo", typeof(string));
                dt.Columns.Add("Descripcion", typeof(string));

                using (var sr = new StreamReader(rutatxt, Encoding.Default))
                {
                    string linea;
                    while ((linea = sr.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(linea)) continue;
                        string[] p = linea.Split(';');
                        if (p.Length < 2) continue;
                        string codigo = p[0].Trim();
                        if (codigo.Length > 8) codigo = codigo.Substring(0, 8);
                        dt.Rows.Add(codigo, p[1].Trim());
                    }
                }

                if (dt.Rows.Count == 0) return;

                CONEXIONMAESTRA.abrir();
                using (var bc = new SqlBulkCopy(CONEXIONMAESTRA.conectar))
                {
                    bc.DestinationTableName = "CodigosProdSunat";
                    bc.BulkCopyTimeout = 0;
                    bc.WriteToServer(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }

    }
}
