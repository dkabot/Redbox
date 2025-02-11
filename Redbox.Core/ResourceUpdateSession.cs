using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Redbox.Core
{
    public class ResourceUpdateSession
    {
        internal const int NumberOfUpdatesPerSession = 8;
        private readonly bool m_deleteExisting;
        private readonly string m_fileName;
        private readonly Dictionary<ResourceKey, byte[]> m_resources = new Dictionary<ResourceKey, byte[]>();

        public ResourceUpdateSession(string fileName)
            : this(fileName, false)
        {
        }

        public ResourceUpdateSession(string fileName, bool deleteExisting)
        {
            m_fileName = fileName;
            m_deleteExisting = deleteExisting;
        }

        public void Save()
        {
            if (m_resources.Count == 0)
                return;
            var hUpdate = BeginUpdateResource(m_fileName, m_deleteExisting);
            if (hUpdate == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            var resourceKeyList = new List<ResourceKey>(m_resources.Keys);
            for (var index = 0; index < m_resources.Keys.Count; ++index)
            {
                var key = resourceKeyList[index];
                UpdateResource(hUpdate, key.Group, key.Name, (ushort)MAKELANGID(9, 1), m_resources[key],
                    m_resources[key].Length);
                var lastWin32Error1 = Marshal.GetLastWin32Error();
                if (lastWin32Error1 != 0)
                    throw new Win32Exception(lastWin32Error1);
                if ((index + 1) % 8 == 0)
                {
                    EndUpdateResource(hUpdate, false);
                    var lastWin32Error2 = Marshal.GetLastWin32Error();
                    if (lastWin32Error2 != 0)
                        throw new Win32Exception(lastWin32Error2);
                    if (index + 1 < m_resources.Keys.Count)
                    {
                        hUpdate = BeginUpdateResource(m_fileName, false);
                        if (hUpdate == IntPtr.Zero)
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }
            }

            if (resourceKeyList.Count % 8 <= 0)
                return;
            EndUpdateResource(hUpdate, false);
            var lastWin32Error = Marshal.GetLastWin32Error();
            if (lastWin32Error != 0)
                throw new Win32Exception(lastWin32Error);
        }

        public void AddResource(string group, string name, byte[] buffer)
        {
            m_resources[new ResourceKey(group, name)] = buffer;
        }

        [DllImport("kernel32")]
        private static extern IntPtr BeginUpdateResource(
            string pFileName,
            bool bDeleteExistingResources);

        [DllImport("kernel32", SetLastError = true)]
        private static extern int UpdateResource(
            IntPtr hUpdate,
            string lpType,
            string lpName,
            ushort wLanguage,
            byte[] lpData,
            int cbData);

        [DllImport("kernel32", SetLastError = true)]
        private static extern int EndUpdateResource(IntPtr hUpdate, bool fDiscard);

        public static int MAKELANGID(int primary, int sub)
        {
            return ((ushort)sub << 10) | (ushort)primary;
        }

        public static int PRIMARYLANGID(int lcid)
        {
            return (ushort)lcid & 1023;
        }

        public static int SUBLANGID(int lcid)
        {
            return (ushort)lcid >> 10;
        }

        internal class ResourceKey
        {
            public string Group;
            public string Name;

            public ResourceKey(string group, string name)
            {
                Group = group.ToUpper();
                Name = name.ToUpper();
            }

            public override bool Equals(object obj)
            {
                if (obj == this)
                    return true;
                return obj is ResourceKey resourceKey && resourceKey.Group == Group && resourceKey.Name == Name;
            }

            public override int GetHashCode()
            {
                return (Group + Name).GetHashCode();
            }
        }
    }
}