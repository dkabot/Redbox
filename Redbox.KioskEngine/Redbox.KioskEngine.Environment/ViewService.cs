using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.KioskHealth;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Redbox.KioskEngine.Environment
{
  public class ViewService : IViewService
  {
    private string _startOverViewName;
    private string _maintenanceViewName;
    private string m_lastShownView;
    private DateTime? m_viewShownOn;
    private readonly IDictionary<string, IBaseViewFrame> m_viewFrameCache = (IDictionary<string, IBaseViewFrame>) new Dictionary<string, IBaseViewFrame>();
    private bool m_isCriticalErrorViewShown;
    private bool m_isCriticalErrorViewOnStack;

    public static ViewService Instance => Singleton<ViewService>.Instance;

    public void Reset()
    {
      this.InnerStack.Clear();
      this.ClearCache(true);
      this.m_isCriticalErrorViewShown = false;
      this.m_isCriticalErrorViewOnStack = false;
      this.RaiseViewStateChanged(new ViewStateChangedArgs()
      {
        Type = ViewStateChangedType.Reset
      });
    }

    public TaskScheduler UITaskScheduler { get; set; }

    public void ShowFrameInDebug()
    {
      ServiceLocator.Instance.GetService<IRenderingService>();
      IViewFrameInstance viewFrameInstance = this.InnerStack.Peek();
      this.LogShowViewFrameInstance(viewFrameInstance);
      viewFrameInstance.ViewFrame.UpdateScene(viewFrameInstance);
      if (!viewFrameInstance.ViewFrame.Errors.ContainsError())
        return;
      ErrorForm errorForm = new ErrorForm();
      errorForm.Errors = (IEnumerable) viewFrameInstance?.ViewFrame?.Errors;
      errorForm.Text = string.Format("Errors for view resource: {0}", (object) viewFrameInstance?.ViewFrame?.ViewName);
      errorForm.Show();
      errorForm.BringToFront();
    }

    private void LogShowViewFrameInstance(IViewFrameInstance viewFrameInstance)
    {
      if (viewFrameInstance?.ViewFrame is IViewFrame viewFrame1)
      {
        LogHelper.Instance.Log("Showing view '{0}' from TOS: clear = {1}, viewWindow = {2}, width = {3}, height = {4}, scene = {5}, onEnter = {6}, onLeave = {7}.", (object) viewFrame1.ViewName, (object) viewFrame1.Clear, (object) viewFrame1.ViewWindow, (object) viewFrame1.Width, (object) viewFrame1.Height, (object) viewFrame1.SceneName, (object) viewFrame1.OnEnterResourceName, (object) viewFrame1.OnLeaveResourceName);
      }
      else
      {
        if (!(viewFrameInstance?.ViewFrame is IWPFViewFrame viewFrame))
          return;
        LogHelper.Instance.Log("Showing view '{0}' from TOS", (object) viewFrame.ViewName);
      }
    }

    public void Show()
    {
      IApplicationState service1 = ServiceLocator.Instance.GetService<IApplicationState>();
      if ((service1 != null ? (service1.IsInMaintenanceMode ? 1 : 0) : 0) != 0 || !this.m_isCriticalErrorViewShown)
      {
        IResourceBundleService service2 = ServiceLocator.Instance.GetService<IResourceBundleService>();
        IShoppingSession shoppingSession = (IShoppingSession) null;
        IShoppingSessionService service3 = ServiceLocator.Instance.GetService<IShoppingSessionService>();
        if (service3 != null)
        {
          shoppingSession = service3.GetCurrentSession();
          if (shoppingSession != null)
            ++shoppingSession.ViewsShown;
        }
        if (shoppingSession != null && this.m_viewShownOn.HasValue)
          this.m_lastShownView = (string) null;
        IRenderingService service4 = ServiceLocator.Instance.GetService<IRenderingService>();
        if (this.InnerStack.Peek() == null)
        {
          LogHelper.Instance.Log("View stack is empty.");
          service2.Filter["view_name"] = (string) null;
          service4.ActiveScene.SuspendDrawing();
          service4.ActiveScene.Clear();
          service4.ActiveScene.ResumeDrawing();
          shoppingSession?.AddEvent(ShoppingSessionEventType.ShowView, "View stack empty.");
        }
        else
        {
          IViewFrameInstance viewFrameInstance = this.InnerStack.Peek();
          service2.Filter["view_name"] = viewFrameInstance?.ViewFrame?.ViewName;
          this.LogShowViewFrameInstance(viewFrameInstance);
          viewFrameInstance.ViewFrame.UpdateScene(viewFrameInstance);
          if (shoppingSession != null)
            ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddView(this.GetCurrentViewAnalyticsName());
          if (!viewFrameInstance.ViewFrame.RaiseOnEnter(viewFrameInstance))
            LogHelper.Instance.Log("No on enter script specified for view named: {0}", (object) viewFrameInstance.ViewFrame.ViewName);
          this.m_viewShownOn = new DateTime?(DateTime.Now);
          this.m_lastShownView = viewFrameInstance.ViewFrame.ViewName;
          this.TTSRunSpeech(viewFrameInstance);
          ServiceLocator.Instance.GetService<IViewHealth>()?.PostActivity("ShowView", sessionId: shoppingSession?.Id.ToString());
          this.ShowErrors(viewFrameInstance);
          if (!this.m_isCriticalErrorViewOnStack)
            return;
          this.m_isCriticalErrorViewShown = true;
        }
      }
      else
        LogHelper.Instance.Log("View not shown due to critical error mode.");
    }

    public void SetAnalyticsEventProperty(string name, object value)
    {
      ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddEventProperty(name, value);
    }

    private void TTSRunSpeech(IViewFrameInstance viewFrameInstance)
    {
      ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
      if (service == null || !service.TTSEnabled)
        return;
      service.RunSpeechWorkflow("tts_" + viewFrameInstance.ViewFrame.ViewName);
    }

    private void ShowErrors(IViewFrameInstance viewFrameInstance)
    {
      if (!viewFrameInstance.ViewFrame.Errors.ContainsError())
        return;
      ErrorForm errorForm = new ErrorForm();
      errorForm.Errors = (IEnumerable) viewFrameInstance.ViewFrame.Errors;
      errorForm.Text = string.Format("Errors for view resource: {0}", (object) viewFrameInstance.ViewFrame.ViewName);
      errorForm.Show();
      errorForm.BringToFront();
      this.RaiseViewStateChanged(new ViewStateChangedArgs()
      {
        Type = ViewStateChangedType.ShowErrors
      });
    }

    public string Pop()
    {
      this.m_isCriticalErrorViewOnStack = false;
      this.m_isCriticalErrorViewShown = false;
      if (this.InnerStack.Peek() == null)
      {
        LogHelper.Instance.Log("View stack is empty; nothing to pop.");
        return (string) null;
      }
      IViewFrameInstance viewFrameInstance1 = this.InnerStack.Pop();
      if (viewFrameInstance1.ViewFrame is IViewFrame viewFrame1)
        LogHelper.Instance.Log("Pop view '{0}' from TOS; executing onLeave = {1}", (object) viewFrameInstance1.ViewFrame.ViewName, (object) viewFrame1.OnLeaveResourceName);
      viewFrameInstance1.ViewFrame.RaiseOnLeave(viewFrameInstance1);
      LogHelper.Instance.Log(this.ViewStackContentsToString());
      IViewFrameInstance viewFrameInstance2 = this.InnerStack.Peek();
      if (viewFrameInstance2?.ViewFrame is IViewFrame viewFrame2 && !viewFrame2.Clear)
      {
        string viewName = this.Pop();
        LogHelper.Instance.Log("Redraw view '{0}' down the stack for transparent TOS view.", (object) viewName);
        this.Show();
        this.Push(viewName);
        this.Show();
      }
      if (viewFrameInstance2?.ViewFrame is IWPFViewFrame viewFrame5 && !viewFrame5.Clear)
      {
        IBaseViewFrame viewFrame3 = viewFrameInstance1.ViewFrame;
        if (viewFrame3 != null)
        {
          string name = viewFrame3 is IViewFrame viewFrame4 ? viewFrame4.Scene?.Actors?.First?.Value?.Name : (string) null;
          object tag = viewFrame3 is IWPFViewFrame ? (object) viewFrameInstance1.Id : (object) name;
          FrameworkElement wpfGridChild = viewFrame3.Scene.GetWPFGridChild(tag);
          if (wpfGridChild != null)
            viewFrame3.Scene.RemoveWPFGridChild(wpfGridChild);
        }
      }
      this.RaiseViewStateChanged(new ViewStateChangedArgs()
      {
        Type = ViewStateChangedType.Pop,
        FromView = viewFrameInstance1.ViewFrame.ViewName,
        ToView = this.InnerStack.Peek()?.ViewFrame?.ViewName
      });
      return viewFrameInstance1.ViewFrame.ViewName;
    }

    public string Peek() => this.InnerStack.PeekViewName();

    public string Peek(int index) => this.PeekViewFrame(index)?.ViewFrame?.ViewName;

    public int ViewStackCount => this.InnerStack.Count;

    public void ClearCache(bool clearResourceViewsOnly)
    {
      LogHelper.Instance.Log("Clearing ViewFrame cache. ResourceViewsOnly: {0}", (object) clearResourceViewsOnly);
      if (!clearResourceViewsOnly)
      {
        this.m_viewFrameCache.Clear();
      }
      else
      {
        List<string> stringList = new List<string>();
        foreach (KeyValuePair<string, IBaseViewFrame> keyValuePair in (IEnumerable<KeyValuePair<string, IBaseViewFrame>>) this.m_viewFrameCache)
        {
          if (keyValuePair.Value is IViewFrame)
            stringList.Add(keyValuePair.Key);
        }
        foreach (string key in stringList)
        {
          if (this.m_viewFrameCache.ContainsKey(key))
            this.m_viewFrameCache.Remove(key);
        }
      }
      this.RaiseViewStateChanged(new ViewStateChangedArgs()
      {
        Type = ViewStateChangedType.ClearCache
      });
    }

    public string PopDiscard()
    {
      this.m_isCriticalErrorViewOnStack = false;
      this.m_isCriticalErrorViewShown = false;
      IViewFrameInstance viewFrameInstance = this.InnerStack.Pop();
      if (viewFrameInstance?.ViewFrame == null)
        return (string) null;
      this.RaiseViewStateChanged(new ViewStateChangedArgs()
      {
        Type = ViewStateChangedType.PopDiscard
      });
      viewFrameInstance.ViewFrame.RaiseOnLeave(viewFrameInstance);
      return viewFrameInstance.ViewFrame.ViewName;
    }

    public void EnterTopOfStock()
    {
      IViewFrameInstance viewFrameInstance = this.InnerStack.Peek();
      if (viewFrameInstance == null)
        return;
      if (viewFrameInstance.ViewFrame is IViewFrame viewFrame)
        viewFrame.Scene.MakeDirty((Rectangle[]) null);
      viewFrameInstance.ViewFrame.RaiseOnEnter(viewFrameInstance);
      this.RaiseViewStateChanged(new ViewStateChangedArgs()
      {
        Type = ViewStateChangedType.EnterTopOfStock
      });
    }

    public string GetCurrentViewAnalyticsName()
    {
      IViewFrameInstance viewFrameInstance = this.InnerStack.Peek();
      string viewAnalyticsName = viewFrameInstance?.Parameter is IViewAnalyticsName parameter ? parameter.ViewAnalyticsName : (string) null;
      if (viewAnalyticsName != null)
        return viewAnalyticsName;
      return viewFrameInstance?.ViewFrame?.ViewName;
    }

    public string GetLastShownView() => this.m_lastShownView;

    public DateTime? GetViewShownTime() => this.m_viewShownOn;

    public void ResetViewShownTime() => this.m_viewShownOn = new DateTime?(DateTime.Now);

    public int GetActiveHitHandlerCount()
    {
      int activeHitHandlerCount = 0;
      IRenderingService service = ServiceLocator.Instance.GetService<IRenderingService>();
      if (service != null)
        activeHitHandlerCount = service.ActiveScene.ActiveHitHandlers;
      return activeHitHandlerCount;
    }

    public void Push(string viewName) => this.Push(viewName, (object) null, false);

    public void PushCriticalErrorView(string viewName, object parameter)
    {
      this.Push(viewName, parameter, true);
    }

    public void Push(string viewName, object parameter, bool isCriticalErrorView = false)
    {
      if (!this.m_isCriticalErrorViewOnStack | isCriticalErrorView || viewName == this.GetMaintenanceViewName())
      {
        this.m_isCriticalErrorViewOnStack = this.m_isCriticalErrorViewShown | isCriticalErrorView;
        IViewFrameInstance viewFrameInstance = this.InnerStack.Peek();
        if (viewFrameInstance?.ViewFrame != null)
          viewFrameInstance.ViewFrame.RaiseOnLeave(viewFrameInstance);
        Guid id = Guid.NewGuid();
        this.InnerStack.Push(id, viewName, parameter);
        LogHelper.Instance.Log("Push view '{0}' {1} to TOS.", (object) viewName, (object) id);
        LogHelper.Instance.Log(this.ViewStackContentsToString());
        this.RaiseViewStateChanged(new ViewStateChangedArgs()
        {
          Type = ViewStateChangedType.Push,
          FromView = viewFrameInstance?.ViewFrame?.ViewName,
          ToView = viewName
        });
      }
      else
        LogHelper.Instance.Log("View " + viewName + " not shown due to critical error view or maintenance mode.");
    }

    public string ViewStackContentsToString()
    {
      StringBuilder stringBuilder = new StringBuilder(string.Format("View Stack Count: {0} Contents: ", (object) this.InnerStack?.Count));
      int num = 0;
      if (this.InnerStack != null)
      {
        foreach (IViewFrameInstance inner in this.InnerStack)
        {
          if (num < 10)
          {
            string str = num > 0 ? ", " : (string) null;
            stringBuilder.Append(str + inner?.ViewFrame?.ViewName);
          }
          else if (num == 10)
          {
            stringBuilder.Append(", ...");
            break;
          }
          ++num;
        }
      }
      return stringBuilder.ToString();
    }

    public IViewFrameInstance PeekViewFrame() => this.InnerStack.Peek();

    public IViewFrameInstance PeekViewFrame(int index)
    {
      IViewFrameInstance viewFrameInstance = (IViewFrameInstance) null;
      int num = 0;
      foreach (IViewFrameInstance inner in this.InnerStack)
      {
        if (index == num)
        {
          viewFrameInstance = inner;
          break;
        }
        ++num;
      }
      return viewFrameInstance;
    }

    public IViewFrameInstance PeekViewFrameByName(string viewName, out int index)
    {
      index = -1;
      IViewFrameInstance viewFrameInstance = (IViewFrameInstance) null;
      int num = 0;
      if (this.ViewStackCount > 0)
      {
        foreach (IViewFrameInstance inner in this.InnerStack)
        {
          if (inner?.ViewFrame?.ViewName == viewName)
          {
            index = num;
            viewFrameInstance = inner;
            break;
          }
          ++num;
        }
      }
      return viewFrameInstance;
    }

    public void CacheViewFrame(IBaseViewFrame baseViewFrame)
    {
      if (baseViewFrame == null || string.IsNullOrEmpty(baseViewFrame.ViewName))
        return;
      this.m_viewFrameCache[baseViewFrame.ViewName] = baseViewFrame;
    }

    public IBaseViewFrame GetViewFrame(string viewName)
    {
      IBaseViewFrame viewFrame1;
      if (this.m_viewFrameCache.ContainsKey(viewName))
      {
        viewFrame1 = this.m_viewFrameCache[viewName];
      }
      else
      {
        LogHelper.Instance.Log("Cache miss, building cached ViewFrame for view named: {0}", (object) viewName);
        ViewFrame viewFrame2 = new ViewFrame();
        viewFrame2.ViewName = viewName;
        viewFrame2.Parse();
        viewFrame1 = (IBaseViewFrame) viewFrame2;
        if (viewFrame1 != null)
          this.m_viewFrameCache[viewName] = viewFrame1;
      }
      return viewFrame1;
    }

    public ReadOnlyCollection<IViewFrameInstance> Stack
    {
      get
      {
        List<IViewFrameInstance> viewFrameInstanceList = new List<IViewFrameInstance>();
        foreach (IViewFrameInstance inner in this.InnerStack)
          viewFrameInstanceList.Add(inner);
        return viewFrameInstanceList.AsReadOnly();
      }
    }

    public string LastHitActorName { get; set; }

    public object LastHitActorTag { get; set; }

    public event EventHandler<ViewStateChangedArgs> ViewStateChanged;

    public bool IsMaintenanceViewShowing()
    {
      return this.PeekViewFrame()?.ViewFrame?.IsMaintenanceModeView ?? false;
    }

    public bool IsSafeToChangeView()
    {
      IApplicationState service = ServiceLocator.Instance.GetService<IApplicationState>();
      return (service != null ? (service.IsInMaintenanceMode ? 1 : 0) : 0) == 0;
    }

    public string GetStartOverViewName()
    {
      if (this._startOverViewName == null && this.m_viewFrameCache != null)
      {
        foreach (KeyValuePair<string, IBaseViewFrame> keyValuePair in (IEnumerable<KeyValuePair<string, IBaseViewFrame>>) this.m_viewFrameCache)
        {
          IBaseViewFrame baseViewFrame = keyValuePair.Value;
          if ((baseViewFrame != null ? (baseViewFrame.IsStartOverView ? 1 : 0) : 0) != 0)
          {
            this._startOverViewName = keyValuePair.Value.ViewName;
            break;
          }
        }
      }
      return this._startOverViewName;
    }

    public string GetMaintenanceViewName()
    {
      if (this._maintenanceViewName == null && this.m_viewFrameCache != null)
      {
        foreach (KeyValuePair<string, IBaseViewFrame> keyValuePair in (IEnumerable<KeyValuePair<string, IBaseViewFrame>>) this.m_viewFrameCache)
        {
          IBaseViewFrame baseViewFrame = keyValuePair.Value;
          if ((baseViewFrame != null ? (baseViewFrame.IsMaintenanceModeView ? 1 : 0) : 0) != 0)
          {
            this._maintenanceViewName = keyValuePair.Value.ViewName;
            break;
          }
        }
      }
      return this._maintenanceViewName;
    }

    public void GoBack(object context, bool popOnly)
    {
      LogHelper.Instance.Log("> view_goback; context: {0}, {1}", context, (object) popOnly);
      if (this.Peek() == this.GetStartOverViewName())
      {
        this.Show();
      }
      else
      {
        this.Pop();
        if (popOnly)
          return;
        this.Show();
      }
    }

    public void GoBackSafe(object context, bool popOnly)
    {
      if (this.IsSafeToChangeView())
        this.GoBack(context, popOnly);
      else
        LogHelper.Instance.Log("view_goback_safe({0}) denied.", context);
    }

    public void UnwindToSafe(string unwindToViewName, bool discard, object context)
    {
      if (this.IsSafeToChangeView())
        this.UnwindTo(unwindToViewName, discard, context);
      else
        LogHelper.Instance.Log("UnwindToSafe( " + context?.ToString() + " ) denied.");
    }

    public void UnwindTo(string unwindToViewName, bool discard, object context)
    {
      string str = this.Peek();
      if (!(str != unwindToViewName) || str == null)
        return;
      for (; str != null && str != unwindToViewName; str = this.Peek())
      {
        if (discard)
        {
          LogHelper.Instance.Log("> PopViewDiscard {0}; context: {1}", (object) str, context);
          this.PopDiscard();
        }
        else
        {
          LogHelper.Instance.Log("> PopView {0}; context: {1}", (object) str, context);
          this.Pop();
        }
      }
      this.Show();
    }

    internal ViewStack InnerStack { get; private set; }

    private ViewService() => this.InnerStack = new ViewStack();

    private void RaiseViewStateChanged(ViewStateChangedArgs e = null)
    {
      if (this.ViewStateChanged == null)
        return;
      this.ViewStateChanged((object) this, e);
    }
  }
}
