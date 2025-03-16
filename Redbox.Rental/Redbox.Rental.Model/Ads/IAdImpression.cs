using System;

namespace Redbox.Rental.Model.Ads
{
    public interface IAdImpression
    {
        IAd Ad { get; set; }

        DateTime? StartDisplayTime { get; }

        void Start();

        DateTime? EndDisplayTime { get; }

        void End();

        bool HasEnded { get; }

        bool IsDisplayTimeLongEnough { get; }

        int DisplayMilliseconds { get; }

        AdLocation AdLocation { get; set; }
    }
}