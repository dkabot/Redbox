using System.Windows;

namespace Redbox.Rental.UI.Controls
{
    public partial class PerksOfferActiveListItem : BaseUserControl
    {
        private static readonly DependencyProperty DetailsButtonCommandProperty =
            DependencyProperty.Register("DetailsButtonCommand", typeof(DynamicRoutedCommand),
                typeof(PerksOfferActiveListItem), new PropertyMetadata(null));

        public PerksOfferActiveListItem()
        {
            InitializeComponent();
        }

        public DynamicRoutedCommand DetailsButtonCommand
        {
            get { return Dispatcher.Invoke(() => (DynamicRoutedCommand)GetValue(DetailsButtonCommandProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(DetailsButtonCommandProperty, value); }); }
        }
    }
}