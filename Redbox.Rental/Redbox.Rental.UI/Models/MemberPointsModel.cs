using System.Collections.Generic;
using System.Windows;
using Redbox.Rental.UI.ControllersLogic;

namespace Redbox.Rental.UI.Models
{
    public class MemberPointsModel : BaseModel<MemberPointsModel>
    {
        public static readonly DependencyProperty PointsTitleProperty =
            CreateDependencyProperty("PointsTitle", TYPES.STRING);

        public static readonly DependencyProperty PointsMessageProperty =
            CreateDependencyProperty("PointsMessage", TYPES.STRING);

        public static readonly DependencyProperty PointsCartMessageProperty =
            CreateDependencyProperty("PointsCartMessage", TYPES.STRING);

        public static readonly DependencyProperty PointsMarketingProperty =
            CreateDependencyProperty("PointsMarketing", TYPES.STRING);

        public static readonly DependencyProperty CancelButtonTextProperty =
            CreateDependencyProperty("CancelButtonText", TYPES.STRING);

        public static readonly DependencyProperty ContinueButtonTextProperty =
            CreateDependencyProperty("ContinueButtonText", TYPES.STRING);

        public static readonly DependencyProperty ContinueButtonEnabledProperty =
            CreateDependencyProperty("ContinueButtonEnabled", TYPES.BOOL, false);

        public static readonly DependencyProperty SelectedProductIdsProperty =
            CreateDependencyProperty("SelectedProductIds", typeof(List<long>));

        public static readonly DependencyProperty DisplayProductModelsProperty =
            CreateDependencyProperty("DisplayProductModels", typeof(List<DisplayProductModel>));

        public MemberPointsModel()
        {
            SelectedProductIds = new List<long>();
            EntryShoppingProductIds = new List<long>();
        }

        public MemberPointsLogic Logic { get; set; }

        public List<long> EntryShoppingProductIds { get; set; }

        public DynamicRoutedCommand PointsIncludeCommand { get; set; }

        public DynamicRoutedCommand PointsExcludeCommand { get; set; }

        public DynamicRoutedCommand CancelButtonCommand { get; set; }

        public DynamicRoutedCommand ContinueButtonCommand { get; set; }

        public string PointsTitle
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(PointsTitleProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PointsTitleProperty, value); }); }
        }

        public string PointsMessage
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(PointsMessageProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PointsMessageProperty, value); }); }
        }

        public string PointsCartMessage
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(PointsCartMessageProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PointsCartMessageProperty, value); }); }
        }

        public string PointsMarketing
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(PointsMarketingProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PointsMarketingProperty, value); }); }
        }

        public string CancelButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(CancelButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(CancelButtonTextProperty, value); }); }
        }

        public string ContinueButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(ContinueButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ContinueButtonTextProperty, value); }); }
        }

        public bool ContinueButtonEnabled
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(ContinueButtonEnabledProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ContinueButtonEnabledProperty, value); }); }
        }

        public List<long> SelectedProductIds
        {
            get { return Dispatcher.Invoke(() => (List<long>)GetValue(SelectedProductIdsProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(SelectedProductIdsProperty, value); }); }
        }

        public List<DisplayProductModel> DisplayProductModels
        {
            get { return Dispatcher.Invoke(() => (List<DisplayProductModel>)GetValue(DisplayProductModelsProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(DisplayProductModelsProperty, value); }); }
        }
    }
}