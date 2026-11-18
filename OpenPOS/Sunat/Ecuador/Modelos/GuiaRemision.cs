using System.Collections.Generic;

namespace OpenPOS.Sunat.Ecuador
{
    public class GuiaRemision
    {
        public string Ruc { get; set; }
        public string PuntoEmision { get; set; }
        public int Secuencial { get; set; }
        public System.DateTime FechaEmision { get; set; }
        public string RazonSocialComprador { get; set; }
        public string IdentificacionComprador { get; set; }
        public string DireccionDestino { get; set; }
        public string MotivoTraslado { get; set; }
        public List<DetalleGuia> Detalles { get; set; } = new List<DetalleGuia>();
    }

    public class DetalleGuia
    {
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
        public decimal Cantidad { get; set; }
        public string UnidadMedida { get; set; }
    }
}