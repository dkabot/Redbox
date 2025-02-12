using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using System;
using System.Collections.Generic;

namespace Redbox.HAL.Client.Services
{
  internal sealed class ClientReadPickerInputsResult : AbstractClientReadInputsResult<PickerInputs>
  {
    protected override InputState OnGetInputState(PickerInputs input)
    {
      return !(this.Inputs[(int) input] == "1") ? InputState.Inactive : InputState.Active;
    }

    protected override void OnForeachInput(Action<PickerInputs> a)
    {
      foreach (PickerInputs pickerInputs in (IEnumerable<PickerInputs>) Enum<PickerInputs>.GetValues())
        a(pickerInputs);
    }

    protected override string LogHeader => "Picker Inputs";

    internal ClientReadPickerInputsResult(HardwareService service)
      : base(service, "SENSOR READ-PICKER-INPUTS")
    {
    }
  }
}
