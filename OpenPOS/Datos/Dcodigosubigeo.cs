using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using OpenPOS.CONEXION;
using System.IO;
using System.Windows.Forms;
using Sunat.Logica;

namespace OpenPOS.Datos
{
   public class Dcodigosubigeo
    {
        public void Insertarcodigosubigeo()
        {
            try
            {
                string rutatxt = Path.GetDirectoryName(Application.ExecutablePath) + @"\listaubigeos.txt";
                if (!File.Exists(rutatxt))
                {
                    MessageBox.Show("No se encontró el archivo de códigos de ubigeo:\n" + rutatxt);
                    return;
                }

                // Se lee el archivo desde la aplicación (cuenta del usuario que ejecuta
                // openPOS) y se inserta con SqlBulkCopy. BULK INSERT de SQL Server corría bajo
                // la cuenta del servicio SQL y fallaba con permisos al no poder leer la carpeta.
                var dt = new DataTable();
                dt.Columns.Add("Departamento", typeof(string));
                dt.Columns.Add("Provincia", typeof(string));
                dt.Columns.Add("Distrito", typeof(string));
                dt.Columns.Add("Ubigeo", typeof(string));

                using (var sr = new StreamReader(rutatxt, Encoding.Default))
                {
                    string linea;
                    while ((linea = sr.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(linea)) continue;
                        string[] p = linea.Split(';');
                        if (p.Length < 4) continue;
                        dt.Rows.Add(Recortar(p[0]), Recortar(p[1]), Recortar(p[2]), Recortar(p[3]));
                    }
                }

                if (dt.Rows.Count == 0) return;

                CONEXIONMAESTRA.abrir();
                using (var bc = new SqlBulkCopy(CONEXIONMAESTRA.conectar))
                {
                    bc.DestinationTableName = "Codigosubigeos";
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

        private static string Recortar(string valor)
        {
            if (valor == null) return null;
            valor = valor.Trim();
            return valor.Length > 50 ? valor.Substring(0, 50) : valor;
        }
        public void mostrarCodigosubigeo(ref DataTable dt)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("select * from Codigosubigeos", CONEXIONMAESTRA.conectar);
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
        public void ObtenerUbicaionXubigeo(ref DataTable dt, Lcodigosubigeos parametros)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                var da = new SqlDataAdapter("ObtenerUbicaionXubigeo", CONEXIONMAESTRA.conectar);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue ("@CodUbigeo", parametros.Ubigeo);
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

        public void ObtenercodigoUbigeo(ref string ubigeo,Lcodigosubigeos parametros)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                var da = new SqlCommand("ObtenercodigoUbigeo", CONEXIONMAESTRA.conectar);
                da.CommandType = CommandType.StoredProcedure;
                da.Parameters.AddWithValue("@Depa", parametros.Departamento);
                da.Parameters.AddWithValue("@Prov", parametros.Provincia);
                da.Parameters.AddWithValue("@Dist", parametros.Distrito);
                ubigeo =Convert.ToString( da.ExecuteScalar());
            }
            catch (Exception ex)
            {
                ubigeo = "-";
                MessageBox.Show(ex.Message);
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }
    }
}
