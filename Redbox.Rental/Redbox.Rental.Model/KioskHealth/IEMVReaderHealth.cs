namespace Redbox.Rental.Model.KioskHealth
{
    public interface IEMVReaderHealth
    {
        void SessionWasInTechnicalFallback(bool inFallback);

        void TrackCardReaderDisconnects();

        bool ContactlessOffDueToDisconnectThresholdReached { get; }
    }
}