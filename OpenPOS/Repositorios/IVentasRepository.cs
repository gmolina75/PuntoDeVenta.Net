using System;
using System.Data;
using Sunat.Logica;

namespace OpenPOS.Repositorios
{
    public interface IVentasRepository
    {
        void MostrarVentasXidcaja(ref DataTable dt);
        void MostrarVentasPend(ref DataTable dt, string codigo);
        void MostrarResumendiarioPendiente(ref DataTable dt, string fecha);
        void MostrarBoletasPendientes(ref DataTable dt);
        void MostrarVentasXidventa(ref DataTable dt, int idVenta);
        void BuscarVentas(ref DataTable dt, string buscador);
        void BuscarFacturas(ref DataTable dt, string buscador, string codigoCompro);
        bool InsertarVenta(ref int idVentaGenerada);
        bool ConfirmarVenta(Lventas parametros);
        bool EliminarVentasIncomp();
        bool EliminarVenta(Lventas parametros);
        bool EditarEstadoSunatVenta(Lventas parametros);
        void ContarFBAprobadas(ref int contador, string codigoComp);
        void ContarFBPendientes(ref int contador, string codigoComp);
        bool EditarVentaAnulada(Lventas parametros);
        void BuscarVentasPorFechas(ref DataTable dt, DateTime fi, DateTime ff);
    }
}