using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenPOS.Logica;
using OpenPOS.CONEXION;
using System.Data.SqlClient;
using System.Data;
using System.Collections;
using System.Xml.Linq;
using System.Drawing;
using System.Windows.Forms;
namespace OpenPOS.Datos
{
    public class Obtener_datos
    {
        private static string serialPC;
        private static int idcaja;

        public static void Obtener_id_caja_PorSerial(ref int idcaja)
        {

            try
            {
                Bases.Obtener_serialPC(ref serialPC);
                CONEXIONMAESTRA.abrir();
                idcaja = ConsultarIdCajaPorSerial(serialPC);
                if (idcaja == 0)
                {
                    CrearCajaParaEsteEquipo(serialPC);
                    idcaja = ConsultarIdCajaPorSerial(serialPC);
                }
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

        private static int ConsultarIdCajaPorSerial(string serial)
        {
            SqlCommand com = new SqlCommand("mostrar_cajas_por_Serial_de_DiscoDuro", CONEXIONMAESTRA.conectar);
            com.CommandType = CommandType.StoredProcedure;
            com.Parameters.AddWithValue("@Serial", serial);
            object r = com.ExecuteScalar();
            return (r == null || r == DBNull.Value) ? 0 : Convert.ToInt32(r);
        }

        private static void CrearCajaParaEsteEquipo(string serial)
        {
            string descripcion = ("CAJA " + Environment.MachineName).Replace("'", "").Trim();
            if (descripcion.Length > 50) descripcion = descripcion.Substring(0, 50);
            string tipo = ExisteCajaPrincipal() ? "SECUNDARIA" : "PRINCIPAL";
            SqlCommand cmd = new SqlCommand("Insertar_caja", CONEXIONMAESTRA.conectar);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@descripcion", descripcion);
            cmd.Parameters.AddWithValue("@Tema", "Redentor");
            cmd.Parameters.AddWithValue("@Serial_PC", serial);
            cmd.Parameters.AddWithValue("@Impresora_Ticket", "Ninguna");
            cmd.Parameters.AddWithValue("@Impresora_A4", "Ninguna");
            cmd.Parameters.AddWithValue("@Tipo", tipo);
            cmd.ExecuteNonQuery();
        }

        private static bool ExisteCajaPrincipal()
        {
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Caja WHERE Tipo = 'PRINCIPAL'", CONEXIONMAESTRA.conectar);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public static void mostrar_ventas_en_espera_con_fecha_y_monto(ref DataTable dt)
        {

            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("mostrar_ventas_en_espera_con_fecha_y_monto", CONEXIONMAESTRA.conectar);
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
        public static void mostrar_productos_agregados_a_ventas_en_espera(ref DataTable dt, int idventa)
        {

            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("mostrar_productos_agregados_a_ventas_en_espera", CONEXIONMAESTRA.conectar);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@idventa", idventa);
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
        public static void buscar_conceptos(ref DataTable dt, string buscador)
        {

            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("buscar_conceptos", CONEXIONMAESTRA.conectar);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@letra", buscador);
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

        public static void mostrar_gastos_por_turnos(int idcaja, DateTime fi, DateTime ff, ref DataTable dt)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("mostrar_gastos_por_turnos", CONEXIONMAESTRA.conectar);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@idcaja", idcaja);
                da.SelectCommand.Parameters.AddWithValue("@fi", fi);
                da.SelectCommand.Parameters.AddWithValue("@ff", ff);

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
        public static void mostrar_ingresos_por_turnos(int idcaja, DateTime fi, DateTime ff, ref DataTable dt)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("mostrar_ingresos_por_turnos", CONEXIONMAESTRA.conectar);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@idcaja", idcaja);
                da.SelectCommand.Parameters.AddWithValue("@fi", fi);
                da.SelectCommand.Parameters.AddWithValue("@ff", ff);

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

        public static void mostrar_cierre_de_caja_pendiente(ref DataTable dt)
        {
            Obtener_id_caja_PorSerial(ref idcaja);

            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("mostrar_cierre_de_caja_pendiente", CONEXIONMAESTRA.conectar);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@idcaja", idcaja);
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
      
       
        public static void mostrar_ventas_en_efectivo_por_turno(int idcaja, DateTime fi, DateTime ff, ref double monto)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand da = new SqlCommand("mostrar_ventas_en_efectivo_por_turno", CONEXIONMAESTRA.conectar);
                da.CommandType = CommandType.StoredProcedure;
                da.Parameters.AddWithValue("@idcaja", idcaja);
                da.Parameters.AddWithValue("@fi", fi);
                da.Parameters.AddWithValue("@ff", ff);
                monto = Convert.ToDouble(da.ExecuteScalar());
            }
            catch (Exception ex)
            {

                monto = 0;

            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }

        }
        public static void mostrar_cobros_en_efectivo_por_turno(int idcaja, DateTime fi, DateTime ff, ref double monto)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand da = new SqlCommand("mostrar_cobros_en_efectivo_por_turno", CONEXIONMAESTRA.conectar);
                da.CommandType = CommandType.StoredProcedure;
                da.Parameters.AddWithValue("@idcaja", idcaja);
                da.Parameters.AddWithValue("@fi", fi);
                da.Parameters.AddWithValue("@ff", ff);
                monto = Convert.ToDouble(da.ExecuteScalar());
            }
            catch (Exception ex)
            {

                monto = 0;

            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }

        }
        public static void mostrar_cobros_tarjeta_por_turno(int idcaja, DateTime fi, DateTime ff, ref double monto)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand da = new SqlCommand("mostrar_cobros_tarjeta_por_turno", CONEXIONMAESTRA.conectar);
                da.CommandType = CommandType.StoredProcedure;
                da.Parameters.AddWithValue("@idcaja", idcaja);
                da.Parameters.AddWithValue("@fi", fi);
                da.Parameters.AddWithValue("@ff", ff);
                monto = Convert.ToDouble(da.ExecuteScalar());
            }
            catch (Exception ex)
            {

                monto = 0;

            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }

        }

        public static void M_ventas_Tarjeta_por_turno(int idcaja, DateTime fi, DateTime ff, ref double monto)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand da = new SqlCommand("M_ventas_Tarjeta_por_turno", CONEXIONMAESTRA.conectar);
                da.CommandType = CommandType.StoredProcedure;
                da.Parameters.AddWithValue("@idcaja", idcaja);
                da.Parameters.AddWithValue("@fi", fi);
                da.Parameters.AddWithValue("@ff", ff);
                monto = Convert.ToDouble(da.ExecuteScalar());
            }
            catch (Exception ex)
            {
                monto = 0;
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }
        public static void M_ventas_credito_por_turno(int idcaja, DateTime fi, DateTime ff, ref double monto)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand da = new SqlCommand("M_ventas_credito_por_turno", CONEXIONMAESTRA.conectar);
                da.CommandType = CommandType.StoredProcedure;
                da.Parameters.AddWithValue("@idcaja", idcaja);
                da.Parameters.AddWithValue("@fi", fi);
                da.Parameters.AddWithValue("@ff", ff);
                monto = Convert.ToDouble(da.ExecuteScalar());
            }
            catch (Exception ex)
            {

                monto = 0;
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }

        public static void sumar_ingresos_por_turno(int idcaja, DateTime fi, DateTime ff, ref double monto)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand da = new SqlCommand("sumar_ingresos_por_turno", CONEXIONMAESTRA.conectar);
                da.CommandType = CommandType.StoredProcedure;
                da.Parameters.AddWithValue("@idcaja", idcaja);
                da.Parameters.AddWithValue("@fi", fi);
                da.Parameters.AddWithValue("@ff", ff);
                monto = Convert.ToDouble(da.ExecuteScalar());
            }
            catch (Exception ex)
            {
                monto = 0;

            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }

        }
        public static void sumar_gastos_por_turno(int idcaja, DateTime fi, DateTime ff, ref double monto)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand da = new SqlCommand("sumar_gastos_por_turno", CONEXIONMAESTRA.conectar);
                da.CommandType = CommandType.StoredProcedure;
                da.Parameters.AddWithValue("@idcaja", idcaja);
                da.Parameters.AddWithValue("@fi", fi);
                da.Parameters.AddWithValue("@ff", ff);
                monto = Convert.ToDouble(da.ExecuteScalar());
            }
            catch (Exception ex)
            {
                monto = 0;

            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }

        }
        public static void buscar_Proveedores(ref DataTable dt, string buscador)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("buscar_Proveedores", CONEXIONMAESTRA.conectar);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@letra", buscador);
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
        public static void sumar_CreditoPorPagar(int idcaja, DateTime fi, DateTime ff, ref double monto)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand da = new SqlCommand("sumar_CreditoPorPagar", CONEXIONMAESTRA.conectar);
                da.CommandType = CommandType.StoredProcedure;
                da.Parameters.AddWithValue("@idcaja", idcaja);
                da.Parameters.AddWithValue("@fi", fi);
                da.Parameters.AddWithValue("@ff", ff);
                monto = Convert.ToDouble(da.ExecuteScalar());
            }
            catch (Exception)
            {

                monto = 0;
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }

        public static void sumar_CreditoPorCobrar(int idcaja, DateTime fi, DateTime ff, ref double monto)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand da = new SqlCommand("sumar_CreditoPorCobrar", CONEXIONMAESTRA.conectar);
                da.CommandType = CommandType.StoredProcedure;
                da.Parameters.AddWithValue("@idcaja", idcaja);
                da.Parameters.AddWithValue("@fi", fi);
                da.Parameters.AddWithValue("@ff", ff);
                monto = Convert.ToDouble(da.ExecuteScalar());
            }
            catch (Exception)
            {

                monto = 0;
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }

        public static void mostrar_cajas(ref DataTable dt)
        {
            try
            {
                Bases.Obtener_serialPC(ref serialPC);
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("mostrar_cajas_por_Serial_de_DiscoDuro", CONEXIONMAESTRA.conectar);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@Serial", serialPC);
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
      
        public static void mostrarCorreoBase(ref DataTable dt)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("Select * from CorreoBase", CONEXIONMAESTRA.conectar);
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

        //Clientes
        public static void mostrar_clientes(ref DataTable dt)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("mostrar_clientes", CONEXIONMAESTRA.conectar);
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
        public static void buscar_clientes(ref DataTable dt, string buscador)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("buscar_clientes", CONEXIONMAESTRA.conectar);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@letra", buscador);
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
        public static void mostrarEstadosCuentaCliente(ref DataTable dt, int idcliente)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("mostrarEstadosCuentaCliente", CONEXIONMAESTRA.conectar);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@idcliente", idcliente);
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
      
        //controlCobros
        public static void mostrar_ControlCobros(ref DataTable dt)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("mostrar_ControlCobros", CONEXIONMAESTRA.conectar);
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
        public static void ReportePorCobrar(ref double Monto )
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand da = new SqlCommand("ReportePorCobrar", CONEXIONMAESTRA.conectar);
                Monto =Convert.ToDouble ( da.ExecuteScalar());

            }
            catch (Exception)
            {
                Monto = 0;
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }
        public static void ReporteCantClientes(ref int Cantidad)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand da = new SqlCommand("select count(idclientev) from clientes", CONEXIONMAESTRA.conectar);
                Cantidad = Convert.ToInt32(da.ExecuteScalar());
            }
            catch (Exception)
            {
                Cantidad = 0;
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }

    
        //Proveedores
        public static void ReportePorPagar(ref double Monto)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand da = new SqlCommand("ReportePorPagar", CONEXIONMAESTRA.conectar);
                Monto = Convert.ToDouble(da.ExecuteScalar());

            }
            catch (Exception)
            {
                Monto = 0;
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }
        public static void mostrar_Proveedores(ref DataTable dt)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("mostrar_Proveedores", CONEXIONMAESTRA.conectar);
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

     
        //Ventas
        public static void mostrarVentasGrafica(ref DataTable dt)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("mostrarVentasGrafica", CONEXIONMAESTRA.conectar);
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
        public static void mostrarVentasGraficaFechas(ref DataTable dt,DateTime fi,DateTime ff)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("mostrarVentasGraficaFechas", CONEXIONMAESTRA.conectar);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@fi", fi);
                da.SelectCommand.Parameters.AddWithValue("@ff", ff);

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
        public static void ReporteTotalVentas(ref double Monto)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand da = new SqlCommand("ReporteTotalVentas", CONEXIONMAESTRA.conectar);
                Monto = Convert.ToDouble(da.ExecuteScalar());

            }
            catch (Exception)
            {
                Monto = 0;
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }
        public static void ReporteTotalVentasFechas(ref double Monto,DateTime fi, DateTime ff)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand  da = new SqlCommand("ReporteTotalVentasFechas", CONEXIONMAESTRA.conectar);
                da.CommandType = CommandType.StoredProcedure;
                da.Parameters.AddWithValue("@fi", fi);
                da.Parameters.AddWithValue("@ff", ff);
                Monto = Convert.ToDouble ( da.ExecuteScalar());
            }
            catch (Exception ex)
            {
                Monto = 0;
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }
        
        
        public static void contarVentasEspera(ref int Contador)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand da = new SqlCommand("contarVentasEspera", CONEXIONMAESTRA.conectar);
                Contador = Convert.ToInt32 ( da.ExecuteScalar());
            }
            catch (Exception)
            {
                Contador = 0;


            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }
        public static void ReporteResumenVentasHoy(ref DataTable dt)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("ReporteResumenVentasHoy", CONEXIONMAESTRA.conectar);
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
        public static void ReporteResumenVentasHoyEmpleado(ref DataTable dt, int idEmpleado)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("ReporteResumenVentasHoyEmpleado", CONEXIONMAESTRA.conectar);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@idEmpleado", idEmpleado);

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

        public static void ReporteResumenVentasFechas(ref DataTable dt,DateTime fi , DateTime ff)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("ReporteResumenVentasFechas", CONEXIONMAESTRA.conectar);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@fi", fi);
                da.SelectCommand.Parameters.AddWithValue("@ff", ff);
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
        public static void ReporteResumenVentasEmpleadoFechas(ref DataTable dt, int idEmpleado, DateTime fi, DateTime ff)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("ReporteResumenVentasEmpleadoFechas", CONEXIONMAESTRA.conectar);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@idEmpleado", idEmpleado);
                da.SelectCommand.Parameters.AddWithValue("@fi", fi);
                da.SelectCommand.Parameters.AddWithValue("@ff", ff);

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

        //Detalle ventas
        public static void ReporteGanancias(ref double Monto)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand da = new SqlCommand("ReporteGanancias", CONEXIONMAESTRA.conectar);
                Monto = Convert.ToDouble(da.ExecuteScalar());

            }
            catch (Exception)
            {
                Monto = 0;
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }
        public static void ReporteGananciasFecha(ref double Monto, DateTime fi, DateTime ff)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand da = new SqlCommand("ReporteGananciasFecha", CONEXIONMAESTRA.conectar);
                da.CommandType = CommandType.StoredProcedure;
                da.Parameters.AddWithValue("@fi", fi);
                da.Parameters.AddWithValue("@ff", ff);
                Monto = Convert.ToDouble(da.ExecuteScalar());
            }
            catch (Exception ex)
            {
                Monto = 0;
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }
       
        //Productos
        public static void ReporteProductoBajoMinimo(ref int Monto)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand da = new SqlCommand("ReporteProductoBajoMinimo", CONEXIONMAESTRA.conectar);
                Monto = Convert.ToInt32(da.ExecuteScalar());
            }
            catch (Exception)
            {
                Monto = 0;
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }
        public static void ReporteCantProductos(ref int Cantidad)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand da = new SqlCommand("select count(Id_Producto1) from Producto1", CONEXIONMAESTRA.conectar);
                Cantidad = Convert.ToInt32(da.ExecuteScalar());
            }
            catch (Exception)
            {
                Cantidad = 0;
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }
        public static void mostrarPmasVendidos(ref DataTable dt)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("mostrarPmasVendidos", CONEXIONMAESTRA.conectar);
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
       
        public static void mostrar_productos_vencidos(ref DataTable dt)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("mostrar_productos_vencidos", CONEXIONMAESTRA.conectar);
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
      
        public static void BUSCAR_PRODUCTOS_KARDEX(ref DataTable dt,string buscador)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("BUSCAR_PRODUCTOS_KARDEX", CONEXIONMAESTRA.conectar);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@letrab", buscador);
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

        //Empresa
        public static void MostrarMoneda(ref string moneda)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand da = new SqlCommand("select SimboloMoneda  from EMPRESA", CONEXIONMAESTRA.conectar);
                moneda =Convert.ToString ( da.ExecuteScalar());
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
        //Gastos
        public static void ReporteGastosAnioCombo(ref DataTable dt)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("ReporteGastosAnioCombo", CONEXIONMAESTRA.conectar);
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
        public static void ReporteGastosMesCombo(ref DataTable dt, int anio)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("ReporteGastosMesCombo", CONEXIONMAESTRA.conectar);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@anio", anio);
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

        public static void ReporteGastosAnio(ref DataTable dt, int año)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("ReporteGastosAnioGrafica", CONEXIONMAESTRA.conectar);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@anio", año);
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
        public static void ReporteGastosAnioMesGrafica(ref DataTable dt, int año, string mes)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("ReporteGastosAnioMesGrafica", CONEXIONMAESTRA.conectar);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@anio", año);
                da.SelectCommand.Parameters.AddWithValue("@mes", mes);

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
        //Caja
        public static void mostrarPuertos(ref DataTable dt)
        {
            try
            {
                Obtener_datos.Obtener_id_caja_PorSerial(ref idcaja);
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("mostrarPuertos", CONEXIONMAESTRA.conectar);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@idcaja", idcaja);
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
        public static void mostrarTemaCaja(ref string Tema)
        {
            try
            {
                Obtener_id_caja_PorSerial(ref idcaja);
                CONEXIONMAESTRA.abrir();
                SqlCommand  da = new SqlCommand("mostrarTemaCaja", CONEXIONMAESTRA.conectar);
                da.CommandType = CommandType.StoredProcedure;
                da.Parameters.AddWithValue("@idcaja", idcaja);
                Tema = da.ExecuteScalar().ToString();

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

        //Tickets
        public static void mostrar_ticket_impreso(ref DataTable dt,int idventa,string TotalLetras)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("mostrar_ticket_impreso", CONEXIONMAESTRA.conectar);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@Id_venta", idventa);
                da.SelectCommand.Parameters.AddWithValue("@total_en_letras", TotalLetras);
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
        //Usuarios
        public static void mostrarUsuarios(ref DataTable dt)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("Select * from Usuario", CONEXIONMAESTRA.conectar);
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
        //Clientes
        public static void ReporteCuestasPorCobrar(ref DataTable dt)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("ReporteCuestasPorCobrar", CONEXIONMAESTRA.conectar);
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
        //Proveedores
        public static void ReporteCuestasPorPagar(ref DataTable dt)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("ReporteCuestasPorPagar", CONEXIONMAESTRA.conectar);
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


    }
}
