using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System.Collections.Generic;

namespace Redbox.KioskEngine.Environment
{
  public class ObjectCacheService : IObjectCacheService
  {
    private readonly IDictionary<string, object> m_objects = (IDictionary<string, object>) new Dictionary<string, object>();

    public static ObjectCacheService Instance => Singleton<ObjectCacheService>.Instance;

    public T GetObject<T>(string name)
    {
      return !this.m_objects.ContainsKey(name) ? default (T) : (T) ConversionHelper.ChangeType(this.m_objects[name], typeof (T));
    }

    public void SetObject<T>(string name, T value) => this.m_objects[name] = (object) value;

    private ObjectCacheService()
    {
    }
  }
}
