using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Redbox.Core
{
    internal class ShellShortcut : IDisposable
    {
        private const int INFOTIPSIZE = 1024;
        private const int MAX_PATH = 260;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_SHOWMINNOACTIVE = 7;
        private ShellShortcut.IShellLinkW m_link;
        private readonly string m_path;

        public ShellShortcut(string linkPath)
        {
            this.m_path = linkPath;
            this.m_link = (ShellShortcut.IShellLinkW)new ShellShortcut.ShellLink();
            if (!File.Exists(linkPath))
                return;
            ((ShellShortcut.IPersistFile)this.m_link).Load(linkPath, 0);
        }

        public void Save() => ((ShellShortcut.IPersistFile)this.m_link).Save(this.m_path, true);

        public void Dispose()
        {
            if (this.m_link == null)
                return;
            Marshal.ReleaseComObject((object)this.m_link);
            this.m_link = (ShellShortcut.IShellLinkW)null;
        }

        public string Arguments
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder(1024);
                ShellShortcut.IShellLinkW link = this.m_link;
                StringBuilder pszArgs = stringBuilder;
                int capacity = pszArgs.Capacity;
                link.GetArguments(pszArgs, capacity);
                return stringBuilder.ToString();
            }
            set => this.m_link.SetArguments(value);
        }

        public string Description
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder(1024);
                ShellShortcut.IShellLinkW link = this.m_link;
                StringBuilder pszName = stringBuilder;
                int capacity = pszName.Capacity;
                link.GetDescription(pszName, capacity);
                return stringBuilder.ToString();
            }
            set => this.m_link.SetDescription(value);
        }

        public string WorkingDirectory
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder(260);
                ShellShortcut.IShellLinkW link = this.m_link;
                StringBuilder pszDir = stringBuilder;
                int capacity = pszDir.Capacity;
                link.GetWorkingDirectory(pszDir, capacity);
                return stringBuilder.ToString();
            }
            set => this.m_link.SetWorkingDirectory(value);
        }

        public string Path
        {
            get
            {
                ShellShortcut.WIN32_FIND_DATAW wiN32FindDataw = new ShellShortcut.WIN32_FIND_DATAW();
                StringBuilder stringBuilder = new StringBuilder(260);
                ShellShortcut.IShellLinkW link = this.m_link;
                StringBuilder pszFile = stringBuilder;
                int capacity = pszFile.Capacity;
                ref ShellShortcut.WIN32_FIND_DATAW local = ref wiN32FindDataw;
                link.GetPath(pszFile, capacity, out local, ShellShortcut.SLGP_FLAGS.SLGP_UNCPRIORITY);
                return stringBuilder.ToString();
            }
            set => this.m_link.SetPath(value);
        }

        public string IconPath
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder(260);
                ShellShortcut.IShellLinkW link = this.m_link;
                StringBuilder pszIconPath = stringBuilder;
                int capacity = pszIconPath.Capacity;
                int num;
                link.GetIconLocation(pszIconPath, capacity, out num);
                return stringBuilder.ToString();
            }
            set => this.m_link.SetIconLocation(value, this.IconIndex);
        }

        public int IconIndex
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder(260);
                ShellShortcut.IShellLinkW link = this.m_link;
                StringBuilder pszIconPath = stringBuilder;
                int capacity = pszIconPath.Capacity;
                int iconIndex;
                link.GetIconLocation(pszIconPath, capacity, out iconIndex);
                return iconIndex;
            }
            set => this.m_link.SetIconLocation(this.IconPath, value);
        }

        public Icon Icon
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder(260);
                ShellShortcut.IShellLinkW link = this.m_link;
                StringBuilder pszIconPath = stringBuilder;
                int capacity = pszIconPath.Capacity;
                int nIconIndex;
                link.GetIconLocation(pszIconPath, capacity, out nIconIndex);
                IntPtr icon1 = ShellShortcut.ExtractIcon(Marshal.GetHINSTANCE(this.GetType().Module), stringBuilder.ToString(), nIconIndex);
                if (icon1 == IntPtr.Zero)
                    return null;
                Icon icon2 = Icon.FromHandle(icon1);
                Icon icon3 = (Icon)icon2.Clone();
                icon2.Dispose();
                ShellShortcut.DestroyIcon(icon1);
                return icon3;
            }
        }

        public ProcessWindowStyle WindowStyle
        {
            get
            {
                int piShowCmd;
                this.m_link.GetShowCmd(out piShowCmd);
                switch (piShowCmd)
                {
                    case 2:
                    case 7:
                        return ProcessWindowStyle.Minimized;
                    case 3:
                        return ProcessWindowStyle.Maximized;
                    default:
                        return ProcessWindowStyle.Normal;
                }
            }
            set
            {
                int iShowCmd;
                switch (value)
                {
                    case ProcessWindowStyle.Normal:
                        iShowCmd = 1;
                        break;
                    case ProcessWindowStyle.Minimized:
                        iShowCmd = 7;
                        break;
                    case ProcessWindowStyle.Maximized:
                        iShowCmd = 3;
                        break;
                    default:
                        throw new ArgumentException("Unsupported ProcessWindowStyle value.");
                }
                this.m_link.SetShowCmd(iShowCmd);
            }
        }

        public Keys Hotkey
        {
            get
            {
                short pwHotkey;
                this.m_link.GetHotkey(out pwHotkey);
                return (Keys)(((int)pwHotkey & 65280) << 8 | (int)pwHotkey & (int)byte.MaxValue);
            }
            set
            {
                if ((value & Keys.Modifiers) == Keys.None)
                    throw new ArgumentException("Hotkey must include a modifier key.");
                this.m_link.SetHotkey((short)((Keys)((int)(value & Keys.Modifiers) >> 8) | value & Keys.KeyCode));
            }
        }

        public object Instance => (object)this.m_link;

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [Flags]
        private enum SLR_FLAGS
        {
            SLR_NO_UI = 1,
            SLR_ANY_MATCH = 2,
            SLR_UPDATE = 4,
            SLR_NOUPDATE = 8,
            SLR_NOSEARCH = 16, // 0x00000010
            SLR_NOTRACK = 32, // 0x00000020
            SLR_NOLINKINFO = 64, // 0x00000040
            SLR_INVOKE_MSI = 128, // 0x00000080
        }

        [Flags]
        private enum SLGP_FLAGS
        {
            SLGP_SHORTPATH = 1,
            SLGP_UNCPRIORITY = 2,
            SLGP_RAWPATH = 4,
        }

        private struct WIN32_FIND_DATAA
        {
            public int dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public int nFileSizeHigh;
            public int nFileSizeLow;
            public int dwReserved0;
            public int dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
            private const int MAX_PATH = 260;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WIN32_FIND_DATAW
        {
            public int dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public int nFileSizeHigh;
            public int nFileSizeLow;
            public int dwReserved0;
            public int dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
            private const int MAX_PATH = 260;
        }

        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("0000010B-0000-0000-C000-000000000046")]
        [ComImport]
        private interface IPersistFile
        {
            void GetClassID(out Guid pClassID);

            [MethodImpl(MethodImplOptions.PreserveSig)]
            int IsDirty();

            void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, int dwMode);

            void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);

            void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);

            void GetCurFile(out IntPtr ppszFileName);
        }

        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214EE-0000-0000-C000-000000000046")]
        [ComImport]
        private interface IShellLinkA
        {
            void GetPath(
              [MarshalAs(UnmanagedType.LPStr), Out] StringBuilder pszFile,
              int cchMaxPath,
              out ShellShortcut.WIN32_FIND_DATAA pfd,
              ShellShortcut.SLGP_FLAGS fFlags);

            void GetIDList(out IntPtr ppidl);

            void SetIDList(IntPtr pidl);

            void GetDescription([MarshalAs(UnmanagedType.LPStr), Out] StringBuilder pszName, int cchMaxName);

            void SetDescription([MarshalAs(UnmanagedType.LPStr)] string pszName);

            void GetWorkingDirectory([MarshalAs(UnmanagedType.LPStr), Out] StringBuilder pszDir, int cchMaxPath);

            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPStr)] string pszDir);

            void GetArguments([MarshalAs(UnmanagedType.LPStr), Out] StringBuilder pszArgs, int cchMaxPath);

            void SetArguments([MarshalAs(UnmanagedType.LPStr)] string pszArgs);

            void GetHotkey(out short pwHotkey);

            void SetHotkey(short wHotkey);

            void GetShowCmd(out int piShowCmd);

            void SetShowCmd(int iShowCmd);

            void GetIconLocation([MarshalAs(UnmanagedType.LPStr), Out] StringBuilder pszIconPath, int cchIconPath, out int piIcon);

            void SetIconLocation([MarshalAs(UnmanagedType.LPStr)] string pszIconPath, int iIcon);

            void SetRelativePath([MarshalAs(UnmanagedType.LPStr)] string pszPathRel, int dwReserved);

            void Resolve(IntPtr hwnd, ShellShortcut.SLR_FLAGS fFlags);

            void SetPath([MarshalAs(UnmanagedType.LPStr)] string pszFile);
        }

        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        [ComImport]
        private interface IShellLinkW
        {
            void GetPath(
              [MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder pszFile,
              int cchMaxPath,
              out ShellShortcut.WIN32_FIND_DATAW pfd,
              ShellShortcut.SLGP_FLAGS fFlags);

            void GetIDList(out IntPtr ppidl);

            void SetIDList(IntPtr pidl);

            void GetDescription([MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder pszName, int cchMaxName);

            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);

            void GetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder pszDir, int cchMaxPath);

            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);

            void GetArguments([MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder pszArgs, int cchMaxPath);

            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

            void GetHotkey(out short pwHotkey);

            void SetHotkey(short wHotkey);

            void GetShowCmd(out int piShowCmd);

            void SetShowCmd(int iShowCmd);

            void GetIconLocation([MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder pszIconPath, int cchIconPath, out int piIcon);

            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);

            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);

            void Resolve(IntPtr hwnd, ShellShortcut.SLR_FLAGS fFlags);

            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        [Guid("00021401-0000-0000-C000-000000000046")]
        [ComImport]
        private class ShellLink
        {
        }
    }
}
