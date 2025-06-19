using System.Windows.Controls;
using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls
{
    public partial class BrowseCheckOutBarUserControl : UserControl, IWPFActor
    {
        public BrowseCheckOutBarUserControl()
        {
            InitializeComponent();
            ApplyTheme();
        }

        private BrowseViewModel BrowseViewModel => DataContext as BrowseViewModel;

        public event WPFHitHandler OnWPFHit;

        public IActor Actor { get; set; }

        private void ApplyTheme()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            if (theme != null) theme.SetStyle(checkout_button);
            if (theme == null) return;
            theme.SetStyle(SwitchProductFamilyButton1);
        }

        private void HandleWPFHit()
        {
            var service = ServiceLocator.Instance.GetService<IRenderingService>();
            if (service == null) return;
            var activeScene = service.ActiveScene;
            if (activeScene == null) return;
            activeScene.PrcoessWPFHit();
        }

        private void CheckOutCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var browseViewModel = BrowseViewModel;
            if (browseViewModel != null) browseViewModel.ProcessOnCheckOutButtonClicked();
            HandleWPFHit();
        }

        private void CheckOutCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ToggleTitleFamily1CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var browseViewModel = BrowseViewModel;
            if (browseViewModel != null) browseViewModel.ProcessOnToggleTitleFamilyButton1Clicked();
            HandleWPFHit();
        }

        private void SignInCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var browseViewModel = BrowseViewModel;
            if (browseViewModel != null) browseViewModel.ProcessOnSignInButtonClicked();
            HandleWPFHit();
        }

        private void SignOutCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var browseViewModel = BrowseViewModel;
            if (browseViewModel != null) browseViewModel.ProcessOnSignOutButtonClicked();
            HandleWPFHit();
        }

        private void MyPerksCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var browseViewModel = BrowseViewModel;
            if (browseViewModel != null) browseViewModel.ProcessOnMyPerksButtonClicked();
            HandleWPFHit();
        }
    }
}