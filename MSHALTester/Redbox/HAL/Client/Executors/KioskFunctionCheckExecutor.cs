using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Executors;

public sealed class KioskFunctionCheckExecutor : JobExecutor
{
    public KioskFunctionCheckExecutor(HardwareService service)
        : base(service)
    {
        Sessions = new List<IKioskFunctionCheckData>();
    }

    public IList<IKioskFunctionCheckData> Sessions { get; }

    protected override string JobName => "load-kiosk-function-check-data";

    protected override void DisposeInner()
    {
        Sessions.Clear();
    }

    protected override void OnJobCompleted()
    {
        Stack<string> stack;
        if (!Job.GetStack(out stack).Success)
            return;
        var num = int.Parse(stack.Pop());
        for (var index = 0; index < num; ++index)
            Sessions.Add(new KFCData
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