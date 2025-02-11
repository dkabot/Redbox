using System.Collections.Generic;
using Newtonsoft.Json;

namespace UpdateClientService.API.Services.Configuration
{
    public class KioskSettingChangesResponse
    {
        public int? PageNumber { get; set; }

        [JsonIgnore]
        public bool IsFirstPage
        {
            get
            {
                if (!PageNumber.HasValue)
                    return true;
                var pageNumber = PageNumber;
                var num = 1;
                return (pageNumber.GetValueOrDefault() == num) & pageNumber.HasValue;
            }
        }

        [JsonIgnore]
        public bool IsLastPage
        {
            get
            {
                if (!PageNumber.HasValue)
                    return true;
                if (!PageNumber.HasValue)
                    return false;
                var pageNumber = PageNumber;
                var pageCount = PageCount;
                return (pageNumber.GetValueOrDefault() == pageCount.GetValueOrDefault()) &
                       (pageNumber.HasValue == pageCount.HasValue);
            }
        }

        public int? PageCount { get; set; }

        public bool VersionIsCurrent { get; set; }

        public string ConfigurationVersionHash { get; set; }

        public IEnumerable<long> RemovedConfigurationSettingValueIds { get; set; }

        public bool? RemoveAllExistingConfigurationSettingValues { get; set; }

        public long? OriginalConfigurationVersionId { get; set; }

        public GetKioskConfigurationSettingValues NewConfigurationSettingValues { get; set; }
    }
}