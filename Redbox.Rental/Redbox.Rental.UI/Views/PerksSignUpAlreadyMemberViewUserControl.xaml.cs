using System.Windows.Controls;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "PerksSignUpAlreadyMemberView")]
    public partial class PerksSignUpAlreadyMemberViewUserControl : ViewUserControl
    {
        public PerksSignUpAlreadyMemberViewUserControl()
        {
            InitializeComponent();
            ApplyTheme<Control>(MainControl);
        }
    }
}