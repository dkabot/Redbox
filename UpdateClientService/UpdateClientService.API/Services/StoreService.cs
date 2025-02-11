using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace UpdateClientService.API.Services
{
    public class StoreService : IStoreService
    {
        private readonly ILogger<StoreService> _logger;
        private string _banner;
        public string _dataPath;
        private long _kioskId;
        private string _market;

        public StoreService(ILogger<StoreService> logger)
        {
            _logger = logger;
        }

        public string Banner
        {
            get
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(_banner))
                    {
                        var str1 = nameof(Banner);
                        var str2 = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store";
                        var obj = Registry.GetValue(str2, str1, null);
                        if (obj == null)
                        {
                            _logger.LogInformation("Banner is not set in the registry at {0}\\{1}", str2, str1);
                            return null;
                        }

                        _banner = ((string)obj).Trim();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception occurred in StoreManagerService.GetMarket");
                }

                return _banner;
            }
        }

        public long KioskId
        {
            get
            {
                try
                {
                    var str1 = "ID";
                    var str2 = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store";
                    var str3 = Registry.GetValue(str2, str1, "")?.ToString();
                    if (string.IsNullOrEmpty(str3))
                    {
                        _logger.LogInformation("Kiosk ID is not set in the registry at {0}\\{1}", str2, str1);
                        return 0;
                    }

                    _kioskId = Convert.ToInt32(str3);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception occurred in StoreManagerService.GetKioskId");
                }

                return _kioskId;
            }
        }

        public string Market
        {
            get
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(_market))
                    {
                        var str1 = nameof(Market);
                        var str2 = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store";
                        var obj = Registry.GetValue(str2, str1, null);
                        if (obj == null)
                        {
                            _logger.LogInformation("Market is not set in the registry at {0}\\{1}", str2, str1);
                            return null;
                        }

                        _market = ((string)obj).Trim();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled exception occurred in StoreManagerService.GetMarket");
                }

                return _market;
            }
        }

        public string DataPath
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_dataPath))
                    return _dataPath;
                _dataPath = Path.GetFullPath(Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData,
                        Environment.SpecialFolderOption.Create), "Redbox\\UpdateClient"));
                return _dataPath;
            }
        }

        public string RunningPath => Path.GetDirectoryName(typeof(StoreService).Assembly.Location);
    }
}