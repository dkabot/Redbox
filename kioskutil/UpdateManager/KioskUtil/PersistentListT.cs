using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Redbox.Core;

namespace Redbox.UpdateManager.KioskUtil
{
    public class PersistentList<T> : IEnumerable<T>, IEnumerable
    {
        private readonly string m_path;

        public PersistentList(string path)
        {
            m_path = path;
            if (ConvertIfLegacyFile())
                return;
            EnsureDirectoryExists();
            PackList();
        }

        public T this[int i]
        {
            get
            {
                var path = Path.Combine(m_path, i.ToString());
                try
                {
                    return !File.Exists(path) ? default : File.ReadAllText(path).ToObject<T>();
                }
                catch (Exception ex)
                {
                    if (File.Exists(path))
                    {
                        if (i == 0)
                            Clear();
                        else
                            DecapitateAt(i);
                    }

                    return default;
                }
            }
            set
            {
                EnsureDirectoryExists();
                File.WriteAllText(Path.Combine(m_path, i.ToString()), value.ToJson());
            }
        }

        public int Count
        {
            get
            {
                EnsureDirectoryExists();
                return Directory.GetFiles(m_path).Length;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new PersistentListEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Add(T value)
        {
            var count = Count;
            File.WriteAllText(Path.Combine(m_path, count.ToString()), value.ToJson());
            return count;
        }

        public void Clear()
        {
            Directory.Delete(m_path);
            EnsureDirectoryExists();
        }

        public void DecapitateAt(int i)
        {
            for (var index = i; index < Count; ++index)
            {
                var path = Path.Combine(m_path, index.ToString());
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        public ReadOnlyCollection<T> AsReadOnly()
        {
            EnsureDirectoryExists();
            return new List<T>(this).AsReadOnly();
        }

        public List<T> AsList()
        {
            EnsureDirectoryExists();
            return new List<T>(this);
        }

        public void RemoveAt(int index)
        {
            File.Delete(Path.Combine(m_path, index.ToString()));
            PackList();
        }

        public void RemoveAllAt(IEnumerable<int> indexList)
        {
            foreach (var index in indexList)
                File.Delete(Path.Combine(m_path, index.ToString()));
            PackList();
        }

        private bool ConvertIfLegacyFile()
        {
            if (!File.Exists(m_path))
                return false;
            try
            {
                var objList = File.ReadAllText(m_path).ToObject<List<T>>();
                File.Delete(m_path);
                EnsureDirectoryExists();
                foreach (var obj in objList)
                    Add(obj);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(
                    string.Format("Conversion of legacy file '{0}' failed; removing the file.", m_path), ex);
                File.Delete(m_path);
            }

            return true;
        }

        private void EnsureDirectoryExists()
        {
            if (Directory.Exists(m_path))
                return;
            Directory.CreateDirectory(m_path);
        }

        private void PackList()
        {
            try
            {
                var list = Directory.GetFiles(m_path).ToList().Select((f, i) => Path.GetFileName(f)).ToList()
                    .OrderBy(s => Convert.ToInt32(s)).ToList();
                for (var index = 0; index < list.Count(); ++index)
                    if (list[index] != index.ToString())
                        File.Move(Path.Combine(m_path, list[index]), Path.Combine(m_path, index.ToString()));
            }
            catch
            {
                Clear();
            }
        }

        private sealed class PersistentListEnumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private readonly PersistentList<T> m_list;
            private int m_index = -1;

            public PersistentListEnumerator(PersistentList<T> list)
            {
                m_list = list;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                ++m_index;
                return m_index < m_list.Count;
            }

            public void Reset()
            {
                m_index = 0;
            }

            public T Current => m_list[m_index];

            object IEnumerator.Current => Current;
        }
    }
}