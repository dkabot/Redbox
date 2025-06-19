using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "TitleDetailsView")]
    public partial class TitleDetailsUserControl : TextToSpeechUserControl, IWPFActor
    {
        public TitleDetailsUserControl()
        {
            InitializeComponent();
            Loaded += delegate { Keyboard.Focus(this); };
        }

        protected static IAnalyticsService AnalyticsService => ServiceLocator.Instance.GetService<IAnalyticsService>();

        private TitleDetailModel Model => DataContext as TitleDetailModel;

        public override ISpeechControl GetSpeechControl()
        {
            var model = Model;
            if (model == null) return null;
            return model.ProcessGetSpeechControl();
        }

        private UserControl GetBrowseItemUserControl(object source)
        {
            var frameworkElement = source as FrameworkElement;
            while (frameworkElement != null && !(frameworkElement is UserControl))
                frameworkElement = VisualTreeHelper.GetParent(frameworkElement) as FrameworkElement;
            return frameworkElement as UserControl;
        }

        private void GoBack(string reason, bool popOnly = false)
        {
            var service = ServiceLocator.Instance.GetService<IViewService>();
            if (service == null) return;
            service.GoBack("go back", popOnly);
        }

        private void OnGoBack_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var analyticsService = AnalyticsService;
            if (analyticsService != null) analyticsService.AddButtonPressEvent("TitleDetailsBack");
            GoBack("go_back");
            HandleWPFHit();
        }

        private void Buy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var browseItemUserControl = GetBrowseItemUserControl(e.OriginalSource);
            var browseItemModel =
                (browseItemUserControl != null ? browseItemUserControl.DataContext : null) as IBrowseItemModel;
            Model.ProcessBuyButtonHit(browseItemModel, e.Parameter);
            HandleWPFHit();
        }

        private void RentProduct_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var browseItemUserControl = GetBrowseItemUserControl(e.OriginalSource);
            var browseItemModel =
                (browseItemUserControl != null ? browseItemUserControl.DataContext : null) as IBrowseItemModel;
            Model.ProcessAddProduct(browseItemModel, e.Parameter, (TitleType)((Control)e.OriginalSource).Tag);
            HandleWPFHit();
        }

        private void ResetIdleTimerCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            HandleWPFHit();
        }

        private void MoreInfo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var browseItemUserControl = GetBrowseItemUserControl(e.OriginalSource);
            var browseItemModel =
                (browseItemUserControl != null ? browseItemUserControl.DataContext : null) as IBrowseItemModel;
            Model.ProcessMoreInfoHit(browseItemModel);
            HandleWPFHit();
        }

        private void SeeFullDetails_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var browseItemUserControl = GetBrowseItemUserControl(e.OriginalSource);
            var browseItemModel =
                (browseItemUserControl != null ? browseItemUserControl.DataContext : null) as IBrowseItemModel;
            Model.ProcessSeeFullDetails(browseItemModel);
            HandleWPFHit();
        }

        private void RedboxPlusInfo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Model.ProcessRedboxPlusInfo();
            HandleWPFHit();
        }

        private void DualInStockLearnMore_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Model.ProcessDualInStockLearnMore();
            HandleWPFHit();
        }

        private void StatisticsPopup_Executed(object sender, ExecutedRoutedEventArgs e)
        {
        }
    }
}