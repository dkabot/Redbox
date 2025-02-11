using System.Security.Cryptography.X509Certificates;

namespace Redbox.Core
{
    internal static class CertificateHelper
    {
        public static X509Certificate2 GetCertificateFromFile(string fileName)
        {
            return new X509Certificate2(fileName);
        }

        public static X509Certificate2 GetCertificateFromFile(string fileName, string password)
        {
            return new X509Certificate2(fileName, password);
        }

        public static X509Certificate2 GetCertificateBySubjectName(
          StoreName storeName,
          StoreLocation storeLocation,
          string subjectName)
        {
            if (string.IsNullOrEmpty(subjectName))
                return (X509Certificate2)null;
            X509Store x509Store = (X509Store)null;
            try
            {
                x509Store = new X509Store(storeName, storeLocation);
                x509Store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certificate2Collection = x509Store.Certificates.Find(X509FindType.FindBySubjectName, (object)subjectName, false);
                return certificate2Collection.Count > 0 ? certificate2Collection[0] : (X509Certificate2)null;
            }
            finally
            {
                x509Store?.Close();
            }
        }
    }
}
