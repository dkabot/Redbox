using System;
using System.IO;
using System.Text;
using Redbox.Core;

namespace Redbox.UpdateService.Client
{
    public class UpdateServiceCredentials
    {
        public static UpdateServiceCredentials _instance;
        private static readonly string _key1 = "{EFFBA16E-4871-4495-9E6B-BB78452BBD97}";
        private static readonly string _key2 = "{929CD6EA-8D36-4ECB-B8B7-6B5053536968}";
        private Credentials _credentials;

        public static UpdateServiceCredentials Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new UpdateServiceCredentials();
                return _instance;
            }
        }

        public Credentials GetCredentials()
        {
            try
            {
                var directoryName = Path.GetDirectoryName(typeof(UpdateService).Assembly.Location);
                var str1 = Path.Combine(directoryName, "usdata.active");
                var str2 = Path.Combine(directoryName, "usdata.staged");
                if (File.Exists(str2))
                {
                    if (File.Exists(str1))
                        File.Delete(str1);
                    File.Move(str2, str1);
                    _credentials = null;
                }

                if (_credentials != null)
                    return _credentials;
                if (!File.Exists(str1))
                {
                    _credentials = new Credentials
                    {
                        Key1 = _key1,
                        Key2 = _key2
                    };
                    return _credentials;
                }

                _credentials =
                    StringExtensions.ToObject<Credentials>(
                        Encoding.UTF8.GetString(ByteArrayExtensions.Decrypt(File.ReadAllBytes(str1))));
                return _credentials;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException("An unhandled exception occurred in getting credentials", ex);
            }

            return null;
        }

        public string GetCredentialHash(string kioskId)
        {
            var credentials = GetCredentials();
            return GetCredentialHash(kioskId, credentials.Key1, credentials.Key2);
        }

        private string GetCredentialHash(string kioskId, string key1, string key2)
        {
            return ByteArrayExtensions.ToASCIISHA1Hash(
                Encoding.ASCII.GetBytes(string.Format("{0}-{1}-{2}", kioskId, key1, key2)));
        }
    }
}