using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using System;
using System.Collections.Generic;

namespace Redbox.HAL.Client.Services
{
  public sealed class MotionControlDataExecutor : JobExecutor
  {
    protected override string JobName => "motion-control-get-data";

    protected override void OnJobCompleted()
    {
      if (HardwareJobStatus.Completed != this.EndStatus)
        return;
      List<IMotionControlLimit> motionControlLimitList = new List<IMotionControlLimit>();
      bool ok = true;
      bool readOk = true;
      int? x = new int?();
      int? y = new int?();
      foreach (ProgramResult result1 in this.Results)
      {
        if (result1.Code == "PositionReadError")
          ok = false;
        else if (result1.Code == "PositionX")
        {
          int result2;
          if (int.TryParse(result1.Message, out result2))
            x = new int?(result2);
        }
        else if (result1.Code == "PositionY")
        {
          int result3;
          if (int.TryParse(result1.Message, out result3))
            y = new int?(result3);
        }
        else if (result1.Code == "LimitReadError")
          readOk = false;
        else if (result1.Code == "CurrentLocation")
        {
          this.CurrentLocation = result1.Message;
        }
        else
        {
          MotionControlLimits l = Enum<MotionControlLimits>.Parse(result1.Code, MotionControlLimits.None);
          if (l != MotionControlLimits.None && motionControlLimitList.Find((Predicate<IMotionControlLimit>) (each => each.Limit == l)) == null)
            motionControlLimitList.Add((IMotionControlLimit) new ClientControlLimit(l, result1.Message == "BLOCKED"));
        }
      }
      this.Position = (IControllerPosition) new ClientControllerPosition(ok, x, y);
      this.Limits = (IMotionControlLimitResponse) new LimitResponse(readOk, motionControlLimitList.ToArray());
    }

    public IControllerPosition Position { get; private set; }

    public IMotionControlLimitResponse Limits { get; private set; }

    public string CurrentLocation { get; private set; }

        public MotionControlDataExecutor(HardwareService service) : base(service)
        {
        }
    }
}
