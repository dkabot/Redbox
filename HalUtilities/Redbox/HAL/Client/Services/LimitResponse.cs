using Redbox.HAL.Component.Model;
using System;

namespace Redbox.HAL.Client.Services
{
  internal sealed class LimitResponse : IMotionControlLimitResponse
  {
    public bool IsLimitBlocked(MotionControlLimits limit) => throw new NotImplementedException();

    public bool ReadOk { get; private set; }

    public IMotionControlLimit[] Limits { get; private set; }

    internal LimitResponse(bool readOk, IMotionControlLimit[] limits)
    {
      this.ReadOk = readOk;
      this.Limits = limits;
    }
  }
}
