using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System.Collections.Generic;

namespace Redbox.KioskEngine.Environment
{
  public class TweenService : ITweenService
  {
    private IDictionary<string, Tween> m_tweens;
    private readonly object m_syncObject = new object();

    public static TweenService Instance => Singleton<TweenService>.Instance;

    public void Reset()
    {
      lock (this.m_syncObject)
        this.Tweens.Clear();
    }

    public void CreateTween(
      string name,
      TweenType type,
      float begin,
      float finish,
      float duration,
      TweenChange changeHandler,
      TweenEnd endHandler)
    {
      lock (this.m_syncObject)
      {
        if (this.Tweens.ContainsKey(name))
        {
          Tween tween = this.Tweens[name];
          tween.Time = 0.0f;
          tween.Enabled = true;
          tween.Type = type;
          tween.Begin = begin;
          tween.Finish = finish;
          tween.Duration = duration;
          if (endHandler != null)
          {
            tween.ClearEnd();
            tween.End += endHandler;
          }
          if (changeHandler == null)
            return;
          tween.ClearChangeState();
          tween.ChangeState += changeHandler;
        }
        else
        {
          Tween tween = new Tween()
          {
            Time = 0.0f,
            Type = type,
            Name = name,
            Begin = begin,
            Finish = finish,
            Duration = duration
          };
          if (endHandler != null)
            tween.End += endHandler;
          if (changeHandler != null)
            tween.ChangeState += changeHandler;
          this.Tweens[name] = tween;
        }
      }
    }

    public void StopTween(string name)
    {
      lock (this.m_syncObject)
      {
        if (!this.Tweens.ContainsKey(name))
          return;
        this.Tweens[name].Enabled = false;
      }
    }

    public void StartTween(string name)
    {
      lock (this.m_syncObject)
      {
        if (!this.Tweens.ContainsKey(name))
          return;
        this.Tweens[name].Enabled = true;
      }
    }

    public void RemoveTween(string name)
    {
      lock (this.m_syncObject)
      {
        if (!this.Tweens.ContainsKey(name))
          return;
        this.Tweens[name].ClearChangeState();
        this.Tweens[name].ClearEnd();
        this.Tweens.Remove(name);
      }
    }

    public void UpdateTweens()
    {
      lock (this.m_syncObject)
      {
        List<Tween> tweenList = new List<Tween>((IEnumerable<Tween>) this.Tweens.Values);
        for (int index = 0; index < tweenList.Count && index < tweenList.Count; ++index)
        {
          if (!tweenList[index].Update())
          {
            tweenList[index].RaiseEnd();
            tweenList[index].Enabled = false;
          }
        }
      }
    }

    internal IDictionary<string, Tween> Tweens
    {
      get
      {
        if (this.m_tweens == null)
          this.m_tweens = (IDictionary<string, Tween>) new Dictionary<string, Tween>();
        return this.m_tweens;
      }
    }

    private TweenService()
    {
    }
  }
}
