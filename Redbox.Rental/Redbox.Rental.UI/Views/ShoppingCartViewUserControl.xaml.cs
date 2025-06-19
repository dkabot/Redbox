using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "ShoppingCartView")]
    public partial class ShoppingCartViewUserControl : TextToSpeechUserControl, IWPFActor
    {
        public ShoppingCartViewUserControl()
        {
            InitializeComponent();
            Loaded += ShoppingCartViewUserControl_Loaded;
        }

        private ShoppingCartViewModel Model => DataContext as ShoppingCartViewModel;

        public override ISpeechControl GetSpeechControl()
        {
            var model = Model;
            if (model == null) return null;
            return model.ProcessGetSpeechControl();
        }

        private void GoBackCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var model = Model;
            if (model != null) model.ProcessOnBackButtonClicked();
            HandleWPFHit();
        }

        private void TermsAndPrivacyCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var model = Model;
            if (model != null) model.ProcessOnTermsAndPrivacyButtonClicked();
            HandleWPFHit();
        }

        private void PromoCodeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var model = Model;
            if (model != null) model.ProcessOnPromoCodeButtonClicked();
            HandleWPFHit();
        }

        private void PayCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var model = Model;
            if (model != null) model.ProcessOnPayButtonClicked();
            HandleWPFHit();
        }

        private void AddMovieCommandCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var model = Model;
            if (model != null) model.ProcessOnAddMovieButtonClicked();
            HandleWPFHit();
        }

        private void ShoppingCartViewUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            PopulateDisplayProductModels();
        }

        private void PopulateDisplayProductModels()
        {
            var list = new List<DisplayProductCheckoutUserControl>();
            foreach (var obj in ShoppingCartStackPanel.Children)
                if (obj is DisplayProductCheckoutUserControl)
                    list.Add(obj as DisplayProductCheckoutUserControl);

            foreach (var displayProductCheckoutUserControl in list)
                ShoppingCartStackPanel.Children.Remove(displayProductCheckoutUserControl);
            var model = Model;
            if ((model != null ? model.DisplayProductModels : null) != null)
                foreach (var displayCheckoutProductModel in Model.DisplayProductModels)
                {
                    var displayProductCheckoutUserControl2 = new DisplayProductCheckoutUserControl
                    {
                        DataContext = displayCheckoutProductModel,
                        Width = 590.0,
                        Height = 94.0
                    };
                    ShoppingCartStackPanel.Children.Insert(
                        Model.DisplayProductModels.IndexOf(displayCheckoutProductModel),
                        displayProductCheckoutUserControl2);
                }
        }

        private void BrowseItemCancelCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var frameworkElement = e.OriginalSource as FrameworkElement;
            var displayCheckoutProductModel =
                (frameworkElement != null ? frameworkElement.DataContext : null) as DisplayCheckoutProductModel;
            if (Model != null) Model.ProcessOnCancelBrowseItemModel(displayCheckoutProductModel, e.Parameter);
            HandleWPFHit();
        }

        private void UpdateBagCommandCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var model = Model;
            if (model == null) return;
            model.ProcessOnUpdateBagButtonClicked();
        }

        private void SignInCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var model = Model;
            if (model == null) return;
            model.ProcessOnSignInButtonClicked();
        }

        private void UsePointsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var model = Model;
            if (model == null) return;
            model.ProcessOnUsePointsButtonClicked();
        }

        private void PointsBalanceInfoCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var model = Model;
            if (model == null) return;
            model.ProcessOnPointsBalanceInfoButtonClicked();
        }

        private void ViewOfferDetailsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var frameworkElement = e.OriginalSource as FrameworkElement;
            var displayCheckoutProductModel =
                (frameworkElement != null ? frameworkElement.DataContext : null) as DisplayCheckoutProductModel;
            if (Model != null) Model.ProcessOnViewOfferDetailsButtonClicked(displayCheckoutProductModel);
        }

        private void ShoppingCartViewUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var model = Model;
            if (model == null) return;
            model.ProcessOnLoaded();
        }

        private void ViewRedboxPlusOfferCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var model = Model;
            if (model == null) return;
            model.ProcessOnViewRedboxPlusOfferButtonClicked();
        }

        private void ScanRedboxPlusQRCodeCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var model = Model;
            if (model == null) return;
            model.ProcessOnScanRedboxPlusQRCodeButtonClicked();
        }
    }
}