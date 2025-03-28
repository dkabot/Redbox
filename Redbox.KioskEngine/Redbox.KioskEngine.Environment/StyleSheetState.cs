using Redbox.KioskEngine.ComponentModel;
using System.Collections.Generic;

namespace Redbox.KioskEngine.Environment
{
  public class StyleSheetState : IStyleSheetState
  {
    private readonly IDictionary<string, object> m_properties = (IDictionary<string, object>) new Dictionary<string, object>();

    public string Name { get; internal set; }

    public object this[string key]
    {
      get => !this.m_properties.ContainsKey(key) ? (object) null : this.m_properties[key];
      internal set => this.m_properties[key] = value;
    }
  }
}
