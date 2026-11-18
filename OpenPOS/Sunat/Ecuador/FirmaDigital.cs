using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace OpenPOS.Sunat.Ecuador
{
    internal static class FirmaDigital
    {
        public static byte[] FirmarXml(string xmlContent, string certificadoPath, string contraseña)
        {
            var cert = new X509Certificate2(certificadoPath, contraseña, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
            
            using (var rsa = cert.GetRSAPrivateKey())
            {
                var data = Encoding.UTF8.GetBytes(xmlContent);
                var firma = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                return firma;
            }
        }

        public static string FirmarYAdjuntar(string xmlContent, string certificadoPath, string contraseña)
        {
            var firma = FirmarXml(xmlContent, certificadoPath, contraseña);
            var firmaBase64 = Convert.ToBase64String(firma);
            
            int insertPos = xmlContent.LastIndexOf("</comprobante>");
            if (insertPos == -1) throw new InvalidOperationException("XML inválido: falta etiqueta comprobante");
            
            var signatureBlock = $"<firma digital=\"{firmaBase64}\"/>";
            return xmlContent.Insert(insertPos, signatureBlock);
        }
    }
}