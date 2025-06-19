using System;
using System.Collections.Generic;
using System.Windows;

namespace Redbox.Rental.UI.Models
{
    public class DisplayGroupSelectionModel : DependencyObject
    {
        public static readonly DependencyProperty GroupSelectionItemsProperty =
            DependencyProperty.Register("GroupSelectionItems", typeof(List<DisplayGroupSelectionItemModel>),
                typeof(DisplayGroupSelectionModel), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty BrowseGroupSelectionVisibilityProperty =
            DependencyProperty.Register("BrowseGroupSelectionVisibility", typeof(Visibility),
                typeof(DisplayGroupSelectionModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty BrowseGroupSelectionMarginProperty =
            DependencyProperty.Register("BrowseGroupSelectionMargin", typeof(Thickness),
                typeof(DisplayGroupSelectionModel), new FrameworkPropertyMetadata(null));

        public Action<DisplayGroupSelectionItemModel> OnGroupSelectionItemClicked;

        public DisplayGroupSelectionModel()
        {
            GroupSelectionItems = new List<DisplayGroupSelectionItemModel>();
        }

        public List<DisplayGroupSelectionItemModel> GroupSelectionItems
        {
            get
            {
                return Dispatcher.Invoke(() =>
                    (List<DisplayGroupSelectionItemModel>)GetValue(GroupSelectionItemsProperty));
            }
            set { Dispatcher.Invoke(delegate { SetValue(GroupSelectionItemsProperty, value); }); }
        }

        public Visibility BrowseGroupSelectionVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(BrowseGroupSelectionVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BrowseGroupSelectionVisibilityProperty, value); }); }
        }

        public Thickness BrowseGroupSelectionMargin
        {
            get { return Dispatcher.Invoke(() => (Thickness)GetValue(BrowseGroupSelectionMarginProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BrowseGroupSelectionMarginProperty, value); }); }
        }

        public void ProcessOnGroupSelectionItemClicked(DisplayGroupSelectionItemModel displayGroupSelectionItemModel)
        {
            if (OnGroupSelectionItemClicked != null)
            {
                OnGroupSelectionItemClicked(displayGroupSelectionItemModel);
                Commands.ResetIdleTimerCommand.Execute(null, null);
            }
        }
    }
}