using Redbox.Core;
using System;
using System.IO;
using System.Text;

namespace Redbox.UpdateService.Client
{
    internal class UpdateServiceCredentials
    {
        public static UpdateServiceCredentials _instance;
        private Credentials _credentials;
        private static readonly string _key1 = "{EFFBA16E-4871-4495-9E6B-BB78452BBD97}";
        private static readonly string _key2 = "{929CD6EA-8D36-4ECB-B8B7-6B5053536968}";

        public static UpdateServiceCredentials Instance
        {
            get
            {
                if (UpdateServiceCredentials._instance == null)
                    UpdateServiceCredentials._instance = new UpdateServiceCredentials();
                return UpdateServiceCredentials._instance;
            }
        }

        public Credentials GetCredentials()
        {
            try
            {
                string directoryName = Path.GetDirectoryName(typeof(Redbox.UpdateService.Client.UpdateService).Assembly.Location);
                string str1 = Path.Combine(directoryName, "usdata.active");
                string str2 = Path.Combine(directoryName, "usdata.staged");
                if (File.Exists(str2))
                {
                    if (File.Exists(str1))
                        File.Delete(str1);
                    File.Move(str2, str1);
                    this._credentials = (Credentials)null;
                }
                if (this._credentials != null)
                    return this._credentials;
                if (!File.Exists(str1))
                {
                    this._credentials = new Credentials()
                    {
                        Key1 = UpdateServiceCredentials._key1,
                        Key2 = UpdateServiceCredentials._key2
                    };
                    return this._credentials;
                }
                this._credentials = Encoding.UTF8.GetString(File.ReadAllBytes(str1).Decrypt()).ToObject<Credentials>();
                return this._credentials;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException("An unhandled exception occurred in getting credentials", ex);
            }
            return (Credentials)null;
        }

        public string GetCredentialHash(string kioskId)
        {
            Credentials credentials = this.GetCredentials();
            return this.GetCredentialHash(kioskId, credentials.Key1, credentials.Key2);
        }

        private string GetCredentialHash(string kioskId, string key1, string key2)
        {
            return Encoding.ASCII.GetBytes(string.Format("{0}-{1}-{2}", (object)kioskId, (object)key1, (object)key2)).ToASCIISHA1Hash();
        }
    }
}
