using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class ReadPickerInputsResult : AbstractReadInputsResult<PickerInputs>
    {
        internal ReadPickerInputsResult(CoreResponse response)
            : base(response)
        {
        }

        protected override string LogHeader => "Picker Inputs";

        protected override InputState OnGetInputState(PickerInputs input)
        {
            return GetInputState((int)input);
        }

        protected override void OnForeachInput(Action<PickerInputs> a)
        {
            foreach (var pickerInputs in Enum<PickerInputs>.GetValues())
                a(pickerInputs);
        }
    }
}