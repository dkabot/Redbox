using Redbox.Core;
using System;

namespace Redbox.KioskEngine.ComponentModel.KioskServices
{
  public sealed class RemoteServiceCallbackEntry : ICallbackEntry
  {
    public RemoteServiceCallback Callback { get; set; }

    public IRemoteServiceResult Result { get; set; }

    public string Message { get; set; }

    public string Name => nameof (RemoteServiceCallbackEntry);

    public void Invoke()
    {
      if (this.Callback == null)
        return;
      try
      {
        if (!string.IsNullOrEmpty(this.Message))
          LogHelper.Instance.Log(this.Message);
        this.Callback(this.Result);
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was raised in SimpleCallback handler.", ex);
      }
    }
  }
}
