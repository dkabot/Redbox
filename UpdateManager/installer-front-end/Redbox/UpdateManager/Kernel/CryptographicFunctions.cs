using Redbox.Core;
using System.IO;

namespace Redbox.UpdateManager.Kernel
{
    internal static class CryptographicFunctions
    {
        [KernelFunction(Name = "kernel.hashfile")]
        internal static string HashFile(string fileName)
        {
            using (FileStream inputStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                return inputStream.ToASCIISHA1Hash();
        }

        [KernelFunction(Name = "kernel.comparehash")]
        internal static bool CompareHash(string lhs, string rhs)
        {
            return CryptographicFunctions.HashFile(lhs) == CryptographicFunctions.HashFile(rhs);
        }
    }
}
