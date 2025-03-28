using Redbox.KioskEngine.ComponentModel;
using System.Collections.Generic;

namespace Redbox.KioskEngine.Environment
{
  public class StyleSheetStyle : IStyleSheetStyle
  {
    private readonly IDictionary<string, IStyleSheetState> m_states = (IDictionary<string, IStyleSheetState>) new Dictionary<string, IStyleSheetState>();

    public void RemoveState(string name) => this.m_states.Remove(name);

    public IStyleSheetState AddState(string name)
    {
      if (this.m_states.ContainsKey(name))
        return this.m_states[name];
      StyleSheetState styleSheetState = new StyleSheetState()
      {
        Name = name
      };
      this.m_states[name] = (IStyleSheetState) styleSheetState;
      return (IStyleSheetState) styleSheetState;
    }

    public IStyleSheetState GetState(string name)
    {
      return !this.m_states.ContainsKey(name) ? (IStyleSheetState) null : this.m_states[name];
    }

    public string Name { get; internal set; }
  }
}
