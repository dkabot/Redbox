using System.Windows;
using System.Windows.Data;

namespace Redbox.Controls
{
    [ValueConversion(typeof(object), typeof(Visibility))]
    public class NullToVisibilityConverter : ReadOnlyConverter
    {
        public override object Convert(object value)
        {
            return value != null ? (object)Visibility.Visible : (object)Visibility.Collapsed;
        }
    }
}