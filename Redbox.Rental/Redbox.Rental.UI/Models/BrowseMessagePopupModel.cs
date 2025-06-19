using System;
using System.Windows;

namespace Redbox.Rental.UI.Models
{
    public class BrowseMessagePopupModel : DependencyObject
    {
        public static readonly DependencyProperty BrowseMessagePopupUserControlVisibilityProperty =
            DependencyProperty.Register("BrowseMessagePopupUserControlVisibility", typeof(Visibility),
                typeof(BrowseMessagePopupModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
                {
                    AffectsRender = true
                });

        public Action OnBrowseMessagePopupCloseClicked;

        public Action OnBrowseMessagePopupInfoClicked;
        public string Text1 { get; set; }

        public string Text2 { get; set; }

        public string InfoButtonText { get; set; }

        public Visibility BrowseMessagePopupUserControlVisibility
        {
            get
            {
                return Dispatcher.Invoke(() => (Visibility)GetValue(BrowseMessagePopupUserControlVisibilityProperty));
            }
            set { Dispatcher.Invoke(delegate { SetValue(BrowseMessagePopupUserControlVisibilityProperty, value); }); }
        }

        public void ProcessOnBrowseMessagePopupInfoClicked()
        {
            var onBrowseMessagePopupInfoClicked = OnBrowseMessagePopupInfoClicked;
            if (onBrowseMessagePopupInfoClicked == null) return;
            onBrowseMessagePopupInfoClicked();
        }

        public void ProcessOnBrowseMessagePopupCloseClicked()
        {
            var onBrowseMessagePopupCloseClicked = OnBrowseMessagePopupCloseClicked;
            if (onBrowseMessagePopupCloseClicked == null) return;
            onBrowseMessagePopupCloseClicked();
        }
    }
}