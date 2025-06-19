using System.Windows.Controls;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "ApplicationLoadView")]
    public partial class ApplicationLoadViewUserControl : UserControl
    {
        public ApplicationLoadViewUserControl()
        {
            InitializeComponent();
            Spinner.IsAnimated = true;
        }
    }
}