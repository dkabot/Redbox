using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Commands;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;
using System;

namespace Redbox.KioskEngine.ComponentModel
{
  public interface IDeviceServiceClientHelper
  {
    bool ConnectToDeviceService();

    bool IsConnectedToDeviceService { get; }

    bool IsCardReaderConnected { get; }

    bool IsCardReaderTampered { get; set; }

    ValidateVersionModel ValidateVersion();

    Version DeviceServiceClientVersion { get; }

    IReadCardJob StartCardRead(
      DeviceInputType deviceInputType,
      Action<BaseResponseEvent> readCardResponseHandler,
      Action<BaseResponseEvent> readCardEventHandler,
      int timeout,
      VasMode vasMode);

    void CancelCommand(Guid id);

    void ReportAuthorizeResult(bool authorizeSuccessful);

    void PollForCardReaderConnectionAndValidVersion();

    void CheckDeviceActivation();

    Version DeviceServiceVersion { get; }

    void GetCardInsertedStatus(
      Action<GetCardInsertedStatusResponseEvent> completeCallback);

    void RegisterOnDeviceServiceCanShutDownHandler(
      OnDeviceServiceCanShutDown onDeviceServiceCanShutDown);

    IDeviceServiceShutDownInfo DeviceServiceShutDownInfo { get; }

    bool ShutDownDeviceService(bool forceShutdown, ShutDownReason shutDownReason);

    void CheckDeviceStatus();

    bool SupportsEMV { get; }

    bool SupportsVas { get; }

    void SendTamperedCardReaderKioskAlertMessage();
  }
}
