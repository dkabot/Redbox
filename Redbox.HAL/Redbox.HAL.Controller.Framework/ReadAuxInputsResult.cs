using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class ReadAuxInputsResult : AbstractReadInputsResult<AuxInputs>
    {
        internal ReadAuxInputsResult(CoreResponse response)
            : base(response)
        {
        }

        protected override string LogHeader => "AUX Inputs";

        protected override InputState OnGetInputState(AuxInputs input)
        {
            return GetInputState((int)input);
        }

        protected override void OnForeachInput(Action<AuxInputs> a)
        {
            foreach (var auxInputs in Enum<AuxInputs>.GetValues())
                a(auxInputs);
        }
    }
}