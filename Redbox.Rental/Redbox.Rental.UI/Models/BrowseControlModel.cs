using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Redbox.Rental.Model.Browse;

namespace Redbox.Rental.UI.Models
{
    public class BrowseControlModel : DependencyObject
    {
        public static readonly DependencyProperty BottomPageNumberVisibilityProperty =
            DependencyProperty.Register("BottomPageNumberVisibility", typeof(Visibility), typeof(BrowseControlModel),
                new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty ShowPaddlesProperty = DependencyProperty.Register("ShowPaddles",
            typeof(bool), typeof(TitleDetailModel), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty SelectedDisplayProductUserControlSizeAndPositionProperty =
            DependencyProperty.Register("SelectedDisplayProductUserControlSizeAndPosition", typeof(SizeAndPosition),
                typeof(BrowseControlModel),
                new FrameworkPropertyMetadata(new SizeAndPosition(),
                    OnSelectedDisplayProductUserControlSizeAndPositionPropertyChanged));

        public static readonly DependencyProperty TitleRollupOverlayModelProperty =
            DependencyProperty.Register("TitleRollupOverlayModel", typeof(TitleRollupOverlayModel),
                typeof(BrowseControlModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty NoResultsTextProperty = DependencyProperty.Register("NoResultsText",
            typeof(string), typeof(BrowseControlModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty PageNumberProperty = DependencyProperty.Register("PageNumber",
            typeof(int), typeof(BrowseControlModel), new FrameworkPropertyMetadata(1));

        public static readonly DependencyProperty BrowseItemsProperty = DependencyProperty.Register("BrowseItems",
            typeof(List<IBrowseItemModel>), typeof(BrowseControlModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty RedboxPlusMiniCartVisiblityProperty =
            DependencyProperty.Register("RedboxPlusMiniCartVisiblity", typeof(Visibility), typeof(BrowseControlModel),
                new FrameworkPropertyMetadata(Visibility.Collapsed));

        public Visibility BorderShadowVisibility { get; set; }

        public BrowseLayoutDirection LayoutDirection { get; set; }

        public int RowCount { get; set; }

        public int ColumnCount { get; set; }

        public int PageNumber
        {
            get { return Dispatcher.Invoke(() => (int)GetValue(PageNumberProperty)); }
        }

        public bool UseFullPageWidth { get; set; }

        public bool ShowNoResultsMessage { get; set; }

        public Color BackgroundColor { get; set; } = Color.FromRgb(229, 229, 229);

        public int VisibleItemCount => RowCount * ColumnCount;

        public bool ShowBorderItems { get; set; }

        public Visibility PageNumberVisibility { get; set; }

        public Visibility BottomPageNumberVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(BottomPageNumberVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BottomPageNumberVisibilityProperty, value); }); }
        }

        public double PaddleOverlapWidth { get; set; }

        public Thickness BrowseItemMargin { get; set; }

        public Thickness Margin { get; set; }

        public List<Control> BrowseControls { get; set; } = new List<Control>();

        public List<IBrowseItemModel> BrowseItems
        {
            get { return Dispatcher.Invoke(() => (List<IBrowseItemModel>)GetValue(BrowseItemsProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BrowseItemsProperty, value); }); }
        }

        public SizeAndPosition SelectedBrowseItemSizeAndPosition
        {
            get
            {
                return Dispatcher.Invoke(() =>
                    (SizeAndPosition)GetValue(SelectedDisplayProductUserControlSizeAndPositionProperty));
            }
            set
            {
                Dispatcher.Invoke(delegate
                {
                    SetValue(SelectedDisplayProductUserControlSizeAndPositionProperty, value);
                });
            }
        }

        public string NoResultsText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(NoResultsTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(NoResultsTextProperty, value); }); }
        }

        public int ItemsPerPage
        {
            get { return Dispatcher.Invoke(() => RowCount * ColumnCount); }
        }

        public int ItemsOnCurrentPage
        {
            get
            {
                return Dispatcher.Invoke(delegate
                {
                    if (PageNumber >= TotalPages)
                    {
                        var browseItems = BrowseItems;
                        return (browseItems != null ? browseItems.Count : 0) % ItemsPerPage;
                    }

                    return ItemsPerPage;
                });
            }
        }

        public int TotalPages
        {
            get
            {
                return Dispatcher.Invoke(delegate
                {
                    var browseItems = BrowseItems;
                    var num = (browseItems != null ? browseItems.Count : 0) / ItemsPerPage;
                    var browseItems2 = BrowseItems;
                    return num + ((browseItems2 != null ? browseItems2.Count : 0) % ItemsPerPage > 0 ? 1 : 0);
                });
            }
        }

        public bool ShowPaddles
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(ShowPaddlesProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ShowPaddlesProperty, value); }); }
        }

        public string PageNumberText { get; set; }
        public event BrowseItemModelEvent OnBrowseItemModelSelected;

        public event BrowseItemModelEvent OnAddBrowseItemModel;

        public event BrowseItemModelEvent OnCancelBrowseItemModel;

        public event PageChangedEvent OnPageChanged;

        public event BrowseItemsModelEvent OnNotifyBrowseItemsShown;

        public void SetPageNumber(int value, PageChangeSource pageChangeSource)
        {
            var pageNumber = PageNumber;
            Dispatcher.Invoke(delegate { SetValue(PageNumberProperty, value); });
            ProcessPageChanged(pageNumber, value, pageChangeSource);
        }

        public int GetPageOfBrowseItemIndex(int browseItemIndex)
        {
            return Convert.ToInt32(Math.Ceiling(Convert.ToDouble(browseItemIndex + 1) / VisibleItemCount));
        }

        public void ProcessAddBrowseItemModel(IBrowseItemModel browseItemModel, object parameter)
        {
            if (OnAddBrowseItemModel != null) OnAddBrowseItemModel(browseItemModel, parameter);
        }

        public void ProcessCancelBrowseItemModel(IBrowseItemModel browseItemModel, object parameter)
        {
            if (OnCancelBrowseItemModel != null) OnCancelBrowseItemModel(browseItemModel, parameter);
        }

        public void ProcessBrowseItemModelSelected(IBrowseItemModel browseItemModel, object parameter)
        {
            if (OnBrowseItemModelSelected != null) OnBrowseItemModelSelected(browseItemModel, parameter);
        }

        public void ProcessPageChanged(int prevPageNumber, int newPageNumber, PageChangeSource pageChangeSource)
        {
            var onPageChanged = OnPageChanged;
            if (onPageChanged == null) return;
            onPageChanged(prevPageNumber, newPageNumber, pageChangeSource);
        }

        public void ProcessNotifyBrowseItemsShown(List<IBrowseItemModel> browseitemModels, object parameter)
        {
            if (OnNotifyBrowseItemsShown != null) OnNotifyBrowseItemsShown(browseitemModels, parameter);
        }

        public event Action<BrowseControlModel> OnSelectedDisplayProductUserControlSizeAndPositionChanged;

        private static void OnSelectedDisplayProductUserControlSizeAndPositionPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var browseControlModel = d as BrowseControlModel;
            if (browseControlModel != null &&
                browseControlModel.OnSelectedDisplayProductUserControlSizeAndPositionChanged != null)
                browseControlModel.OnSelectedDisplayProductUserControlSizeAndPositionChanged(browseControlModel);
        }
    }
}