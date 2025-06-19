using System.Windows.Controls;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "PerksConfirmView")]
    public partial class PerksConfirmViewUserControl : ViewUserControl
    {
        public PerksConfirmViewUserControl()
        {
            InitializeComponent();
            ApplyTheme<Control>(MainControl);
            SpeechViewName = "perks_confirm_view";
        }

        private PerksConfirmViewModel Model { get; set; }

        protected override void DataContextHasChanged()
        {
            if (DataContext == null || !(DataContext is PerksConfirmViewModel)) return;
            Model = DataContext as PerksConfirmViewModel;
        }

        public override ISpeechControl GetSpeechControl()
        {
            var perksConfirmViewModel = (PerksConfirmViewModel)DataContext;
            if (perksConfirmViewModel == null) return null;
            var processGetSpeechControl = perksConfirmViewModel.ProcessGetSpeechControl;
            if (processGetSpeechControl == null) return null;
            return processGetSpeechControl();
        }
    }
}