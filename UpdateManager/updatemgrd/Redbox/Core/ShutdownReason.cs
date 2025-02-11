using System;

namespace Redbox.Core
{
    [Flags]
    internal enum ShutdownReason : uint
    {
        MajorApplication = 262144, // 0x00040000
        MajorHardware = 65536, // 0x00010000
        MajorLegacyApi = 458752, // 0x00070000
        MajorOperatingSystem = 131072, // 0x00020000
        MajorOther = 0,
        MajorPower = MajorOperatingSystem | MajorApplication, // 0x00060000
        MajorSoftware = MajorOperatingSystem | MajorHardware, // 0x00030000
        MajorSystem = MajorHardware | MajorApplication, // 0x00050000
        MinorBlueScreen = 15, // 0x0000000F
        MinorCordUnplugged = 11, // 0x0000000B
        MinorDisk = 7,
        MinorEnvironment = 12, // 0x0000000C
        MinorHardwareDriver = 13, // 0x0000000D
        MinorHotfix = 17, // 0x00000011
        MinorHung = 5,
        MinorInstallation = 2,
        MinorMaintenance = 1,
        MinorMMC = 25, // 0x00000019
        MinorNetworkConnectivity = 20, // 0x00000014
        MinorNetworkCard = 9,
        MinorOther = 0,
        MinorOtherDriver = MinorInstallation | MinorEnvironment, // 0x0000000E
        MinorPowerSupply = 10, // 0x0000000A
        MinorProcessor = 8,
        MinorReconfig = 4,
        MinorSecurity = 19, // 0x00000013
        MinorSecurityFix = 18, // 0x00000012
        MinorSecurityFixUninstall = 24, // 0x00000018
        MinorServicePack = 16, // 0x00000010
        MinorServicePackUninstall = MinorServicePack | MinorReconfig | MinorInstallation, // 0x00000016
        MinorTermSrv = 32, // 0x00000020
        MinorUnstable = MinorReconfig | MinorInstallation, // 0x00000006
        MinorUpgrade = MinorMaintenance | MinorInstallation, // 0x00000003
        MinorWMI = MinorServicePack | MinorReconfig | MinorMaintenance, // 0x00000015
        FlagUserDefined = 1073741824, // 0x40000000
        FlagPlanned = 2147483648, // 0x80000000
    }
}
