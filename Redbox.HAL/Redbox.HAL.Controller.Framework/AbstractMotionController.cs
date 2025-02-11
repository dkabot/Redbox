using System;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    internal abstract class AbstractMotionController
    {
        protected readonly TextWriter LogFile;

        protected AbstractMotionController()
        {
            try
            {
                LogFile = new StreamWriter(File.Open(
                    Path.Combine(
                        ServiceLocator.Instance.GetService<IFormattedLogFactoryService>().CreateSubpath("Service"),
                        "MotionControlErrorLog.log"), FileMode.Append, FileAccess.Write, FileShare.Read))
                {
                    AutoFlush = true
                };
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("[MotionControl] Create motion control error log caught an exception.", ex);
                LogFile = StreamWriter.Null;
            }
        }

        protected void WriteToLog(string fmt, params object[] stuff)
        {
            fmt = string.Format(fmt, stuff);
            WriteToLog(fmt);
        }

        protected void WriteToLog(string msg)
        {
            try
            {
                var now = DateTime.Now;
                LogFile.WriteLine("{0} {1} {2}", now.ToShortDateString(), now.ToShortTimeString(), msg);
            }
            catch
            {
            }
        }

        internal abstract bool OnStartup();

        internal abstract bool OnShutdown();

        internal abstract IMotionControlLimitResponse ReadLimits();

        internal abstract bool CommunicationOk();

        internal abstract IControllerPosition ReadPositions();

        internal abstract ErrorCodes MoveToTarget(ref MoveTarget target);

        internal abstract ErrorCodes MoveToVend(MoveMode mode);

        internal abstract ErrorCodes HomeAxis(Axis axis);

        internal abstract bool OnResetDeviceDriver();

        internal virtual void OnConfigurationLoad()
        {
        }

        internal virtual void OnConfigurationChangeStart()
        {
        }

        internal virtual void OnConfigurationChangeEnd()
        {
        }
    }
}