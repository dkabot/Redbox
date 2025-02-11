using System;
using System.Runtime.InteropServices;

namespace Redbox.Core
{
    public static class SelfRegistrationTool
    {
        public static void Register(string fileName)
        {
            using (var libraryModule = new LibraryModule(fileName))
            {
                var procAddress = libraryModule.GetProcAddress("DllRegisterServer");
                if (procAddress == IntPtr.Zero)
                    return;
                Marshal.GetDelegateForFunctionPointer(procAddress, typeof(DllRegisterDelegate))?.DynamicInvoke(null);
            }
        }

        public static void Unregister(string fileName)
        {
            using (var libraryModule = new LibraryModule(fileName))
            {
                var procAddress = libraryModule.GetProcAddress("DllUnregisterServer");
                if (procAddress == IntPtr.Zero)
                    return;
                Marshal.GetDelegateForFunctionPointer(procAddress, typeof(DllUnregisterDelegate))?.DynamicInvoke(null);
            }
        }

        public static unsafe bool IsModuleSelfRegistering(string fileName)
        {
            try
            {
                int handle;
                var fileVersionInfoSize = GetFileVersionInfoSize(fileName, out handle);
                if (fileVersionInfoSize == 0)
                    return false;
                var numArray = new byte[fileVersionInfoSize];
                short* pValue;
                uint len;
                if (!GetFileVersionInfo(fileName, handle, fileVersionInfoSize, numArray) ||
                    !VerQueryValue(numArray, "\\VarFileInfo\\Translation", out pValue, out len))
                    return false;
                var pSubBlock = "\\StringFileInfo\\" + pValue->ToString("X4") + pValue[1].ToString("X4") +
                                "\\OLESelfRegister";
                return VerQueryValue(numArray, pSubBlock, out string _, out len);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [DllImport("version.dll")]
        private static extern bool GetFileVersionInfo(
            string sFileName,
            int handle,
            int size,
            byte[] infoBuffer);

        [DllImport("version.dll")]
        private static extern int GetFileVersionInfoSize(string sFileName, out int handle);

        [DllImport("version.dll")]
        private static extern bool VerQueryValue(
            byte[] pBlock,
            string pSubBlock,
            out string pValue,
            out uint len);

        [DllImport("version.dll")]
        private static extern unsafe bool VerQueryValue(
            byte[] pBlock,
            string pSubBlock,
            out short* pValue,
            out uint len);

        [return: MarshalAs(UnmanagedType.Error)]
        private delegate int DllRegisterDelegate();

        [return: MarshalAs(UnmanagedType.Error)]
        private delegate int DllUnregisterDelegate();
    }
}