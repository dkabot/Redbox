using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls
{
    public partial class BrowseUserControl : UserControl
    {
        public static readonly DependencyProperty ShowPaddlesProperty = DependencyProperty.Register("ShowPaddles",
            typeof(bool), typeof(BrowseUserControl), new FrameworkPropertyMetadata(false, OnShowPaddlesPropertyChanged)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty LeftPaddleVisibilityProperty = DependencyProperty.Register(
            "LeftPaddleVisibility", typeof(Visibility), typeof(BrowseUserControl),
            new FrameworkPropertyMetadata(Visibility.Visible)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty RightPaddleVisibilityProperty = DependencyProperty.Register(
            "RightPaddleVisibility", typeof(Visibility), typeof(BrowseUserControl),
            new FrameworkPropertyMetadata(Visibility.Visible)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty BorderShadowVisibilityProperty = DependencyProperty.Register(
            "BorderShadowVisibility", typeof(Visibility), typeof(BrowseUserControl),
            new FrameworkPropertyMetadata(Visibility.Visible)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty PageGridWidthProperty = DependencyProperty.Register("PageGridWidth",
            typeof(double), typeof(BrowseUserControl), new FrameworkPropertyMetadata(1024.0)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty PaddleOverlapWidthProperty = DependencyProperty.Register(
            "PaddleOverlapWidth", typeof(double), typeof(BrowseUserControl), new FrameworkPropertyMetadata(0.0)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty PageWrapGridUnderPaddlesProperty = DependencyProperty.Register(
            "PageWrapPanelUnderPaddles", typeof(bool), typeof(BrowseUserControl), new FrameworkPropertyMetadata(false)
            {
                AffectsRender = true
            });

        private readonly List<Control> m_currentPageBrowseControls = new List<Control>();

        private readonly List<Control> m_nextPageBrowseControls = new List<Control>();

        private readonly List<Control> m_priorPageBrowseControls = new List<Control>();

        private PageDirection _animationScrollDirection;

        private bool _animationScrollInProgress;

        private BrowseControlModel _browseControlModel;

        private int _browseItemIndex;

        private FrameworkElement _scrollControl;

        public BrowseUserControl()
        {
            InitializeComponent();
            ApplyStyle();
        }

        public int RowCount { get; set; }

        public int ColumnCount { get; set; }

        public int VisibleItemCount => RowCount * ColumnCount;

        public int PageCount
        {
            get
            {
                var browseControlModel = _browseControlModel;
                int? num;
                if (browseControlModel == null)
                {
                    num = null;
                }
                else
                {
                    var browseItems = browseControlModel.BrowseItems;
                    num = browseItems != null ? new int?(browseItems.Count) : null;
                }

                var num2 = num;
                return Convert.ToInt32(Math.Ceiling(Convert.ToDouble(num2.GetValueOrDefault()) / VisibleItemCount));
            }
        }

        public int CurrentPage
        {
            get => GetPageOfBrowseItemIndex(_browseItemIndex);
            set => _browseItemIndex =
                Math.Min(VisibleItemCount * (Math.Max(1, value) - 1), PageCount * VisibleItemCount);
        }

        public bool ShowPaddles
        {
            get => (bool)GetValue(ShowPaddlesProperty);
            set => SetValue(ShowPaddlesProperty, value);
        }

        public Visibility LeftPaddleVisibility
        {
            get => (Visibility)GetValue(LeftPaddleVisibilityProperty);
            set => SetValue(LeftPaddleVisibilityProperty, value);
        }

        public Visibility RightPaddleVisibility
        {
            get => (Visibility)GetValue(RightPaddleVisibilityProperty);
            set => SetValue(RightPaddleVisibilityProperty, value);
        }

        public Visibility BorderShadowVisibility
        {
            get => (Visibility)GetValue(BorderShadowVisibilityProperty);
            set => SetValue(BorderShadowVisibilityProperty, value);
        }

        public double PageGridWidth
        {
            get => (double)GetValue(PageGridWidthProperty);
            set => SetValue(PageGridWidthProperty, value);
        }

        public double PaddleOverlapWidth
        {
            get => (double)GetValue(PaddleOverlapWidthProperty);
            set => SetValue(PaddleOverlapWidthProperty, value);
        }

        public bool PageWrapGridUnderPaddles
        {
            get => (bool)GetValue(PageWrapGridUnderPaddlesProperty);
            set => SetValue(PageWrapGridUnderPaddlesProperty, value);
        }

        private void ApplyStyle()
        {
            var service = ServiceLocator.Instance.GetService<IThemeService>();
            var theme = service != null ? service.CurrentTheme : null;
            if (theme != null) theme.SetStyle(LeftPaddleButton);
            if (theme == null) return;
            theme.SetStyle(RightPaddleButton);
        }

        public event BrowsePageNumberChangedEvent OnBeforePageNumberChange;

        private static void OnShowPaddlesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as BrowseUserControl).RenderNavigationButtons();
        }

        public int GetPageOfBrowseItemIndex(int browseItemIndex)
        {
            return Convert.ToInt32(Math.Ceiling(Convert.ToDouble(browseItemIndex + 1) / VisibleItemCount));
        }

        public void Render()
        {
            var list = new List<IBrowseItemModel>();
            list.AddRange(RenderPage(m_priorPageBrowseControls, VisibleItemCount * -1, true));
            list.AddRange(RenderPage(m_currentPageBrowseControls, 0, false));
            list.AddRange(RenderPage(m_nextPageBrowseControls, VisibleItemCount, true));
            RenderNoResultsMessage();
            RenderNavigationButtons();
            RenderPageNumbers();
            var browseControlModel = _browseControlModel;
            if (browseControlModel == null) return;
            browseControlModel.ProcessNotifyBrowseItemsShown(list, null);
        }

        private void RenderNoResultsMessage()
        {
            UIElement noResultsGrid = NoResultsGrid;
            var browseControlModel = _browseControlModel;
            Visibility visibility;
            if (browseControlModel != null && browseControlModel.ShowNoResultsMessage)
            {
                var browseControlModel2 = _browseControlModel;
                int? num;
                if (browseControlModel2 == null)
                {
                    num = null;
                }
                else
                {
                    var browseItems = browseControlModel2.BrowseItems;
                    num = browseItems != null ? new int?(browseItems.Count) : null;
                }

                var num2 = num;
                if (num2.GetValueOrDefault() == 0)
                {
                    visibility = Visibility.Visible;
                    goto IL_005B;
                }
            }

            visibility = Visibility.Collapsed;
            IL_005B:
            noResultsGrid.Visibility = visibility;
        }

        private List<IBrowseItemModel> RenderPage(List<Control> pageControls, int indexOffset, bool isBorderPage)
        {
            var list = new List<IBrowseItemModel>();
            for (var i = 0; i < VisibleItemCount; i++)
            {
                var num = _browseItemIndex + i + indexOffset;
                var browseItemModel =
                    num > -1 && _browseControlModel.BrowseItems != null && num < _browseControlModel.BrowseItems.Count
                        ? _browseControlModel.BrowseItems[num]
                        : null;
                if (browseItemModel != null)
                {
                    browseItemModel.IsBorderItem = isBorderPage;
                    browseItemModel.VisibleItemIndex = i;
                    var rowAndColumnFromIndex = GetRowAndColumnFromIndex(i);
                    browseItemModel.CurrentGridRow = rowAndColumnFromIndex.Row + 1;
                    browseItemModel.CurrentGridColumn = rowAndColumnFromIndex.Column + 1;
                    browseItemModel.IsBottomRow =
                        _browseControlModel != null && _browseControlModel.LayoutDirection ==
                        BrowseLayoutDirection.LeftToRightThenTopToBottom
                            ? i >= (RowCount - 1) * ColumnCount
                            : (i + 1) % RowCount == 0;
                    list.Add(browseItemModel);
                }

                if (pageControls.Count > i)
                {
                    var control = pageControls[i];
                    if (control != null) control.DataContext = browseItemModel;
                }
            }

            return list;
        }

        private void RenderNavigationButtons()
        {
            if (_browseControlModel != null)
            {
                if (ShowPaddles)
                {
                    LeftPaddleVisibility = _browseItemIndex > 0 ? Visibility.Visible : Visibility.Collapsed;
                    var num = _browseItemIndex + VisibleItemCount;
                    var browseControlModel = _browseControlModel;
                    int? num2;
                    if (browseControlModel == null)
                    {
                        num2 = null;
                    }
                    else
                    {
                        var browseItems = browseControlModel.BrowseItems;
                        num2 = browseItems != null ? new int?(browseItems.Count) : null;
                    }

                    var num3 = num2;
                    RightPaddleVisibility = num < num3.GetValueOrDefault() ? Visibility.Visible : Visibility.Collapsed;
                    return;
                }

                LeftPaddleVisibility = Visibility.Collapsed;
                RightPaddleVisibility = Visibility.Collapsed;
            }
        }

        private void RenderPageNumbers()
        {
            var browseControlModel = _browseControlModel;
            var text = string.Format(
                (browseControlModel != null ? browseControlModel.PageNumberText : null) ??
                Properties.Resources.browse_view_control_page_mask, CurrentPage, PageCount);
            PageNumberText.Text = text;
            BottomPageNumberText.Text = text;
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _browseControlModel = e.NewValue as BrowseControlModel;
            var browseControlModel = _browseControlModel;
            RowCount = browseControlModel != null ? browseControlModel.RowCount : 1;
            var browseControlModel2 = _browseControlModel;
            ColumnCount = browseControlModel2 != null ? browseControlModel2.ColumnCount : 1;
            if (_browseControlModel != null)
            {
                BorderShadowVisibility = _browseControlModel.BorderShadowVisibility;
                ConfigureBrowseItemGrid();
                UpdatePageGridWidth();
                CurrentPage = _browseControlModel.PageNumber;
                Render();
                _browseControlModel.OnPageChanged += _browseControlModel_OnPageChanged;
            }
        }

        private void _browseControlModel_OnPageChanged(int prevPageNumber, int newPageNumber,
            PageChangeSource pageChangedSource)
        {
            if (newPageNumber != prevPageNumber)
            {
                CurrentPage = newPageNumber;
                Render();
            }
        }

        private void ConfigureBrowseItemGrid()
        {
            m_priorPageBrowseControls.Clear();
            m_currentPageBrowseControls.Clear();
            m_nextPageBrowseControls.Clear();
            PriorPageGrid.Children.Clear();
            CurrentPageGrid.Children.Clear();
            NextPageGrid.Children.Clear();
            CurrentPageGrid.Margin = _browseControlModel.Margin;
            PriorPageGrid.Margin = _browseControlModel.Margin;
            NextPageGrid.Margin = _browseControlModel.Margin;
            PriorPageGrid.RowDefinitions.Clear();
            PriorPageGrid.ColumnDefinitions.Clear();
            CurrentPageGrid.RowDefinitions.Clear();
            CurrentPageGrid.ColumnDefinitions.Clear();
            NextPageGrid.RowDefinitions.Clear();
            NextPageGrid.ColumnDefinitions.Clear();
            for (var i = 0; i < RowCount; i++)
            {
                PriorPageGrid.RowDefinitions.Add(new RowDefinition());
                CurrentPageGrid.RowDefinitions.Add(new RowDefinition());
                NextPageGrid.RowDefinitions.Add(new RowDefinition());
            }

            for (var j = 0; j < ColumnCount; j++)
            {
                PriorPageGrid.ColumnDefinitions.Add(new ColumnDefinition());
                CurrentPageGrid.ColumnDefinitions.Add(new ColumnDefinition());
                NextPageGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (var k = 0; k < VisibleItemCount; k++)
            {
                if (_browseControlModel != null && _browseControlModel.ShowBorderItems)
                {
                    var browseControlModel = _browseControlModel;
                    var control = browseControlModel != null ? browseControlModel.BrowseControls[k] : null;
                    ConfigureBrowseItemControlInGrid(PriorPageGrid, control, m_priorPageBrowseControls, k);
                    var browseControlModel2 = _browseControlModel;
                    var control2 = browseControlModel2 != null
                        ? browseControlModel2.BrowseControls[k + VisibleItemCount * 2]
                        : null;
                    ConfigureBrowseItemControlInGrid(NextPageGrid, control2, m_nextPageBrowseControls, k);
                }

                var num = _browseControlModel != null && _browseControlModel.ShowBorderItems ? VisibleItemCount : 0;
                var browseControlModel3 = _browseControlModel;
                var control3 = browseControlModel3 != null ? browseControlModel3.BrowseControls[k + num] : null;
                ConfigureBrowseItemControlInGrid(CurrentPageGrid, control3, m_currentPageBrowseControls, k);
            }
        }

        private void ConfigureBrowseItemControlInGrid(Grid grid, Control control, List<Control> controlList, int index)
        {
            control.Margin = _browseControlModel.BrowseItemMargin;
            controlList.Add(control);
            var rowAndColumnFromIndex = GetRowAndColumnFromIndex(index);
            Grid.SetColumn(control, rowAndColumnFromIndex.Column);
            Grid.SetRow(control, rowAndColumnFromIndex.Row);
            grid.Children.Add(control);
        }

        private RowAndColumn GetRowAndColumnFromIndex(int index)
        {
            var rowAndColumn = default(RowAndColumn);
            if (_browseControlModel.LayoutDirection == BrowseLayoutDirection.LeftToRightThenTopToBottom)
            {
                rowAndColumn.Row = index / ColumnCount;
                rowAndColumn.Column = index % ColumnCount;
            }
            else if (_browseControlModel.LayoutDirection == BrowseLayoutDirection.TopToBottomThenLeftToRight)
            {
                rowAndColumn.Row = index % RowCount;
                rowAndColumn.Column = index / RowCount;
            }

            return rowAndColumn;
        }

        private void UpdateBrowseItemIndex(int newBrowseItemIndex)
        {
            _browseItemIndex = newBrowseItemIndex;
            Render();
        }

        private void PageLeftCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Commands.ResetIdleTimerCommand.Execute(null, this);
            Scroll(PageDirection.Left);
        }

        private void PageRightCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Commands.ResetIdleTimerCommand.Execute(null, this);
            Scroll(PageDirection.Right);
        }

        private void Scroll(PageDirection direction)
        {
            UpdateSelectedBrowseItemSizeAndPosition(null);
            if (OnBeforePageNumberChange != null) OnBeforePageNumberChange(CurrentPage);
            ScrollProducts(direction);
        }

        private void ScrollProducts(PageDirection direction)
        {
            var num = direction == PageDirection.Left
                ? Math.Max(0, _browseItemIndex - VisibleItemCount)
                : Math.Min(_browseItemIndex + VisibleItemCount, _browseControlModel.BrowseItems.Count - 1);
            UpdateBrowseItemIndex(num);
            if (_browseControlModel != null)
                _browseControlModel.SetPageNumber(CurrentPage, PageChangeSource.PaddleButton);
        }

        private void AnimateScroll(PageDirection direction)
        {
            if (!_animationScrollInProgress)
            {
                _animationScrollInProgress = true;
                _animationScrollDirection = direction;
                _scrollControl = BrowsePagesStackPanel;
                var timeSpan = TimeSpan.FromSeconds(0.5);
                var storyboard = new Storyboard();
                storyboard.Duration = timeSpan;
                var num = _scrollControl.ActualWidth / 3.0 * (direction == PageDirection.Left ? 1 : -1);
                var doubleAnimation = new DoubleAnimation(0.0, num, timeSpan);
                doubleAnimation.Completed += Animation_Completed;
                var translateTransform = new TranslateTransform();
                _scrollControl.RenderTransform = translateTransform;
                Storyboard.SetTarget(doubleAnimation, _scrollControl);
                Storyboard.SetTargetProperty(doubleAnimation,
                    new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
                storyboard.Children.Add(doubleAnimation);
                storyboard.Begin();
            }
        }

        private void Animation_Completed(object sender, EventArgs e)
        {
            _scrollControl.RenderTransform = null;
            ScrollProducts(_animationScrollDirection);
            _animationScrollInProgress = false;
            CommandManager.InvalidateRequerySuggested();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePageGridWidth();
        }

        private void UpdatePageGridWidth()
        {
            var num = ShowPaddles ? Math.Max(LeftPaddleButton.ActualWidth, LeftPaddleButton.Width) * 2.0 : 0.0;
            var num2 = 0.0;
            var actualWidth = ActualWidth;
            double num4;
            if (!PageWrapGridUnderPaddles)
            {
                var browseControlModel = _browseControlModel;
                if (browseControlModel == null || !browseControlModel.UseFullPageWidth)
                {
                    var num3 = num;
                    var browseControlModel2 = _browseControlModel;
                    num4 = Math.Max(
                        (num3 - (browseControlModel2 != null
                            ? new double?(browseControlModel2.PaddleOverlapWidth)
                            : null)).GetValueOrDefault(), 0.0);
                    goto IL_00CA;
                }
            }

            num4 = 0.0;
            IL_00CA:
            PageGridWidth = Math.Max(num2, actualWidth - num4);
        }

        private void BrowseItemAddCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var browseItemUserControl = GetBrowseItemUserControl(e.OriginalSource);
            if (browseItemUserControl != null) UpdateSelectedBrowseItemSizeAndPosition(browseItemUserControl);
            Commands.ResetIdleTimerCommand.Execute(null, this);
            var frameworkElement = (e != null ? e.OriginalSource : null) as FrameworkElement;
            var browseItemModel = (frameworkElement != null ? frameworkElement.DataContext : null) as IBrowseItemModel;
            if (_browseControlModel != null)
                _browseControlModel.ProcessAddBrowseItemModel(browseItemModel, e.Parameter);
        }

        private void UpdateSelectedBrowseItemSizeAndPosition(UserControl selectedUserControl)
        {
            var sizeAndPosition = new SizeAndPosition();
            if (selectedUserControl != null)
            {
                var point = selectedUserControl.TransformToAncestor(this).Transform(new Point(0.0, 0.0));
                sizeAndPosition.Left = point.X;
                sizeAndPosition.Top = point.Y;
                sizeAndPosition.Width = selectedUserControl.ActualWidth;
                sizeAndPosition.Height = selectedUserControl.ActualHeight;
            }

            if (_browseControlModel != null) _browseControlModel.SelectedBrowseItemSizeAndPosition = sizeAndPosition;
        }

        private UserControl GetBrowseItemUserControl(object source)
        {
            var frameworkElement = source as FrameworkElement;
            while (frameworkElement != null && !(frameworkElement is UserControl) &&
                   frameworkElement.Parent != CurrentPageGrid)
                frameworkElement = frameworkElement.Parent as FrameworkElement;
            return frameworkElement as UserControl;
        }

        private void BrowseItemCancelCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var browseItemUserControl = GetBrowseItemUserControl(e.OriginalSource);
            if (browseItemUserControl != null)
            {
                Commands.ResetIdleTimerCommand.Execute(null, this);
                var browseItemModel = browseItemUserControl.DataContext as IBrowseItemModel;
                if (_browseControlModel != null)
                    _browseControlModel.ProcessCancelBrowseItemModel(browseItemModel, e.Parameter);
            }
        }

        private void BrowseItemSelectedCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var browseItemUserControl = GetBrowseItemUserControl(e.OriginalSource);
            if (browseItemUserControl != null) UpdateSelectedBrowseItemSizeAndPosition(browseItemUserControl);
            Commands.ResetIdleTimerCommand.Execute(null, this);
            var frameworkElement = (e != null ? e.OriginalSource : null) as FrameworkElement;
            var browseItemModel = (frameworkElement != null ? frameworkElement.DataContext : null) as IBrowseItemModel;
            if (_browseControlModel != null && browseItemModel != null && !browseItemModel.IsBorderItem)
                _browseControlModel.ProcessBrowseItemModelSelected(browseItemModel, e.Parameter);
        }

        private void BrowseItemAddCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var frameworkElement = (e != null ? e.OriginalSource : null) as FrameworkElement;
            var browseItemModel = (frameworkElement != null ? frameworkElement.DataContext : null) as IBrowseItemModel;
            e.CanExecute = browseItemModel != null && browseItemModel.CanAdd;
        }

        private struct RowAndColumn
        {
            public int Row { get; set; }

            public int Column { get; set; }
        }

        private enum PageDirection
        {
            Left,
            Right
        }
    }
}