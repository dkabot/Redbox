using System.Windows;
using System.Windows.Input;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views
{
    [ThemedControl(ThemeName = "Redbox Classic", ControlName = "TitleDetailsDevView")]
    public partial class TitleDetailsDevViewUserControl : RentalUserControl, IWPFActor
    {
        public TitleDetailsDevViewUserControl()
        {
            InitializeComponent();
            Loaded += delegate
            {
                Keyboard.Focus(this);
                title_rollup_button.IsEnabled = false;
                dvd_button.IsEnabled = Model.CurrentProduct.IsDvd;
                bluray_button.IsEnabled = Model.CurrentProduct.IsBluRay;
                digital_button.IsEnabled = Model.CurrentProduct.IsDigitalTitle;
                uhd_button.IsEnabled = Model.CurrentProduct.Is4KUhdTitle;
            };
        }

        private TitleDetailDevModel Model => DataContext as TitleDetailDevModel;

        private void close_button_Click(object sender, RoutedEventArgs e)
        {
            var model = Model;
            if ((model != null ? model.OnCloseButtonClicked : null) != null) Model.OnCloseButtonClicked();
        }

        private void dvd_button_Click(object sender, RoutedEventArgs e)
        {
            dvd_button.IsEnabled = false;
            var model = Model;
            if ((model != null ? model.OnDvdButtonClicked : null) != null) Model.OnDvdButtonClicked();
            title_rollup_button.IsEnabled = Model.CurrentProduct.HasRollup;
            bluray_button.IsEnabled = Model.CurrentProduct.IsBluRay;
            digital_button.IsEnabled = Model.CurrentProduct.IsDigitalTitle;
            uhd_button.IsEnabled = Model.CurrentProduct.Is4KUhdTitle;
        }

        private void bluray_button_Click(object sender, RoutedEventArgs e)
        {
            title_rollup_button.IsEnabled = Model.CurrentProduct.HasRollup;
            dvd_button.IsEnabled = Model.CurrentProduct.IsDvd;
            bluray_button.IsEnabled = false;
            digital_button.IsEnabled = Model.CurrentProduct.IsDigitalTitle;
            uhd_button.IsEnabled = Model.CurrentProduct.Is4KUhdTitle;
            var model = Model;
            if ((model != null ? model.OnBlurayButtonClicked : null) != null) Model.OnBlurayButtonClicked();
        }

        private void digital_button_Click(object sender, RoutedEventArgs e)
        {
            title_rollup_button.IsEnabled = Model.CurrentProduct.HasRollup;
            dvd_button.IsEnabled = Model.CurrentProduct.IsDvd;
            bluray_button.IsEnabled = Model.CurrentProduct.IsBluRay;
            uhd_button.IsEnabled = Model.CurrentProduct.Is4KUhdTitle;
            digital_button.IsEnabled = false;
            var model = Model;
            if ((model != null ? model.OnDigitalButtonClicked : null) != null) Model.OnDigitalButtonClicked();
        }

        private void title_rollup_button_Click(object sender, RoutedEventArgs e)
        {
            title_rollup_button.IsEnabled = false;
            dvd_button.IsEnabled = Model.CurrentProduct.IsDvd;
            bluray_button.IsEnabled = Model.CurrentProduct.IsBluRay;
            digital_button.IsEnabled = Model.CurrentProduct.IsDigitalTitle;
            uhd_button.IsEnabled = Model.CurrentProduct.Is4KUhdTitle;
            var model = Model;
            if ((model != null ? model.OnTitleRollupButtonClicked : null) != null) Model.OnTitleRollupButtonClicked();
        }

        private void uhd_button_Click(object sender, RoutedEventArgs e)
        {
            title_rollup_button.IsEnabled = Model.CurrentProduct.HasRollup;
            dvd_button.IsEnabled = Model.CurrentProduct.IsDvd;
            bluray_button.IsEnabled = Model.CurrentProduct.IsBluRay;
            digital_button.IsEnabled = Model.CurrentProduct.IsDigitalTitle;
            uhd_button.IsEnabled = false;
            var model = Model;
            if ((model != null ? model.On4KUhdButtonClicked : null) != null) Model.On4KUhdButtonClicked();
        }

        private void DisplayAddCustomPriceInfo(object sender, RoutedEventArgs e)
        {
            ToggleAddCustomPrice(true);
        }

        private void Add_product_with_custom_price_button_Click(object sender, RoutedEventArgs e)
        {
            var model = Model;
            if ((model != null ? model.CurrentProduct : null) != null)
            {
                decimal num2;
                var num = decimal.TryParse(price_input.Text, out num2) ? new decimal?(num2) : null;
                if (num != null)
                {
                    var model2 = Model;
                    if (model2 != null)
                    {
                        var onAddProductWithCustomPriceButtonClicked = model2.OnAddProductWithCustomPriceButtonClicked;
                        if (onAddProductWithCustomPriceButtonClicked != null)
                            onAddProductWithCustomPriceButtonClicked(Model.CurrentProduct.ProductId, num.Value);
                    }
                }
            }

            ToggleAddCustomPrice(false);
        }

        private void ToggleAddCustomPrice(bool showPrice)
        {
            Model.CurrentProduct.AddProductWithCustomPriceButtonVisibility =
                showPrice ? Visibility.Visible : Visibility.Collapsed;
            Model.CurrentProduct.DisplayAddProductWithCustomPriceButtonVisibility =
                showPrice ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}