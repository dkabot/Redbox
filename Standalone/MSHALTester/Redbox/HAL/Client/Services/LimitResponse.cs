using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Services;

internal sealed class LimitResponse : IMotionControlLimitResponse
{
    internal LimitResponse(bool readOk, IMotionControlLimit[] limits)
    {
        ReadOk = readOk;
        Limits = limits;
    }

    public bool IsLimitBlocked(MotionControlLimits limit)
    {
        throw new NotImplementedException();
    }

    public bool ReadOk { get; }

    public IMotionControlLimit[] Limits { get; }
}