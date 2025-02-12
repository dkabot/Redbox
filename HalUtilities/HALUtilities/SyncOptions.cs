using System;

namespace HALUtilities
{
    [Flags]
    internal enum SyncOptions
    {
        None = 0,
        Debug = 1,
        Analyze = 2,
        Attach = 4,
        OutputSuccessfulSyncCompare = 8,
        AsyncSoftSync = 16, // 0x00000010
        All = AsyncSoftSync | OutputSuccessfulSyncCompare | Attach | Analyze | Debug // 0x0000001F
    }
}