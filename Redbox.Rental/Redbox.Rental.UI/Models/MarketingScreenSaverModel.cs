using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace Redbox.Rental.UI.Models
{
    public class MarketingScreenSaverModel : BaseModel<MarketingScreenSaverModel>
    {
        public static readonly DependencyProperty IsAnimationOnProperty =
            CreateDependencyProperty("IsAnimationOn", typeof(bool), false);

        public static readonly DependencyProperty GridHeightProperty = DependencyProperty.Register("GridHeight",
            typeof(int), typeof(MarketingScreenSaverModel), new PropertyMetadata(768));

        public Action OnLeaveLastImage;
        public ObservableCollection<ClickableImageModel> ImageModels { get; set; }

        public DynamicRoutedCommand BrowseCommand { get; set; }

        public string StartButtonText { get; set; }

        public bool IsAnimationOn
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(IsAnimationOnProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(IsAnimationOnProperty, value); }); }
        }

        public int GridHeight
        {
            get { return Dispatcher.Invoke(() => (int)GetValue(GridHeightProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(GridHeightProperty, value); }); }
        }
    }
}