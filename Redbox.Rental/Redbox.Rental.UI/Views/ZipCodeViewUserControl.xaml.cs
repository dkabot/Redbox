using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "ZipCodeView")]
    public partial class ZipCodeViewUserControl : TextToSpeechUserControl, IWPFActor
    {
        public ZipCodeViewUserControl()
        {
            InitializeComponent();
            ApplyTheme();
        }

        private ZipCodeViewModel ZipCodeViewModel => DataContext as ZipCodeViewModel;

        public override ISpeechControl GetSpeechControl()
        {
            var zipCodeViewModel = ZipCodeViewModel;
            if (zipCodeViewModel == null) return null;
            return zipCodeViewModel.ProcessGetSpeechControl();
        }

        private void ApplyTheme()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            if (theme == null) return;
            theme.SetStyle(NextButton);
        }

        private void ResetIdleTimerCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            HandleWPFHit();
        }

        private void NextCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var zipCodeViewModel = ZipCodeViewModel;
            if (zipCodeViewModel != null)
            {
                var zipCodeViewModel2 = ZipCodeViewModel;
                zipCodeViewModel.ProcessOnNextButtonClicked(zipCodeViewModel2 != null
                    ? zipCodeViewModel2.DisplayText
                    : null);
            }

            HandleWPFHit();
        }

        private void Grid_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var zipCodeViewModel = ZipCodeViewModel;
            if (zipCodeViewModel != null)
            {
                var keypadModel = zipCodeViewModel.KeypadModel;
                if (keypadModel != null)
                {
                    keypadModel.ProcessOnNumberButtonClicked("1");
                    keypadModel.ProcessOnNumberButtonClicked("1");
                    keypadModel.ProcessOnNumberButtonClicked("0");
                    keypadModel.ProcessOnNumberButtonClicked("3");
                    keypadModel.ProcessOnNumberButtonClicked("7");

                    var zipCodeViewModel2 = ZipCodeViewModel;
                    zipCodeViewModel.ProcessOnNextButtonClicked(zipCodeViewModel2 != null
                        ? zipCodeViewModel2.DisplayText
                        : null);

                    HandleWPFHit();
                }
            }
        }
    }
}