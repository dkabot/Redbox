using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;
using System;
using System.IO;

namespace HALUtilities
{
  internal sealed class HardwareSurvey
  {
    private readonly SurveyModes Mode;
    private const string SurveyFile = "HardwareSurvey.txt";

    internal void Run(HardwareService service)
    {
      TextWriter writer = Console.Out;
      if (SurveyModes.File == this.Mode)
        writer = (TextWriter) new StreamWriter("HardwareSurvey.txt", true);
      this.RunSurvey(service, writer);
      if (SurveyModes.File != this.Mode)
        return;
      writer.Dispose();
    }

    internal HardwareSurvey(SurveyModes mode) => this.Mode = mode;

    private void RunSurvey(HardwareService service, TextWriter writer)
    {
      using (HardwareSurveyExecutor hardwareSurveyExecutor = new HardwareSurveyExecutor(service))
      {
        using (ExecutionTimer executionTimer = new ExecutionTimer())
        {
          hardwareSurveyExecutor.Run();
          executionTimer.Stop();
          writer.WriteLine("## Hardware survey on {0} Kiosk = {1} ##", (object) hardwareSurveyExecutor.Timestamp, (object) hardwareSurveyExecutor.Kiosk);
          writer.WriteLine("\tPC Manufacturer         :  {0}", (object) hardwareSurveyExecutor.PcManufacturer);
          writer.WriteLine("\tPC Model                :  {0}", (object) hardwareSurveyExecutor.PcModel);
          writer.WriteLine("\tMemory                  :  {0} GB", (object) (hardwareSurveyExecutor.Memory >> 20));
          writer.WriteLine("\tDisk free space         :  {0} GB", (object) (hardwareSurveyExecutor.FreeDiskSpace >> 30));
          TextWriter textWriter1 = writer;
          DeviceStatus deviceStatus = hardwareSurveyExecutor.QuickReturn;
          string str1 = deviceStatus.ToString();
          textWriter1.WriteLine("\tQuick return            :  {0}", (object) str1);
          TextWriter textWriter2 = writer;
          deviceStatus = hardwareSurveyExecutor.AirExchanger;
          string str2 = deviceStatus.ToString();
          textWriter2.WriteLine("\tAir exchanger           :  {0}", (object) str2);
          writer.WriteLine("\tAuxRelay board          :  {0}", hardwareSurveyExecutor.HasAuxRelayBoard ? (object) "Yes" : (object) "No");
          writer.WriteLine("\tFraud sensor            :  {0}", (object) hardwareSurveyExecutor.FraudDevice.ToString());
          writer.WriteLine("\tABE device              :  {0}", (object) hardwareSurveyExecutor.ABEDevice);
          writer.WriteLine("\tTouchscreen model       :  {0}", (object) hardwareSurveyExecutor.Touchscreen);
          string str3 = hardwareSurveyExecutor.TouchscreenFirmware;
          try
          {
            Version version = new Version(hardwareSurveyExecutor.TouchscreenFirmware);
            bool flag = version.Major == 4 && version.Minor == 30;
            str3 = string.Format("{0} ( {1} CURRENT )", (object) hardwareSurveyExecutor.TouchscreenFirmware, flag ? (object) "MOST" : (object) "NOT MOST");
          }
          catch (Exception ex)
          {
            writer.WriteLine("** ERROR** version format '{0}' caught an exception", (object) hardwareSurveyExecutor.TouchscreenFirmware);
            writer.WriteLine(ex.Message);
          }
          writer.WriteLine("\tTouchscreen firmware    :  {0}", (object) str3);
          writer.WriteLine("\tCamera                  :  {0}", (object) hardwareSurveyExecutor.CameraVersion);
          writer.WriteLine("\tMonitor model           :  {0}", (object) hardwareSurveyExecutor.Monitor);
          writer.WriteLine("\tUPS                     :  {0}", (object) hardwareSurveyExecutor.UpsModel);
          writer.WriteLine("\tSerial controller f/w   :  {0}", (object) hardwareSurveyExecutor.SerialControllerVersion);
          writer.WriteLine("## Execution time = {0} ##", (object) executionTimer.Elapsed);
        }
      }
    }
  }
}
