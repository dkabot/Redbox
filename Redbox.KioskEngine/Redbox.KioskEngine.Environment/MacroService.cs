using Redbox.KioskEngine.ComponentModel;
using Redbox.Macros;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Redbox.KioskEngine.Environment
{
  public class MacroService : IMacroService
  {
    private readonly PropertyDictionary m_properties = new PropertyDictionary();

    public void Clear(ICollection<string> exclusions)
    {
      if (exclusions.Count == 0)
      {
        this.m_properties.Clear();
      }
      else
      {
        List<string> stringList = new List<string>();
        foreach (object key in (IEnumerable) this.m_properties.Keys)
        {
          if (!exclusions.Contains((string) key))
            stringList.Add((string) key);
        }
        foreach (string name in stringList)
          this.m_properties.Remove(name);
      }
    }

    public string ExpandProperties(string input)
    {
      return this.m_properties.ExpandProperties(input, Location.UnknownLocation);
    }

    public string this[string name]
    {
      get => this.m_properties[name];
      set => this.m_properties[name] = value;
    }

    public ICollection Keys => this.m_properties.Keys;

    public ICollection Values => this.m_properties.Values;

    static MacroService()
    {
      FunctionFactory.ScanDir(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), false);
    }
  }
}
