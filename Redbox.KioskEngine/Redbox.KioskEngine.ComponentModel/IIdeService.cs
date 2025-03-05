using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace Redbox.KioskEngine.ComponentModel
{
  public interface IIdeService
  {
    ErrorList OpenProject(string path);

    ErrorList CloseProject();

    ErrorList Build();

    ErrorList Build(string path);

    Form MainForm { get; }

    void Show();

    void Close();

    IProject CurrentProject { get; }

    ReadOnlyCollection<string> RecentProjects { get; }

    void ActivateDebugger(string resourceName, int lineNumber, string error);

    void SetDebuggerInstance(object instance);
  }
}
