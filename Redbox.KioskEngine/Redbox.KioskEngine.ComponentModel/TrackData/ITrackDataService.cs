using DeviceService.ComponentModel.Commands;
using System;

namespace Redbox.KioskEngine.ComponentModel.TrackData
{
  public interface ITrackDataService
  {
    void StartCardRead(
      CardReadRequestInfo request,
      Action<IReadCardJob> OnReadJobStart,
      Action<Guid> OnCardProcessing,
      Action<CardReadRequestInfo, CardReadCompleteDetails> onComplete);

    void StopCardRead(CancelReason cancelReason);

    void StopAllSessionCardReads(Guid sessionId, CancelReason cancelReason);

    bool IsOlderReader { get; }

    bool IsNewReader { get; }
  }
}
