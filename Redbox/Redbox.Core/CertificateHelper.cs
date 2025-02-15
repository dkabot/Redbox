using System.Security.Cryptography.X509Certificates;

namespace Redbox.Core
{
    public static class CertificateHelper
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
                return null;
            var x509Store = (X509Store)null;
            try
            {
                x509Store = new X509Store(storeName, storeLocation);
                x509Store.Open(OpenFlags.ReadOnly);
                var certificate2Collection =
                    x509Store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, false);
                return certificate2Collection.Count > 0 ? certificate2Collection[0] : null;
            }
            finally
            {
                x509Store?.Close();
            }
        }
    }
}