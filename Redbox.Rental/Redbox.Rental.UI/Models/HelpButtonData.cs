using System.Windows;
using Redbox.Rental.Model.Session;

namespace Redbox.Rental.UI.Models
{
    public class HelpButtonData : BaseModel<HelpButtonData>
    {
        public static DependencyProperty IsEnabledProperty = CreateDependencyProperty("IsEnabled", TYPES.BOOL, false);
        public string ButtonText { get; set; }

        public HelpDocuments DocumentToShow { get; set; }

        public bool IsEnabled
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(IsEnabledProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(IsEnabledProperty, value); }); }
        }
    }
}