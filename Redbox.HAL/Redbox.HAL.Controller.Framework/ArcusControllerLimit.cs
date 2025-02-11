using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class ArcusControllerLimit : IMotionControlLimit
    {
        internal ArcusControllerLimit(MotionControlLimits n, bool b)
        {
            Limit = n;
            Blocked = b;
        }

        public MotionControlLimits Limit { get; }

        public bool Blocked { get; }
    }
}