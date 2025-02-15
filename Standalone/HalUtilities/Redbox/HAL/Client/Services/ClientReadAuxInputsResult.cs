using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Client.Services
{
    internal sealed class ClientReadAuxInputsResult : AbstractClientReadInputsResult<AuxInputs>
    {
        internal ClientReadAuxInputsResult(HardwareService service)
            : base(service, "SENSOR READ-AUX-INPUTS")
        {
        }

        protected override string LogHeader => "Aux Inputs";

        protected override InputState OnGetInputState(AuxInputs input)
        {
            return !(Inputs[(int)input] == "1") ? InputState.Inactive : InputState.Active;
        }

        protected override void OnForeachInput(Action<AuxInputs> a)
        {
            foreach (var auxInputs in Enum<AuxInputs>.GetValues())
                a(auxInputs);
        }
    }
}