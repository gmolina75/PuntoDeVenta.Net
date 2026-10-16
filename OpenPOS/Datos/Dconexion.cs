using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using OpenPOS.CONEXION;

namespace OpenPOS.Datos
{
   public class Dconexion
    {
		public bool validarConexion(ref int contador)
		{
			try
			{
				CONEXIONMAESTRA.abrir();
				SqlCommand cmd = new SqlCommand("select count(idUsuario) from Usuario", CONEXIONMAESTRA.conectar);
				contador = Convert.ToInt32(cmd.ExecuteScalar());
				return true;
			}
			catch (Exception)
			{
				return false;
			}
			finally
			{
				CONEXIONMAESTRA.cerrar();
			}
			
		}
	}
}
