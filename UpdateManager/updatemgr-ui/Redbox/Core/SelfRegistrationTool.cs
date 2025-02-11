using System;
using System.Runtime.InteropServices;

namespace Redbox.Core
{
    internal static class SelfRegistrationTool
    {
        public static void Register(string fileName)
        {
            using (LibraryModule libraryModule = new LibraryModule(fileName))
            {
                IntPtr procAddress = libraryModule.GetProcAddress("DllRegisterServer");
                if (procAddress == IntPtr.Zero)
                    return;
                Marshal.GetDelegateForFunctionPointer(procAddress, typeof(SelfRegistrationTool.DllRegisterDelegate))?.DynamicInvoke((object[])null);
            }
        }

        public static void Unregister(string fileName)
        {
            using (LibraryModule libraryModule = new LibraryModule(fileName))
            {
                IntPtr procAddress = libraryModule.GetProcAddress("DllUnregisterServer");
                if (procAddress == IntPtr.Zero)
                    return;
                Marshal.GetDelegateForFunctionPointer(procAddress, typeof(SelfRegistrationTool.DllUnregisterDelegate))?.DynamicInvoke((object[])null);
            }
        }

        public static unsafe bool IsModuleSelfRegistering(string fileName)
        {
            try
            {
                int handle;
                int fileVersionInfoSize = SelfRegistrationTool.GetFileVersionInfoSize(fileName, out handle);
                if (fileVersionInfoSize == 0)
                    return false;
                byte[] numArray = new byte[fileVersionInfoSize];
                short* pValue;
                uint len;
                if (!SelfRegistrationTool.GetFileVersionInfo(fileName, handle, fileVersionInfoSize, numArray) || !SelfRegistrationTool.VerQueryValue(numArray, "\\VarFileInfo\\Translation", out pValue, out len))
                    return false;
                string pSubBlock = "\\StringFileInfo\\" + pValue->ToString("X4") + pValue[1].ToString("X4") + "\\OLESelfRegister";
                return SelfRegistrationTool.VerQueryValue(numArray, pSubBlock, out string _, out len);
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
        unsafe private static extern bool VerQueryValue(
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
