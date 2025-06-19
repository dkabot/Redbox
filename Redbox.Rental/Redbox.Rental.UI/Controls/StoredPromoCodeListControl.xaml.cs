using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls
{
    public partial class StoredPromoCodeListControl : BaseUserControl
    {
        private static readonly DependencyProperty PromoCodeModelsProperty =
            DependencyProperty.Register("PromoCodeModels", typeof(List<IPerksOfferListItem>),
                typeof(StoredPromoCodeListControl), new PropertyMetadata(null, PromoCodeModelsChanged));

        private static readonly DependencyProperty IsAdaModeProperty = DependencyProperty.Register("IsAdaMode",
            typeof(bool), typeof(StoredPromoCodeListControl), new PropertyMetadata(false, IsAdaModeChanged));

        private static readonly DependencyProperty ShowAdaLeftArrowProperty =
            DependencyProperty.Register("ShowAdaLeftArrow", typeof(bool), typeof(StoredPromoCodeListControl),
                new PropertyMetadata(false));

        private static readonly DependencyProperty ShowAdaRightArrowProperty =
            DependencyProperty.Register("ShowAdaRightArrow", typeof(bool), typeof(StoredPromoCodeListControl),
                new PropertyMetadata(false));

        private static readonly DependencyProperty CurrentPageNumberProperty =
            DependencyProperty.Register("CurrentPageNumber", typeof(int), typeof(StoredPromoCodeListControl),
                new PropertyMetadata(1, CurrentPageNumberChanged));

        private static readonly DependencyProperty CurrentPageCountProperty =
            DependencyProperty.Register("CurrentPageCount", typeof(int), typeof(StoredPromoCodeListControl),
                new PropertyMetadata(1, CurrentPageNumberChanged));

        private static readonly DependencyProperty ItemsPerPageProperty = DependencyProperty.Register("ItemsPerPage",
            typeof(int), typeof(StoredPromoCodeListControl), new PropertyMetadata(3, ItemsPerPageChanged));

        private static readonly DependencyProperty NextPageClickedCommandProperty =
            DependencyProperty.Register("NextPageClickedCommand", typeof(DynamicRoutedCommand),
                typeof(StoredPromoCodeListControl), new PropertyMetadata(null));

        private static readonly DependencyProperty PreviousPageClickedCommandProperty =
            DependencyProperty.Register("PreviousPageClickedCommand", typeof(DynamicRoutedCommand),
                typeof(StoredPromoCodeListControl), new PropertyMetadata(null));

        private static readonly DependencyProperty PageNumberClickedCommandProperty =
            DependencyProperty.Register("PageNumberClickedCommand", typeof(DynamicRoutedCommand),
                typeof(StoredPromoCodeListControl), new PropertyMetadata(null));

        private static readonly DependencyProperty PageShownCommandProperty =
            DependencyProperty.Register("PageShownCommand", typeof(DynamicRoutedCommand),
                typeof(StoredPromoCodeListControl), new PropertyMetadata(null));

        private static readonly DependencyPropertyKey NumberOfPagesPropertyKey =
            DependencyProperty.RegisterReadOnly("NumberOfPages", typeof(int), typeof(StoredPromoCodeListControl),
                new PropertyMetadata(0, NumberOfPagesChanged));

        public static readonly DependencyProperty NumberOfPagesProperty = NumberOfPagesPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey PageControlVisibilityPropertyKey =
            DependencyProperty.RegisterReadOnly("PageControlVisibility", typeof(Visibility),
                typeof(StoredPromoCodeListControl), new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty PageControlVisibilityProperty =
            PageControlVisibilityPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey AdaArrowColumnWidthPropertyKey =
            DependencyProperty.RegisterReadOnly("AdaArrowColumnWidth", typeof(double),
                typeof(StoredPromoCodeListControl), new PropertyMetadata(0.0));

        public static readonly DependencyProperty AdaArrowColumnWidthProperty =
            PageControlVisibilityPropertyKey.DependencyProperty;

        private static readonly DependencyProperty CurrentPagePromosProperty =
            DependencyProperty.Register("CurrentPagePromos", typeof(ObservableCollection<IPerksOfferListItem>),
                typeof(StoredPromoCodeListControl),
                new PropertyMetadata(new ObservableCollection<IPerksOfferListItem>()));

        private static readonly DependencyProperty AdaButtonDataProperty = DependencyProperty.Register("AdaButtonData",
            typeof(ObservableCollection<AdaButtonInfo>), typeof(StoredPromoCodeListControl),
            new PropertyMetadata(new ObservableCollection<AdaButtonInfo>()));

        private static readonly DependencyProperty AdaLeftArrowPressedCommandProperty =
            DependencyProperty.Register("AdaLeftArrowPressedCommand", typeof(DynamicRoutedCommand),
                typeof(StoredPromoCodeListControl), new PropertyMetadata(null));

        private static readonly DependencyProperty AdaRightArrowPressedCommandProperty =
            DependencyProperty.Register("AdaRightArrowPressedCommand", typeof(DynamicRoutedCommand),
                typeof(StoredPromoCodeListControl), new PropertyMetadata(null));

        private int _isUpdatingPage;

        public StoredPromoCodeListControl()
        {
            InitializeComponent();
            PageControl.ButtonPressed += PageControl_ButtonPressed;
            AdaLeftArrowPressedCommand =
                DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), AdaLeftArrowPressed);
            AdaRightArrowPressedCommand =
                DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), AdaRightArrowPressed);
        }

        public List<IPerksOfferListItem> PromoCodeModels
        {
            get { return Dispatcher.Invoke(() => (List<IPerksOfferListItem>)GetValue(PromoCodeModelsProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PromoCodeModelsProperty, value); }); }
        }

        public bool IsAdaMode
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(IsAdaModeProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(IsAdaModeProperty, value); }); }
        }

        public bool ShowAdaLeftArrow
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(ShowAdaLeftArrowProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ShowAdaLeftArrowProperty, value); }); }
        }

        public bool ShowAdaRightArrow
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(ShowAdaRightArrowProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ShowAdaRightArrowProperty, value); }); }
        }

        public int CurrentPageNumber
        {
            get { return Dispatcher.Invoke(() => (int)GetValue(CurrentPageNumberProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(CurrentPageNumberProperty, value); }); }
        }

        public int CurrentPageCount
        {
            get { return Dispatcher.Invoke(() => (int)GetValue(CurrentPageCountProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(CurrentPageCountProperty, value); }); }
        }

        public int ItemsPerPage
        {
            get { return Dispatcher.Invoke(() => (int)GetValue(ItemsPerPageProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ItemsPerPageProperty, value); }); }
        }

        public DynamicRoutedCommand NextPageClickedCommand
        {
            get { return Dispatcher.Invoke(() => (DynamicRoutedCommand)GetValue(NextPageClickedCommandProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(NextPageClickedCommandProperty, value); }); }
        }

        public DynamicRoutedCommand PreviousPageClickedCommand
        {
            get { return Dispatcher.Invoke(() => (DynamicRoutedCommand)GetValue(PreviousPageClickedCommandProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PreviousPageClickedCommandProperty, value); }); }
        }

        public DynamicRoutedCommand PageNumberClickedCommand
        {
            get { return Dispatcher.Invoke(() => (DynamicRoutedCommand)GetValue(PageNumberClickedCommandProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PageNumberClickedCommandProperty, value); }); }
        }

        public DynamicRoutedCommand PageShownCommand
        {
            get { return Dispatcher.Invoke(() => (DynamicRoutedCommand)GetValue(PageShownCommandProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PageShownCommandProperty, value); }); }
        }

        public int NumberOfPages
        {
            get { return Dispatcher.Invoke(() => (int)GetValue(NumberOfPagesProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(NumberOfPagesPropertyKey, value); }); }
        }

        public Visibility PageControlVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(PageControlVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PageControlVisibilityPropertyKey, value); }); }
        }

        public double AdaArrowColumnWidth
        {
            get { return Dispatcher.Invoke(() => (double)GetValue(AdaArrowColumnWidthProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(AdaArrowColumnWidthPropertyKey, value); }); }
        }

        public ObservableCollection<IPerksOfferListItem> CurrentPagePromos
        {
            get
            {
                return Dispatcher.Invoke(() =>
                    (ObservableCollection<IPerksOfferListItem>)GetValue(CurrentPagePromosProperty));
            }
            set { Dispatcher.Invoke(delegate { SetValue(CurrentPagePromosProperty, value); }); }
        }

        public ObservableCollection<AdaButtonInfo> AdaButtonData
        {
            get
            {
                return Dispatcher.Invoke(() => (ObservableCollection<AdaButtonInfo>)GetValue(AdaButtonDataProperty));
            }
            set { Dispatcher.Invoke(delegate { SetValue(AdaButtonDataProperty, value); }); }
        }

        public DynamicRoutedCommand AdaLeftArrowPressedCommand
        {
            get { return Dispatcher.Invoke(() => (DynamicRoutedCommand)GetValue(AdaLeftArrowPressedCommandProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(AdaLeftArrowPressedCommandProperty, value); }); }
        }

        public DynamicRoutedCommand AdaRightArrowPressedCommand
        {
            get { return Dispatcher.Invoke(() => (DynamicRoutedCommand)GetValue(AdaRightArrowPressedCommandProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(AdaRightArrowPressedCommandProperty, value); }); }
        }

        private static void PromoCodeModelsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var storedPromoCodeListControl = d as StoredPromoCodeListControl;
            if (storedPromoCodeListControl != null) storedPromoCodeListControl.UpdatePageInfo();
        }

        private static void IsAdaModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var storedPromoCodeListControl = d as StoredPromoCodeListControl;
            if (storedPromoCodeListControl != null) storedPromoCodeListControl.UpdatePageInfo();
        }

        private void UpdatePageInfo()
        {
            if (Interlocked.CompareExchange(ref _isUpdatingPage, 1, 0) == 1) return;
            try
            {
                CurrentPagePromos = new ObservableCollection<IPerksOfferListItem>();
                AdaButtonData = new ObservableCollection<AdaButtonInfo>();
                var promoCodeModels = PromoCodeModels;
                if ((promoCodeModels != null ? promoCodeModels.Count : 0) > 0)
                {
                    NumberOfPages =
                        (int)Math.Round(PromoCodeModels.Sum(m => m.NumberOfSpaces) / (double)ItemsPerPage + 0.4);
                    CurrentPageNumber = Math.Min(CurrentPageNumber, NumberOfPages);
                    CurrentPageNumber = Math.Max(CurrentPageNumber, 1);
                    var num = 0;
                    var num2 = 1;
                    using (var enumerator = PromoCodeModels.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            var perksOfferListItem = enumerator.Current;
                            num += perksOfferListItem.NumberOfSpaces;
                            if (num > (CurrentPageNumber - 1) * ItemsPerPage && num <= CurrentPageNumber * ItemsPerPage)
                            {
                                CurrentPagePromos.Add(perksOfferListItem);
                                perksOfferListItem.AdaButtonNumber = num2;
                                var flag = true;
                                DynamicRoutedCommand dynamicRoutedCommand;
                                if (perksOfferListItem is StoredPromoCodeModel)
                                {
                                    dynamicRoutedCommand = ((StoredPromoCodeModel)perksOfferListItem)
                                        .AddRemoveButtonCommand;
                                }
                                else
                                {
                                    flag = ((PerksOfferListItemModel)perksOfferListItem).OfferStatus != "Completed";
                                    dynamicRoutedCommand = ((PerksOfferListItemModel)perksOfferListItem)
                                        .DetailsButtonCommand;
                                }

                                if (flag)
                                {
                                    AdaButtonData.Add(new AdaButtonInfo
                                    {
                                        AdaButtonNumber = num2,
                                        ButtonCommand = dynamicRoutedCommand,
                                        Item = perksOfferListItem
                                    });
                                    num2++;
                                }
                            }
                        }

                        goto IL_01A6;
                    }
                }

                NumberOfPages = 0;
                CurrentPageNumber = 0;
                IL_01A6:
                PageControlVisibility = !IsAdaMode && NumberOfPages > 1 ? Visibility.Visible : Visibility.Collapsed;
                ShowAdaLeftArrow = IsAdaMode && CurrentPageNumber > 1;
                ShowAdaRightArrow = IsAdaMode && CurrentPageNumber < NumberOfPages;
                AdaArrowColumnWidth = IsAdaMode ? 60 : 0;
            }
            finally
            {
                _isUpdatingPage = 0;
            }
        }

        private static void CurrentPageNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var storedPromoCodeListControl = d as StoredPromoCodeListControl;
            if (storedPromoCodeListControl != null) storedPromoCodeListControl.UpdatePageInfo();
        }

        private static void ItemsPerPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var storedPromoCodeListControl = d as StoredPromoCodeListControl;
            if (storedPromoCodeListControl != null)
            {
                if (storedPromoCodeListControl.ItemsPerPage < 1) storedPromoCodeListControl.ItemsPerPage = 1;
                storedPromoCodeListControl.UpdatePageInfo();
            }
        }

        private static void NumberOfPagesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var storedPromoCodeListControl = d as StoredPromoCodeListControl;
            if (storedPromoCodeListControl != null)
                storedPromoCodeListControl.CurrentPageCount = storedPromoCodeListControl.NumberOfPages;
        }

        private void PageControl_ButtonPressed(object sender, PageButtonPressedArgs e)
        {
            var buttonType = e.ButtonType;
            if (buttonType != PageButtonType.LeftArrow)
            {
                if (buttonType != PageButtonType.RightArrow)
                {
                    var pageNumberClickedCommand = PageNumberClickedCommand;
                    if (pageNumberClickedCommand != null) pageNumberClickedCommand.Execute(e.NewPageNumber);
                }
                else
                {
                    var nextPageClickedCommand = NextPageClickedCommand;
                    if (nextPageClickedCommand != null) nextPageClickedCommand.Execute(null);
                }
            }
            else
            {
                var previousPageClickedCommand = PreviousPageClickedCommand;
                if (previousPageClickedCommand != null) previousPageClickedCommand.Execute(null);
            }

            HandleWPFHit();
        }

        private void AdaRightArrowPressed()
        {
            CurrentPageNumber++;
            var nextPageClickedCommand = NextPageClickedCommand;
            if (nextPageClickedCommand == null) return;
            nextPageClickedCommand.Execute(null);
        }

        private void AdaLeftArrowPressed()
        {
            CurrentPageNumber--;
            var previousPageClickedCommand = PreviousPageClickedCommand;
            if (previousPageClickedCommand == null) return;
            previousPageClickedCommand.Execute(null);
        }
    }
}