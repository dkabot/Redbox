using System.Windows.Controls;
using System.Windows.Input;
using Redbox.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls
{
    public partial class CheckMarkButtonUserControl : UserControl
    {
        public static readonly RoutedCommand Command = new RoutedCommand();

        public CheckMarkButtonUserControl()
        {
            InitializeComponent();
        }

        private CheckMarkButtonModel CheckMarkButtonModel => DataContext as CheckMarkButtonModel;

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var roundedButton = e.OriginalSource as RoundedButton;
            var checkMarkButtonModel =
                (roundedButton != null ? roundedButton.DataContext : null) as CheckMarkButtonModel;
            var checkMarkButtonModel2 = CheckMarkButtonModel;
            if (checkMarkButtonModel2 == null) return;
            checkMarkButtonModel2.ProcessOnClicked(checkMarkButtonModel);
        }
    }
}