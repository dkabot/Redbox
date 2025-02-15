using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Client.Services;

internal sealed class ClientReadPickerInputsResult : AbstractClientReadInputsResult<PickerInputs>
{
    internal ClientReadPickerInputsResult(HardwareService service)
        : base(service, "SENSOR READ-PICKER-INPUTS")
    {
    }

    protected override string LogHeader => "Picker Inputs";

    protected override InputState OnGetInputState(PickerInputs input)
    {
        return !(Inputs[(int)input] == "1") ? InputState.Inactive : InputState.Active;
    }

    protected override void OnForeachInput(Action<PickerInputs> a)
    {
        foreach (var pickerInputs in Enum<PickerInputs>.GetValues())
            a(pickerInputs);
    }
}