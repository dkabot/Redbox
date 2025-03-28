using DeviceService.ComponentModel.Commands;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;
using System;
using System.Threading;

namespace Redbox.KioskEngine.Environment
{
  internal class ReadCardJob : IReadCardJob
  {
    private Guid _requestId;

    public CardReadRequest CardReadRequest { get; private set; }

    public Action<BaseResponseEvent> ReadCardResponseHandler { get; private set; }

    public Action<BaseResponseEvent> ReadCardEventHandler { get; private set; }

    public int CommandTimeout { get; private set; }

    public bool IsCardInserted { get; set; }

    public bool IsExecuting { get; private set; }

    public ReadCardJob(
      CardReadRequest cardReadRequest,
      Action<BaseResponseEvent> readCardResponseHandler,
      Action<BaseResponseEvent> readCardEventHandler,
      int commandTimeout)
    {
      this.CardReadRequest = cardReadRequest;
      this.ReadCardResponseHandler = readCardResponseHandler;
      this.ReadCardEventHandler = readCardEventHandler;
      this.CommandTimeout = commandTimeout;
      this._requestId = Guid.NewGuid();
      this.IsExecuting = false;
    }

    public Guid RequestId => this._requestId;

    public bool Cancel(Action<BaseResponseEvent> cancelCompleteCallback)
    {
      if (this.IsExecuting)
      {
        this.IsExecuting = false;
        CancelCommandResponseEvent commandResponseEvent1 = new CancelCommandResponseEvent((BaseCommandRequest) null);
        commandResponseEvent1.RequestId = this._requestId;
        commandResponseEvent1.Success = true;
        commandResponseEvent1.EventName = nameof (Cancel);
        CancelCommandResponseEvent commandResponseEvent2 = commandResponseEvent1;
        Thread.Sleep(1000);
        this.ReadCardResponseHandler((BaseResponseEvent) commandResponseEvent2);
        cancelCompleteCallback((BaseResponseEvent) commandResponseEvent2);
      }
      return true;
    }

    public bool Execute()
    {
      this.IsExecuting = true;
      return true;
    }

    public void Stop() => this.IsExecuting = false;
  }
}
