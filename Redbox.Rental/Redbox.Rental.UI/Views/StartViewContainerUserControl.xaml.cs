using System.Windows.Input;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "StartView")]
    public partial class StartViewContainerUserControl : TextToSpeechUserControl, IWPFActor
    {
        public StartViewContainerUserControl()
        {
            InitializeComponent();
            RegisterRoutedCommand("RentOrBuyMoviesCommand", UI.Commands.RentOrBuyMoviesCommand);
            RegisterRoutedCommand("BuyMoviesCommand", UI.Commands.BuyMoviesCommand);
            RegisterRoutedCommand("PickupProductCommand", UI.Commands.PickupProductCommand);
            RegisterRoutedCommand("ReturnProductCommand", UI.Commands.ReturnProductCommand);
            RegisterRoutedCommand("HelpCommand", UI.Commands.HelpCommand);
            RegisterRoutedCommand("SignInCommand", UI.Commands.SignInCommand);
        }

        private StartViewModel StartViewModel => DataContext as StartViewModel;

        private void RentOrBuyMoviesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var startViewModel = StartViewModel;
            if (startViewModel != null) startViewModel.ProcessRentMovieButtonClick();
            HandleWPFHit();
        }

        private void RentOrBuyMoviesCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ToggleLanguageModeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var startViewModel = StartViewModel;
            if (startViewModel != null) startViewModel.ProcessToggleLanguage();
            HandleWPFHit();
        }

        private void ToggleLanguageModeCommandBingind_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void BuyMoviesCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var startViewModel = StartViewModel;
            e.CanExecute = startViewModel != null && startViewModel.BuyMoviesEnabled;
        }

        private void BuyMoviesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var startViewModel = StartViewModel;
            if (startViewModel != null) startViewModel.ProcessBuyMoviesButtonClick();
            HandleWPFHit();
        }

        private void PickupProductCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var startViewModel = StartViewModel;
            if (startViewModel != null) startViewModel.ProcessPickupButtonClick();
            HandleWPFHit();
        }

        private void ReturnProductCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var startViewModel = StartViewModel;
            if (startViewModel != null) startViewModel.ProcessReturnButtonClicked();
            HandleWPFHit();
        }

        private void HelpCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var startViewModel = StartViewModel;
            if (startViewModel != null) startViewModel.ProcessHelpButtonClicked();
            HandleWPFHit();
        }

        private void ToggleADAModeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var startViewModel = StartViewModel;
            if (startViewModel != null) startViewModel.ProcessToggleADAMode();
            HandleWPFHit();
        }

        private void ExitBurnInViewCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var startViewModel = StartViewModel;
            if (startViewModel != null) startViewModel.ProcessExitBurnInView();
            HandleWPFHit();
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
        }

        private void BannerClickCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StartViewModel.ProcessBannerClicked();
            HandleWPFHit();
        }

        private void SignInCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var startViewModel = StartViewModel;
            if (startViewModel != null) startViewModel.ProcessSignInButtonClicked();
            HandleWPFHit();
        }

        public override ISpeechControl GetSpeechControl()
        {
            var startViewModel = StartViewModel;
            if (startViewModel == null) return null;
            return startViewModel.ProcessGetSpeechControl();
        }
    }
}