using System.Windows.Controls;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "SonySweepstakesConfirmationView")]
    public partial class SonySweepstakesConfirmationViewUserControl : ViewUserControl
    {
        public SonySweepstakesConfirmationViewUserControl()
        {
            InitializeComponent();
            ApplyTheme<Control>(MainControl);
        }
    }
}