using System.Windows;
using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    public partial class StartViewTestUserControl : TextToSpeechUserControl
    {
        public StartViewTestUserControl()
        {
            InitializeComponent();
            ApplyTheme();
            PostHealthActivity();
        }

        private StartViewModel StartViewModel => DataContext as StartViewModel;

        private void ApplyTheme()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            if (theme != null) theme.SetStyle(rent_movies_button);
            if (theme != null) theme.SetStyle(buy_movies_button);
            if (theme != null) theme.SetStyle(return_button);
            if (theme != null) theme.SetStyle(pick_up_button);
            if (theme != null) theme.SetStyle(handicap_button);
            if (theme != null) theme.SetStyle(espanol_button);
            if (theme == null) return;
            theme.SetStyle(help_button);
        }

        private void RentOrBuyMoviesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var startViewModel = StartViewModel;
            if (startViewModel != null) startViewModel.ProcessRentMovieButtonClick();
            HandleWPFHit();
        }

        private void RentOrBuyMoviesCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = rent_movies_button.Visibility == Visibility.Visible;
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
            e.CanExecute = buy_movies_button.Visibility == Visibility.Visible;
        }

        private void BuyMoviesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RunCommandOnce(delegate
            {
                try
                {
                    var startViewModel = StartViewModel;
                    if (startViewModel != null) startViewModel.ProcessBuyMoviesButtonClick();
                    HandleWPFHit();
                }
                finally
                {
                    CompleteRunOnce();
                }
            });
        }

        private void PickupProductCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RunCommandOnce(delegate
            {
                try
                {
                    var startViewModel = StartViewModel;
                    if (startViewModel != null) startViewModel.ProcessPickupButtonClick();
                    HandleWPFHit();
                }
                finally
                {
                    CompleteRunOnce();
                }
            });
        }

        private void ReturnProductCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RunCommandOnce(delegate
            {
                try
                {
                    var startViewModel = StartViewModel;
                    if (startViewModel != null) startViewModel.ProcessReturnButtonClicked();
                    HandleWPFHit();
                }
                finally
                {
                    CompleteRunOnce();
                }
            });
        }

        private void HelpCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RunCommandOnce(delegate
            {
                try
                {
                    var startViewModel = StartViewModel;
                    if (startViewModel != null) startViewModel.ProcessHelpButtonClicked();
                    HandleWPFHit();
                }
                finally
                {
                    CompleteRunOnce();
                }
            });
        }

        private void ToggleADAModeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RunCommandOnce(delegate
            {
                try
                {
                    var startViewModel = StartViewModel;
                    if (startViewModel != null) startViewModel.ProcessToggleADAMode();
                    HandleWPFHit();
                }
                finally
                {
                    CompleteRunOnce();
                }
            });
        }

        private void ExitBurnInViewCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RunCommandOnce(delegate
            {
                try
                {
                    var startViewModel = StartViewModel;
                    if (startViewModel != null) startViewModel.ProcessExitBurnInView();
                    HandleWPFHit();
                }
                finally
                {
                    CompleteRunOnce();
                }
            });
        }

        private void BannerClickCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RunCommandOnce(delegate
            {
                try
                {
                    StartViewModel.ProcessBannerClicked();
                    HandleWPFHit();
                }
                finally
                {
                    CompleteRunOnce();
                }
            });
        }

        private void SignInCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RunCommandOnce(delegate
            {
                try
                {
                    var startViewModel = StartViewModel;
                    if (startViewModel != null) startViewModel.ProcessSignInButtonClicked();
                    HandleWPFHit();
                }
                finally
                {
                    CompleteRunOnce();
                }
            });
        }
    }
}