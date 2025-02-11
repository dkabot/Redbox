using System;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

public sealed class Computerinfo
{
    public Computerinfo()
    {
        foreach (ManagementObject instance in new ManagementClass("Win32_ComputerSystem").GetInstances())
        {
            Manufacturer = instance[nameof(Manufacturer)].ToString();
            Model = instance[nameof(Model)].ToString();
        }

        long totalMemoryInKilobytes;
        if (GetPhysicallyInstalledSystemMemory(out totalMemoryInKilobytes))
            InstalledMemory = totalMemoryInKilobytes;
        else
            LogHelper.Instance.Log("[{0}] Unable to obtain memory: GLE = {1}", GetType().Name,
                Marshal.GetLastWin32Error());
        foreach (var drive in DriveInfo.GetDrives())
            if (drive.Name.StartsWith("c", StringComparison.CurrentCultureIgnoreCase) && drive.IsReady)
                DiskFreeSpace = drive.TotalFreeSpace;
    }

    public long DiskFreeSpace { get; private set; }

    public long InstalledMemory { get; private set; }

    public string Manufacturer { get; private set; }

    public string Model { get; private set; }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetPhysicallyInstalledSystemMemory(out long totalMemoryInKilobytes);
}