using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.UI.Models
{
    public class UpsellViewModel : DependencyObject
    {
        public static readonly DependencyProperty DisplayProductModelsProperty = DependencyProperty.Register(
            "DisplayProductModels", typeof(List<DisplayUpsellProductModel>), typeof(UpsellViewModel),
            new FrameworkPropertyMetadata(null)
            {
                AffectsRender = true
            });

        public Action OnBackButtonClicked;

        public Func<bool> OnCanExecuteUpgradeButton;

        public Action OnNoThanksButtonClicked;

        public Action OnUpgradeButtonClicked;
        public TitleType UpsellFromTitleType { get; set; }

        public Action<DisplayUpsellProductModel> OnCheckBoxClick { get; set; }

        public string TitleText { get; set; }

        public string MessageText { get; set; }

        public string Message2Text { get; set; }

        public string BackButtonText { get; set; }

        public string NoThanksButtonText { get; set; }

        public string UpgradeButtonText { get; set; }

        public List<DisplayUpsellProductModel> DisplayProductModels
        {
            get
            {
                return Dispatcher.Invoke(() => (List<DisplayUpsellProductModel>)GetValue(DisplayProductModelsProperty));
            }
            set { Dispatcher.Invoke(delegate { SetValue(DisplayProductModelsProperty, value); }); }
        }

        public bool ProcessOnCanExecuteUpgradeButton()
        {
            var onCanExecuteUpgradeButton = OnCanExecuteUpgradeButton;
            return onCanExecuteUpgradeButton != null && onCanExecuteUpgradeButton();
        }

        public void ProcessOnCheckBoxClick(DisplayUpsellProductModel displayUpsellProductModel)
        {
            var onCheckBoxClick = OnCheckBoxClick;
            if (onCheckBoxClick == null) return;
            onCheckBoxClick(displayUpsellProductModel);
        }

        public void ProcessOnBackButtonClicked()
        {
            var onBackButtonClicked = OnBackButtonClicked;
            if (onBackButtonClicked == null) return;
            onBackButtonClicked();
        }

        public void ProcessOnNoThanksButtonClicked()
        {
            var onNoThanksButtonClicked = OnNoThanksButtonClicked;
            if (onNoThanksButtonClicked == null) return;
            onNoThanksButtonClicked();
        }

        public void ProcessOnUpgradeButtonClicked()
        {
            var onUpgradeButtonClicked = OnUpgradeButtonClicked;
            if (onUpgradeButtonClicked == null) return;
            onUpgradeButtonClicked();
        }
    }
}