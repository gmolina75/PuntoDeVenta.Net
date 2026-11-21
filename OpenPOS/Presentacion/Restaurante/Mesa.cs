using System;

namespace OpenPOS.Presentacion.Restaurante
{
    public class Mesa
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int Capacidad { get; set; }
        public EstadoMesa Estado { get; set; } = EstadoMesa.Libre;
        public int? IdVenta { get; set; }
        public DateTime HoraUltima { get; set; }

        public bool EstaOcupada => Estado == EstadoMesa.Ocupada || Estado == EstadoMesa.Reservada;
    }

    public enum EstadoMesa
    {
        Libre,
        Ocupada,
        Reservada,
        Limpieza
    }
}