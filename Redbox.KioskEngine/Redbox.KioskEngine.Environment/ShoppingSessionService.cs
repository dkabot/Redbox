using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace Redbox.KioskEngine.Environment
{
  public class ShoppingSessionService : IShoppingSessionService, IDisposable
  {
    private const string LegacyActiveSessionId = "__active__";
    private string m_currentSessionId;
    private readonly object m_syncObject = new object();
    private readonly IDictionary<string, IShoppingSession> m_sessions = (IDictionary<string, IShoppingSession>) new Dictionary<string, IShoppingSession>();
    private Queue<IShoppingSession> _sessionQueue = new Queue<IShoppingSession>(5);

    public static ShoppingSessionService Instance => Singleton<ShoppingSessionService>.Instance;

    public void Reset()
    {
      LogHelper.Instance.Log("Reset Shopping Session Service.");
      this.AbandonAll((string) null);
    }

    public void Dispose()
    {
      lock (this.m_syncObject)
        this.Reset();
    }

    public void AbandonAll(string reason)
    {
      LogHelper.Instance.Log("Adbandon all shopping sessions.");
      lock (this.m_syncObject)
      {
        foreach (IShoppingSession shoppingSession in (IEnumerable<IShoppingSession>) this.m_sessions.Values)
        {
          LogHelper.Instance.Log("...Marking session '{0}' abandoned and saving.", (object) shoppingSession.Id);
          shoppingSession.Abandon(reason);
          shoppingSession.End();
        }
        this.m_sessions.Clear();
      }
    }

    public IShoppingSession GetSession(string id)
    {
      lock (this.m_syncObject)
      {
        if (string.IsNullOrEmpty(id))
        {
          LogHelper.Instance.Log("The ID passed to GetSession is null or empty, returning null.");
          return (IShoppingSession) null;
        }
        if (this.m_sessions.ContainsKey(id))
          return this.m_sessions[id];
        LogHelper.Instance.Log("Shopping Session '{0}' produced a cache miss, reloading from database.", (object) id);
        IShoppingSession session = this.Load(id);
        if (session != null)
        {
          LogHelper.Instance.Log("...Session found, placing in cache.");
          this.m_sessions[id] = session;
          return session;
        }
        LogHelper.Instance.Log("Shopping Session '{0}' was not found.", (object) id);
        return (IShoppingSession) null;
      }
    }

    public IShoppingSession GetCurrentSession()
    {
      lock (this.m_syncObject)
      {
        if (!this.m_sessions.ContainsKey("__active__"))
          return this.GetSession(this.m_currentSessionId);
        LogHelper.Instance.Log("The Shopping Session cache contains the legacy session ID, retrieiving it.");
        return this.m_sessions["__active__"];
      }
    }

    public string StartNewSession(string storeNumber, string sessionId)
    {
      lock (this.m_syncObject)
      {
        if (this.m_sessions.ContainsKey("__active__"))
        {
          LogHelper.Instance.Log("The Shopping Session cache contains the Legacy ID.");
          IShoppingSession session = this.m_sessions["__active__"];
        }
        if (this.m_sessions.Count > 0)
        {
          this.m_sessions.ForEach<KeyValuePair<string, IShoppingSession>>((Action<KeyValuePair<string, IShoppingSession>>) (s =>
          {
            if (this._sessionQueue.Contains(s.Value))
              return;
            this._sessionQueue.Enqueue(s.Value);
          }));
          while (this._sessionQueue.Count > 25)
            this._sessionQueue.Dequeue();
        }
        this.m_sessions.Clear();
        ShoppingSession shoppingSession = new ShoppingSession(!string.IsNullOrEmpty(sessionId) ? new Guid(sessionId) : Guid.NewGuid(), true)
        {
          StartedOn = DateTime.Now,
          StoreNumber = storeNumber
        };
        shoppingSession.ShoppingCart = (IShoppingCart) new ShoppingCart(shoppingSession.Id)
        {
          Type = ShoppingCartType.Online
        };
        string key = shoppingSession.Id.ToString();
        this.m_sessions[key] = (IShoppingSession) shoppingSession;
        LogHelper.Instance.Log("Shopping Session '{0}' added to cache and started.", (object) key);
        return key;
      }
    }

    public ErrorList Initialize(string path)
    {
      ServiceLocator.Instance.AddService(typeof (IShoppingSessionService), (object) ShoppingSessionService.Instance);
      ErrorList errorList = new ErrorList();
      try
      {
        if (File.Exists(path))
          File.Delete(path);
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was raised in ShoppingSessionServie.Initialize when trying to delete session.data.", ex);
      }
      return errorList;
    }

    public void SetCurrentSession(string id)
    {
      LogHelper.Instance.Log("Setting current Shopping Session to: {0}", (object) id);
      this.m_currentSessionId = id;
    }

    public bool StartUserInteraction()
    {
      lock (this.m_syncObject)
      {
        IShoppingSession currentSession = this.GetCurrentSession();
        return currentSession != null && currentSession.StartUserInteraction();
      }
    }

    private ShoppingSessionService()
    {
    }

    private IShoppingSession Load(string id)
    {
      IShoppingSession found = (IShoppingSession) null;
      this._sessionQueue.ForEach<IShoppingSession>((Action<IShoppingSession>) (item =>
      {
        if (found != null || !item.Id.Equals(new Guid(id)))
          return;
        found = item;
      }));
      return found;
    }
  }
}
