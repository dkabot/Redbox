using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Services
{
    internal sealed class ClientControlLimit : IMotionControlLimit
    {
        internal ClientControlLimit(MotionControlLimits lim, bool blocked)
        {
            Limit = lim;
            Blocked = blocked;
        }

        public MotionControlLimits Limit { get; }

        public bool Blocked { get; }
    }
}