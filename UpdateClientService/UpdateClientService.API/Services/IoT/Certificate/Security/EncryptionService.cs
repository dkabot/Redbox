using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UpdateClientService.API.Services.Configuration;

namespace UpdateClientService.API.Services.IoT.Certificate.Security
{
    public class EncryptionService : IEncryptionService
    {
        private readonly string _key =
            ",$=Aqy*)eChz+62ySJUTRX\\j5.hjeCO;8p*R+(90LQvCg?-K(}+at7'ns^IvL,CM+4;Dk3}Pt7@~ai(6u}Ub6Eg^tsl:KEB@&yX+,SK?:$6h[V4hwXEY#*|Oe5G9J6tmvNRDu*Gs4]lRzN4\\mzJkZ&?IOipWJZ6,DpXFh?t\"%LEb+At&V'Iwx|[w}R!.M`L6`{|q#u@o.@]16,v\\tmxe2\\\\[3o-UzFlEYV:We>5qq>eT7`]@c8$mVq;SELkHU3q\"R)x?XtFU\\B@<$qX;IE_'bSCsbf3ezF<'0<w}uQ(L0P*/x\"#:2<V![z0n'I;alt#8`<V)J];7__lNhD@?kD\\gzFI+GrmYsqT)\"`U[T(5/b$KKumUb+G+|>fe)IGQFaf^`X<`0ap-+cd_t{q8/weN6n/Jdqmu8*6EC7U{$[+3quaAiADOMz4k@d2yJ,Nv<pE=X`R^3.%WwZ|%)ge5[E@YBF1Eul9$w\"fm0Lu-7Jds{O?XDJ>'pUW[A";

        private readonly int _saltSize = 32;

        public async Task<string> Encrypt(ConfigurationEncryptionType encryptionType, string plainText)
        {
            if (encryptionType == ConfigurationEncryptionType.EncryptType1)
                return await Encrypt(plainText);
            throw new ArgumentException(string.Format("Invalid encryptionType value: {0}", encryptionType));
        }

        public async Task<string> Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentNullException(nameof(plainText));
            if (string.IsNullOrEmpty(_key))
                throw new ArgumentNullException("key");
            string base64String;
            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(_key, _saltSize))
            {
                var salt = rfc2898DeriveBytes.Salt;
                var bytes1 = rfc2898DeriveBytes.GetBytes(32);
                var bytes2 = rfc2898DeriveBytes.GetBytes(16);
                using (var aesManaged = new AesManaged())
                {
                    using (var encryptor = aesManaged.CreateEncryptor(bytes1, bytes2))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                using (var streamWriter = new StreamWriter(cryptoStream))
                                {
                                    streamWriter.Write(plainText);
                                }
                            }

                            var array = memoryStream.ToArray();
                            Array.Resize(ref salt, salt.Length + array.Length);
                            Array.Copy(array, 0, salt, _saltSize, array.Length);
                            base64String = Convert.ToBase64String(salt);
                        }
                    }
                }
            }

            return base64String;
        }

        public async Task<string> Decrypt(ConfigurationEncryptionType encryptionType, string cipherText)
        {
            if (encryptionType == ConfigurationEncryptionType.EncryptType1)
                return await Decrypt(cipherText);
            throw new ArgumentException(string.Format("Invalid encryptionType value: {0}", encryptionType));
        }

        public async Task<string> Decrypt(string ciphertext)
        {
            if (string.IsNullOrEmpty(ciphertext))
                throw new ArgumentNullException("cipherText");
            if (string.IsNullOrEmpty(_key))
                throw new ArgumentNullException("key");
            var source = Convert.FromBase64String(ciphertext);
            var array1 = source.Take(_saltSize).ToArray();
            var array2 = source.Skip(_saltSize).Take(source.Length - _saltSize).ToArray();
            string endAsync;
            using (var keyDerivationFunction = new Rfc2898DeriveBytes(_key, array1))
            {
                var bytes1 = keyDerivationFunction.GetBytes(32);
                var bytes2 = keyDerivationFunction.GetBytes(16);
                using (var aesManaged = new AesManaged())
                {
                    using (var decryptor = aesManaged.CreateDecryptor(bytes1, bytes2))
                    {
                        using (var memoryStream = new MemoryStream(array2))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                using (var streamReader = new StreamReader(cryptoStream))
                                {
                                    endAsync = await streamReader.ReadToEndAsync();
                                }
                            }
                        }
                    }
                }
            }

            return endAsync;
        }
    }
}