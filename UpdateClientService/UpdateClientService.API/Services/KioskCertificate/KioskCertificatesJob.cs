using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.KioskCertificate
{
    internal class KioskCertificatesJob : IKioskCertificatesJob, IInvocable
    {
        private readonly string _certDataPath;
        private readonly ILogger<KioskCertificatesJob> _logger;

        public KioskCertificatesJob(ILogger<KioskCertificatesJob> logger)
        {
            _logger = logger;
            _certDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Redbox\\UpdateClient\\Certificates\\");
        }

        public async Task Invoke()
        {
            try
            {
                await LookForCerts();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithSource(ex, "There was an exception in Invoke()",
                    "/sln/src/UpdateClientService.API/Services/KioskCertificate/KioskCertificatesJob.cs");
            }
        }

        private async Task LookForCerts()
        {
            if (Directory.Exists(_certDataPath))
            {
                var files = Directory.GetFiles(_certDataPath, "*.staged");
                if (files.Length == 0)
                    return;
                _logger.LogInfoWithSource("Found following certs to process: " + string.Join(";", files),
                    "/sln/src/UpdateClientService.API/Services/KioskCertificate/KioskCertificatesJob.cs");
                var strArray = files;
                for (var index = 0; index < strArray.Length; ++index)
                {
                    var file = strArray[index];
                    try
                    {
                        var data = File.ReadAllBytes(file);
                        if (!CertificateHelper.Exists(StoreName.My, StoreLocation.LocalMachine, data))
                        {
                            CertificateHelper.Add(StoreName.My, StoreLocation.LocalMachine, data);
                            _logger.LogInfoWithSource("added cert: " + file + " to the capi store",
                                "/sln/src/UpdateClientService.API/Services/KioskCertificate/KioskCertificatesJob.cs");
                        }
                        else
                        {
                            _logger.LogInfoWithSource("cert: " + file + " exists in capi store",
                                "/sln/src/UpdateClientService.API/Services/KioskCertificate/KioskCertificatesJob.cs");
                        }

                        var str = file.Replace(".staged", "");
                        if (File.Exists(str))
                            File.Delete(str);
                        File.Move(file, str);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogErrorWithSource(ex, "exception occured processing certificate: " + file,
                            "/sln/src/UpdateClientService.API/Services/KioskCertificate/KioskCertificatesJob.cs");
                    }

                    file = null;
                }

                strArray = null;
            }
            else
            {
                _logger.LogInfoWithSource("creating certificate path: " + _certDataPath,
                    "/sln/src/UpdateClientService.API/Services/KioskCertificate/KioskCertificatesJob.cs");
                Directory.CreateDirectory(_certDataPath);
            }
        }
    }
}