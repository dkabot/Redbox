using System.Windows;
using System.Windows.Controls;

namespace Redbox.Rental.UI.Controls
{
    public partial class TitleDetailsDevLabelValueUserControl : UserControl
    {
        public static readonly DependencyProperty LabelTextProperty = DependencyProperty.Register("LabelText",
            typeof(string), typeof(TitleDetailsDevLabelValueUserControl), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ValueTextProperty = DependencyProperty.Register("ValueText",
            typeof(string), typeof(TitleDetailsDevLabelValueUserControl), new PropertyMetadata(string.Empty));

        public TitleDetailsDevLabelValueUserControl()
        {
            InitializeComponent();
        }

        public string LabelText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(LabelTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(LabelTextProperty, value); }); }
        }

        public string ValueText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(ValueTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ValueTextProperty, value); }); }
        }
    }
}