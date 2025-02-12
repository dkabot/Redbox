using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using Redbox.HAL.Client;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Management.Console.Properties;
using Range = Redbox.HAL.Core.Range;

namespace Redbox.HAL.Management.Console
{
    internal static class CommonFunctions
    {
        private static string m_servicePath;

        private static string ServicePath
        {
            get
            {
                if (string.IsNullOrEmpty(m_servicePath))
                    try
                    {
                        var registryKey =
                            Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\HalSvc$default");
                        if (registryKey != null)
                        {
                            var path = (string)registryKey.GetValue("ImagePath");
                            if (!string.IsNullOrEmpty(path))
                            {
                                if (path.StartsWith("\""))
                                    path = path.Remove(0, 1);
                                if (path.EndsWith("\""))
                                {
                                    var str = path;
                                    path = str.Remove(str.Length - 1, 1);
                                }

                                m_servicePath = Path.GetDirectoryName(path);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        OutputWindow.Instance.Append("Unable to locate HAL service path " + ex.Message);
                        m_servicePath = null;
                    }

                return m_servicePath;
            }
        }

        internal static Location CurrentLocation()
        {
            TouchScreenAccess.WriteInstructionToOutput("LOC");
            var hardwareJob = ExecuteInstruction("LOC");
            hardwareJob.WaitForCompletion();
            Stack<string> stack;
            hardwareJob.GetStack(out stack);
            if (stack == null || stack.Count < 2)
                return null;
            int result1;
            if (!int.TryParse(stack.Pop(), out result1))
            {
                LogHelper.Instance.Log("Currently not at a deck, slot location", LogEntryType.Error);
                return null;
            }

            int result2;
            if (!int.TryParse(stack.Pop(), out result2))
            {
                LogHelper.Instance.Log("Currently not at a deck, slot location", LogEntryType.Error);
                return null;
            }

            return new Location
            {
                Deck = result1,
                Slot = result2
            };
        }

        internal static void OnReadPickerSensors(object sender, EventArgs e)
        {
            using (TouchScreenAccess.Instance.Manager.MakeAspect(sender))
            {
                var num1 = 1;
                var num2 = 6;
                var str1 = string.Format("SENSOR READ PICKER-SENSOR={0}..{1}", num1, num2);
                OutputWindow.Instance.Append(str1);
                var hardwareJob = ExecuteInstruction(str1);
                Stack<string> stack;
                if (hardwareJob == null || !hardwareJob.GetStack(out stack).Success)
                    return;
                SensorView.Instance.ResetSensors();
                if (stack.Count == 0)
                    return;
                stack.Pop();
                for (var index = num2; index >= num1; --index)
                {
                    var str2 = stack.Pop();
                    SensorView.Instance.Sensors[index - 1] = str2.Contains("BLOCKED");
                }

                var str3 = "SENSOR PICKER-OFF";
                OutputWindow.Instance.Append(str3);
                ExecuteInstruction(str3);
            }
        }

        internal static void OnLaunchCameraProperties(object sender, EventArgs e)
        {
            using (TouchScreenAccess.Instance.Manager.MakeAspect(sender))
            {
                if (string.IsNullOrEmpty(ServicePath))
                    OutputWindow.Instance.Append("Unable to locate service path; cannot launch amcap.");
                else
                    CommonCameraFunctions.LaunchAndWaitForTuner(ProfileManager.Instance.Service);
            }
        }

        internal static bool SyncSlot()
        {
            try
            {
                var location = CurrentLocation();
                if (location == null)
                    return false;
                var service = ProfileManager.Instance.Service;
                var range = new SyncRange(location.Deck, location.Deck, new Range(location.Slot, location.Slot));
                var schedule = new HardwareJobSchedule();
                schedule.Priority = HardwareJobPriority.Highest;
                HardwareJob job;
                if (service.HardSync(range, schedule, out job).Success)
                {
                    job.Resume();
                    job.WaitForCompletion();
                    ProfileManager.Instance.Service.GetJob(job.ID, out job);
                    return job.Status == HardwareJobStatus.Completed || job.Status == HardwareJobStatus.Stopped;
                }

                LogHelper.Instance.Log("Sync slot failed.", LogEntryType.Error);
                return false;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("Unhandled exception in SyncSlot.", ex);
                return false;
            }
        }

        internal static HardwareJob ExecuteInstruction(string instruction)
        {
            HardwareJob job;
            ProcessCommandResult(ProfileManager.Instance.Service.ExecuteImmediate(instruction,
                Settings.Default.ImmediateInstructionTimeout, out job));
            return job;
        }

        internal static void ScheduleJob(
            string jobName,
            string label,
            HardwareJobSchedule schedule,
            bool resumeNow)
        {
            HardwareJob job;
            var result = ProfileManager.Instance.Service.ScheduleJob(jobName, label, false, schedule, out job);
            ProcessCommandResult(result);
            if (!(result.Success & resumeNow))
                return;
            job.Resume();
        }

        internal static void ProcessCommandResult(HardwareCommandResult result)
        {
            LogHelper.Instance.Log(result.CommandText);
            OutputWindow.Instance.Append(">>> Attempt to execute: " + result.CommandText);
            if (!result.Success)
            {
                ProcessCommandResultError(result);
                OutputWindow.Instance.Append("Unable to schedule job");
                LogHelper.Instance.Log("ERROR: Failed to execute command", LogEntryType.Error);
                foreach (var commandMessage in result.CommandMessages)
                {
                    OutputWindow.Instance.Append(commandMessage);
                    LogHelper.Instance.Log(commandMessage, LogEntryType.Error);
                }
            }
            else
            {
                if (ErrorListView.Instance.KeepOpenOnSuccessfulInstruction)
                    return;
                ErrorListView.Instance.Clear();
                ListViewTabControl.Instance.Remove(ListViewNames.Errors);
            }
        }

        internal static void ProcessCommandResultError(HardwareCommandResult result)
        {
            if (result.Errors.ContainsCode("J001"))
            {
                ProfileManager.Instance.Disconnect();
                var num = (int)MessageBox.Show(
                    string.Format(
                        "Unable to connect to the designated HAL Service at {0}.\n\nEnsure the HAL Service is installed and running, bound to the desired network interface and port.",
                        Settings.Default.DefaultConnectionURL), "Connect to Service", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
            }

            EnvironmentHelper.DisplayErrors(result.Errors, false, false);
        }
    }
}