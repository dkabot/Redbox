using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace Redbox.Core
{
    public class ShellShortcut : IDisposable
    {
        private const int INFOTIPSIZE = 1024;
        private const int MAX_PATH = 260;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_SHOWMINNOACTIVE = 7;
        private readonly string m_path;
        private IShellLinkW m_link;

        public ShellShortcut(string linkPath)
        {
            m_path = linkPath;
            m_link = (IShellLinkW)new ShellLink();
            if (!File.Exists(linkPath))
                return;
            ((IPersistFile)m_link).Load(linkPath, 0);
        }

        public string Arguments
        {
            get
            {
                var pszArgs = new StringBuilder(1024);
                m_link.GetArguments(pszArgs, pszArgs.Capacity);
                return pszArgs.ToString();
            }
            set => m_link.SetArguments(value);
        }

        public string Description
        {
            get
            {
                var pszName = new StringBuilder(1024);
                m_link.GetDescription(pszName, pszName.Capacity);
                return pszName.ToString();
            }
            set => m_link.SetDescription(value);
        }

        public string WorkingDirectory
        {
            get
            {
                var pszDir = new StringBuilder(260);
                m_link.GetWorkingDirectory(pszDir, pszDir.Capacity);
                return pszDir.ToString();
            }
            set => m_link.SetWorkingDirectory(value);
        }

        public string Path
        {
            get
            {
                var pfd = new WIN32_FIND_DATAW();
                var pszFile = new StringBuilder(260);
                m_link.GetPath(pszFile, pszFile.Capacity, out pfd, SLGP_FLAGS.SLGP_UNCPRIORITY);
                return pszFile.ToString();
            }
            set => m_link.SetPath(value);
        }

        public string IconPath
        {
            get
            {
                var pszIconPath = new StringBuilder(260);
                m_link.GetIconLocation(pszIconPath, pszIconPath.Capacity, out _);
                return pszIconPath.ToString();
            }
            set => m_link.SetIconLocation(value, IconIndex);
        }

        public int IconIndex
        {
            get
            {
                var pszIconPath = new StringBuilder(260);
                int piIcon;
                m_link.GetIconLocation(pszIconPath, pszIconPath.Capacity, out piIcon);
                return piIcon;
            }
            set => m_link.SetIconLocation(IconPath, value);
        }

        public Icon Icon
        {
            get
            {
                var pszIconPath = new StringBuilder(260);
                int piIcon;
                m_link.GetIconLocation(pszIconPath, pszIconPath.Capacity, out piIcon);
                var icon1 = ExtractIcon(Marshal.GetHINSTANCE(GetType().Module), pszIconPath.ToString(), piIcon);
                if (icon1 == IntPtr.Zero)
                    return null;
                var icon2 = Icon.FromHandle(icon1);
                var icon3 = (Icon)icon2.Clone();
                icon2.Dispose();
                DestroyIcon(icon1);
                return icon3;
            }
        }

        public ProcessWindowStyle WindowStyle
        {
            get
            {
                int piShowCmd;
                m_link.GetShowCmd(out piShowCmd);
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

                m_link.SetShowCmd(iShowCmd);
            }
        }

        public Keys Hotkey
        {
            get
            {
                short pwHotkey;
                m_link.GetHotkey(out pwHotkey);
                return (Keys)(((pwHotkey & 65280) << 8) | (pwHotkey & byte.MaxValue));
            }
            set
            {
                if ((value & Keys.Modifiers) == Keys.None)
                    throw new ArgumentException("Hotkey must include a modifier key.");
                m_link.SetHotkey((short)((Keys)((int)(value & Keys.Modifiers) >> 8) | (value & Keys.KeyCode)));
            }
        }

        public object Instance => m_link;

        public void Dispose()
        {
            if (m_link == null)
                return;
            Marshal.ReleaseComObject(m_link);
            m_link = null;
        }

        public void Save()
        {
            ((IPersistFile)m_link).Save(m_path, true);
        }

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
            SLR_INVOKE_MSI = 128 // 0x00000080
        }

        [Flags]
        private enum SLGP_FLAGS
        {
            SLGP_SHORTPATH = 1,
            SLGP_UNCPRIORITY = 2,
            SLGP_RAWPATH = 4
        }

        private struct WIN32_FIND_DATAA
        {
            public int dwFileAttributes;
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
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
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
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

            void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
                [MarshalAs(UnmanagedType.Bool)] bool fRemember);

            void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);

            void GetCurFile(out IntPtr ppszFileName);
        }

        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214EE-0000-0000-C000-000000000046")]
        [ComImport]
        private interface IShellLinkA
        {
            void GetPath(
                [MarshalAs(UnmanagedType.LPStr)] [Out] StringBuilder pszFile,
                int cchMaxPath,
                out WIN32_FIND_DATAA pfd,
                SLGP_FLAGS fFlags);

            void GetIDList(out IntPtr ppidl);

            void SetIDList(IntPtr pidl);

            void GetDescription([MarshalAs(UnmanagedType.LPStr)] [Out] StringBuilder pszName, int cchMaxName);

            void SetDescription([MarshalAs(UnmanagedType.LPStr)] string pszName);

            void GetWorkingDirectory([MarshalAs(UnmanagedType.LPStr)] [Out] StringBuilder pszDir, int cchMaxPath);

            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPStr)] string pszDir);

            void GetArguments([MarshalAs(UnmanagedType.LPStr)] [Out] StringBuilder pszArgs, int cchMaxPath);

            void SetArguments([MarshalAs(UnmanagedType.LPStr)] string pszArgs);

            void GetHotkey(out short pwHotkey);

            void SetHotkey(short wHotkey);

            void GetShowCmd(out int piShowCmd);

            void SetShowCmd(int iShowCmd);

            void GetIconLocation([MarshalAs(UnmanagedType.LPStr)] [Out] StringBuilder pszIconPath, int cchIconPath,
                out int piIcon);

            void SetIconLocation([MarshalAs(UnmanagedType.LPStr)] string pszIconPath, int iIcon);

            void SetRelativePath([MarshalAs(UnmanagedType.LPStr)] string pszPathRel, int dwReserved);

            void Resolve(IntPtr hwnd, SLR_FLAGS fFlags);

            void SetPath([MarshalAs(UnmanagedType.LPStr)] string pszFile);
        }

        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        [ComImport]
        private interface IShellLinkW
        {
            void GetPath(
                [MarshalAs(UnmanagedType.LPWStr)] [Out]
                StringBuilder pszFile,
                int cchMaxPath,
                out WIN32_FIND_DATAW pfd,
                SLGP_FLAGS fFlags);

            void GetIDList(out IntPtr ppidl);

            void SetIDList(IntPtr pidl);

            void GetDescription([MarshalAs(UnmanagedType.LPWStr)] [Out] StringBuilder pszName, int cchMaxName);

            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);

            void GetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] [Out] StringBuilder pszDir, int cchMaxPath);

            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);

            void GetArguments([MarshalAs(UnmanagedType.LPWStr)] [Out] StringBuilder pszArgs, int cchMaxPath);

            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

            void GetHotkey(out short pwHotkey);

            void SetHotkey(short wHotkey);

            void GetShowCmd(out int piShowCmd);

            void SetShowCmd(int iShowCmd);

            void GetIconLocation([MarshalAs(UnmanagedType.LPWStr)] [Out] StringBuilder pszIconPath, int cchIconPath,
                out int piIcon);

            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);

            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);

            void Resolve(IntPtr hwnd, SLR_FLAGS fFlags);

            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        [Guid("00021401-0000-0000-C000-000000000046")]
        [ComImport]
        private class ShellLink
        {
        }
    }
}