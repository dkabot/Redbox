using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Redbox.KioskEngine.Environment
{
  public class WPFViewFrame : BaseViewFrame, IWPFViewFrame, IBaseViewFrame
  {
    public override void UpdateScene(IViewFrameInstance viewFrameInstance)
    {
      try
      {
        this.Scene.SuspendDrawing();
        this.Scene.SceneRenderType = RenderType.WPF;
        bool flag = true;
        if (this.Clear)
          this.Scene.Clear();
        else if (this.Scene.GetWPFGridChild((object) viewFrameInstance.Id) != null)
          flag = false;
        if (flag)
        {
          FrameworkElement controlInstance = this.CreateControlInstance();
          controlInstance.Tag = (object) viewFrameInstance?.Id;
          this.Scene.AddWPFGridChild(controlInstance);
        }
        this.Scene.WPFGrid?.Focus();
        this.Scene.ResumeDrawing();
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was raised in WPFViewFrame.UpdateScene.", ex);
      }
    }

    private FrameworkElement CreateControlInstance()
    {
      return ServiceLocator.Instance.GetService<IThemeService>()?.GetNewThemedControlInstance(this.ViewDefinition?.ControlName);
    }

    public override bool RaiseOnEnter(IViewFrameInstance viewFrameInstance)
    {
      bool flag = false;
      if (this.ViewDefinition.OnEnter != null)
      {
        flag = true;
        Action<IViewFrameInstance> onEnter = this.ViewDefinition.OnEnter;
        if (onEnter != null)
          onEnter(viewFrameInstance);
      }
      return flag;
    }

    public override void RaiseOnLeave(IViewFrameInstance viewFrameInstance)
    {
      Action<IViewFrameInstance> onLeave = this.ViewDefinition.OnLeave;
      if (onLeave == null)
        return;
      onLeave(viewFrameInstance);
    }

    private IViewDefinition ViewDefinition { get; set; }

    public static void CacheWPFViewFrames(string assemblyName)
    {
      IViewService service1 = ServiceLocator.Instance.GetService<IViewService>();
      if (service1 == null)
        return;
      string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), assemblyName);
      if (File.Exists(path))
      {
        Assembly assembly = Assembly.LoadFile(path);
        if (assembly != (Assembly) null)
        {
          foreach (Type type in ((IEnumerable<Type>) assembly.GetTypes()).Where<Type>((Func<Type, bool>) (x => x.IsClass && x.IsDefined(typeof (ViewDefinitionAttribute)))))
          {
            if (Activator.CreateInstance(type) is IViewDefinition instance)
            {
              IRenderingService service2 = ServiceLocator.Instance.GetService<IRenderingService>();
              WPFViewFrame wpfViewFrame1 = new WPFViewFrame();
              wpfViewFrame1.ViewName = instance.ViewName;
              wpfViewFrame1.ActiveFlag = instance.ActiveFlag.ToString();
              wpfViewFrame1.IsStartOverView = instance.IsStartOverView;
              wpfViewFrame1.IsMaintenanceModeView = instance.IsMaintenanceModeView;
              wpfViewFrame1.Clear = instance.Clear;
              wpfViewFrame1.ViewDefinition = instance;
              wpfViewFrame1.Scene = service2?.GetScene("Default");
              IWPFViewFrame wpfViewFrame2 = (IWPFViewFrame) wpfViewFrame1;
              service1.CacheViewFrame((IBaseViewFrame) wpfViewFrame2);
            }
          }
        }
        else
          LogHelper.Instance.Log("Unable to load assembly {0}", (object) path);
      }
      else
        LogHelper.Instance.Log("Unable to find assembly {0}", (object) path);
    }
  }
}
