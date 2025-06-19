using System.Collections.ObjectModel;
using System.Windows;
using Redbox.Rental.UI.ControllersLogic;

namespace Redbox.Rental.UI.Models
{
    public class MemberPerksPopupViewModel : BaseModel<MemberPerksPopupViewModel>
    {
        public static readonly DependencyProperty TitleProperty = CreateDependencyProperty("Title", TYPES.STRING);

        public static readonly DependencyProperty NextLevelRentalsProperty =
            CreateDependencyProperty("NextLevelRentals", TYPES.OBJECT);

        public static readonly DependencyProperty BulletPointsProperty =
            CreateDependencyProperty("BulletPoints", typeof(ObservableCollection<string>));

        public static readonly DependencyProperty MemberTierProperty =
            CreateDependencyProperty("MemberTier", typeof(TierType), TierType.Member);

        public DynamicRoutedCommand CancelButtonCommand { get; set; }

        public string CancelButtonText { get; set; }

        public string Title
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(TitleProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(TitleProperty, value); }); }
        }

        public string NextLevelRentals
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(NextLevelRentalsProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(NextLevelRentalsProperty, value); }); }
        }

        public ObservableCollection<string> BulletPoints
        {
            get { return Dispatcher.Invoke(() => (ObservableCollection<string>)GetValue(BulletPointsProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BulletPointsProperty, value); }); }
        }

        public TierType MemberTier
        {
            get { return Dispatcher.Invoke(() => (TierType)GetValue(MemberTierProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(MemberTierProperty, value); }); }
        }
    }
}