using System;
using System.Windows;
using System.Windows.Input;
using Redbox.Controls;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.Model.Session;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "HelpViewUserControl")]
    public partial class HelpViewUserControl : TextToSpeechUserControl, IWPFActor
    {
        public HelpViewUserControl()
        {
            InitializeComponent();
            Loaded += HelpViewUserControl_Loaded;
            RegisterRoutedCommand("ShowDocument", HelpViewCommands.ShowDocument);
            RegisterRoutedCommand("PageLeft", HelpViewCommands.PageLeft);
            RegisterRoutedCommand("PageRight", HelpViewCommands.PageRight);
            RegisterRoutedCommand("GoBack", HelpViewCommands.GoBack);
        }

        private HelpViewModel Model => (HelpViewModel)DataContext;

        private void HelpViewUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            if (theme != null) theme.SetStyle(back_button);
            for (var i = 0; i < buttons_items_control.Items.Count; i++)
            {
                var frameworkElement =
                    (FrameworkElement)buttons_items_control.ItemContainerGenerator.ContainerFromIndex(i);
                if (frameworkElement != null && theme != null) theme.SetStyle(frameworkElement);
            }
        }

        private void GoBackCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var model = Model;
            if (model != null) model.ProcessGoBack();
            HandleWPFHit();
        }

        private void ShowDocumentCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var flag = false;
            var helpDocuments = HelpDocuments.Faq;
            var roundedButton = e.OriginalSource as RoundedButton;
            if (roundedButton != null)
            {
                helpDocuments = (HelpDocuments)roundedButton.Tag;
                flag = true;
            }
            else if (e.Parameter != null)
            {
                try
                {
                    helpDocuments = (HelpDocuments)Enum.Parse(typeof(HelpDocuments), e.Parameter.ToString());
                    flag = true;
                }
                catch
                {
                }
            }

            if (flag)
            {
                var model = Model;
                if (model != null) model.ProcessHelpDocumentButtonPress(helpDocuments);
            }

            HandleWPFHit();
        }

        private void RedboxLogoCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var model = Model;
            if (model == null) return;
            model.ProcessRedboxLogoCommand();
        }

        private void HandleHelpPaddleButtonPresed(string source)
        {
            var model = Model;
            if (model == null) return;
            model.ProcessHelpPaddleButtonPressed(source);
        }

        public override ISpeechControl GetSpeechControl()
        {
            var model = Model;
            if (model == null) return null;
            return model.OnGetSpeechControl();
        }
    }
}