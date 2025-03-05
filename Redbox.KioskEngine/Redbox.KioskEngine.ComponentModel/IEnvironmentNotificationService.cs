using Redbox.KioskEngine.ComponentModel.TrackData;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Redbox.KioskEngine.ComponentModel
{
  public interface IEnvironmentNotificationService
  {
    void Reset();

    void Register(IntPtr handle);

    void Unregister(IntPtr handle);

    void ToggleTouchScreen();

    string TouchScreenDriver();

    TouchScreenState TouchScreenStatus();

    void RaiseValidClassicCardReader();

    void RaiseInvalidClassicCardReader();

    void RaiseValidDeviceServiceCardReader();

    void RaiseInvalidDeviceServiceCardReader();

    void LaunchSecureBrowser(
      IDictionary<string, string> approvedUrls,
      NavigateUrlCallback navigateUrlCallback);

    void NotifyConfigurationCallbacks(
      string store,
      string action,
      string path,
      string key,
      string newValue);

    void RegisterConfigurationCallback(string name, ConfigurationCallback callback);

    void UnregisterConfigurationCallback(string name);

    void RegisterCorruptDbCallback(CorruptDbCallback callback);

    void RaiseCorruptDb();

    void RegisterTamperedCardReaderCallback(TamperedCardReaderCallback callback);

    void RaiseTamperedCardReader();

    void RegisterCardReaderConnectedCallback(Action cardReaderConnectedCallback);

    void RegisterCardReaderDisconnectedCallback(
      Action<ITrackDataService> cardReaderDisconnectedCallback);

    EnvironmentNotificationType ProcessClassicCardReaderNotification(ref Message msg);

    bool HasValidCardReader { get; }

    bool CheckCardReader { get; set; }

    int NullKioskId { get; set; }

    void DetectCardReaderAndRegisterTrackDataService();

    bool IsClassicTrackDataServiceRegistered { get; }

    bool IsDeviceServiceTrackDataServiceRegistered { get; }

    bool IsTrackDataServiceRegistered { get; }

    bool PreviousTrackDataServiceWasDeviceServiceTrackDataService { get; }

    bool PreviousTrackDataServiceWasClassicTrackDataService { get; }
  }
}
