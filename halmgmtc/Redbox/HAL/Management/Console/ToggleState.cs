using System;
using System.Windows.Forms;

namespace Redbox.HAL.Management.Console
{
    internal class ToggleState : IDisposable
    {
        private readonly Control m_control;

        public ToggleState(object o)
        {
            m_control = o as Control;
            if (m_control == null)
                return;
            m_control.Enabled = false;
        }

        public void Dispose()
        {
            if (m_control == null)
                return;
            m_control.Enabled = true;
        }
    }
}