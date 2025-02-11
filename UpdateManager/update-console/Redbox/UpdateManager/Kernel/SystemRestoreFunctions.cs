using System.Management;

namespace Redbox.UpdateManager.Kernel
{
    internal static class SystemRestoreFunctions
    {
        [KernelFunction(Name = "kernel.systemrestoredisable")]
        internal static void Disable(string drive)
        {
            ManagementScope scope = new ManagementScope("\\\\localhost\\root\\default");
            ManagementPath managementPath = new ManagementPath("SystemRestore");
            ObjectGetOptions objectGetOptions = new ObjectGetOptions();
            ManagementPath path = managementPath;
            ObjectGetOptions options = objectGetOptions;
            ManagementClass managementClass = new ManagementClass(scope, path, options);
            ManagementBaseObject methodParameters = managementClass.GetMethodParameters(nameof(Disable));
            methodParameters["Drive"] = (object)drive;
            managementClass.InvokeMethod(nameof(Disable), methodParameters, (InvokeMethodOptions)null);
        }

        [KernelFunction(Name = "kernel.systemrestoreenable")]
        internal static void Enable(string drive)
        {
            ManagementScope scope = new ManagementScope("\\\\localhost\\root\\default");
            ManagementPath managementPath = new ManagementPath("SystemRestore");
            ObjectGetOptions objectGetOptions = new ObjectGetOptions();
            ManagementPath path = managementPath;
            ObjectGetOptions options = objectGetOptions;
            ManagementClass managementClass = new ManagementClass(scope, path, options);
            ManagementBaseObject methodParameters = managementClass.GetMethodParameters(nameof(Enable));
            methodParameters["Drive"] = (object)drive;
            managementClass.InvokeMethod(nameof(Enable), methodParameters, (InvokeMethodOptions)null);
        }
    }
}
