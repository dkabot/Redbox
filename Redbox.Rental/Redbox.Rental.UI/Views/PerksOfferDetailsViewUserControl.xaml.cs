using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "PerksOfferDetailsView")]
    public partial class PerksOfferDetailsViewUserControl : TextToSpeechUserControl, IWPFActor
    {
        public PerksOfferDetailsViewUserControl()
        {
            InitializeComponent();
            ApplyTheme();
        }

        private PerksOfferDetailsViewModel PerksOfferDetailsViewModel => DataContext as PerksOfferDetailsViewModel;

        public override ISpeechControl GetSpeechControl()
        {
            var perksOfferDetailsViewModel = PerksOfferDetailsViewModel;
            if (perksOfferDetailsViewModel == null) return null;
            return perksOfferDetailsViewModel.ProcessGetSpeechControl();
        }

        private void ApplyTheme()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            if (theme == null) return;
            theme.SetStyle(CancelButton);
        }

        private void PerksOfferDetailsViewUserControl_DataContextChanged(object sender,
            DependencyPropertyChangedEventArgs e)
        {
            PopulateProgressStackPanel();
        }

        private void PopulateProgressStackPanel()
        {
            if (PerksOfferDetailsViewModel.CurrentValue != null && PerksOfferDetailsViewModel.MaxValue != null &&
                PerksOfferDetailsViewModel.MaxValue.Value > 0 && PerksOfferDetailsViewModel.MaxValue.Value <= 7)
            {
                ProgressStackPanel.Width = PerksOfferDetailsViewModel.MaxValue.Value * 60;
                for (var i = 0; i < PerksOfferDetailsViewModel.MaxValue.Value; i++)
                {
                    var text = i < PerksOfferDetailsViewModel.CurrentValue.Value
                        ? "style_circle_checkmark_image"
                        : "style_circle_empty_image";
                    ProgressStackPanel.Children.Add(new Image
                    {
                        Style = FindResource(text) as Style
                    });
                }
            }
        }

        private void CancelCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var perksOfferDetailsViewModel = PerksOfferDetailsViewModel;
            if (perksOfferDetailsViewModel == null) return;
            perksOfferDetailsViewModel.ProcessOnCancelButtonClicked();
        }
    }
}