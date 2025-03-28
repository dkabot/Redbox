using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment
{
  public class DebugService : IDebugService
  {
    private bool m_isApplicationRunning;

    public static DebugService Instance => Singleton<DebugService>.Instance;

    public ErrorList Initialize()
    {
      ErrorList errorList = new ErrorList();
      ServiceLocator.Instance.AddService(typeof (IDebugService), (object) DebugService.Instance);
      return errorList;
    }

    public void ActivateDebugger()
    {
      if (!this.IsEnabled)
        return;
      ServiceLocator.Instance.GetService<IResourceBundleService>();
      ServiceLocator.Instance.GetService<IIdeService>().ActivateDebugger((string) null, 1, (string) null);
    }

    public void ActivateDebugger(string resourceName)
    {
      if (!this.IsEnabled)
        return;
      ServiceLocator.Instance.GetService<IResourceBundleService>();
      ServiceLocator.Instance.GetService<IIdeService>().ActivateDebugger(resourceName, 1, (string) null);
    }

    public void ActivateDebugger(string resourceName, int lineNumber, string error)
    {
      if (!this.IsEnabled)
        return;
      ServiceLocator.Instance.GetService<IResourceBundleService>();
      ServiceLocator.Instance.GetService<IIdeService>().ActivateDebugger(resourceName, lineNumber, error);
    }

    public void SetDebuggerInstance(object instance)
    {
      IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
      this.DebuggerInstance = instance;
      int num = this.IsEnabled ? 1 : 0;
      service.SetDebuggingState(num != 0);
    }

    public bool IsApplicationRunning
    {
      get => this.m_isApplicationRunning;
      set => this.m_isApplicationRunning = value;
    }

    public object DebuggerInstance { get; private set; }

    public bool IsEnabled
    {
      get
      {
        IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
        return service != null && service.GetValue<bool>("Core", "DebuggerEnabled", false);
      }
      set
      {
        ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.SetValue<bool>("Core", "DebuggerEnabled", value);
      }
    }

    private DebugService()
    {
      ServiceLocator.Instance.GetService<IKernelService>().DebuggerStopping += (DebuggerStopping) ((source, lineNumber, action) =>
      {
        if (lineNumber == -1 || !(action == "Run"))
          return;
        this.ActivateDebugger(source, lineNumber, "Breakpoint hit");
      });
    }
  }
}
