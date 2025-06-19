using System.Windows.Controls;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "PerksThanksView")]
    public partial class PerksThanksViewUserControl : ViewUserControl
    {
        public PerksThanksViewUserControl()
        {
            InitializeComponent();
            ApplyTheme<Control>(MainControl);
        }
    }
}