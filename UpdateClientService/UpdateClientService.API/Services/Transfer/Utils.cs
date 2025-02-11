using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace UpdateClientService.API.Services.Transfer
{
    internal static class Utils
    {
        private static readonly BitsVersion version = GetBitsVersion();

        internal static BitsVersion BITSVersion => version;

        internal static string GetName(string SID)
        {
            var maxValue1 = (long)byte.MaxValue;
            var maxValue2 = (long)byte.MaxValue;
            var SID1 = new IntPtr(0);
            var psUse = 0;
            var name = new StringBuilder(byte.MaxValue);
            var domainName = new StringBuilder(byte.MaxValue);
            return NativeMethods.ConvertStringSidToSidW(SID, ref SID1) && NativeMethods.LookupAccountSidW(string.Empty,
                SID1, name, ref maxValue1, domainName, ref maxValue2, ref psUse)
                ? domainName + "\\" + name
                : string.Empty;
        }

        internal static FILETIME DateTime2FileTime(DateTime dateTime)
        {
            long num = 0;
            if (dateTime != DateTime.MinValue)
                num = dateTime.ToFileTime();
            return new FILETIME
            {
                dwLowDateTime = (uint)((ulong)num & uint.MaxValue),
                dwHighDateTime = (uint)(num >> 32)
            };
        }

        internal static DateTime FileTime2DateTime(FILETIME fileTime)
        {
            return fileTime.dwHighDateTime == 0U && fileTime.dwLowDateTime == 0U
                ? DateTime.MinValue
                : DateTime.FromFileTime(((long)fileTime.dwHighDateTime << 32) + fileTime.dwLowDateTime);
        }

        private static BitsVersion GetBitsVersion()
        {
            try
            {
                var sFileName = Path.Combine(Environment.SystemDirectory, "qmgr.dll");
                int handle;
                var fileVersionInfoSize = NativeMethods.GetFileVersionInfoSize(sFileName, out handle);
                if (fileVersionInfoSize == 0)
                    return BitsVersion.Bits0_0;
                var numArray = new byte[fileVersionInfoSize];
                if (!NativeMethods.GetFileVersionInfo(sFileName, handle, fileVersionInfoSize, numArray))
                    return BitsVersion.Bits0_0;
                var pValue1 = IntPtr.Zero;
                uint len;
                if (!NativeMethods.VerQueryValue(numArray, "\\VarFileInfo\\Translation", out pValue1, out len))
                    return BitsVersion.Bits0_0;
                var pSubBlock = string.Format("\\StringFileInfo\\{0:X4}{1:X4}\\ProductVersion",
                    (int)Marshal.ReadInt16(pValue1), (int)Marshal.ReadInt16((IntPtr)((int)pValue1 + 2)));
                string pValue2;
                if (!NativeMethods.VerQueryValue(numArray, pSubBlock, out pValue2, out len))
                    return BitsVersion.Bits0_0;
                var strArray = pValue2.Split('.');
                if (strArray == null || strArray.Length < 2)
                    return BitsVersion.Bits0_0;
                var num1 = int.Parse(strArray[0]);
                var num2 = int.Parse(strArray[1]);
                switch (num1)
                {
                    case 6:
                        switch (num2)
                        {
                            case 0:
                                return BitsVersion.Bits1_0;
                            case 2:
                                return BitsVersion.Bits1_2;
                            case 5:
                                return BitsVersion.Bits1_5;
                            case 6:
                                return BitsVersion.Bits2_0;
                            case 7:
                                return BitsVersion.Bits2_5;
                            default:
                                return BitsVersion.Bits0_0;
                        }
                    case 7:
                        return BitsVersion.Bits3_0;
                    default:
                        return BitsVersion.Bits0_0;
                }
            }
            catch
            {
                return BitsVersion.Bits0_0;
            }
        }
    }
}