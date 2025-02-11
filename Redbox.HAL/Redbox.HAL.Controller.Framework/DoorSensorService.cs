using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class DoorSensorService : IConfigurationObserver, IDoorSensorService, IMoveVeto
    {
        private bool m_enabled = true;
        private bool m_softwareOverride;

        internal DoorSensorService()
        {
            m_softwareOverride = ServiceLocator.Instance.GetService<IPersistentMapService>().GetMap()
                .GetValue("SensorSoftwareOverride", false);
            ServiceLocator.Instance.GetService<IMotionControlService>().AddVeto(this);
            ControllerConfiguration.Instance.AddObserver(this);
        }

        public void NotifyConfigurationLoaded()
        {
            LogHelper.Instance.Log("[DoorSensorService] Configuration load.");
            m_enabled = ControllerConfiguration.Instance.IsVMZMachine;
            if (!m_enabled)
            {
                LogHelper.Instance.Log("The door sensors are not configured.");
            }
            else if (SoftwareOverride)
            {
                LogHelper.Instance.Log(
                    "** WARNING **: Door sensors are configured, however a software override in place.");
                LogHelper.Instance.Log("** WARNING **: Kiosk is operating without door sensors.");
            }
            else
            {
                LogHelper.Instance.Log("Door sensors are configured.");
            }
        }

        public void NotifyConfigurationChangeStart()
        {
        }

        public void NotifyConfigurationChangeEnd()
        {
        }

        public ErrorCodes CanMove()
        {
            var doorSensorResult = Query();
            if (doorSensorResult == DoorSensorResult.Ok)
                return ErrorCodes.Success;
            LogHelper.Instance.Log(string.Format("Door sensor query returned {0}", doorSensorResult.ToString()),
                LogEntryType.Error);
            return ErrorCodes.DoorOpen;
        }

        public DoorSensorResult Query()
        {
            return !m_enabled || SoftwareOverride ? DoorSensorResult.Ok : RawQuery();
        }

        public DoorSensorResult QueryStateForDisplay()
        {
            if (!m_enabled)
                return DoorSensorResult.Ok;
            return !m_softwareOverride ? RawQuery() : DoorSensorResult.SoftwareOverride;
        }

        public bool SensorsEnabled => m_enabled && !SoftwareOverride;

        public bool SoftwareOverride
        {
            get => m_softwareOverride;
            set
            {
                m_softwareOverride = value;
                ServiceLocator.Instance.GetService<IPersistentMapService>().GetMap()
                    .SetValue("SensorSoftwareOverride", m_softwareOverride);
            }
        }

        private DoorSensorResult RawQuery()
        {
            var readInputsResult = ServiceLocator.Instance.GetService<IControlSystem>().ReadAuxInputs();
            if (!readInputsResult.Success)
                return DoorSensorResult.AuxReadError;
            if (readInputsResult.IsInputActive(AuxInputs.QlmDown))
                return DoorSensorResult.Ok;
            LogHelper.Instance.WithContext(LogEntryType.Error, "[DoorSensorService] read inputs shows door not closed");
            readInputsResult.Log(LogEntryType.Error);
            return DoorSensorResult.FrontDoor;
        }
    }
}