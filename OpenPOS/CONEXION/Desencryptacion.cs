
using System.Xml;


namespace OpenPOS.CONEXION

{
   public  class Desencryptacion
    {
      static   private AES aes = new AES();
       static  public string CnString;
       static   string dbcnString ;

        /// <summary>
        /// Clave maestra de cifrado. Ya no está incrustada en el código:
        /// se resuelve mediante <see cref="KeyProvider"/> (entorno, configuración
        /// o archivo protegido con DPAPI).
        /// </summary>
        public static string appPwdUnique
        {
            get { return KeyProvider.MasterKey; }
        }


        public static object checkServer()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("ConnectionString.xml");
            XmlElement root = doc.DocumentElement;
             dbcnString = root.Attributes[0].Value;
             CnString = ( aes.Decrypt(dbcnString, appPwdUnique,  int.Parse("256")));
            return CnString;

        }
        //public static object checkServerWEB()
        //{
        //    XmlDocument doc = new XmlDocument();
        //    doc.Load("ConnectionString.xml");
        //    XmlElement root = doc.DocumentElement;
        //    dbcnString = root.Attributes[0].Value;
        //    CnString = (aes.Decrypt(dbcnString, appPwdUnique, int.Parse("256")));
        //    return CnString;

        //}
        internal class label
        {

        }
        public static object UsuariosEncryp()
        {
            XmlDocument doc = new XmlDocument();
            label root = new label();
        
            dbcnString = root.ToString () ;
            CnString = (aes.Decrypt(dbcnString, appPwdUnique, int.Parse("256")));
            return CnString;

        }
    }

    }
