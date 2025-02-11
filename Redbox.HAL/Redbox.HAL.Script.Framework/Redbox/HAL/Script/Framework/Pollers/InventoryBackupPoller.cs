using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework.Pollers
{
    [Poller(Name = "InventoryBackup", ConfigNotifications = true)]
    internal sealed class InventoryBackupPoller : ScriptPoller
    {
        private bool m_isConfigured;
        private int m_pollSleep = 900000;

        private InventoryBackupPoller()
        {
        }

        protected override ProgramPriority Priority => ProgramPriority.High;

        protected override string JobName => "backup-inventory";

        protected override string ThreadName => "Inventory backup";

        protected override bool IsConfigured => m_isConfigured;

        protected override int PollSleep => m_pollSleep;

        protected override void OnConfigurationLoad()
        {
            m_pollSleep = ControllerConfiguration.Instance.InventoryBackupPause;
            m_isConfigured = m_pollSleep > 0;
        }

        protected override void OnConfigurationChangeEnd()
        {
            m_pollSleep = ControllerConfiguration.Instance.InventoryBackupPause;
            m_isConfigured = m_pollSleep > 0;
        }
    }
}