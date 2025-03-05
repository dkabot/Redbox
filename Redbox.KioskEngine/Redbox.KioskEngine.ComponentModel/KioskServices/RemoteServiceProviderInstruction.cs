using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel.KioskServices
{
  public class RemoteServiceProviderInstruction : IRemoteServiceProviderInstruction
  {
    private readonly IDictionary<string, object> m_parameters = (IDictionary<string, object>) new Dictionary<string, object>();

    public InstructionType Type { get; set; }

    public IDictionary<string, object> Parameters => this.m_parameters;
  }
}
