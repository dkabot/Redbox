using System;
using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    [NativeJob(ProgramName = "door-sensor-software-override")]
    internal sealed class ChangeDoorSensorConfigurationJob : NativeJobAdapter
    {
        internal ChangeDoorSensorConfigurationJob(ExecutionResult result, ExecutionContext ctx)
            : base(result, ctx)
        {
        }

        protected override void ExecuteInner()
        {
            if (!ControllerConfiguration.Instance.IsVMZMachine)
            {
                AddError("Unsupported door switch configuration change on non-VMZ kiosk.");
            }
            else
            {
                var service = ServiceLocator.Instance.GetService<IDoorSensorService>();
                var str1 = Context.PopTop<string>();
                service.SoftwareOverride = str1.Equals("off", StringComparison.CurrentCultureIgnoreCase);
                using (var streamWriter =
                       new StreamWriter(
                           Path.Combine(
                               ServiceLocator.Instance.GetService<IFormattedLogFactoryService>()
                                   .CreateSubpath("Service"), "DoorSensorConfiguration.log"), true))
                {
                    var str2 = string.Format("{0} {1}", DateTime.Now.ToShortDateString(),
                        DateTime.Now.ToShortTimeString());
                    streamWriter.WriteLine(service.SoftwareOverride
                        ? string.Format(
                            "{0} : Door sensors have a software override in place: they are not active - *UNCHECKED MOVEMENT IS POSSIBLE*.",
                            str2)
                        : string.Format("{0} : Door sensors do not have a software override; they are active.", str2));
                }
            }
        }
    }
}