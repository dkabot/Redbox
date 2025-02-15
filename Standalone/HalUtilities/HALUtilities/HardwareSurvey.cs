using System;
using System.IO;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Component.Model.Timers;

namespace HALUtilities
{
    internal sealed class HardwareSurvey
    {
        private const string SurveyFile = "HardwareSurvey.txt";
        private readonly SurveyModes Mode;

        internal HardwareSurvey(SurveyModes mode)
        {
            Mode = mode;
        }

        internal void Run(HardwareService service)
        {
            var writer = Console.Out;
            if (SurveyModes.File == Mode)
                writer = new StreamWriter("HardwareSurvey.txt", true);
            RunSurvey(service, writer);
            if (SurveyModes.File != Mode)
                return;
            writer.Dispose();
        }

        private void RunSurvey(HardwareService service, TextWriter writer)
        {
            using (var hardwareSurveyExecutor = new HardwareSurveyExecutor(service))
            {
                using (var executionTimer = new ExecutionTimer())
                {
                    hardwareSurveyExecutor.Run();
                    executionTimer.Stop();
                    writer.WriteLine("## Hardware survey on {0} Kiosk = {1} ##", hardwareSurveyExecutor.Timestamp,
                        hardwareSurveyExecutor.Kiosk);
                    writer.WriteLine("\tPC Manufacturer         :  {0}", hardwareSurveyExecutor.PcManufacturer);
                    writer.WriteLine("\tPC Model                :  {0}", hardwareSurveyExecutor.PcModel);
                    writer.WriteLine("\tMemory                  :  {0} GB", hardwareSurveyExecutor.Memory >> 20);
                    writer.WriteLine("\tDisk free space         :  {0} GB", hardwareSurveyExecutor.FreeDiskSpace >> 30);
                    var textWriter1 = writer;
                    var deviceStatus = hardwareSurveyExecutor.QuickReturn;
                    var str1 = deviceStatus.ToString();
                    textWriter1.WriteLine("\tQuick return            :  {0}", str1);
                    var textWriter2 = writer;
                    deviceStatus = hardwareSurveyExecutor.AirExchanger;
                    var str2 = deviceStatus.ToString();
                    textWriter2.WriteLine("\tAir exchanger           :  {0}", str2);
                    writer.WriteLine("\tAuxRelay board          :  {0}",
                        hardwareSurveyExecutor.HasAuxRelayBoard ? "Yes" : (object)"No");
                    writer.WriteLine("\tFraud sensor            :  {0}", hardwareSurveyExecutor.FraudDevice.ToString());
                    writer.WriteLine("\tABE device              :  {0}", hardwareSurveyExecutor.ABEDevice);
                    writer.WriteLine("\tTouchscreen model       :  {0}", hardwareSurveyExecutor.Touchscreen);
                    var str3 = hardwareSurveyExecutor.TouchscreenFirmware;
                    try
                    {
                        var version = new Version(hardwareSurveyExecutor.TouchscreenFirmware);
                        var flag = version.Major == 4 && version.Minor == 30;
                        str3 = string.Format("{0} ( {1} CURRENT )", hardwareSurveyExecutor.TouchscreenFirmware,
                            flag ? "MOST" : (object)"NOT MOST");
                    }
                    catch (Exception ex)
                    {
                        writer.WriteLine("** ERROR** version format '{0}' caught an exception",
                            hardwareSurveyExecutor.TouchscreenFirmware);
                        writer.WriteLine(ex.Message);
                    }

                    writer.WriteLine("\tTouchscreen firmware    :  {0}", str3);
                    writer.WriteLine("\tCamera                  :  {0}", hardwareSurveyExecutor.CameraVersion);
                    writer.WriteLine("\tMonitor model           :  {0}", hardwareSurveyExecutor.Monitor);
                    writer.WriteLine("\tUPS                     :  {0}", hardwareSurveyExecutor.UpsModel);
                    writer.WriteLine("\tSerial controller f/w   :  {0}",
                        hardwareSurveyExecutor.SerialControllerVersion);
                    writer.WriteLine("## Execution time = {0} ##", executionTimer.Elapsed);
                }
            }
        }
    }
}