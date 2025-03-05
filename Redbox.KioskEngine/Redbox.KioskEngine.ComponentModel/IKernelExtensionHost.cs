using System;

namespace Redbox.KioskEngine.ComponentModel
{
  public interface IKernelExtensionHost : IDisposable
  {
    ErrorList PreLoad();

    ErrorList Load();

    ErrorList Reset();

    ErrorList Activate();

    ErrorList Deactivate();

    void HandleHostCrash(Exception e);

    bool CanSwitch(out ErrorList errors);

    IKernelExtension Extension { get; set; }

    void RegisterCommunicationActivity(Action<object, EventArgs> action);
  }
}
