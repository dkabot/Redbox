using Redbox.KioskEngine.ComponentModel.TrackData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Redbox.KioskEngine.Environment.CardDataReader
{
  public class CardReadAttemptCollection : 
    List<ICardReadAttempt>,
    ICardReadAttemptCollection,
    IList<ICardReadAttempt>,
    ICollection<ICardReadAttempt>,
    IEnumerable<ICardReadAttempt>,
    IEnumerable,
    ITechnicalFallbackTracking
  {
    public ICardReadAttempt GetMostRecent()
    {
      ICardReadAttempt mostRecent = (ICardReadAttempt) null;
      if (this.Count > 0)
        mostRecent = this.FirstOrDefault<ICardReadAttempt>((Func<ICardReadAttempt, bool>) (x =>
        {
          DateTime? startDateTime = x.StartDateTime;
          DateTime? nullable = this.Max<ICardReadAttempt, DateTime?>((Func<ICardReadAttempt, DateTime?>) (y => y.StartDateTime));
          if (startDateTime.HasValue != nullable.HasValue)
            return false;
          return !startDateTime.HasValue || startDateTime.GetValueOrDefault() == nullable.GetValueOrDefault();
        }));
      return mostRecent;
    }

    public ICardReadAttempt GetByCardReadJobCommandRequestId(Guid cardReadJobCommandRequestId)
    {
      return this.FirstOrDefault<ICardReadAttempt>((Func<ICardReadAttempt, bool>) (x => x.CardReadJobCommandRequestId == cardReadJobCommandRequestId));
    }

    public int CancelledCount
    {
      get
      {
        return this.Count<ICardReadAttempt>((Func<ICardReadAttempt, bool>) (x => x.CardReadAttemptStatus == CardReadAttemptStatus.Cancelled));
      }
    }

    public int TimedOutCount
    {
      get
      {
        return this.Count<ICardReadAttempt>((Func<ICardReadAttempt, bool>) (x => x.CardReadAttemptStatus == CardReadAttemptStatus.TimedOut));
      }
    }

    public int SuccessCount
    {
      get
      {
        return this.Count<ICardReadAttempt>((Func<ICardReadAttempt, bool>) (x => x.CardReadAttemptStatus == CardReadAttemptStatus.Success));
      }
    }

    public int ErrorCount
    {
      get
      {
        return this.Count<ICardReadAttempt>((Func<ICardReadAttempt, bool>) (x => x.CardReadAttemptStatus == CardReadAttemptStatus.Error));
      }
    }

    public bool SupportsTechnicalFallbackTracking
    {
      get
      {
        return this.Any<ICardReadAttempt>((Func<ICardReadAttempt, bool>) (x => x.SupportsTechnicalFallbackTracking));
      }
    }

    public bool IsInTrackableTechnicalFallback
    {
      get
      {
        return this.Any<ICardReadAttempt>((Func<ICardReadAttempt, bool>) (x => x.IsInTrackableTechnicalFallback));
      }
    }
  }
}
