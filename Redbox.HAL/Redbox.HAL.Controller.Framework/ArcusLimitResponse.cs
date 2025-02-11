using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class ArcusLimitResponse : IMotionControlLimitResponse
    {
        internal ArcusLimitResponse(string queryResponse)
        {
            ReadOk = false;
            Limits = null;
            if (queryResponse == null)
                return;
            var strArray = queryResponse.Trim().Split(',');
            if (strArray.Length != 28)
                return;
            int result;
            if (!int.TryParse(strArray[13].TrimEnd('.'), out result))
                return;
            ReadOk = true;
            Limits = new IMotionControlLimit[2];
            Limits[0] = new ArcusControllerLimit(MotionControlLimits.Upper, (result & 16) != 0);
            Limits[1] = new ArcusControllerLimit(MotionControlLimits.Lower, (result & 32) != 0);
        }

        public bool IsLimitBlocked(MotionControlLimits limit)
        {
            if (!ReadOk || Limits == null || limit == MotionControlLimits.None)
                throw new InvalidOperationException("Limits not setup.");
            foreach (var limit1 in Limits)
                if (limit1.Limit == limit)
                    return limit1.Blocked;
            LogHelper.Instance.Log("[ArcusLimitResponse] Unable to find limit {0}", limit.ToString());
            return true;
        }

        public bool ReadOk { get; }

        public IMotionControlLimit[] Limits { get; }
    }
}