using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Management.Console
{
    internal class JobComboBox : ToolStripComboBox
    {
        private static JobComboBox m_instance;
        private string m_previousSelectedID;

        private JobComboBox()
        {
            ComboBox.DataSource = JobHelper.JobComboList;
            Name = "m_jobToolStripComboBox";
            Size = new Size(121, 25);
            DropDownStyle = ComboBoxStyle.DropDownList;
            Enabled = false;
            SelectedIndexChanged += OnSelectedJobChanged;
            ProfileManager.Instance.Connected += (_param1, _param2) => Enabled = true;
            ProfileManager.Instance.Disconnected += (_param1, _param2) =>
            {
                Enabled = false;
                JobHelper.ClearJobComboList();
            };
        }

        public static JobComboBox Instance
        {
            get
            {
                if (m_instance == null)
                    m_instance = new JobComboBox();
                return m_instance;
            }
        }

        public List<HardwareJobWrapper> GetSelectedJobList()
        {
            return new List<HardwareJobWrapper>
            {
                (HardwareJobWrapper)SelectedItem
            };
        }

        public string GetJobID()
        {
            return SelectedItem == null || SelectedItem.ToString() == Constants.None ? null : SelectedItem.ToString();
        }

        private void OnSelectedJobChanged(object sender, EventArgs e)
        {
            if (!ProfileManager.Instance.IsConnected ||
                (m_previousSelectedID != null && m_previousSelectedID == SelectedItem.ToString()))
                return;
            m_previousSelectedID = SelectedItem.ToString();
            EnvironmentHelper.OnSelectedJobChanged(sender, e);
        }
    }
}