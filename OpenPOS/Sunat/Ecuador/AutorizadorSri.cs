using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OpenPOS.Sunat.Ecuador
{
    internal static class AutorizadorSri
    {
        private static readonly string SriUrl = "https://cel.sri.gob.ec";

        public static async Task<AutorizacionResult> AutorizarAsync(string xmlContent, string certificadoPath, string contraseña)
        {
            var firma = FirmaDigital.FirmarXml(xmlContent, certificadoPath, contraseña);
            var claveAcceso = CalcularClaveAcceso(xmlContent);

            using (var webClient = new WebClient())
            {
                webClient.Headers.Add("Content-Type", "application/xml");
                var solicitud = new XDocument(
                    new XElement("solicitudAutorizacion",
                        new XElement("claveAcceso", claveAcceso),
                        new XElement("comprobante", Convert.ToBase64String(Encoding.UTF8.GetBytes(xmlContent)))
                    )
                );

                var response = await webClient.UploadStringTaskAsync(new Uri(SriUrl + "/autorizacion/api/v1/autorizacion"), solicitud.ToString());
                var responseXml = XDocument.Parse(response);

                var estado = responseXml.Descendants("estado").FirstOrDefault()?.Value;
                var clave = responseXml.Descendants("claveAcceso").FirstOrDefault()?.Value;

                return new AutorizacionResult
                {
                    Autorizado = estado == "AUTORIZADO",
                    ClaveAcceso = clave,
                    Mensaje = estado,
                    XmlRespuesta = responseXml.ToString()
                };
            }
        }

        private static string CalcularClaveAcceso(string xmlContent)
        {
            var bytes = Encoding.UTF8.GetBytes(xmlContent);
            var hash = System.Security.Cryptography.SHA1.Create();
            var hashBytes = hash.ComputeHash(bytes);
            var clave = Convert.ToBase64String(hashBytes);
            return clave.Substring(0, 27) + "|" + clave.Substring(27, 2) + "|" + clave.Substring(29);
        }
    }

    public class AutorizacionResult
    {
        public bool Autorizado { get; set; }
        public string ClaveAcceso { get; set; }
        public string Mensaje { get; set; }
        public string XmlRespuesta { get; set; }
    }
}