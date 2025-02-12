using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Client.Services
{
    public sealed class MotionControlDataExecutor : JobExecutor
    {
        public MotionControlDataExecutor(HardwareService service) : base(service)
        {
        }

        protected override string JobName => "motion-control-get-data";

        public IControllerPosition Position { get; private set; }

        public IMotionControlLimitResponse Limits { get; private set; }

        public string CurrentLocation { get; private set; }

        protected override void OnJobCompleted()
        {
            if (HardwareJobStatus.Completed != EndStatus)
                return;
            var motionControlLimitList = new List<IMotionControlLimit>();
            var ok = true;
            var readOk = true;
            var x = new int?();
            var y = new int?();
            foreach (var result1 in Results)
                if (result1.Code == "PositionReadError")
                {
                    ok = false;
                }
                else if (result1.Code == "PositionX")
                {
                    int result2;
                    if (int.TryParse(result1.Message, out result2))
                        x = result2;
                }
                else if (result1.Code == "PositionY")
                {
                    int result3;
                    if (int.TryParse(result1.Message, out result3))
                        y = result3;
                }
                else if (result1.Code == "LimitReadError")
                {
                    readOk = false;
                }
                else if (result1.Code == "CurrentLocation")
                {
                    CurrentLocation = result1.Message;
                }
                else
                {
                    var l = Enum<MotionControlLimits>.Parse(result1.Code, MotionControlLimits.None);
                    if (l != MotionControlLimits.None && motionControlLimitList.Find(each => each.Limit == l) == null)
                        motionControlLimitList.Add(new ClientControlLimit(l, result1.Message == "BLOCKED"));
                }

            Position = new ClientControllerPosition(ok, x, y);
            Limits = new LimitResponse(readOk, motionControlLimitList.ToArray());
        }
    }
}