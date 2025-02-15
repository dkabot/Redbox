using System.Windows.Forms;

namespace Redbox.HAL.Common.GUI.Functions
{
    public static class TextBoxExtensions
    {
        public static int GetInteger(this TextBox tb, string friendlyName, OutputBox box)
        {
            if (string.IsNullOrEmpty(tb.Text))
                return -1;
            int result;
            if (int.TryParse(tb.Text, out result))
                return result;
            box.Write(string.Format("The value '{0}' in box {1} is not an integer.", tb.Text, friendlyName));
            return -1;
        }
    }
}