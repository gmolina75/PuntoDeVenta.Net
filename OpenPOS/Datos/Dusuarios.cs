using OpenPOS.CONEXION;
using OpenPOS.Logica;
using RestCsharp.Presentacion.Licencia;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPOS.Datos
{
    public class Dusuarios
    {
        //public void mostrarUsuario(ref DataTable dt)
        //{
        //    try
        //    {
        //        CONEXIONMAESTRA.abrir();
        //        SqlDataAdapter da = new SqlDataAdapter("mostrar_usuario", CONEXIONMAESTRA.conectar);
        //        da.Fill(dt);
        //    }
        //    catch (Exception ex)
        //    {
        //        OpenPOS.Logging.Logger.ShowError(ex);
        //    }
        //    finally
        //    {
        //        CONEXIONMAESTRA.cerrar();
        //    }
        //}
        //public bool InsertarUsuarios(Lusuarios parametros)
        //{
        //    try
        //    {
        //        CONEXIONMAESTRA.abrir();
        //        SqlCommand cmd = new SqlCommand("insertar_usuario", CONEXIONMAESTRA.conectar);
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        cmd.Parameters.AddWithValue("@nombres", parametros.Nombres_y_Apellidos);
        //        cmd.Parameters.AddWithValue("@Login", parametros.Login);
        //        cmd.Parameters.AddWithValue("@Password", parametros.Password);
        //        cmd.Parameters.AddWithValue("@Nombre_de_icono", parametros.Nombre_de_icono);
        //        cmd.Parameters.AddWithValue("@Correo", "-");
        //        cmd.Parameters.AddWithValue("@Rol", parametros.Rol);
        //        cmd.Parameters.AddWithValue("@Estado", "ACTIVO");
        //        cmd.Parameters.AddWithValue("@Icono", parametros.Icono);
        //        cmd.ExecuteNonQuery();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //        return false;
        //    }
        //    finally
        //    {
        //        CONEXIONMAESTRA.cerrar();
        //    }
        //}
        //public bool editarUsuario(Lusuarios parametros)
        //{
        //    try
        //    {
        //        CONEXIONMAESTRA.abrir();
        //        SqlCommand cmd = new SqlCommand("editar_usuario", CONEXIONMAESTRA.conectar);
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        cmd.Parameters.AddWithValue("@idUsuario", parametros.idUsuario);
        //        cmd.Parameters.AddWithValue("@nombres", parametros.Nombres_y_Apellidos);
        //        cmd.Parameters.AddWithValue("@Login", parametros.Login);
        //        cmd.Parameters.AddWithValue("@Password", parametros.Password);
        //        cmd.Parameters.AddWithValue("@Nombre_de_icono", parametros.Nombre_de_icono);
        //        cmd.Parameters.AddWithValue("@Correo", parametros.Correo);
        //        cmd.Parameters.AddWithValue("@Rol", parametros.Rol);
        //        cmd.Parameters.AddWithValue("@Icono", parametros.Icono);
        //        cmd.ExecuteNonQuery();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //        return false;
        //    }
        //    finally
        //    {
        //        CONEXIONMAESTRA.cerrar();
        //    }
        //}
        //public bool eliminarUsuario(Lusuarios parametros)
        //{
        //    try
        //    {
        //        CONEXIONMAESTRA.abrir();
        //        SqlCommand cmd = new SqlCommand("eliminar_usuario", CONEXIONMAESTRA.conectar);
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        cmd.Parameters.AddWithValue("@idusuario", parametros.idUsuario);
        //        cmd.Parameters.AddWithValue("@login", parametros.Login);

        //        cmd.ExecuteNonQuery();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //        return false;
        //    }
        //    finally
        //    {
        //        CONEXIONMAESTRA.cerrar();
        //    }
        //}
        //public void buscarUsuario(ref DataTable dt,string buscador)
        //{
        //    try
        //    {
        //        CONEXIONMAESTRA.abrir();
        //        SqlDataAdapter da = new SqlDataAdapter("buscar_usuario", CONEXIONMAESTRA.conectar);
        //        da.SelectCommand.CommandType = CommandType.StoredProcedure;
        //        da.SelectCommand.Parameters.AddWithValue("@letra", buscador);
        //        da.Fill(dt);
        //    }
        //    catch (Exception ex)
        //    {
        //        OpenPOS.Logging.Logger.ShowError(ex);
        //    }
        //    finally
        //    {
        //        CONEXIONMAESTRA.cerrar();
        //    }
        //}
        int idusuario;
        string resultadoLic;
        string tipocaja;
        public bool editar_Usuarios(Lusuarios parametros)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand cmd = new SqlCommand("editar_usuario", CONEXIONMAESTRA.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdUsuario", parametros.IdUsuario);
                cmd.Parameters.AddWithValue("@Nombre", parametros.Nombre);
                cmd.Parameters.AddWithValue("@Login", parametros.Login);
                cmd.Parameters.AddWithValue("@Password", parametros.Password);
                cmd.Parameters.AddWithValue("@Icono", parametros.Icono);
                cmd.Parameters.AddWithValue("@Correo", parametros.Correo);
                cmd.Parameters.AddWithValue("@Rol", parametros.Rol);

                cmd.ExecuteNonQuery();
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
        public bool eliminar_Usuarios(Lusuarios parametros)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand cmd = new SqlCommand("eliminar_usuario", CONEXIONMAESTRA.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@idusuario", parametros.IdUsuario);
                cmd.Parameters.AddWithValue("@login", parametros.Login);
                cmd.ExecuteNonQuery();
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
        public bool restaurar_usuario(Lusuarios parametros)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand cmd = new SqlCommand("restaurar_usuario", CONEXIONMAESTRA.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@idusuario", parametros.IdUsuario);

                cmd.ExecuteNonQuery();
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
        public void mostrar_Usuarios(ref DataTable dt)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("mostrar_Usuarios", CONEXIONMAESTRA.conectar);
                da.Fill(dt);
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
        public void buscar_usuarios(ref DataTable dt, string buscador)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("buscar_usuarios", CONEXIONMAESTRA.conectar);
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
        public void ObtenerIdUsuario(ref int idusuario, string Login)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand da = new SqlCommand("ObtenerIdUsuario", CONEXIONMAESTRA.conectar);
                da.CommandType = CommandType.StoredProcedure;
                da.Parameters.AddWithValue("@Login", Login);
                idusuario = Convert.ToInt32(da.ExecuteScalar());
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

        public bool InsertarUsuarios(Lusuarios parametros)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand cmd = new SqlCommand("insertar_Usuarios", CONEXIONMAESTRA.conectar);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Nombre", parametros.Nombre);
                cmd.Parameters.AddWithValue("@Login", parametros.Login);
                cmd.Parameters.AddWithValue("@Password", parametros.Password);
                cmd.Parameters.AddWithValue("@Icono", parametros.Icono);
                cmd.Parameters.AddWithValue("@Correo", parametros.Correo);
                cmd.Parameters.AddWithValue("@Rol", parametros.Rol);
                cmd.Parameters.AddWithValue("@Estado", "ACTIVO");
                cmd.ExecuteNonQuery();
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
        private void Mostrartipocaja()
        {
            var funcion = new Dcaja();
            var dt = new DataTable();
            funcion.mostrarCajaSerialTable(ref dt);
            tipocaja = dt.Rows[0][1].ToString();

        }
        public void licenciasExtras(ref string resultado, ref Panel panel)
        {
            resultado = "NO APLICA";
        }
        public void dibujarUsuarios(ref DataTable dt, ref string resultado, ref Panel panel)
        {
            resultado = "";
            mostrarUsuarios(dt);
        }
        private void mostrarUsuarios(DataTable dt)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlDataAdapter da = new SqlDataAdapter("Select * from Usuario where Estado='ACTIVO' and Rol<>'Cliente'", CONEXIONMAESTRA.conectar);
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
        /// <summary>
        /// Valida el login recibido en claro. Si la contraseña almacenada aún usa el
        /// cifrado reversible histórico, la valida y la migra automáticamente a hash.
        /// </summary>
        public void validarUsuario(Lusuarios parametros, ref int id)
        {
            id = 0;
            try
            {
                string almacenado = ObtenerPasswordAlmacenado(parametros.Login);
                if (string.IsNullOrEmpty(almacenado)) return;

                if (PasswordHasher.EsHash(almacenado))
                {
                    if (PasswordHasher.Verificar(parametros.Password, almacenado))
                    {
                        ObtenerIdUsuario(ref id, parametros.Login);
                    }
                }
                else
                {
                    // Contraseña histórica cifrada reversible: puede haberse cifrado
                    // con la clave heredada (antes de la migración) o con la actual.
                    string legado = Bases.Encriptar(parametros.Password);
                    bool coincide = string.Equals(legado, almacenado, StringComparison.Ordinal);
                    if (!coincide && !KeyProvider.UsandoClaveHeredada)
                    {
                        string legadoHeredado = Bases.Encriptar(parametros.Password, KeyProvider.LegacyKey);
                        coincide = string.Equals(legadoHeredado, almacenado, StringComparison.Ordinal);
                    }
                    if (coincide)
                    {
                        ObtenerIdUsuario(ref id, parametros.Login);
                        if (id > 0)
                        {
                            ActualizarPassword(id, PasswordHasher.Hash(parametros.Password));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OpenPOS.Logging.Logger.ShowError(ex);
                id = 0;
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }

        private string ObtenerPasswordAlmacenado(string login)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand cmd = new SqlCommand(
                    "SELECT Password FROM Usuario WHERE Login=@Login AND Estado='ACTIVO'",
                    CONEXIONMAESTRA.conectar);
                cmd.Parameters.AddWithValue("@Login", login);
                object resultado = cmd.ExecuteScalar();
                if (resultado == null || resultado == DBNull.Value) return null;
                return Convert.ToString(resultado);
            }
            catch (Exception ex)
            {
                OpenPOS.Logging.Logger.ShowError(ex);
                return null;
            }
            finally
            {
                CONEXIONMAESTRA.cerrar();
            }
        }

        private void ActualizarPassword(int idUsuario, string nuevoHash)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Usuario SET Password=@Password WHERE idUsuario=@IdUsuario",
                    CONEXIONMAESTRA.conectar);
                cmd.Parameters.AddWithValue("@Password", nuevoHash);
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cmd.ExecuteNonQuery();
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

        public void mostrarRoles(Lusuarios parametros, ref string rol)
        {
            try
            {
                CONEXIONMAESTRA.abrir();
                SqlCommand da = new SqlCommand("mostrarRoles", CONEXIONMAESTRA.conectar);
                da.CommandType = CommandType.StoredProcedure;
                da.Parameters.AddWithValue("@idusario", parametros.IdUsuario);
                rol = da.ExecuteScalar().ToString();
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
