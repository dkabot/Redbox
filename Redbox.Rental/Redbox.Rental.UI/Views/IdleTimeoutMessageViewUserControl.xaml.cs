using System.Windows;
using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "IdleTimeoutMessageView")]
    public partial class IdleTimeoutMessageViewUserControl : TextToSpeechUserControl
    {
        private IdleTimeoutMessageViewModel _idleTimeoutMessageViewModel;

        private object m_lockObject = new object();

        public IdleTimeoutMessageViewUserControl()
        {
            InitializeComponent();
            ApplyTheme();
            RegisterRoutedCommand("Yes", Commands.Yes);
            RegisterRoutedCommand("No", Commands.No);
        }

        private void ApplyTheme()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            if (theme != null) theme.SetStyle(YesButton);
            if (theme == null) return;
            theme.SetStyle(NoButton);
        }

        public override ISpeechControl GetSpeechControl()
        {
            return _idleTimeoutMessageViewModel.OnGetSpeechControl();
        }

        private void YesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var idleTimeoutMessageViewModel = _idleTimeoutMessageViewModel;
            if (idleTimeoutMessageViewModel == null) return;
            idleTimeoutMessageViewModel.ProcessOnButtonYesClicked();
        }

        private void NoCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var idleTimeoutMessageViewModel = _idleTimeoutMessageViewModel;
            if (idleTimeoutMessageViewModel == null) return;
            idleTimeoutMessageViewModel.ProcessOnButtonNoClicked();
        }

        private void TextToSpeechUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _idleTimeoutMessageViewModel = DataContext as IdleTimeoutMessageViewModel;
        }
    }
}