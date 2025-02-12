using Redbox.HAL.Component.Model;
using System;
using System.Collections.Generic;

namespace Redbox.HAL.Client.Executors
{
  public sealed class KioskFunctionCheckExecutor : JobExecutor
  {
    public IList<IKioskFunctionCheckData> Sessions { get; private set; }

    public KioskFunctionCheckExecutor(HardwareService service)
      : base(service)
    {
      this.Sessions = (IList<IKioskFunctionCheckData>) new List<IKioskFunctionCheckData>();
    }

    protected override void DisposeInner() => this.Sessions.Clear();

    protected override void OnJobCompleted()
    {
      Stack<string> stack;
      if (!this.Job.GetStack(out stack).Success)
        return;
      int num = int.Parse(stack.Pop());
      for (int index = 0; index < num; ++index)
        this.Sessions.Add((IKioskFunctionCheckData) new KioskFunctionCheckExecutor.KFCData()
        {
          VerticalSlotTestResult = stack.Pop(),
          InitTestResult = stack.Pop(),
          VendDoorTestResult = stack.Pop(),
          TrackTestResult = stack.Pop(),
          SnapDecodeTestResult = stack.Pop(),
          CameraDriverTestResult = stack.Pop(),
          TouchscreenDriverTestResult = stack.Pop(),
          Timestamp = DateTime.Parse(stack.Pop()),
          UserIdentifier = stack.Pop()
        });
    }

    protected override string JobName => "load-kiosk-function-check-data";

    private class KFCData : IKioskFunctionCheckData
    {
      public string VerticalSlotTestResult { get; internal set; }

      public string InitTestResult { get; internal set; }

      public string VendDoorTestResult { get; internal set; }

      public string TrackTestResult { get; internal set; }

      public string SnapDecodeTestResult { get; internal set; }

      public string TouchscreenDriverTestResult { get; internal set; }

      public string CameraDriverTestResult { get; internal set; }

      public DateTime Timestamp { get; internal set; }

      public string UserIdentifier { get; internal set; }
    }
  }
}
