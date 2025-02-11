using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.DataUpdate
{
    public class DataUpdateResponse : ApiBaseResponse
    {
        public Guid RequestId { get; set; }

        public UpdateDataTable TableName { get; set; }

        public List<RecordResponse> RecordResponses { get; set; } = new List<RecordResponse>();

        public int? PageNumber { get; set; }

        public int? PageCount { get; set; }

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
    }
}