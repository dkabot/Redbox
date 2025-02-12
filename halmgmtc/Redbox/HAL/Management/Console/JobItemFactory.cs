using System.Windows.Forms;
using Redbox.HAL.Core;

namespace Redbox.HAL.Management.Console
{
    internal class JobItemFactory
    {
        private readonly JobItems[] m_orderedJobItems = new JobItems[13]
        {
            JobItems.Garbage,
            JobItems.ViewErrors,
            JobItems.Seperator,
            JobItems.Connect,
            JobItems.Disconnect,
            JobItems.Seperator,
            JobItems.SetLabel,
            JobItems.SetStartTime,
            JobItems.Seperator,
            JobItems.Resume,
            JobItems.Suspend,
            JobItems.Stop,
            JobItems.Trash
        };

        private JobItemFactory()
        {
        }

        internal static JobItemFactory Instance => Singleton<JobItemFactory>.Instance;

        internal ToolStrip MakeToolStrip(UpdateSource source)
        {
            var toolStrip = new ToolStrip();
            toolStrip.Name = "Jobz";
            toolStrip.Text = "Jobz";
            toolStrip.Tag = source;
            foreach (var orderedJobItem in m_orderedJobItems)
                if (orderedJobItem == JobItems.Seperator)
                    toolStrip.Items.Add(new ToolStripSeparator());
                else
                    toolStrip.Items.Add(JobItem.Instance.MakeJobToolStripButton(orderedJobItem, false));
            return toolStrip;
        }

        internal ToolStripMenuItem MakeMenuItem(UpdateSource source)
        {
            var toolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem.Name = "JOBz";
            toolStripMenuItem.Text = "JOBz";
            toolStripMenuItem.Tag = source;
            foreach (var orderedJobItem in m_orderedJobItems)
                if (orderedJobItem == JobItems.Seperator)
                    toolStripMenuItem.DropDown.Items.Add(new ToolStripSeparator());
                else
                    toolStripMenuItem.DropDown.Items.Add(JobItem.Instance.MakeJobMenuItem(orderedJobItem, false));
            return toolStripMenuItem;
        }

        internal object MakeMyControl(ControlType ctrlType, UpdateSource source)
        {
            if (ctrlType == ControlType.ToolStrip)
                return MakeToolStrip(source);
            return ctrlType == ControlType.MenuItem ? MakeMenuItem(source) : (object)null;
        }
    }
}