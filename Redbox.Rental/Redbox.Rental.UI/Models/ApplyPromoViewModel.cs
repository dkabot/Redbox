using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.ControllersLogic;

namespace Redbox.Rental.UI.Models
{
    public class ApplyPromoViewModel : BaseModel<ApplyPromoViewModel>, IPromoListModel
    {
        public static readonly DependencyProperty CurrentPagePromosProperty =
            CreateDependencyProperty("CurrentPagePromos", typeof(ObservableCollection<IPerksOfferListItem>),
                new ObservableCollection<IPerksOfferListItem>());

        public static readonly DependencyProperty CurrentPageNumberProperty =
            CreateDependencyProperty("CurrentPageNumber", TYPES.INT, 1);

        public Func<ISpeechControl> OnGetSpeechControl;

        public string TitleText { get; set; }

        public string TopMessage { get; set; }

        public string EnterPromoMessage { get; set; }

        public DynamicRoutedCommand EnterPromoCommand { get; set; }

        public string EnterPromoButtonText { get; set; }

        public DynamicRoutedCommand CancelCommand { get; set; }

        public string CancelButtonText { get; set; }

        public DynamicRoutedCommand OnLoadedCommand { get; set; }
        public PromoListLogic PromoLogic { get; set; }

        public List<IPerksOfferListItem> StoredPromoCodes { get; set; }

        public DynamicRoutedCommand NextPagePressedCommand { get; set; }

        public DynamicRoutedCommand PreviousPagePressedCommand { get; set; }

        public DynamicRoutedCommand PageNumberPressedCommand { get; set; }

        public int NumberOfPages { get; set; }

        public int PromosPerPage { get; set; }

        public DynamicRoutedCommand AddOrRemovePromoCommand { get; set; }

        public DynamicRoutedCommand DetailsCommand { get; set; }

        public ObservableCollection<IPerksOfferListItem> CurrentPagePromos
        {
            get
            {
                return Dispatcher.Invoke(() =>
                    (ObservableCollection<IPerksOfferListItem>)GetValue(CurrentPagePromosProperty));
            }
            set { Dispatcher.Invoke(delegate { SetValue(CurrentPagePromosProperty, value); }); }
        }

        public int CurrentPageNumber
        {
            get { return Dispatcher.Invoke(() => (int)GetValue(CurrentPageNumberProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(CurrentPageNumberProperty, value); }); }
        }
    }
}