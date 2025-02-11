using System.Runtime.InteropServices;

namespace Redbox.Core
{
    internal static class ConsoleTool
    {
        internal const int AttachParentProcess = -1;

        public static bool AttachConsoleToParentProcess() => ConsoleTool.AttachConsole(-1);

        public static bool FreeAttachedConsole() => ConsoleTool.FreeConsole();

        public static void SetConsoleControlHandler(ControlCallback handler)
        {
            ConsoleTool.SetConsoleCtrlHandler(handler, true);
        }

        [DllImport("Kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        [DllImport("Kernel32.dll")]
        private static extern bool FreeConsole();

        [DllImport("kernel32")]
        private static extern bool SetConsoleCtrlHandler(ControlCallback handler, bool add);
    }
}
