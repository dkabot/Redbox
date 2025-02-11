using System.Collections.Generic;

namespace UpdateClientService.API.Services.IoT.Commands.DownloadFiles
{
    public class DownloadFileJobComparer : IEqualityComparer<DownloadFileJob>
    {
        public bool Equals(DownloadFileJob first, DownloadFileJob second)
        {
            return first?.DownloadFileJobId == second?.DownloadFileJobId;
        }

        public int GetHashCode(DownloadFileJob key)
        {
            var num = 17L * 23L;
            var hashCode = key?.DownloadFileJobId.ToUpper().GetHashCode();
            var nullable = hashCode.HasValue ? hashCode.GetValueOrDefault() : new long?();
            return (int)((nullable.HasValue ? num + nullable.GetValueOrDefault() : new long?()) ?? 0.GetHashCode());
        }
    }
}