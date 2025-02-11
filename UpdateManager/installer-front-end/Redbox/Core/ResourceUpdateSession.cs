using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Redbox.Core
{
    internal class ResourceUpdateSession
    {
        internal const int NumberOfUpdatesPerSession = 8;
        private readonly string m_fileName;
        private readonly bool m_deleteExisting;
        private readonly Dictionary<ResourceUpdateSession.ResourceKey, byte[]> m_resources = new Dictionary<ResourceUpdateSession.ResourceKey, byte[]>();

        public ResourceUpdateSession(string fileName)
          : this(fileName, false)
        {
        }

        public ResourceUpdateSession(string fileName, bool deleteExisting)
        {
            this.m_fileName = fileName;
            this.m_deleteExisting = deleteExisting;
        }

        public void Save()
        {
            if (this.m_resources.Count == 0)
                return;
            IntPtr hUpdate = ResourceUpdateSession.BeginUpdateResource(this.m_fileName, this.m_deleteExisting);
            if (hUpdate == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            List<ResourceUpdateSession.ResourceKey> resourceKeyList = new List<ResourceUpdateSession.ResourceKey>((IEnumerable<ResourceUpdateSession.ResourceKey>)this.m_resources.Keys);
            for (int index = 0; index < this.m_resources.Keys.Count; ++index)
            {
                ResourceUpdateSession.ResourceKey key = resourceKeyList[index];
                ResourceUpdateSession.UpdateResource(hUpdate, key.Group, key.Name, (ushort)ResourceUpdateSession.MAKELANGID(9, 1), this.m_resources[key], this.m_resources[key].Length);
                int lastWin32Error1 = Marshal.GetLastWin32Error();
                if (lastWin32Error1 != 0)
                    throw new Win32Exception(lastWin32Error1);
                if ((index + 1) % 8 == 0)
                {
                    ResourceUpdateSession.EndUpdateResource(hUpdate, false);
                    int lastWin32Error2 = Marshal.GetLastWin32Error();
                    if (lastWin32Error2 != 0)
                        throw new Win32Exception(lastWin32Error2);
                    if (index + 1 < this.m_resources.Keys.Count)
                    {
                        hUpdate = ResourceUpdateSession.BeginUpdateResource(this.m_fileName, false);
                        if (hUpdate == IntPtr.Zero)
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }
            }
            if (resourceKeyList.Count % 8 <= 0)
                return;
            ResourceUpdateSession.EndUpdateResource(hUpdate, false);
            int lastWin32Error = Marshal.GetLastWin32Error();
            if (lastWin32Error != 0)
                throw new Win32Exception(lastWin32Error);
        }

        public void AddResource(string group, string name, byte[] buffer)
        {
            this.m_resources[new ResourceUpdateSession.ResourceKey(group, name)] = buffer;
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
            return (int)(ushort)sub << 10 | (int)(ushort)primary;
        }

        public static int PRIMARYLANGID(int lcid) => (int)(ushort)lcid & 1023;

        public static int SUBLANGID(int lcid) => (int)(ushort)lcid >> 10;

        internal class ResourceKey
        {
            public string Name;
            public string Group;

            public ResourceKey(string group, string name)
            {
                this.Group = group.ToUpper();
                this.Name = name.ToUpper();
            }

            public override bool Equals(object obj)
            {
                if (obj == this)
                    return true;
                return obj is ResourceUpdateSession.ResourceKey resourceKey && resourceKey.Group == this.Group && resourceKey.Name == this.Name;
            }

            public override int GetHashCode() => (this.Group + this.Name).GetHashCode();
        }
    }
}
