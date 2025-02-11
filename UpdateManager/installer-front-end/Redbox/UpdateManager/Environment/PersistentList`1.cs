using Redbox.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Redbox.UpdateManager.Environment
{
    internal class PersistentList<T> : IEnumerable<T>, IEnumerable
    {
        private readonly string m_path;

        public PersistentList(string path)
        {
            this.m_path = path;
            if (this.ConvertIfLegacyFile())
                return;
            this.EnsureDirectoryExists();
            this.PackList();
        }

        public T this[int i]
        {
            get
            {
                string path = Path.Combine(this.m_path, i.ToString());
                try
                {
                    return !File.Exists(path) ? default(T) : File.ReadAllText(path).ToObject<T>();
                }
                catch (Exception ex)
                {
                    if (File.Exists(path))
                    {
                        if (i == 0)
                            this.Clear();
                        else
                            this.DecapitateAt(i);
                    }
                    return default(T);
                }
            }
            set
            {
                this.EnsureDirectoryExists();
                File.WriteAllText(Path.Combine(this.m_path, i.ToString()), ((object)value).ToJson());
            }
        }

        public int Count
        {
            get
            {
                this.EnsureDirectoryExists();
                return Directory.GetFiles(this.m_path).Length;
            }
        }

        public int Add(T value)
        {
            int count = this.Count;
            File.WriteAllText(Path.Combine(this.m_path, count.ToString()), ((object)value).ToJson());
            return count;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return (IEnumerator<T>)new PersistentList<T>.PersistentListEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() => (IEnumerator)this.GetEnumerator();

        public void Clear()
        {
            Directory.Delete(this.m_path, true);
            this.EnsureDirectoryExists();
        }

        public void DecapitateAt(int i)
        {
            for (int index = i; index < this.Count; ++index)
            {
                string path = Path.Combine(this.m_path, index.ToString());
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        public ReadOnlyCollection<T> AsReadOnly()
        {
            this.EnsureDirectoryExists();
            return new List<T>((IEnumerable<T>)this).AsReadOnly();
        }

        public List<T> AsList()
        {
            this.EnsureDirectoryExists();
            return new List<T>((IEnumerable<T>)this);
        }

        public void RemoveAt(int index)
        {
            File.Delete(Path.Combine(this.m_path, index.ToString()));
            this.PackList();
        }

        public void RemoveAllAt(IEnumerable<int> indexList)
        {
            foreach (int index in indexList)
                File.Delete(Path.Combine(this.m_path, index.ToString()));
            this.PackList();
        }

        private bool ConvertIfLegacyFile()
        {
            if (!File.Exists(this.m_path))
                return false;
            try
            {
                List<T> objList = File.ReadAllText(this.m_path).ToObject<List<T>>();
                File.Delete(this.m_path);
                this.EnsureDirectoryExists();
                foreach (T obj in objList)
                    this.Add(obj);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(string.Format("Conversion of legacy file '{0}' failed; removing the file.", (object)this.m_path), ex);
                File.Delete(this.m_path);
            }
            return true;
        }

        private void EnsureDirectoryExists()
        {
            if (Directory.Exists(this.m_path))
                return;
            Directory.CreateDirectory(this.m_path);
        }

        private void PackList()
        {
            try
            {
                List<string> list = ((IEnumerable<string>)Directory.GetFiles(this.m_path)).ToList<string>().Select<string, string>((Func<string, int, string>)((f, i) => Path.GetFileName(f))).ToList<string>().OrderBy<string, int>((Func<string, int>)(s => Convert.ToInt32(s))).ToList<string>();
                for (int index = 0; index < list.Count<string>(); ++index)
                {
                    if (list[index] != index.ToString())
                        File.Move(Path.Combine(this.m_path, list[index]), Path.Combine(this.m_path, index.ToString()));
                }
            }
            catch (Exception ex)
            {
                this.Clear();
            }
        }

        private sealed class PersistentListEnumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private int m_index = -1;
            private readonly PersistentList<T> m_list;

            public PersistentListEnumerator(PersistentList<T> list) => this.m_list = list;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                ++this.m_index;
                return this.m_index < this.m_list.Count;
            }

            public void Reset() => this.m_index = 0;

            public T Current => this.m_list[this.m_index];

            object IEnumerator.Current => (object)this.Current;
        }
    }
}
