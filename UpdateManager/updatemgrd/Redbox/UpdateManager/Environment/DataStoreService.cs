using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.IO;
using System.Reflection;

namespace Redbox.UpdateManager.Environment
{
    internal class DataStoreService : IDataStoreService
    {
        private readonly object m_writeLock = new object();

        public static DataStoreService Instance => Singleton<DataStoreService>.Instance;

        public void Initialize(string rootPath)
        {
            string str = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(rootPath);
            if (!Path.IsPathRooted(str))
                str = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), str);
            this.Root = str;
            if (!Directory.Exists(this.Root))
                Directory.CreateDirectory(this.Root);
            if (ServiceLocator.Instance.GetService<IDataStoreService>() != null)
                return;
            ServiceLocator.Instance.AddService(typeof(IDataStoreService), (object)this);
        }

        public void Reset()
        {
            if (Directory.Exists(this.Root))
                Directory.Delete(this.Root, true);
            this.Initialize(this.Root);
        }

        public void Set(string id, object o) => this.SetRaw(id, o.ToJson());

        public void SetRaw(Guid id, string o) => this.SetRaw(id.ToString(), o);

        public void SetRaw(string id, string o)
        {
            string path = this.FormatFilePath(id);
            lock (this.m_writeLock)
                File.WriteAllText(path, o);
        }

        public T Get<T>(string id)
        {
            string raw = this.GetRaw(id);
            if (raw == null)
                return default(T);
            T obj;
            try
            {
                obj = raw.ToObject<T>();
            }
            catch (Exception ex)
            {
                obj = default(T);
                this.Delete(id);
                LogHelper.Instance.LogException("DataStoreService.Get<T> - An exception occurred deserializing an object. Deleting file.", ex);
            }
            return obj;
        }

        public string GetRaw(Guid id) => this.GetRaw(id.ToString());

        public string GetRaw(string id)
        {
            string path = this.FormatFilePath(id);
            if (!File.Exists(path))
                return (string)null;
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (StreamReader streamReader = new StreamReader((Stream)fileStream))
                    return streamReader.ReadToEnd();
            }
        }

        public void Delete(string id)
        {
            string path = this.FormatFilePath(id);
            if (!File.Exists(path))
                return;
            File.Delete(path);
        }

        public void Set(Guid id, object o) => this.Set(id.ToString(), o);

        public T Get<T>(Guid id) => this.Get<T>(id.ToString());

        public void Delete(Guid id) => this.Delete(id.ToString());

        public void CleanUp()
        {
            foreach (string file in Directory.GetFiles(this.Root, "*.dat"))
            {
                if (!Path.GetFileNameWithoutExtension(file).StartsWith("config", StringComparison.CurrentCultureIgnoreCase))
                    File.Delete(file);
            }
        }

        public void CleanUp(string extension)
        {
            foreach (string file in Directory.GetFiles(this.Root, "*" + extension))
            {
                if (!Path.GetFileNameWithoutExtension(file).StartsWith("config", StringComparison.CurrentCultureIgnoreCase))
                    File.Delete(file);
            }
        }

        private DataStoreService()
        {
        }

        private string FormatFilePath(string id)
        {
            return Path.HasExtension(id) ? Path.Combine(this.Root, id) : Path.Combine(this.Root, string.Format("{0}.dat", (object)id));
        }

        private string Root { get; set; }
    }
}
