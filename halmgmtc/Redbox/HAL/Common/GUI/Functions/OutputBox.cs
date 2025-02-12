using System.Windows.Forms;

namespace Redbox.HAL.Common.GUI.Functions
{
    public class OutputBox
    {
        private readonly ListBox TheBox;

        protected OutputBox(ListBox box)
        {
            TheBox = box;
        }

        public void Write(string msg)
        {
            if (TheBox.InvokeRequired)
                TheBox.Invoke(new WriteCallback(WriteToOutput), msg);
            else
                WriteToOutput(msg);
        }

        public void Write(string fmt, params object[] stuff)
        {
            Write(string.Format(fmt, stuff));
        }

        public void Clear()
        {
            TheBox.Items.Clear();
        }

        protected virtual void PostWrite(string s)
        {
        }

        protected virtual string PrewriteFormat(string msg)
        {
            return msg;
        }

        private void WriteToOutput(string msg)
        {
            TheBox.Items.Insert(0, PrewriteFormat(msg));
            PostWrite(msg);
        }

        private delegate void WriteCallback(string s);
    }
}