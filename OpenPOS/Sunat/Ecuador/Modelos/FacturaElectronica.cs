using System;
using System.Collections.Generic;

namespace OpenPOS.Sunat.Ecuador
{
    public class FacturaElectronica
    {
        public string Ruc { get; set; }
        public string PuntoEmision { get; set; }
        public int Secuencial { get; set; }
        public DateTime FechaEmision { get; set; }
        public string RazonSocialComprador { get; set; }
        public string IdentificacionComprador { get; set; }
        public string DireccionEstablecimiento { get; set; }
        public decimal TotalSinImpuestos { get; set; }
        public decimal Total { get; set; }
        public List<DetalleFactura> Detalles { get; set; } = new List<DetalleFactura>();
    }

    public class DetalleFactura : DetalleNota
    {
    }
}