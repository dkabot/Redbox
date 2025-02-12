using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Client.Services
{
  public sealed class MoveHelper
  {
    private readonly HardwareService Service;
    private readonly int Timeout;

    public ErrorCodes MoveTo(int deck, int slot, string mode)
    {
      HardwareJob job;
      return !this.Service.ExecuteImmediate(string.Format("MOVE DECK={0} SLOT={1} MODE={2}", (object) deck, (object) slot, (object) mode), new int?(this.Timeout), out job).Success ? ErrorCodes.ServiceChannelError : Enum<ErrorCodes>.ParseIgnoringCase(job.GetTopOfStack(), ErrorCodes.ServiceChannelError);
    }

    public ErrorCodes MoveTo(int deck, int slot) => this.MoveTo(deck, slot, "NORMAL");

    public IControllerPosition GetPosition()
    {
      using (MotionControlDataExecutor controlDataExecutor = new MotionControlDataExecutor(this.Service))
      {
        controlDataExecutor.Run();
        return controlDataExecutor.Position;
      }
    }

    public ErrorCodes MoveAbs(Axis axis, int units) => this.MoveAbs(axis, units, true);

    public ErrorCodes MoveAbs(Axis axis, int units, bool checkSensors)
    {
      HardwareJob job;
      return !this.Service.ExecuteImmediate(string.Format("MOVEABS AXIS={0} UNITS={1} SENSOR-CHECK={2}", (object) axis.ToString().ToUpper(), (object) units.ToString(), (object) checkSensors.ToString().ToUpper()), new int?(this.Timeout), out job).Success ? ErrorCodes.ServiceChannelError : Enum<ErrorCodes>.ParseIgnoringCase(job.GetTopOfStack(), ErrorCodes.ServiceChannelError);
    }

    public IMotionControlLimitResponse ReadLimits()
    {
      using (MotionControlDataExecutor controlDataExecutor = new MotionControlDataExecutor(this.Service))
      {
        controlDataExecutor.Run();
        return controlDataExecutor.Limits;
      }
    }

    public MoveHelper(HardwareService service, int timeout)
    {
      this.Service = service;
      this.Timeout = timeout;
    }

    public MoveHelper(HardwareService service)
      : this(service, 64000)
    {
    }
  }
}
