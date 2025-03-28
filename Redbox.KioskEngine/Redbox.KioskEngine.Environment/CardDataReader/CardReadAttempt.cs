using DeviceService.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.ComponentModel.TrackData;
using System;

namespace Redbox.KioskEngine.Environment.CardDataReader
{
  public class CardReadAttempt : ICardReadAttempt, ITechnicalFallbackTracking
  {
    private ITrackData _trackData;

    public Guid CardReadJobCommandRequestId { get; set; }

    public DateTime? StartDateTime { get; set; }

    public DateTime? EndDateTime { get; set; }

    public DeviceInputType DeviceInputType { get; set; }

    public TimeSpan Duration
    {
      get
      {
        TimeSpan duration = TimeSpan.FromSeconds(0.0);
        if (this.StartDateTime.HasValue && this.EndDateTime.HasValue)
          duration = this.StartDateTime.Value - this.EndDateTime.Value;
        return duration;
      }
    }

    public ITrackData TrackData
    {
      get => this._trackData;
      set
      {
        this._trackData = value;
        if (this._trackData == null)
          return;
        this.SupportsTechnicalFallbackTracking = this._trackData.CardSourceType.IsEmvContactlessOrQuickChip();
        this.IsInTrackableTechnicalFallback = this._trackData.NextCardReadIsInTechnicalFallback;
      }
    }

    public CardReadAttemptStatus CardReadAttemptStatus { get; set; }

    public bool AttemptedCancel { get; set; }

    public IAuthorizeResult AuthorizeResult { get; set; }

    public bool SupportsTechnicalFallbackTracking { get; private set; }

    public bool IsInTrackableTechnicalFallback { get; private set; }

    public CancelReason CancelReason { get; set; }
  }
}
