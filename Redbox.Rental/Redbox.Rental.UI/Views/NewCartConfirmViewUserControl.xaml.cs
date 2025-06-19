using System.Windows;
using System.Windows.Input;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "NewCartConfirmView")]
    public partial class NewCartConfirmViewUserControl : TextToSpeechUserControl
    {
        public NewCartConfirmViewUserControl()
        {
            InitializeComponent();
        }

        private NewCartConfirmViewModel Model => DataContext as NewCartConfirmViewModel;

        private void BrowseItemCancelCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var frameworkElement = e.OriginalSource as FrameworkElement;
            var displayCheckoutProductModel =
                (frameworkElement != null ? frameworkElement.DataContext : null) as DisplayCheckoutProductModel;
            if (Model != null) Model.ProcessOnCancelBrowseItemModel(displayCheckoutProductModel, e.Parameter);
            HandleWPFHit();
        }

        public override ISpeechControl GetSpeechControl()
        {
            return Model.ProcessGetSpeechControl();
        }
    }
}