using Redbox.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model.Health;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Redbox.KioskEngine.Environment
{
  public class RentalHealthService : IRentalHealthService
  {
    private const string _kfcJson = "KFC.json";

    public void KioskFunctionCheck()
    {
      ITimerService timerService = ServiceLocator.Instance.GetService<ITimerService>();
      ServiceLocator.Instance.GetService<ILogger>();
      string storeNumber = ServiceLocator.Instance.GetService<IMacroService>()["StoreNumber"];
      string timerName = "KioskFunctionCheckTimer";
      timerService.CreateTimer(timerName, new int?(1200000), new int?(), (TimerCallback) (_param1 =>
      {
        timerService.RemoveTimer(timerName);
        Task.Run((Action) (() =>
        {
          try
          {
            IList<IKioskFunctionCheckData> hardwareKfc = this.GetHardwareKFC();
            List<RentalKioskFunctionCheckData> source = this.ReadKFCJson();
            DateTime lastUpdate = source.Any<RentalKioskFunctionCheckData>() ? source.Max<RentalKioskFunctionCheckData, DateTime>((Func<RentalKioskFunctionCheckData, DateTime>) (x => x.Timestamp)) : DateTime.MinValue;
            IEnumerable<IKioskFunctionCheckData> functionCheckDatas = hardwareKfc != null ? hardwareKfc.Where<IKioskFunctionCheckData>((Func<IKioskFunctionCheckData, bool>) (x => x.Timestamp > lastUpdate)) : (IEnumerable<IKioskFunctionCheckData>) null;
            LogHelper.Instance.Log(string.Format("Found {0} new Kiosk Function Check Entries.", (object) (functionCheckDatas != null ? functionCheckDatas.Count<IKioskFunctionCheckData>() : 0)));
            if (functionCheckDatas != null)
              functionCheckDatas.ForEach<IKioskFunctionCheckData>((Action<IKioskFunctionCheckData>) (x => this.SendKioskFunctionCheckData(storeNumber, x)));
            this.UpdateKFCJson(hardwareKfc);
          }
          catch (Exception ex)
          {
            LogHelper.Instance.LogException("An error has occurred in RentalHealthService.KioskFunctionCheck", ex);
          }
        }));
      })).Start();
    }

    private IList<IKioskFunctionCheckData> GetHardwareKFC()
    {
      IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
      if (service == null)
      {
        LogHelper.Instance.LogError("S001", "Unable to locate Hardware Service.");
        return (IList<IKioskFunctionCheckData>) null;
      }
      IList<IKioskFunctionCheckData> data = (IList<IKioskFunctionCheckData>) null;
      LogHelper.Instance.Log("Getting Kiosk Function Check Data From HAL.", LogEntryType.Info);
      ErrorList functionCheckData = service.GetKioskFunctionCheckData(out data);
      if (functionCheckData != null && functionCheckData.Any<Redbox.KioskEngine.ComponentModel.Error>())
        functionCheckData.ForEach((Action<Redbox.KioskEngine.ComponentModel.Error>) (x => LogHelper.Instance.LogError(x.Code, x.Description ?? string.Empty + System.Environment.NewLine + x.Details)));
      else if (data != null)
        LogHelper.Instance.Log(data.ToJson());
      else
        LogHelper.Instance.Log("No data retrieved from HAL.");
      LogHelper.Instance.Log("GetHardwareKFC Complete.");
      return data;
    }

    private List<RentalKioskFunctionCheckData> ReadKFCJson()
    {
      try
      {
        if (!File.Exists(this.FullKFCJsonPath))
        {
          LogHelper.Instance.Log(string.Format("Unable to read KFC json, File {0} doesn't exist.", (object) "KFC.json"), LogEntryType.Info);
          return new List<RentalKioskFunctionCheckData>();
        }
        List<RentalKioskFunctionCheckData> source = File.ReadAllText(this.FullKFCJsonPath, Encoding.ASCII).ToObject<List<RentalKioskFunctionCheckData>>();
        LogHelper.Instance.Log(string.Format("Found {0} records in KFC.json file.", (object) (source != null ? source.Count<RentalKioskFunctionCheckData>() : 0)), LogEntryType.Info);
        return source;
      }
      catch (Exception ex1)
      {
        LogHelper.Instance.Log(string.Format("Unable to read KFC json, File {0}", (object) "KFC.json"), ex1);
        try
        {
          string str = Path.ChangeExtension(this.FullKFCJsonPath, ".bad");
          if (File.Exists(str))
            File.Delete(str);
          File.Move(this.FullKFCJsonPath, str);
        }
        catch (Exception ex2)
        {
          LogHelper.Instance.Log("Renaming bad kfc file failed", ex2);
        }
        return new List<RentalKioskFunctionCheckData>();
      }
    }

    private void UpdateKFCJson(IList<IKioskFunctionCheckData> data)
    {
      if (data == null)
      {
        LogHelper.Instance.Log("No data available to update KFC json file.", LogEntryType.Info);
      }
      else
      {
        LogHelper.Instance.Log("Updating Kiosk Function Check Json File.", LogEntryType.Info);
        File.WriteAllBytes(this.FullKFCJsonPath, Encoding.ASCII.GetBytes(data.ToJson()));
      }
    }

    public void SendKioskFunctionCheckData(string storeNumber, IKioskFunctionCheckData data)
    {
      IHealthServices service = ServiceLocator.Instance.GetService<IHealthServices>();
      LogHelper.Instance.Log("Sending Kiosk Function Check to Kiosk Service.", LogEntryType.Info);
      string storeNumber1 = storeNumber;
      string userIdentifier = data.UserIdentifier;
      DateTime timestamp = data.Timestamp;
      string verticalSlotTestResult = data.VerticalSlotTestResult;
      string initTestResult = data.InitTestResult;
      string vendDoorTestResult = data.VendDoorTestResult;
      string trackTestResult = data.TrackTestResult;
      string decodeTestResult = data.SnapDecodeTestResult;
      string driverTestResult1 = data.TouchscreenDriverTestResult;
      string driverTestResult2 = data.CameraDriverTestResult;
      service.SendKioskFunctionCheck(storeNumber1, userIdentifier, timestamp, verticalSlotTestResult, initTestResult, vendDoorTestResult, trackTestResult, decodeTestResult, driverTestResult1, driverTestResult2, (RemoteServiceCallback) null, (Action) (() => LogHelper.Instance.Log("Send Kiosk Function Check Complete.", LogEntryType.Info)));
    }

    private string FullKFCJsonPath
    {
      get
      {
        return Path.Combine(ServiceLocator.Instance.GetService<IEngineApplication>().DataPath, "KFC.json");
      }
    }
  }
}
