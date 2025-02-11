using System;
using System.Runtime.InteropServices;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class SyscallInstruction : Instruction
    {
        [Flags]
        public enum ShutdownFlags : uint
        {
            LogOff = 0,
            ShutDown = 1,
            Reboot = 2,
            Force = 4,
            PowerOff = 8,
            ForceIfHung = 16 // 0x00000010
        }

        [Flags]
        public enum ShutdownReason : uint
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
            FlagPlanned = 2147483648 // 0x80000000
        }

        private const int SE_PRIVILEGE_ENABLED = 2;
        private const int TOKEN_QUERY = 8;
        private const int TOKEN_ADJUST_PRIVILEGES = 32;
        private const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";

        public override string Mnemonic => "SYSCALL";

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (operandTokens.Count == 1 && operandTokens[0].Type == TokenType.Symbol)
                return;
            result.AddInvalidOperandError("Expected a system call to execute");
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var str1 = Operands[0].Value;
            var map = ServiceLocator.Instance.GetService<IPersistentMapService>().GetMap();
            if ("GC".Equals(str1, StringComparison.CurrentCultureIgnoreCase))
            {
                GC.Collect();
            }
            else
            {
                if (!"REBOOT".Equals(str1, StringComparison.CurrentCultureIgnoreCase))
                    return;
                if (context.IsImmediate)
                {
                    context.PushTop("ERROR");
                }
                else if (!ControllerConfiguration.Instance.RebootKioskDuringQlm)
                {
                    LogHelper.Instance.Log("HAL is not configured to reboot the kiosk on Motioncontrol failure.",
                        LogEntryType.Error);
                    AddError("E005", "Kiosk reboot is not configured.", result.Errors);
                }
                else
                {
                    if (map.GetValue("CheckRebootTime", true))
                    {
                        var timeSpan1 = new TimeSpan(0, 0, 0);
                        var timeSpan2 = new TimeSpan(5, 0, 0);
                        var timeOfDay = DateTime.Now.TimeOfDay;
                        if (!(timeOfDay >= timeSpan1) || !(timeOfDay < timeSpan2))
                        {
                            var str2 = string.Format("Cannot reboot - the current time {0} is not within 12 am - 5 am.",
                                DateTime.Now.ToShortTimeString());
                            LogHelper.Instance.Log(str2, LogEntryType.Info);
                            context.AppLog.Write(str2);
                            AddError("E005", "Kiosk reboot now allowed because of time constraint.", result.Errors);
                            return;
                        }
                    }

                    context.DeferredStatusChangeRequested = ExecutionContextStatus.Suspended;
                    ExecutionEngine.Instance.PerformContextSwitch(false);
                    if (Shutdown(ShutdownFlags.Reboot | ShutdownFlags.ForceIfHung, ShutdownReason.FlagPlanned))
                        return;
                    context.AppLog.Write("Unable to execute a reboot.");
                    LogHelper.Instance.Log("Call to reboot kiosk failed.", LogEntryType.Error);
                    AddError("E005", "Request to reboot failed.", result.Errors);
                }
            }
        }

        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(
            IntPtr htok,
            bool disall,
            ref TokPriv1Luid newst,
            int len,
            IntPtr prev,
            IntPtr relen);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool ExitWindowsEx(
            ShutdownFlags uFlags,
            ShutdownReason dwReason);

        public static bool Shutdown(
            ShutdownFlags flags,
            ShutdownReason reason)
        {
            if (ServiceLocator.Instance.GetService<IPersistentMapService>().GetMap()
                .GetValue("SimulateRebootFailure", false))
            {
                LogHelper.Instance.Log("Simulating reboot failure to service ...", LogEntryType.Error);
                return false;
            }

            var currentProcess = GetCurrentProcess();
            var zero = IntPtr.Zero;
            ref var local = ref zero;
            if (!OpenProcessToken(currentProcess, 40, ref local))
            {
                LogHelper.Instance.Log("Unable to adjust token privileges for shutdown.", LogEntryType.Error);
                return false;
            }

            TokPriv1Luid newst;
            newst.Count = 1;
            newst.Luid = 0L;
            newst.Attr = 2;
            var flag1 = LookupPrivilegeValue(null, "SeShutdownPrivilege", ref newst.Luid);
            var flag2 = AdjustTokenPrivileges(zero, false, ref newst, 0, IntPtr.Zero, IntPtr.Zero);
            if (flag1 && flag2)
                return ExitWindowsEx(flags, reason);
            LogHelper.Instance.Log(
                string.Format("Unable to lookup or adjust privileges: lookup = {0}, adjust = {1}", flag1, flag2),
                LogEntryType.Error);
            return false;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }
    }
}