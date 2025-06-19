using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "ApplyPromoView")]
    public partial class ApplyPromoViewUserControl : TextToSpeechUserControl
    {
        public ApplyPromoViewUserControl()
        {
            InitializeComponent();
            Loaded += ApplyPromoViewUserControl_Loaded;
        }

        private ApplyPromoViewModel Model => DataContext as ApplyPromoViewModel;

        private void ApplyPromoViewUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyTheme();
            var model = Model;
            if (model == null) return;
            model.OnLoadedCommand.Execute(null);
        }

        private void ApplyTheme()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            if (theme != null) theme.SetStyle(CancelButton);
            if (theme == null) return;
            theme.SetStyle(EnterPromoButton);
        }

        public override ISpeechControl GetSpeechControl()
        {
            var model = Model;
            if (model == null) return null;
            return model.OnGetSpeechControl();
        }
    }
}