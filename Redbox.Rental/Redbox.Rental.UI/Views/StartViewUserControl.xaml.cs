using System.Windows;
using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    public partial class StartViewUserControl : TextToSpeechUserControl, IWPFActor
    {
        public StartViewUserControl()
        {
            InitializeComponent();
            ApplyTheme();
            RegisterRoutedCommand("RentOrBuyMoviesCommand", UI.Commands.RentOrBuyMoviesCommand);
            RegisterRoutedCommand("BuyMoviesCommand", UI.Commands.BuyMoviesCommand);
            RegisterRoutedCommand("ReturnProductCommand", UI.Commands.ReturnProductCommand);
            RegisterRoutedCommand("PickupProductCommand", UI.Commands.PickupProductCommand);
            RegisterRoutedCommand("ToggleADAModeCommand", UI.Commands.ToggleADAModeCommand);
            RegisterRoutedCommand("ToggleLanguageModeCommand", UI.Commands.ToggleLanguageModeCommand);
            RegisterRoutedCommand("HelpCommand", UI.Commands.HelpCommand);
            RegisterRoutedCommand("SignInCommand", UI.Commands.SignInCommand);
            RegisterRoutedCommand("SignInTooltipCommand", UI.Commands.SignInTooltipCommand);
            RegisterRoutedCommand("ExitBurnInViewCommand", UI.Commands.ExitBurnInViewCommand);
            RegisterRoutedCommand("BannerClickCommand", UI.Commands.BannerClickCommand);
            PostHealthActivity();
        }

        private StartViewModel StartViewModel => DataContext as StartViewModel;

        private void ApplyTheme()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            if (theme != null) theme.SetStyle(all_movies_button);
            if (theme != null) theme.SetStyle(buy_movies_button);
            if (theme != null) theme.SetStyle(online_pickup_button);
            if (theme != null) theme.SetStyle(return_button);
            if (theme != null) theme.SetStyle(sign_in_button);
            if (theme != null) theme.SetStyle(handicap_button);
            if (theme != null) theme.SetStyle(espanol_button);
            if (theme == null) return;
            theme.SetStyle(help_button);
        }

        private void RentOrBuyMoviesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RunCommandOnce(delegate
            {
                var startViewModel = StartViewModel;
                if (startViewModel != null) startViewModel.ProcessRentMovieButtonClick();
                HandleWPFHit();
            });
        }

        private void RentOrBuyMoviesCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = all_movies_button.Visibility == Visibility.Visible;
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

        private void SignInTooltipCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RunCommandOnce(delegate
            {
                try
                {
                    var startViewModel = StartViewModel;
                    if (startViewModel != null) startViewModel.ProcessSignInButtonTooltipClicked();
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