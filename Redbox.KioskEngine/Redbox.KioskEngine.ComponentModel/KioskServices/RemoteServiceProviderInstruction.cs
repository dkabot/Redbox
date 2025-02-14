using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel.KioskServices
{
    public class RemoteServiceProviderInstruction : IRemoteServiceProviderInstruction
    {
        public InstructionType Type { get; set; }

        public IDictionary<string, object> Parameters { get; } = new Dictionary<string, object>();
    }
}