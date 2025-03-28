using System.Windows;
using System.Windows.Data;

namespace Redbox.Controls
{
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class TextToVisibilityConverter : ReadOnlyConverter
    {
        public override object Convert(object value)
        {
            return value is string && string.IsNullOrEmpty(value as string)
                ? (object)Visibility.Visible
                : (object)Visibility.Collapsed;
        }
    }
}