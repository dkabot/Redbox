using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;
using System.Collections.Generic;

namespace Redbox.KioskEngine.Environment
{
  internal sealed class ShoppingSession : IShoppingSession
  {
    private bool _startedUserInteraction;
    private readonly List<IShoppingSessionError> m_errors = new List<IShoppingSessionError>();
    private readonly List<IShoppingSessionEvent> m_events = new List<IShoppingSessionEvent>();

    public ShoppingSession(Guid id, bool logStartEvent)
    {
      this.Id = id;
      if (!logStartEvent)
        return;
      this.AddEvent(ShoppingSessionEventType.Start, "Session has started.");
    }

    public void AddEvent(ShoppingSessionEventType type, string description)
    {
      if (this.EndedOn.HasValue)
        return;
      LogHelper.Instance.Log("Added event to Shopping Session '{0}': type={1}, description={2}", (object) this.Id, (object) type, (object) description);
      this.m_events.Add((IShoppingSessionEvent) new ShoppingSessionEvent()
      {
        Type = type,
        TimeStamp = DateTime.Now,
        Description = description
      });
    }

    public void End()
    {
      LogHelper.Instance.Log("Ending Shopping Session.");
      if (this.EndedOn.HasValue)
      {
        LogHelper.Instance.Log("...Session already ended, returning.");
      }
      else
      {
        this.AddEvent(ShoppingSessionEventType.End, "Session has ended.");
        this.EndedOn = new DateTime?(DateTime.Now);
        LogHelper.Instance.Log("...Session EndedOn set to: {0}", (object) this.EndedOn);
      }
    }

    public void Commit(string reason)
    {
      LogHelper.Instance.Log("Commit Shopping Session '{0}'", (object) this.Id);
      if (this.EndedOn.HasValue)
      {
        LogHelper.Instance.Log("...Shopping Session has already ended; skipping commit.");
      }
      else
      {
        this.AddEvent(ShoppingSessionEventType.Commit, !string.IsNullOrEmpty(reason) ? reason : "Session comitted.");
        this.Status = ShoppingSessionStatus.Committed;
        LogHelper.Instance.Log("...Commit complete.");
      }
    }

    public void Abandon(string reason)
    {
      LogHelper.Instance.Log("Abandon Shopping Session '{0}'", (object) this.Id);
      if (this.EndedOn.HasValue)
      {
        LogHelper.Instance.Log("...Shopping Session has already ended; skipping abandon.");
      }
      else
      {
        this.AddEvent(ShoppingSessionEventType.Abandon, !string.IsNullOrEmpty(reason) ? reason : "Session abandon.");
        this.Status = ShoppingSessionStatus.Abandon;
        LogHelper.Instance.Log("...Abandon complete.");
      }
    }

    public bool StartUserInteraction()
    {
      if (this._startedUserInteraction)
        return false;
      this._startedUserInteraction = true;
      return true;
    }

    public Guid Id { get; internal set; }

    public DateTime? EndedOn { get; internal set; }

    public string StoreNumber { get; internal set; }

    public int ViewsShown { get; set; }

    public DateTime StartedOn { get; internal set; }

    public IShoppingCart ShoppingCart { get; internal set; }

    public ShoppingSessionStatus Status { get; internal set; }

    internal List<IShoppingSessionEvent> Events => this.m_events;

    internal List<IShoppingSessionError> Errors => this.m_errors;
  }
}
