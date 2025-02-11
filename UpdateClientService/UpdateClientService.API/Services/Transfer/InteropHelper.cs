using System;

namespace UpdateClientService.API.Services.Transfer
{
    internal static class InteropHelper
    {
        internal static DateTime? ToUTCDateTime(this FILETIME filetime)
        {
            var dwLowDateTime = (long)filetime.dwLowDateTime;
            var dwHighDateTime = (long)filetime.dwHighDateTime;
            return dwLowDateTime == 0L && dwHighDateTime == 0L
                ? new DateTime?()
                : DateTime.FromFileTimeUtc((dwHighDateTime << 32) + dwLowDateTime);
        }
    }
}