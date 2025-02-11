using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    internal static class MerchSegmentFactory
    {
        private static readonly Dictionary<MerchFlags, MerchSegment> SegmentMap =
            new Dictionary<MerchFlags, MerchSegment>();

        internal static MerchSegment Get(MerchFlags flags)
        {
            if (SegmentMap.ContainsKey(flags))
                return SegmentMap[flags];
            var merchSegment = (MerchSegment)null;
            switch ((long)(flags - 1L))
            {
                case 0:
                    merchSegment = new ThinSegment(ThinHelper.Debug);
                    break;
                case 2:
                    merchSegment = new RebalanceSegment(ThinHelper.Debug);
                    break;
                case 3:
                    merchSegment = new ThinRedeploySegment(ThinHelper.Debug);
                    break;
            }

            if (merchSegment != null)
                SegmentMap[flags] = merchSegment;
            return merchSegment;
        }

        private sealed class RebalanceSegment : MerchSegment
        {
            internal RebalanceSegment(bool debug)
                : base(MerchFlags.Rebalance, debug)
            {
            }

            internal override bool CanDump => false;

            protected internal override MerchFlags NextLower => MerchFlags.ThinRedeploy;
        }

        private sealed class ThinRedeploySegment : MerchSegment
        {
            internal ThinRedeploySegment(bool debug)
                : base(MerchFlags.ThinRedeploy, debug)
            {
            }

            internal override bool CanDump => true;

            protected internal override MerchFlags NextHigher => MerchFlags.Rebalance;

            protected internal override MerchFlags NextLower => MerchFlags.Thin;
        }

        private sealed class ThinSegment : MerchSegment
        {
            internal ThinSegment(bool debug)
                : base(MerchFlags.Thin, debug)
            {
            }

            internal override bool CanDump => true;

            protected internal override MerchFlags NextHigher => MerchFlags.ThinRedeploy;
        }
    }
}