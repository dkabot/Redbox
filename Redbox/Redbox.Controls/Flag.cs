using Redbox.Controls.Utilities;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Redbox.Controls
{
    public class Flag : UserControl
    {
        public static readonly DependencyProperty FlagColorProperty =
            Dependency<Flag>.CreateDependencyProperty(nameof(FlagColor), typeof(Color), (object)Colors.Black);

        public static readonly DependencyProperty FlagGradientColorProperty =
            Dependency<Flag>.CreateDependencyProperty(nameof(FlagGradientColor), typeof(Color), (object)Colors.Black);

        public static readonly DependencyProperty FlagShadowColorProperty =
            Dependency<Flag>.CreateDependencyProperty(nameof(FlagShadowColor), typeof(Color), (object)Colors.Black);

        public Color FlagColor
        {
            get => (Color)GetValue(FlagColorProperty);
            set => SetValue(FlagColorProperty, (object)value);
        }

        public Color FlagGradientColor
        {
            get => (Color)GetValue(FlagGradientColorProperty);
            set => SetValue(FlagGradientColorProperty, (object)value);
        }

        public Color FlagShadowColor
        {
            get => (Color)GetValue(FlagShadowColorProperty);
            set => SetValue(FlagShadowColorProperty, (object)value);
        }
    }
}