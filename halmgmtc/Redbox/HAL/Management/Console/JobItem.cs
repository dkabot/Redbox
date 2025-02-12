using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Core;
using Redbox.HAL.Management.Console.Properties;

namespace Redbox.HAL.Management.Console
{
    public class JobItem
    {
        private JobItem()
        {
        }

        public static JobItem Instance => Singleton<JobItem>.Instance;

        public ToolStripButton MakeJobToolStripButton(JobItems itemName, bool locked)
        {
            var ctrl = new ToolStripButton();
            JobItemInner(ctrl, itemName, locked);
            ctrl.Size = new Size(23, 22);
            ctrl.DisplayStyle = ToolStripItemDisplayStyle.Image;
            return ctrl;
        }

        public ToolStripMenuItem MakeJobMenuItem(JobItems itemName, bool locked)
        {
            var ctrl = new ToolStripMenuItem();
            JobItemInner(ctrl, itemName, locked);
            ctrl.Size = new Size(185, 22);
            return ctrl;
        }

        private void JobItemInner(ToolStripItem ctrl, JobItems itemName, bool locked)
        {
            ctrl.Name = itemName.ToString();
            ctrl.Tag = itemName;
            ctrl.Enabled = ProfileManager.Instance.IsConnected;
            switch (itemName)
            {
                case JobItems.Connect:
                    ctrl.Text = "&Connect";
                    ctrl.Image = Resources.network_connection;
                    ctrl.Click += JobHelper.OnConnect;
                    break;
                case JobItems.Disconnect:
                    ctrl.Text = "&Disconnect";
                    ctrl.Image = Resources.network_connection_disconnected;
                    ctrl.Click += JobHelper.OnDisconnect;
                    break;
                case JobItems.Garbage:
                    ctrl.Text = "&Garbage";
                    ctrl.Image = Resources.clean;
                    ctrl.Click += JobHelper.OnGarbage;
                    break;
                case JobItems.Resume:
                    ctrl.Text = "&Resume";
                    ctrl.Image = Resources.debug_play;
                    ctrl.Click += JobHelper.OnResume;
                    break;
                case JobItems.SetLabel:
                    ctrl.Text = "Set &Label";
                    ctrl.Image = Resources.label_edit;
                    ctrl.Click += JobHelper.OnSetLabel;
                    break;
                case JobItems.SetStartTime:
                    ctrl.Text = "S&et Start Time";
                    ctrl.Image = Resources.planner;
                    ctrl.Click += JobHelper.OnSetStartTimes;
                    break;
                case JobItems.Stop:
                    ctrl.Text = "St&op";
                    ctrl.Image = Resources.debug_stop;
                    ctrl.Click += JobHelper.OnTerminate;
                    break;
                case JobItems.Suspend:
                    ctrl.Text = "Sus&pend";
                    ctrl.Image = Resources.debug_pause;
                    ctrl.Click += JobHelper.OnSuspend;
                    break;
                case JobItems.Trash:
                    ctrl.Text = "&Trash";
                    ctrl.Image = Resources.recycle_bin_full;
                    ctrl.Click += JobHelper.OnTrash;
                    break;
                case JobItems.TrashLocked:
                    ctrl.Text = "&Trash";
                    ctrl.Image = Resources.recycle_bin_full;
                    ctrl.Click += JobHelper.OnTrash;
                    break;
                case JobItems.ViewErrors:
                    ctrl.Text = "&View Errors";
                    ctrl.Image = Resources.error_list;
                    ctrl.Click += JobHelper.OnViewErrors;
                    break;
            }
        }
    }
}