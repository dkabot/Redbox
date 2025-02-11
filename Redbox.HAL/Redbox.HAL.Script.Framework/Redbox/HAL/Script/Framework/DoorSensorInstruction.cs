using System;
using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework
{
    [Serializable]
    public sealed class DoorSensorInstruction : Instruction
    {
        public override string Mnemonic => "DOORSENSORS";

        protected override bool IsOperandRecognized(string operand)
        {
            return Enum<ExpectedOperand>.ParseIgnoringCase(operand, ExpectedOperand.None) != 0;
        }

        protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
        {
            if (Enum<ExpectedOperand>.ParseIgnoringCase(operandTokens[0].Value, ExpectedOperand.None) !=
                ExpectedOperand.None)
                return;
            result.AddInvalidOperandError("Expected a 'Query' or 'IsConfigured' operand.");
        }

        protected override void OnExecute(ExecutionResult result, ExecutionContext context)
        {
            var ignoringCase = Enum<ExpectedOperand>.ParseIgnoringCase(Operands[0].Value, ExpectedOperand.None);
            var service = ServiceLocator.Instance.GetService<IDoorSensorService>();
            switch (ignoringCase)
            {
                case ExpectedOperand.None:
                    LogHelper.Instance.WithContext("DoorSensor instruction: converted '{0}' to operand None",
                        Operands[0].Value);
                    break;
                case ExpectedOperand.IsConfigured:
                    context.PushTop(service.SensorsEnabled);
                    break;
                case ExpectedOperand.Query:
                    RunQuery(service, context);
                    context.PushTop(1);
                    break;
                case ExpectedOperand.Status:
                    context.PushTop(service.Query().ToString().ToUpper());
                    break;
                case ExpectedOperand.SoftwareOverride:
                    string val;
                    if (!GetKeyValuePairValue("OVERRIDE", out val, result.Errors, context))
                        val = "off";
                    var flag = val.Equals("off", StringComparison.CurrentCultureIgnoreCase);
                    service.SoftwareOverride = flag;
                    using (var streamWriter1 = new StreamWriter(File.Open(
                               Path.Combine(
                                   ServiceLocator.Instance.GetService<IFormattedLogFactoryService>().LogsBasePath,
                                   "Service\\DoorSensorConfiguration.log"), FileMode.Append, FileAccess.Write,
                               FileShare.Read)))
                    {
                        var streamWriter2 = streamWriter1;
                        string str;
                        if (!flag)
                        {
                            var now = DateTime.Now;
                            var shortDateString = now.ToShortDateString();
                            now = DateTime.Now;
                            var shortTimeString = now.ToShortTimeString();
                            str = string.Format(
                                "{0} {1}: Door sensors do not have a software override; they are active.",
                                shortDateString, shortTimeString);
                        }
                        else
                        {
                            var now = DateTime.Now;
                            var shortDateString = now.ToShortDateString();
                            now = DateTime.Now;
                            var shortTimeString = now.ToShortTimeString();
                            str = string.Format(
                                "{0} {1}: Door sensors have a software override in place: they are not active - *UNCHECKED MOVEMENT IS POSSIBLE*.",
                                shortDateString, shortTimeString);
                        }

                        streamWriter2.WriteLine(str);
                    }

                    context.PushTop(service.SensorsEnabled);
                    break;
                case ExpectedOperand.TesterQuery:
                    RunQuery(service, context);
                    break;
            }
        }

        private void RunQuery(IDoorSensorService service, ExecutionContext context)
        {
            switch (service.QueryStateForDisplay())
            {
                case DoorSensorResult.Ok:
                    context.PushTop("FRONT DOOR CLOSED");
                    break;
                case DoorSensorResult.FrontDoor:
                    context.PushTop("FRONT DOOR OPEN");
                    break;
                case DoorSensorResult.AuxReadError:
                    context.PushTop("AUX board not responsive.");
                    break;
                case DoorSensorResult.SoftwareOverride:
                    context.PushTop("OVERRIDE IN PLACE.");
                    break;
            }
        }

        private enum ExpectedOperand
        {
            None,
            IsConfigured,
            Query,
            Status,
            SoftwareOverride,
            TesterQuery
        }
    }
}