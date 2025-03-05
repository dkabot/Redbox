namespace Redbox.KioskEngine.ComponentModel
{
  public interface IDebugService
  {
    void SetDebuggerInstance(object instance);

    void ActivateDebugger();

    void ActivateDebugger(string resourceName);

    void ActivateDebugger(string resourceName, int lineNumber, string error);

    bool IsApplicationRunning { get; set; }

    object DebuggerInstance { get; }

    bool IsEnabled { get; set; }
  }
}
