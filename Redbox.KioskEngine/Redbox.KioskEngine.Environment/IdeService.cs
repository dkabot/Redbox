using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE;
using Redbox.Lua;
using Redbox.REDS.Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace Redbox.KioskEngine.Environment
{
  public class IdeService : IIdeService
  {
    private IdeMainForm m_ideMainForm;

    public static IdeService Instance => Singleton<IdeService>.Instance;

    public Redbox.KioskEngine.ComponentModel.ErrorList OpenProject(string path) => new Redbox.KioskEngine.ComponentModel.ErrorList();

    public Redbox.KioskEngine.ComponentModel.ErrorList CloseProject() => new Redbox.KioskEngine.ComponentModel.ErrorList();

    public Redbox.KioskEngine.ComponentModel.ErrorList Build() => this.Build((string) null);

    public Redbox.KioskEngine.ComponentModel.ErrorList Build(string path) => new Redbox.KioskEngine.ComponentModel.ErrorList();

    public Form MainForm => (Form) this.m_ideMainForm;

    public void Show()
    {
      if (this.m_ideMainForm != null && this.m_ideMainForm.IsDisposed)
        this.m_ideMainForm = (IdeMainForm) null;
      if (this.m_ideMainForm == null)
      {
        this.m_ideMainForm = new IdeMainForm();
        IdeMainForm.m_instance = this.m_ideMainForm;
        this.m_ideMainForm.Show();
      }
      else
        this.m_ideMainForm.BringToFront();
    }

    public void Close()
    {
      if (this.m_ideMainForm == null)
        return;
      if (this.m_ideMainForm.Visible)
        this.m_ideMainForm.Close();
      this.m_ideMainForm = (IdeMainForm) null;
    }

    public IProject CurrentProject { get; private set; }

    public ReadOnlyCollection<string> RecentProjects
    {
      get
      {
        IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
        return service == null ? new List<string>().AsReadOnly() : new List<string>((IEnumerable<string>) service.GetValue<string[]>("Ide", nameof (RecentProjects), new string[0])).AsReadOnly();
      }
    }

    public void ActivateDebugger(string resourceName, int lineNumber, string error)
    {
      IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
      IResource resource = service.ActiveBundleSet.GetResource(resourceName, service.Filter);
      this.Show();
      ((IdeMainForm) this.MainForm).OpenResourceEditor(resource, lineNumber, error);
    }

    public void SetDebuggerInstance(object instance) => this.Debugger = instance as LuaDebugger;

    public LuaDebugger Debugger { get; set; }

    private IdeService()
    {
    }
  }
}
