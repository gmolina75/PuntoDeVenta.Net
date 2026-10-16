using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenPOS.CONEXION;
using Sunat.Logica;
using System.Data;
using System.Data.SqlClient;
using OpenPOS.Repositorios;

namespace OpenPOS.Datos
{
    public class Dventas
    {
        private readonly IVentasRepository _repository;

        public Dventas() : this(new SqlVentasRepository()) { }

        public Dventas(IVentasRepository repository)
        {
            _repository = repository;
        }

        public void mostrarVentasXidcaja(ref DataTable dt)
        {
            _repository.MostrarVentasXidcaja(ref dt);
        }

        public void MostrarventasPend(ref DataTable dt, string codigo)
        {
            _repository.MostrarVentasPend(ref dt, codigo);
        }

        public void MostrarResumendiarioPendiente(ref DataTable dt, string fecha)
        {
            _repository.MostrarResumendiarioPendiente(ref dt, fecha);
        }

        public void MostrarBoletaspendientes(ref DataTable dt)
        {
            _repository.MostrarBoletasPendientes(ref dt);
        }

        public void mostrarVentasXidventa(ref DataTable dt, Lventas parametros)
        {
            _repository.MostrarVentasXidventa(ref dt, parametros.idventa);
        }

        public void buscarVentas(ref DataTable dt, string buscador)
        {
            _repository.BuscarVentas(ref dt, buscador);
        }

        public void buscarFacturas(ref DataTable dt, string buscador, string codigoCompro)
        {
            _repository.BuscarFacturas(ref dt, buscador, codigoCompro);
        }

        public bool insertarVenta(ref int idventaGenerada)
        {
            return _repository.InsertarVenta(ref idventaGenerada);
        }

        public bool confirmarVenta(Lventas parametros)
        {
            return _repository.ConfirmarVenta(parametros);
        }

        public bool eliminarVentasIncomp()
        {
            return _repository.EliminarVentasIncomp();
        }

        public bool eliminarVenta(Lventas parametros)
        {
            return _repository.EliminarVenta(parametros);
        }

        public bool editarEstadoSunatVenta(Lventas parametros)
        {
            return _repository.EditarEstadoSunatVenta(parametros);
        }

        public void ContFBAprobadas(ref int contador, string codigoComp)
        {
            _repository.ContarFBAprobadas(ref contador, codigoComp);
        }

        public void ContFBPendientes(ref int contador, string codigoComp)
        {
            _repository.ContarFBPendientes(ref contador, codigoComp);
        }

        public bool editarVentaAnulada(Lventas parametros)
        {
            return _repository.EditarVentaAnulada(parametros);
        }

        public void buscarVentasPorFechas(ref DataTable dt, DateTime fi, DateTime ff)
        {
            _repository.BuscarVentasPorFechas(ref dt, fi, ff);
        }
    }
}