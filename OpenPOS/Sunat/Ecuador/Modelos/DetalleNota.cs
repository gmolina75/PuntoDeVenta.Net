using System;

namespace OpenPOS.Sunat.Ecuador
{
    public class DetalleNota
    {
        public string Codigo { get; set; }
        public string CodigoAuxiliar { get; set; }
        public string Descripcion { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Descuento { get; set; }
        public decimal PrecioTotal { get; set; }
    }
}