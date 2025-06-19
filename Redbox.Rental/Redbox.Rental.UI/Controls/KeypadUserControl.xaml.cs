using System.Windows.Controls;
using System.Windows.Input;
using Redbox.Controls;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls
{
    public partial class KeypadUserControl : UserControl
    {
        public KeypadUserControl()
        {
            InitializeComponent();
            ApplyTheme();
        }

        private KeypadModel KeypadModel => DataContext as KeypadModel;

        private void ApplyTheme()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            if (theme != null) theme.SetStyle(OneButton);
            if (theme != null) theme.SetStyle(TwoButton);
            if (theme != null) theme.SetStyle(ThreeButton);
            if (theme != null) theme.SetStyle(FourButton);
            if (theme != null) theme.SetStyle(FiveButton);
            if (theme != null) theme.SetStyle(SixButton);
            if (theme != null) theme.SetStyle(SevenButton);
            if (theme != null) theme.SetStyle(EightButton);
            if (theme != null) theme.SetStyle(NineButton);
            if (theme != null) theme.SetStyle(ZeroButton);
            if (theme != null) theme.SetStyle(ClearAllButton);
            if (theme == null) return;
            theme.SetStyle(BackButton);
        }

        private void NumberCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var keypadModel = KeypadModel;
            if (keypadModel != null)
            {
                var roundedButton = e.OriginalSource as RoundedButton;
                string text;
                if (roundedButton == null)
                {
                    text = null;
                }
                else
                {
                    var tag = roundedButton.Tag;
                    text = tag != null ? tag.ToString() : null;
                }

                keypadModel.ProcessOnNumberButtonClicked(text);
            }

            Commands.ResetIdleTimerCommand.Execute(null, this);
        }

        private void ClearAllCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var keypadModel = KeypadModel;
            if (keypadModel != null) keypadModel.ProcessOnClearAllButtonClicked();
            Commands.ResetIdleTimerCommand.Execute(null, this);
        }

        private void BackCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var keypadModel = KeypadModel;
            if (keypadModel != null) keypadModel.ProcessOnBackButtonClicked();
            Commands.ResetIdleTimerCommand.Execute(null, this);
        }
    }
}