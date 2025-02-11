using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UpdateClientService.API.Services.IoT.Certificate.Security
{
    public class HashService : IHashService
    {
        public async Task<string> GetKioskPassword(string kioskId)
        {
            var shA512 = (SHA512)new SHA512Managed();
            var base64String = Convert.ToBase64String(shA512.ComputeHash(Encoding.UTF8.GetBytes(kioskId)));
            var theKioskPassword = GetSaltOfTheKioskPassword(kioskId);
            return Convert.ToBase64String(shA512.ComputeHash(Encoding.UTF8.GetBytes(base64String + theKioskPassword)));
        }

        public async Task<string> GetCertificatePassword(string kioskId)
        {
            var shA512 = (SHA512)new SHA512Managed();
            var base64String = Convert.ToBase64String(shA512.ComputeHash(Encoding.UTF8.GetBytes(kioskId)));
            var certificatePassword = GetSaltOfTheCertificatePassword(kioskId);
            return Convert.ToBase64String(
                shA512.ComputeHash(Encoding.UTF8.GetBytes(base64String + certificatePassword)));
        }

        private async Task<string> GetSaltOfTheCertificatePassword(string kioskId)
        {
            var num = 0;
            var ch1 = kioskId[kioskId.Length - 1];
            foreach (var ch2 in kioskId + kioskId)
                num *= ch2 * ch1;
            return num.ToString();
        }

        private async Task<string> GetSaltOfTheKioskPassword(string kioskId)
        {
            var num = 0;
            foreach (var ch in kioskId)
                num *= ch;
            return num.ToString();
        }
    }
}