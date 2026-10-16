using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using OpenPOS.CONEXION;
using OpenPOS.Datos;
using OpenPOS.Logging;
using Sunat.Logica;

namespace OpenPOS.Repositorios
{
    public class SqlVentasRepository : IVentasRepository
    {
        private int _idCaja;
        private int _idUsuario;
        private string _trabajarConImp;
        private double _porcentajeImp;
        private int _contadorProd;

        private void ObtenerIdCaja()
        {
            var funcion = new Dcaja();
            funcion.ObtenerIdcaja(ref _idCaja);
        }

        private void ObtenerIdUsuario()
        {
            var funcion = new DiniciosSesion();
            funcion.mostrarInicioSesion(ref _idUsuario);
        }

        private void ObtenerImpuesto()
        {
            var dt = new DataTable();
            var funcion = new Dempresa();
            funcion.mostrar_empresa(ref dt);
            _trabajarConImp = dt.Rows[0][5].ToString();
            if (_trabajarConImp == "SI")
            {
                double.TryParse(dt.Rows[0][3].ToString(), out _porcentajeImp);
            }
            else
            {
                _porcentajeImp = 0;
            }
        }

        private void ContarProdDv(int idVenta)
        {
            var funcion = new Ddetalleventas();
            var parametros = new LdetalleVenta { idventa = idVenta };
            funcion.ContarprodDv(parametros, ref _contadorProd);
        }

        public void MostrarVentasXidcaja(ref DataTable dt)
        {
            try
            {
                ObtenerIdCaja();
                CONEXIONMAESTRA.abrir();
                using (var da = new SqlDataAdapter("mostrarVentasXidcaja", CONEXIONMAESTRA.conectar))
                {
                    da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    da.SelectCommand.Parameters.AddWithValue("@Id_caja", _idCaja);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                Logger.ShowError(ex);
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }

        public void MostrarVentasPend(ref DataTable dt, string codigo)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                using (var da = new SqlDataAdapter("MostrarventasPend", CONEXIONMAESTRA.conectar))
                {
                    da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    da.SelectCommand.Parameters.AddWithValue("@Codigo", codigo);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                Logger.ShowError(ex);
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }

        public void MostrarResumendiarioPendiente(ref DataTable dt, string fecha)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                using (var da = new SqlDataAdapter("MostrarResumendiario", CONEXIONMAESTRA.conectar))
                {
                    da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    da.SelectCommand.Parameters.AddWithValue("@fecha", fecha);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                Logger.ShowError(ex);
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }

        public void MostrarBoletasPendientes(ref DataTable dt)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                using (var da = new SqlDataAdapter("MostrarBoletaspendientes", CONEXIONMAESTRA.conectar))
                {
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                Logger.ShowError(ex);
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }

        public void MostrarVentasXidventa(ref DataTable dt, int idVenta)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                using (var da = new SqlDataAdapter("mostrarVentasXidventa", CONEXIONMAESTRA.conectar))
                {
                    da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    da.SelectCommand.Parameters.AddWithValue("@Idventa", idVenta);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                Logger.ShowError(ex);
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }

        public void BuscarVentas(ref DataTable dt, string buscador)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                using (var da = new SqlDataAdapter("buscarVentas", CONEXIONMAESTRA.conectar))
                {
                    da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    da.SelectCommand.Parameters.AddWithValue("@buscador", buscador);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                Logger.ShowError(ex);
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }

        public void BuscarFacturas(ref DataTable dt, string buscador, string codigoCompro)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                using (var da = new SqlDataAdapter("buscarFacturasBoletas", CONEXIONMAESTRA.conectar))
                {
                    da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    da.SelectCommand.Parameters.AddWithValue("@buscador", buscador);
                    da.SelectCommand.Parameters.AddWithValue("@Codigo", codigoCompro);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                Logger.ShowError(ex);
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }

        public bool InsertarVenta(ref int idVentaGenerada)
        {
            idVentaGenerada = 0;
            try
            {
                ObtenerIdCaja();
                ObtenerIdUsuario();
                CONEXIONMAESTRA.abrir();
                using (var cmd = new SqlCommand("insertar_venta", CONEXIONMAESTRA.conectar))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_usuario", _idUsuario);
                    cmd.Parameters.AddWithValue("@Idcaja", _idCaja);
                    var result = cmd.ExecuteScalar();
                    idVentaGenerada = Convert.ToInt32(result);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }

        public bool ConfirmarVenta(Lventas parametros)
        {
            try
            {
                ObtenerImpuesto();
                ObtenerIdCaja();
                ObtenerIdUsuario();
                ContarProdDv(parametros.idventa);
                CONEXIONMAESTRA.abrir();
                using (var cmd = new SqlCommand("Confirmar_venta", CONEXIONMAESTRA.conectar))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@idventa", parametros.idventa);
                    cmd.Parameters.AddWithValue("@idcliente", parametros.idclientev);
                    cmd.Parameters.AddWithValue("@fecha_venta", DateTime.Now);
                    cmd.Parameters.AddWithValue("@Monto_total", parametros.Monto_total);
                    cmd.Parameters.AddWithValue("@Tipo_de_pago", parametros.Tipo_de_pago);
                    decimal porcImpuesto = Convert.ToDecimal(_porcentajeImp / 100);
                    decimal subtotal = parametros.Monto_total / (1 + porcImpuesto);
                    decimal igv = parametros.Monto_total - subtotal;
                    cmd.Parameters.AddWithValue("@TotalIgv", igv);
                    cmd.Parameters.AddWithValue("@Id_usuario", _idUsuario);
                    cmd.Parameters.AddWithValue("@Fecha_de_pago", DateTime.Now);
                    cmd.Parameters.AddWithValue("@Saldo", parametros.Saldo);
                    cmd.Parameters.AddWithValue("@Pago_con", parametros.Pago_con);
                    cmd.Parameters.AddWithValue("@Id_caja", _idCaja);
                    cmd.Parameters.AddWithValue("@Referencia_tarjeta", parametros.Referencia_tarjeta);
                    cmd.Parameters.AddWithValue("@Vuelto", parametros.Vuelto);
                    cmd.Parameters.AddWithValue("@Efectivo", parametros.Efectivo);
                    cmd.Parameters.AddWithValue("@Credito", parametros.Credito);
                    cmd.Parameters.AddWithValue("@Tarjeta", parametros.Tarjeta);
                    cmd.Parameters.AddWithValue("@Idcomprobante", parametros.Idcomprobante);
                    cmd.Parameters.AddWithValue("@ContadorProductos", _contadorProd);
                    cmd.Parameters.AddWithValue("@TotSubtotal", subtotal);
                    cmd.Parameters.AddWithValue("@Porcentaje_IGV", _porcentajeImp);
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }

        public bool EliminarVentasIncomp()
        {
            try
            {
                ObtenerIdCaja();
                CONEXIONMAESTRA.abrir();
                using (var cmd = new SqlCommand("eliminarVentasIncomp", CONEXIONMAESTRA.conectar))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Idcaja", _idCaja);
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }

        public bool EliminarVenta(Lventas parametros)
        {
            try
            {
                ObtenerIdCaja();
                CONEXIONMAESTRA.abrir();
                using (var cmd = new SqlCommand("eliminar_venta", CONEXIONMAESTRA.conectar))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@idventa", parametros.idventa);
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }

        public bool EditarEstadoSunatVenta(Lventas parametros)
        {
            try
            {
                ObtenerIdCaja();
                CONEXIONMAESTRA.abrir();
                using (var cmd = new SqlCommand("editarEstadoSunatVenta", CONEXIONMAESTRA.conectar))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Idventa", parametros.idventa);
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }

        public void ContarFBAprobadas(ref int contador, string codigoComp)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                using (var cmd = new SqlCommand("ContFBAprobadas", CONEXIONMAESTRA.conectar))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Codigo", codigoComp);
                    contador = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception)
            {
                contador = 0;
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }

        public void ContarFBPendientes(ref int contador, string codigoComp)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                using (var cmd = new SqlCommand("ContFBPendientes", CONEXIONMAESTRA.conectar))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Codigo", codigoComp);
                    contador = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception)
            {
                contador = 0;
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }

        public bool EditarVentaAnulada(Lventas parametros)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                using (var cmd = new SqlCommand("editarVentaAnulada", CONEXIONMAESTRA.conectar))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@idventa", parametros.idventa);
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }

        public void BuscarVentasPorFechas(ref DataTable dt, DateTime fi, DateTime ff)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                using (var da = new SqlDataAdapter("buscarventasXfechas", CONEXIONMAESTRA.conectar))
                {
                    da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    da.SelectCommand.Parameters.AddWithValue("@fi", fi);
                    da.SelectCommand.Parameters.AddWithValue("@ff", ff);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                Logger.ShowError(ex);
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }
    }
}