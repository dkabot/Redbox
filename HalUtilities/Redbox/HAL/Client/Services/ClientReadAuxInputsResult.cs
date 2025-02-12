using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using System;
using System.Collections.Generic;

namespace Redbox.HAL.Client.Services
{
  internal sealed class ClientReadAuxInputsResult : AbstractClientReadInputsResult<AuxInputs>
  {
    protected override InputState OnGetInputState(AuxInputs input)
    {
      return !(this.Inputs[(int) input] == "1") ? InputState.Inactive : InputState.Active;
    }

    protected override void OnForeachInput(Action<AuxInputs> a)
    {
      foreach (AuxInputs auxInputs in (IEnumerable<AuxInputs>) Enum<AuxInputs>.GetValues())
        a(auxInputs);
    }

    protected override string LogHeader => "Aux Inputs";

    internal ClientReadAuxInputsResult(HardwareService service)
      : base(service, "SENSOR READ-AUX-INPUTS")
    {
    }
  }
}
