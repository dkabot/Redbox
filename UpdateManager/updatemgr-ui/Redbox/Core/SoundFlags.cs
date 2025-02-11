using System;

namespace Redbox.Core
{
    [Flags]
    internal enum SoundFlags
    {
        SND_SYNC = 0,
        SND_ASYNC = 1,
        SND_NODEFAULT = 2,
        SND_MEMORY = 4,
        SND_LOOP = 8,
        SND_NOSTOP = 16, // 0x00000010
        SND_NOWAIT = 8192, // 0x00002000
        SND_ALIAS = 65536, // 0x00010000
        SND_ALIAS_ID = 1114112, // 0x00110000
        SND_FILENAME = 131072, // 0x00020000
        SND_RESOURCE = 262148, // 0x00040004
        SND_PURGE = 64, // 0x00000040
        SND_APPLICATION = 128, // 0x00000080
    }
}
