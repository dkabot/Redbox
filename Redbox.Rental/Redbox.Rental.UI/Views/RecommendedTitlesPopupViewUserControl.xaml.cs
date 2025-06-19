using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "RecommendedTitlesPopupView")]
    public partial class RecommendedTitlesPopupViewUserControl : TextToSpeechUserControl, IWPFActor
    {
        public RecommendedTitlesPopupViewUserControl()
        {
            InitializeComponent();
            ApplyTheme();
        }

        private RecommendedTitlesPopupViewModel RecommendedTitlesPopupViewModel =>
            DataContext as RecommendedTitlesPopupViewModel;

        public override ISpeechControl GetSpeechControl()
        {
            var recommendedTitlesPopupViewModel = RecommendedTitlesPopupViewModel;
            if (recommendedTitlesPopupViewModel == null) return null;
            return recommendedTitlesPopupViewModel.ProcessGetSpeechControl();
        }

        private void ApplyTheme()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            if (theme == null) return;
            theme.SetStyle(CancelButton);
        }

        private void ResetIdleTimerCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            HandleWPFHit();
        }

        private void HowPointsWorkCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var recommendedTitlesPopupViewModel = RecommendedTitlesPopupViewModel;
            if (recommendedTitlesPopupViewModel == null) return;
            recommendedTitlesPopupViewModel.ProcessOnHowPointsWorkButtonClicked();
        }

        private void PickupCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var recommendedTitlesPopupViewModel = RecommendedTitlesPopupViewModel;
            if (recommendedTitlesPopupViewModel == null) return;
            recommendedTitlesPopupViewModel.ProcessOnPickupButtonClicked();
        }

        private void CancelCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var recommendedTitlesPopupViewModel = RecommendedTitlesPopupViewModel;
            if (recommendedTitlesPopupViewModel == null) return;
            recommendedTitlesPopupViewModel.ProcessOnCancelButtonClicked();
        }
    }
}