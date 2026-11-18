using System;
using System.Collections.Generic;

namespace OpenPOS.Sunat.Ecuador
{
    public class NotaDebito
    {
        public string Ruc { get; set; }
        public string PuntoEmision { get; set; }
        public int Secuencial { get; set; }
        public DateTime FechaEmision { get; set; }
        public string RazonSocialComprador { get; set; }
        public string IdentificacionComprador { get; set; }
        public string FormaPago { get; set; }
        public string Observacion { get; set; }
        public List<DetalleNota> Detalles { get; set; } = new List<DetalleNota>();
    }

    public class DetalleDebito : DetalleNota
    {
    }
}