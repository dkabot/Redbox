using System.Windows.Controls;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "PerksSignUpAlreadyMemberOptInView")]
    public partial class PerksSignUpAlreadyMemberOptInViewUserControl : ViewUserControl
    {
        public PerksSignUpAlreadyMemberOptInViewUserControl()
        {
            InitializeComponent();
            ApplyTheme<Control>(MainControl);
        }
    }
}