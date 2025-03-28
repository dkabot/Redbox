using System.Windows;
using System.Windows.Data;

namespace Redbox.Controls
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class ReverseBoolToVisibilityConverter : ReadOnlyConverter
    {
        public override object Convert(object value)
        {
            return value is bool flag && !flag ? (object)Visibility.Visible : (object)Visibility.Collapsed;
        }
    }
}