using Newtonsoft.Json;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Health;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Redbox.KioskEngine.Environment
{
  public class DataFileStoreService : IDataFileStoreService
  {
    private string _dataStoreName;
    private string _extension = "dat";
    private bool _UseEncryption;
    private string _corruptFileExtension = "corrupt_dat";
    private readonly object m_writeLock = new object();
    private Dictionary<string, Dictionary<string, Type>> _cachedSerializationTypes = new Dictionary<string, Dictionary<string, Type>>();

    public void Initialize(
      string dataStoreName,
      string dataFilePath,
      string extension,
      string corruptFileExtension,
      bool useEncryption)
    {
      this._UseEncryption = useEncryption;
      this._dataStoreName = dataStoreName;
      this._extension = extension != null ? extension : this._extension;
      this._corruptFileExtension = corruptFileExtension != null ? corruptFileExtension : this._corruptFileExtension;
      if (!Path.IsPathRooted(dataFilePath))
        dataFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), dataFilePath);
      this._root = dataFilePath;
      if (Directory.Exists(this._root))
        return;
      Directory.CreateDirectory(this._root);
    }

    public void Reset()
    {
      if (string.IsNullOrEmpty(this._root))
        return;
      if (Directory.Exists(this._root))
        Directory.Delete(this._root, true);
      this.Initialize(this._dataStoreName, this._root, this._extension, this._corruptFileExtension, this._UseEncryption);
    }

    private Type GetType(string assemblyName, string typeName)
    {
      Type type = this.GetCachedSerializationType(assemblyName, typeName);
      if (type == (Type) null && !string.IsNullOrEmpty(typeName) && !string.IsNullOrEmpty(assemblyName))
      {
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), assemblyName);
        if (File.Exists(path))
        {
          Assembly assembly = Assembly.LoadFile(path);
          if (assembly != (Assembly) null)
          {
            type = assembly.GetType(typeName);
            if (type != (Type) null)
              this.AddCachedSerializationType(assemblyName, typeName, type);
            else
              LogHelper.Instance.Log("Unable to get type {1} in assembly {0}", (object) path, (object) typeName);
          }
          else
            LogHelper.Instance.Log("Unable to load assembly {0}", (object) path);
        }
        else
          LogHelper.Instance.Log("Unable to find assembly {0}", (object) path);
      }
      return type;
    }

    public bool Set(string fileName, object o)
    {
      bool flag = false;
      if (o != null)
      {
        string str = JsonConvert.SerializeObject(o);
        Type type = o?.GetType();
        DataFileStoreService.JsonWrapper jsonWrapper = new DataFileStoreService.JsonWrapper()
        {
          SerializedType = type?.ToString(),
          AssemblyName = Path.GetFileName(type?.Assembly?.CodeBase),
          JsonData = str
        };
        if (o != null && !string.IsNullOrEmpty(jsonWrapper.SerializedType) && !string.IsNullOrEmpty(jsonWrapper.AssemblyName))
          this.AddCachedSerializationType(jsonWrapper.AssemblyName, jsonWrapper.SerializedType, type);
        string o1 = JsonConvert.SerializeObject((object) jsonWrapper);
        flag = this.SetRaw(fileName, o1);
      }
      return flag;
    }

    public bool SetRaw(Guid guidFileName, string o) => this.SetRaw(guidFileName.ToString(), o);

    public bool SetRaw(string fileName, string o)
    {
      bool flag = false;
      try
      {
        string path = this.FormatFilePath(fileName);
        if (!string.IsNullOrEmpty(path))
        {
          lock (this.m_writeLock)
          {
            if (this._UseEncryption)
              o = o.EncryptToBase64();
            File.WriteAllText(path, o);
            flag = true;
          }
        }
        else
          LogHelper.Instance.Log("Error in DataFileStoreService.SetRaw: target is null.");
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log(string.Format("Exception in DataFileStoreService.SetRaw. {0}", (object) ex));
      }
      return flag;
    }

    public List<T> GetAll<T>()
    {
      List<T> all = new List<T>();
      if (Directory.Exists(this._root))
      {
        foreach (string file in Directory.GetFiles(this._root, "*." + this._extension))
        {
          T deserializedObject;
          if (this.Get<T>(file, out deserializedObject))
            all.Add(deserializedObject);
        }
      }
      return all;
    }

    public bool Get<T>(string fileName, out T deserializedObject)
    {
      bool flag = false;
      deserializedObject = default (T);
      string raw = this.GetRaw(fileName);
      if (raw != null)
      {
        try
        {
          DataFileStoreService.JsonWrapper jsonWrapper = JsonConvert.DeserializeObject<DataFileStoreService.JsonWrapper>(raw);
          if (jsonWrapper != null)
          {
            Type type = this.GetType(jsonWrapper?.AssemblyName, jsonWrapper?.SerializedType);
            if (type != (Type) null)
            {
              object obj1 = JsonConvert.DeserializeObject(jsonWrapper.JsonData, type);
              if (obj1 != null && obj1 is T obj2)
              {
                deserializedObject = obj2;
                flag = true;
              }
              else
                LogHelper.Instance.Log(string.Format("DataFileStoreService.Get<T> - error - deserialized type {0} is not the correct type from file {1}", (object) type, (object) fileName));
            }
            else
              LogHelper.Instance.Log("DataFileStoreService.Get<T> - error - unable to get type " + jsonWrapper?.SerializedType + " from file " + fileName);
          }
          else
            LogHelper.Instance.Log("DataFileStoreService.Get<T> - error - deserialized object was not of type JsonWrapper from file " + fileName);
        }
        catch (Exception ex)
        {
          LogHelper.Instance.LogException("DataFileStoreService.Get<T> - An exception occurred deserializing an object from file " + fileName, ex);
          ServiceLocator.Instance.GetService<IHealthServices>()?.SendAlert(ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId.ToString() ?? (string) null, "ApplicationCrash", this._dataStoreName + ", Read-Error-01", "There was an error in " + this._dataStoreName + " deserializing file " + fileName, DateTime.Now, (RemoteServiceCallback) null);
        }
      }
      if (!flag)
        this.RenameCorruptFileExtension(fileName);
      return flag;
    }

    private void RenameCorruptFileExtension(string fileName)
    {
      fileName = this.FormatFilePath(fileName);
      if (!File.Exists(fileName))
        return;
      try
      {
        string destFileName = Path.ChangeExtension(fileName, this._corruptFileExtension);
        LogHelper.Instance.Log("Renaming corrupted json file " + fileName + " to " + destFileName);
        lock (this.m_writeLock)
          File.Move(fileName, destFileName);
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log(string.Format("Error in DataFileStoreService.RenameCorruptFileExtension. {0}", (object) ex));
      }
    }

    public string GetRaw(Guid fileName) => this.GetRaw(fileName.ToString());

    public string GetRaw(string fileName)
    {
      string path = this.FormatFilePath(fileName);
      if (!File.Exists(path))
        return (string) null;
      using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        using (StreamReader streamReader = new StreamReader((Stream) fileStream))
        {
          string source = streamReader.ReadToEnd();
          if (this._UseEncryption)
          {
            try
            {
              source = source.DecryptFromBase64();
            }
            catch (Exception ex)
            {
              LogHelper.Instance.Log(string.Format("Error in DataFileStoreService.GetRaw.  Unable to Decrypt data. {0}", (object) ex));
              source = (string) null;
            }
          }
          return source;
        }
      }
    }

    public void Delete(string fileName)
    {
      string path = this.FormatFilePath(fileName);
      if (!File.Exists(path))
        return;
      lock (this.m_writeLock)
        File.Delete(path);
    }

    public string Root => this._root;

    public bool UseEncryption
    {
      get => this._UseEncryption;
      set => this._UseEncryption = value;
    }

    private string FormatFilePath(string fileName)
    {
      string str = (string) null;
      string path2 = Path.HasExtension(fileName) ? fileName : fileName + "." + this._extension;
      if (!string.IsNullOrEmpty(this._root) && !string.IsNullOrEmpty(path2))
        str = Path.Combine(this._root, path2);
      return str;
    }

    private string _root { get; set; }

    private Type GetCachedSerializationType(string assemblyName, string typeName)
    {
      Type serializationType = (Type) null;
      Dictionary<string, Type> dictionary = (Dictionary<string, Type>) null;
      this._cachedSerializationTypes.TryGetValue(assemblyName, out dictionary);
      dictionary?.TryGetValue(typeName, out serializationType);
      return serializationType;
    }

    private void AddCachedSerializationType(string assemblyName, string typeName, Type type)
    {
      if (!(this.GetCachedSerializationType(assemblyName, typeName) == (Type) null))
        return;
      Dictionary<string, Type> dictionary = (Dictionary<string, Type>) null;
      this._cachedSerializationTypes.TryGetValue(assemblyName, out dictionary);
      if (dictionary == null)
        dictionary = new Dictionary<string, Type>();
      dictionary[typeName] = type;
      this._cachedSerializationTypes[assemblyName] = dictionary;
    }

    private class JsonWrapper
    {
      public string SerializedType { get; set; }

      public string AssemblyName { get; set; }

      public string JsonData { get; set; }
    }
  }
}
