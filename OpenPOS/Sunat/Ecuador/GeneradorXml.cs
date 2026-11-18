using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace OpenPOS.Sunat.Ecuador
{
    internal static class GeneradorXml
    {
        private const string Version = "2.0.0";
        private static readonly XNamespace XmlNs = "http://www.sri.gob.ec/factura/";

        private static void AddImpuesto(XElement parent, decimal baseImp)
        {
            var imp = new XElement(XmlNs + "impuesto");
            imp.Add(new XElement(XmlNs + "codigo", "2"));
            imp.Add(new XElement(XmlNs + "codigoPorcentaje", "2"));
            imp.Add(new XElement(XmlNs + "tarifa", "12"));
            imp.Add(new XElement(XmlNs + "baseImponible", baseImp.ToString("F2")));
            imp.Add(new XElement(XmlNs + "valor", (baseImp * 0.12m).ToString("F2")));
            parent.Add(imp);
        }

        public static string GenerarFactura(FacturaElectronica factura)
        {
            var root = new XElement(XmlNs + "comprobante", new XAttribute("version", Version));

            var infoTrib = new XElement(XmlNs + "infoTributaria");
            infoTrib.Add(new XElement(XmlNs + "ruc", factura.Ruc));
            infoTrib.Add(new XElement(XmlNs + "tipoIdentificacionComprador", "04"));
            infoTrib.Add(new XElement(XmlNs + "razonSocialComprador", factura.RazonSocialComprador));
            infoTrib.Add(new XElement(XmlNs + "fechaEmision", factura.FechaEmision.ToString("yyyy-MM-dd")));
            infoTrib.Add(new XElement(XmlNs + "puntoEmision", factura.PuntoEmision));
            infoTrib.Add(new XElement(XmlNs + "secuencial", factura.Secuencial.ToString("D9")));
            infoTrib.Add(new XElement(XmlNs + "codDoc", "01"));
            root.Add(infoTrib);

            var infoFac = new XElement(XmlNs + "infoFactura");
            infoFac.Add(new XElement(XmlNs + "fechaEmision", factura.FechaEmision.ToString("yyyy-MM-dd")));
            infoFac.Add(new XElement(XmlNs + "dirEstablecimiento", factura.DireccionEstablecimiento));
            infoFac.Add(new XElement(XmlNs + "obligadoContabilidad", "NO"));
            infoFac.Add(new XElement(XmlNs + "tipoIdentificacionComprador", "04"));
            infoFac.Add(new XElement(XmlNs + "razonSocialComprador", factura.RazonSocialComprador));
            infoFac.Add(new XElement(XmlNs + "identificacionComprador", factura.IdentificacionComprador));
            infoFac.Add(new XElement(XmlNs + "totalSinImpuestos", factura.TotalSinImpuestos.ToString("F2")));
            infoFac.Add(new XElement(XmlNs + "totalDescuento", "0.00"));
            var imps = new XElement(XmlNs + "impuestos");
            AddImpuesto(imps, factura.TotalSinImpuestos);
            infoFac.Add(imps);
            infoFac.Add(new XElement(XmlNs + "valorTotal", factura.Total.ToString("F2")));
            infoFac.Add(new XElement(XmlNs + "formasPago", new XElement(XmlNs + "formaPago", "01")));
            root.Add(infoFac);

            root.Add(ConstruirDetalles(factura.Detalles));
            var doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), root);
            return doc.ToString();
        }

        public static string GenerarNotaCredito(NotaCredito nota)
        {
            var root = new XElement(XmlNs + "comprobante", new XAttribute("version", Version));

            var infoTrib = new XElement(XmlNs + "infoTributaria");
            infoTrib.Add(new XElement(XmlNs + "ruc", nota.Ruc));
            infoTrib.Add(new XElement(XmlNs + "codDoc", "04"));
            root.Add(infoTrib);

            var infoNC = new XElement(XmlNs + "infoNotaCredito");
            infoNC.Add(new XElement(XmlNs + "fechaEmision", nota.FechaEmision.ToString("yyyy-MM-dd")));
            infoNC.Add(new XElement(XmlNs + "puntoEmision", nota.PuntoEmision));
            infoNC.Add(new XElement(XmlNs + "secuencial", nota.Secuencial.ToString("D9")));
            infoNC.Add(new XElement(XmlNs + "razonSocialComprador", nota.RazonSocialComprador));
            infoNC.Add(new XElement(XmlNs + "identificacionComprador", nota.IdentificacionComprador));
            infoNC.Add(new XElement(XmlNs + "codDocModificado", "01"));
            root.Add(infoNC);

            var motivos = new XElement(XmlNs + "motivos");
            var motivo = new XElement(XmlNs + "motivo");
            motivo.Add(new XElement(XmlNs + "codigo", "01"));
            motivos.Add(motivo);
            root.Add(motivos);

            root.Add(ConstruirDetallesNota(nota.Detalles));
            var doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), root);
            return doc.ToString();
        }

        public static string GenerarNotaDebito(NotaDebito nota)
        {
            var root = new XElement(XmlNs + "comprobante", new XAttribute("version", Version));

            var infoTrib = new XElement(XmlNs + "infoTributaria");
            infoTrib.Add(new XElement(XmlNs + "ruc", nota.Ruc));
            infoTrib.Add(new XElement(XmlNs + "codDoc", "05"));
            root.Add(infoTrib);

            var infoND = new XElement(XmlNs + "infoNotaDebito");
            infoND.Add(new XElement(XmlNs + "fechaEmision", nota.FechaEmision.ToString("yyyy-MM-dd")));
            infoND.Add(new XElement(XmlNs + "puntoEmision", nota.PuntoEmision));
            infoND.Add(new XElement(XmlNs + "secuencial", nota.Secuencial.ToString("D9")));
            infoND.Add(new XElement(XmlNs + "razonSocialComprador", nota.RazonSocialComprador));
            infoND.Add(new XElement(XmlNs + "identificacionComprador", nota.IdentificacionComprador));
            infoND.Add(new XElement(XmlNs + "codDocModificado", "01"));
            root.Add(infoND);

            var motivo = new XElement(XmlNs + "motivo");
            motivo.Add(new XElement(XmlNs + "codigo", "01"));
            root.Add(motivo);

            root.Add(ConstruirDetallesNota(nota.Detalles));
            var doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), root);
            return doc.ToString();
        }

        public static string GenerarGuiaRemision(GuiaRemision guia)
        {
            var root = new XElement(XmlNs + "comprobante", new XAttribute("version", Version));

            var infoTrib = new XElement(XmlNs + "infoTributaria");
            infoTrib.Add(new XElement(XmlNs + "ruc", guia.Ruc));
            infoTrib.Add(new XElement(XmlNs + "codDoc", "06"));
            root.Add(infoTrib);

            var infoGR = new XElement(XmlNs + "infoGuiaRemision");
            infoGR.Add(new XElement(XmlNs + "fechaEmision", guia.FechaEmision.ToString("yyyy-MM-dd")));
            infoGR.Add(new XElement(XmlNs + "puntoEmision", guia.PuntoEmision));
            infoGR.Add(new XElement(XmlNs + "secuencial", guia.Secuencial.ToString("D9")));
            infoGR.Add(new XElement(XmlNs + "razonSocialComprador", guia.RazonSocialComprador));
            infoGR.Add(new XElement(XmlNs + "identificacionComprador", guia.IdentificacionComprador));
            infoGR.Add(new XElement(XmlNs + "direccionDestino", guia.DireccionDestino));
            infoGR.Add(new XElement(XmlNs + "motivoTraslado", guia.MotivoTraslado));
            root.Add(infoGR);

            var dets = new XElement(XmlNs + "detalles");
            foreach (var d in guia.Detalles)
            {
                var det = new XElement(XmlNs + "detalle");
                det.Add(new XElement(XmlNs + "codigo", d.Codigo));
                det.Add(new XElement(XmlNs + "descripcion", d.Descripcion));
                det.Add(new XElement(XmlNs + "cantidad", d.Cantidad.ToString("F6")));
                det.Add(new XElement(XmlNs + "unidadMedida", d.UnidadMedida));
                dets.Add(det);
            }
            root.Add(dets);

            var doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), root);
            return doc.ToString();
        }

        public static string GenerarRetencion(Retencion ret)
        {
            var root = new XElement(XmlNs + "comprobante", new XAttribute("version", Version));

            var infoTrib = new XElement(XmlNs + "infoTributaria");
            infoTrib.Add(new XElement(XmlNs + "ruc", ret.Ruc));
            infoTrib.Add(new XElement(XmlNs + "codDoc", "07"));
            root.Add(infoTrib);

            var infoR = new XElement(XmlNs + "infoRetencion");
            infoR.Add(new XElement(XmlNs + "fechaEmision", ret.FechaEmision.ToString("yyyy-MM-dd")));
            infoR.Add(new XElement(XmlNs + "puntoEmision", ret.PuntoEmision));
            infoR.Add(new XElement(XmlNs + "secuencial", ret.Secuencial.ToString("D9")));
            infoR.Add(new XElement(XmlNs + "razonSocialComprador", ret.RazonSocialComprador));
            infoR.Add(new XElement(XmlNs + "identificacionComprador", ret.IdentificacionComprador));
            root.Add(infoR);

            var dets = new XElement(XmlNs + "detalles");
            foreach (var d in ret.Detalles)
            {
                var det = new XElement(XmlNs + "detalle");
                det.Add(new XElement(XmlNs + "codigo", d.Codigo));
                det.Add(new XElement(XmlNs + "descripcion", d.Descripcion));
                det.Add(new XElement(XmlNs + "porcentaje", d.Porcentaje.ToString("F2")));
                det.Add(new XElement(XmlNs + "baseImponible", d.BaseImponible.ToString("F2")));
                det.Add(new XElement(XmlNs + "valorRetenido", d.ValorRetenido.ToString("F2")));
                dets.Add(det);
            }
            root.Add(dets);

            var doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), root);
            return doc.ToString();
        }

        private static XElement ConstruirDetalles(IEnumerable<DetalleFactura> detalles)
        {
            var dets = new XElement(XmlNs + "detalles");
            foreach (var d in detalles)
            {
                var det = new XElement(XmlNs + "detalle");
                det.Add(new XElement(XmlNs + "codigoPrincipal", d.Codigo));
                det.Add(new XElement(XmlNs + "codigoAuxiliar", d.CodigoAuxiliar));
                det.Add(new XElement(XmlNs + "descripcion", d.Descripcion));
                det.Add(new XElement(XmlNs + "cantidad", d.Cantidad.ToString("F6")));
                det.Add(new XElement(XmlNs + "precioUnitario", d.PrecioUnitario.ToString("F6")));
                det.Add(new XElement(XmlNs + "descuento", d.Descuento.ToString("F6")));
                det.Add(new XElement(XmlNs + "precioTotalSinImpuesto", d.PrecioTotal.ToString("F6")));
                var imps = new XElement(XmlNs + "impuestos");
                AddImpuesto(imps, d.PrecioTotal);
                det.Add(imps);
                dets.Add(det);
            }
            return dets;
        }

        private static XElement ConstruirDetallesNota(IEnumerable<DetalleNota> detalles)
        {
            var dets = new XElement(XmlNs + "detalles");
            foreach (var d in detalles)
            {
                var det = new XElement(XmlNs + "detalle");
                det.Add(new XElement(XmlNs + "codigoPrincipal", d.Codigo));
                det.Add(new XElement(XmlNs + "codigoAuxiliar", d.CodigoAuxiliar));
                det.Add(new XElement(XmlNs + "descripcion", d.Descripcion));
                det.Add(new XElement(XmlNs + "cantidad", d.Cantidad.ToString("F6")));
                det.Add(new XElement(XmlNs + "precioUnitario", d.PrecioUnitario.ToString("F6")));
                det.Add(new XElement(XmlNs + "descuento", d.Descuento.ToString("F6")));
                det.Add(new XElement(XmlNs + "precioTotalSinImpuesto", d.PrecioTotal.ToString("F6")));
                var imps = new XElement(XmlNs + "impuestos");
                AddImpuesto(imps, d.PrecioTotal);
                det.Add(imps);
                dets.Add(det);
            }
            return dets;
        }
    }
}