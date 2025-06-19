using System.Windows;
using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "BrowseView")]
    public partial class BrowseViewUserControl : TextToSpeechUserControl, IWPFActor
    {
        private BrowseViewModel _browseViewModel;

        public BrowseViewUserControl()
        {
            InitializeComponent();
            ApplyTheme();
        }

        public override ISpeechControl GetSpeechControl()
        {
            var browseViewModel = _browseViewModel;
            if (browseViewModel == null) return null;
            return browseViewModel.ProcessGetSpeechControl();
        }

        private void ApplyTheme()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            if (service == null) return;
            var currentTheme = service.CurrentTheme;
        }

        private void ResetIdleTimerCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            HandleWPFHit();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _browseViewModel = DataContext as BrowseViewModel;
        }
    }
}