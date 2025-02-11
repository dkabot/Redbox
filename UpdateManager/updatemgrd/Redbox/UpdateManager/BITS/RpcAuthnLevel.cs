using System;

namespace Redbox.UpdateManager.BITS
{
    [Flags]
    internal enum RpcAuthnLevel
    {
        Default = 0,
        None = 1,
        Connect = 2,
        Call = Connect | None, // 0x00000003
        Pkt = 4,
        PktIntegrity = Pkt | None, // 0x00000005
        PktPrivacy = Pkt | Connect, // 0x00000006
    }
}
