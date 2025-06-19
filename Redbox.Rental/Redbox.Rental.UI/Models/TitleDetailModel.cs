using System;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.UI.Models
{
    public class TitleDetailModel : DependencyObject
    {
        public static readonly DependencyProperty BrowseProductControlModelProperty =
            DependencyProperty.Register("BrowseProductControlModel", typeof(BrowseControlModel),
                typeof(TitleDetailModel),
                new FrameworkPropertyMetadata(null, OnBrowseProductControlModelPropertyChanged));

        public static readonly DependencyProperty BackTextProperty = DependencyProperty.Register("BackText",
            typeof(string), typeof(TitleDetailModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty BackgroundImageVisibilityProperty =
            DependencyProperty.Register("BackgroundImageVisibility", typeof(Visibility), typeof(TitleDetailModel),
                new FrameworkPropertyMetadata(Visibility.Collapsed));

        public Func<ISpeechControl> OnGetSpeechControl;

        public BrowseControlModel BrowseProductControlModel
        {
            get { return Dispatcher.Invoke(() => (BrowseControlModel)GetValue(BrowseProductControlModelProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BrowseProductControlModelProperty, value); }); }
        }

        public string BackText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(BackTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BackTextProperty, value); }); }
        }

        public Visibility BackgroundImageVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(BackgroundImageVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BackgroundImageVisibilityProperty, value); }); }
        }

        public event Action<IBrowseItemModel, object, TitleType, bool> RentProduct;

        public event Action<IBrowseItemModel, object> BuyButtonHit;

        public event Action<IBrowseItemModel> MoreInfoHit;

        public event Action GoBackHit;

        public event Action<IBrowseItemModel> SeeFullDetails;

        public event Action RedboxPlusInfo;

        public event Action DualInStockLearnMore;

        public event Action StatisticPopup;

        public event Action<TitleDetailModel> OnTitleDetailModelChanged;

        internal ISpeechControl ProcessGetSpeechControl()
        {
            var onGetSpeechControl = OnGetSpeechControl;
            if (onGetSpeechControl == null) return null;
            return onGetSpeechControl();
        }

        public void ProcessAddProduct(IBrowseItemModel model, object o, TitleType t)
        {
            var rentProduct = RentProduct;
            if (rentProduct == null) return;
            rentProduct(model, o, t, false);
        }

        public void ProcessBuyButtonHit(IBrowseItemModel model, object o)
        {
            var buyButtonHit = BuyButtonHit;
            if (buyButtonHit == null) return;
            buyButtonHit(model, o);
        }

        public void ProcessMoreInfoHit(IBrowseItemModel model)
        {
            var moreInfoHit = MoreInfoHit;
            if (moreInfoHit == null) return;
            moreInfoHit(model);
        }

        public void ProcessGoBackHit()
        {
            var goBackHit = GoBackHit;
            if (goBackHit == null) return;
            goBackHit();
        }

        public void ProcessSeeFullDetails(IBrowseItemModel model)
        {
            var seeFullDetails = SeeFullDetails;
            if (seeFullDetails == null) return;
            seeFullDetails(model);
        }

        public void ProcessRedboxPlusInfo()
        {
            var redboxPlusInfo = RedboxPlusInfo;
            if (redboxPlusInfo == null) return;
            redboxPlusInfo();
        }

        public void ProcessDualInStockLearnMore()
        {
            var dualInStockLearnMore = DualInStockLearnMore;
            if (dualInStockLearnMore == null) return;
            dualInStockLearnMore();
        }

        public void ProcessStatisticPopup()
        {
            var statisticPopup = StatisticPopup;
            if (statisticPopup == null) return;
            statisticPopup();
        }

        private static void OnBrowseProductControlModelPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var titleDetailModel = d as TitleDetailModel;
            if (titleDetailModel != null && titleDetailModel.OnTitleDetailModelChanged != null)
                titleDetailModel.OnTitleDetailModelChanged(titleDetailModel);
        }
    }
}