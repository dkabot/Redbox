using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "OptionSelectionDialog")]
    public partial class OptionSelectionDialogUserControl : TextToSpeechUserControl
    {
        public OptionSelectionDialogUserControl()
        {
            InitializeComponent();
        }

        private OptionSelectionDialogViewModel OptionSelectionDialogViewModel =>
            DataContext as OptionSelectionDialogViewModel;

        private void FormatSelectionPopupUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            for (var i = 0; i < ButtonsItemsControl.Items.Count; i++)
            {
                var frameworkElement =
                    (FrameworkElement)ButtonsItemsControl.ItemContainerGenerator.ContainerFromIndex(i);
                if (frameworkElement != null && theme != null) theme.SetStyle(frameworkElement);
            }

            if (theme != null) theme.SetStyle(CancelButton);
        }

        public override ISpeechControl GetSpeechControl()
        {
            var optionSelectionDialogViewModel = OptionSelectionDialogViewModel;
            if (optionSelectionDialogViewModel == null) return null;
            var onGetSpeechControl = optionSelectionDialogViewModel.OnGetSpeechControl;
            if (onGetSpeechControl == null) return null;
            return onGetSpeechControl();
        }
    }
}