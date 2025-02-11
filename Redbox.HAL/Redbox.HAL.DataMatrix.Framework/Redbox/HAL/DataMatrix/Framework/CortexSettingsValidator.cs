using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.DataMatrix.Framework.Cortex;

namespace Redbox.HAL.DataMatrix.Framework
{
    internal sealed class CortexSettingsValidator
    {
        private static readonly CortexRegister[] Registers = new CortexRegister[17]
        {
            new CortexRegister("93", "1"),
            new CortexRegister("26", "0"),
            new CortexRegister("d1", "2"),
            new CortexRegister("a8", "100"),
            new CortexRegister("a9", "20"),
            new CortexRegister("66", "100"),
            new CortexRegister("1b7", "0"),
            new CortexRegister("256", "2"),
            new CortexRegister("34", "5"),
            new CortexRegister("b4", "1"),
            new CortexRegister("45", "1000"),
            new CortexRegister("a5", "1000"),
            new CortexRegister("a3", "1000"),
            new CortexRegister("21D", "1"),
            new CortexRegister("42", "0"),
            new CortexRegister("08", "2"),
            new CortexRegister("1b", "6")
        };

        private readonly IRuntimeService RuntimeService;
        private readonly char[] Separator = new char[1] { ':' };
        private readonly CortexService Service;

        internal CortexSettingsValidator(CortexService service, IRuntimeService iruntimeService_0)
        {
            Service = service;
            RuntimeService = iruntimeService_0;
        }

        internal bool Validate(IMessageSink sink)
        {
            var errors = 0;
            Array.ForEach(Registers, register =>
            {
                var registerValueCommand = new GetRegisterValueCommand(register);
                registerValueCommand.Send(Service.ConfiguredPort());
                if (!registerValueCommand.IsCorrectlySet)
                {
                    ++errors;
                    sink.Send(string.Format("The register {0} received an unexpected response", register.Register));
                }
                else
                {
                    sink.Send(string.Format("Register {0} matched expected value", register.Register));
                }

                RuntimeService.SpinWait(200);
            });
            return errors == 0;
        }
    }
}