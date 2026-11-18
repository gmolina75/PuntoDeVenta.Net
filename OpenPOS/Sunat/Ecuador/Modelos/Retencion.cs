using System.Collections.Generic;

namespace OpenPOS.Sunat.Ecuador
{
    public class Retencion
    {
        public string Ruc { get; set; }
        public string PuntoEmision { get; set; }
        public int Secuencial { get; set; }
        public System.DateTime FechaEmision { get; set; }
        public string RazonSocialComprador { get; set; }
        public string IdentificacionComprador { get; set; }
        public List<DetalleRetencion> Detalles { get; set; } = new List<DetalleRetencion>();
    }

    public class DetalleRetencion
    {
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal BaseImponible { get; set; }
        public decimal ValorRetenido { get; set; }
    }
}