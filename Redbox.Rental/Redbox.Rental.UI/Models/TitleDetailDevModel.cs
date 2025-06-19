using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.UI.Models
{
    public class TitleDetailDevModel : BaseModel<TitleDetailDevModel>
    {
        public static readonly DependencyProperty ProductsProperty = DependencyProperty.Register("Products",
            typeof(IDictionary<TitleType, TitleProductDetailDevModel>), typeof(TitleDetailDevModel),
            new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty CurrentProductProperty = DependencyProperty.Register("CurrentProduct",
            typeof(TitleProductDetailDevModel), typeof(TitleDetailDevModel), new FrameworkPropertyMetadata(null)
            {
                AffectsArrange = true
            });

        public Action On4KUhdButtonClicked;

        public Action<long, decimal> OnAddProductWithCustomPriceButtonClicked;

        public Action OnBlurayButtonClicked;

        public Action OnCloseButtonClicked;

        public Action OnDigitalButtonClicked;

        public Action OnDvdButtonClicked;

        public Action OnTitleRollupButtonClicked;

        public TitleDetailDevModel()
        {
            Products = new Dictionary<TitleType, TitleProductDetailDevModel>();
        }

        public IDictionary<TitleType, TitleProductDetailDevModel> Products
        {
            get
            {
                return Dispatcher.Invoke(() =>
                    (IDictionary<TitleType, TitleProductDetailDevModel>)GetValue(ProductsProperty));
            }
            set { Dispatcher.Invoke(delegate { SetValue(ProductsProperty, value); }); }
        }

        public TitleProductDetailDevModel CurrentProduct
        {
            get { return Dispatcher.Invoke(() => (TitleProductDetailDevModel)GetValue(CurrentProductProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(CurrentProductProperty, value); }); }
        }
    }
}