using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "MultiNightPricingPopupView")]
    public partial class MultiNightPricingPopupViewUserControl : TextToSpeechUserControl, IWPFActor
    {
        public MultiNightPricingPopupViewUserControl()
        {
            InitializeComponent();
            ApplyTheme();
        }

        private MultiNightPricingPopupViewModel MultiNightPricingPopupViewModel =>
            DataContext as MultiNightPricingPopupViewModel;

        public override ISpeechControl GetSpeechControl()
        {
            var multiNightPricingPopupViewModel = MultiNightPricingPopupViewModel;
            if (multiNightPricingPopupViewModel == null) return null;
            return multiNightPricingPopupViewModel.ProcessGetSpeechControl();
        }

        private void ApplyTheme()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            if (theme != null) theme.SetStyle(OneNightButton);
            if (theme != null) theme.SetStyle(MultiNightButton);
            if (theme != null) theme.SetStyle(BuyButton);
            if (theme == null) return;
            theme.SetStyle(CancelButton);
        }

        private void OneNightCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var multiNightPricingPopupViewModel = MultiNightPricingPopupViewModel;
            if (multiNightPricingPopupViewModel == null) return;
            multiNightPricingPopupViewModel.ProcessOnOneNightButtonClicked();
        }

        private void MultiNightCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var multiNightPricingPopupViewModel = MultiNightPricingPopupViewModel;
            if (multiNightPricingPopupViewModel == null) return;
            multiNightPricingPopupViewModel.ProcessOnMultiNightButtonClicked();
        }

        private void BuyCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var multiNightPricingPopupViewModel = MultiNightPricingPopupViewModel;
            if (multiNightPricingPopupViewModel == null) return;
            multiNightPricingPopupViewModel.ProcessOnBuyButtonClicked();
        }

        private void CancelCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var multiNightPricingPopupViewModel = MultiNightPricingPopupViewModel;
            if (multiNightPricingPopupViewModel == null) return;
            multiNightPricingPopupViewModel.ProcessOnCancelButtonClicked();
        }
    }
}