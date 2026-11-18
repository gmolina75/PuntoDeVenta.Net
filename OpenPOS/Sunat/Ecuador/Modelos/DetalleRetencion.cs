namespace OpenPOS.Sunat.Ecuador
{
    public class DetalleRetencion
    {
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal BaseImponible { get; set; }
        public decimal ValorRetenido { get; set; }
    }
}