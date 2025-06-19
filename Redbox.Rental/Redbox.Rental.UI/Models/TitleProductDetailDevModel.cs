using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using Redbox.Rental.Model.Cache;

namespace Redbox.Rental.UI.Models
{
    public class TitleProductDetailDevModel : BaseModel<TitleDetailDevModel>
    {
        public static readonly DependencyProperty ImgFileProperty =
            CreateDependencyProperty("ImgFileProperty", TYPES.OBJECT);

        public static readonly DependencyProperty TypeProperty = CreateDependencyProperty("TypeProperty", TYPES.STRING);

        public static readonly DependencyProperty ShowOutOfStockProperty =
            CreateDependencyProperty("ShowOutOfStockProperty", TYPES.BOOL, false);

        public static readonly DependencyProperty IsAvailableProperty =
            CreateDependencyProperty("IsAvailableProperty", TYPES.BOOL, false);

        public static readonly DependencyProperty ProductTypeNameProperty =
            CreateDependencyProperty("ProductTypeNameProperty", TYPES.STRING);

        public static readonly DependencyProperty ProductFamilyNameProperty =
            CreateDependencyProperty("ProductFamilyNameProperty", TYPES.STRING);

        public static readonly DependencyProperty CanBeSoldProperty =
            CreateDependencyProperty("CanBeSoldProperty", TYPES.BOOL, false);

        public static readonly DependencyProperty InStockProperty =
            CreateDependencyProperty("InStockProperty", TYPES.BOOL, false);

        public static readonly DependencyProperty IsComingSoonProperty =
            CreateDependencyProperty("IsComingSoonProperty", TYPES.BOOL, false);

        public static readonly DependencyProperty GenreIdsProperty =
            CreateDependencyProperty("GenreIdsProperty", TYPES.OBJECT);

        public static readonly DependencyProperty GenreListProperty =
            CreateDependencyProperty("GenreListProperty", TYPES.OBJECT);

        public static readonly DependencyProperty PurchaseProperty =
            CreateDependencyProperty("PurchaseProperty", TYPES.DECIMAL, 0m);

        public static readonly DependencyProperty PromoValueProperty =
            CreateDependencyProperty("PromoValueProperty", TYPES.DECIMAL, 0m);

        public static readonly DependencyProperty NonReturnDaysProperty =
            CreateDependencyProperty("NonReturnDaysProperty", TYPES.DECIMAL, 0m);

        public static readonly DependencyProperty NonReturnProperty =
            CreateDependencyProperty("NonReturnProperty", TYPES.DECIMAL, 0m);

        public static readonly DependencyProperty InitialNightProperty =
            CreateDependencyProperty("InitialNightProperty", TYPES.DECIMAL, 0m);

        public static readonly DependencyProperty PriceSetIdProperty =
            CreateDependencyProperty("PriceSetIdProperty", TYPES.STRING);

        public static readonly DependencyProperty ExpirationPriceProperty =
            CreateDependencyProperty("ExpirationPriceProperty", TYPES.DECIMAL, 0m);

        public static readonly DependencyProperty ExtraNightProperty =
            CreateDependencyProperty("ExtraNightProperty", TYPES.DECIMAL, 0m);

        public static readonly DependencyProperty IsAlternatePricedProperty =
            CreateDependencyProperty("IsAlternatePricedProperty", TYPES.BOOL, false);

        public static readonly DependencyProperty ReleaseDateProperty =
            CreateDependencyProperty("ReleaseDateProperty", TYPES.STRING);

        public static readonly DependencyProperty SellThruProperty =
            CreateDependencyProperty("SellThruProperty", TYPES.BOOL, false);

        public static readonly DependencyProperty SortDateProperty =
            CreateDependencyProperty("SortDateProperty", TYPES.STRING);

        public static readonly DependencyProperty MerchandiseDateProperty =
            CreateDependencyProperty("MerchandiseDateProperty", TYPES.STRING);

        public static readonly DependencyProperty ProductTypeIdProperty =
            CreateDependencyProperty("ProductTypeIdProperty", TYPES.INT, 0);

        public static readonly DependencyProperty RaitingIdProperty =
            CreateDependencyProperty("RaitingIdProperty", TYPES.INT, 0);

        public static readonly DependencyProperty TitleRollupProductGroupIdProperty =
            CreateDependencyProperty("TitleRollupProductGroupIdProperty", typeof(long), 0L);

        public static readonly DependencyProperty DiscountRestrictionProperty =
            CreateDependencyProperty("DiscountRestrictionProperty", TYPES.STRING);

        public static readonly DependencyProperty BarCodesProperty = DependencyProperty.Register("BarCodes",
            typeof(ObservableCollection<BarcodeProduct>), typeof(TitleProductDetailDevModel),
            new FrameworkPropertyMetadata(new ObservableCollection<BarcodeProduct>())
            {
                AffectsArrange = true
            });

        public static readonly DependencyProperty SellThruNewProperty = DependencyProperty.Register("SellThruNew",
            typeof(bool), typeof(TitleProductDetailDevModel), new FrameworkPropertyMetadata(false)
            {
                AffectsArrange = true
            });

        public static readonly DependencyProperty ClosedCaptionedProperty = DependencyProperty.Register(
            "ClosedCaptioned", typeof(bool), typeof(TitleProductDetailDevModel), new FrameworkPropertyMetadata(false)
            {
                AffectsArrange = true
            });

        public static readonly DependencyProperty BoxOfficeGrossProperty = DependencyProperty.Register("BoxOfficeGross",
            typeof(decimal), typeof(TitleProductDetailDevModel), new FrameworkPropertyMetadata(null)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty NationalStreetDateProperty = DependencyProperty.Register(
            "NationalStreetDate", typeof(string), typeof(TitleProductDetailDevModel),
            new FrameworkPropertyMetadata(null)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("DevTitle",
            typeof(string), typeof(TitleProductDetailDevModel), new FrameworkPropertyMetadata(null)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty ComingSoonDaysProperty = DependencyProperty.Register("ComingSoonDays",
            typeof(int), typeof(TitleProductDetailDevModel), new FrameworkPropertyMetadata(null)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty AddProductWithCustomPriceButtonVisibilityProperty =
            DependencyProperty.Register("AddProductWithCustomPriceButtonVisibility", typeof(Visibility),
                typeof(TitleProductDetailDevModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
                {
                    AffectsRender = true
                });

        public static readonly DependencyProperty DisplayAddProductWithCustomPriceButtonVisibilityProperty =
            DependencyProperty.Register("DisplayAddProductWithCustomPriceButtonVisibility", typeof(Visibility),
                typeof(TitleProductDetailDevModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
                {
                    AffectsRender = true
                });

        public TitleProductDetailDevModel()
        {
            GenreList = new List<string>();
        }

        public bool HasRollup { get; set; }

        public bool IsDvd { get; set; }

        public bool IsBluRay { get; set; }

        public bool IsDigitalTitle { get; set; }

        public bool Is4KUhdTitle { get; set; }

        public long ProductId { get; set; }

        public string DevTitle
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(TitleProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(TitleProperty, value); }); }
        }

        public int ComingSoonDays
        {
            get { return Dispatcher.Invoke(() => (int)GetValue(ComingSoonDaysProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ComingSoonDaysProperty, value); }); }
        }

        public string NationalStreetDate
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(NationalStreetDateProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(NationalStreetDateProperty, value); }); }
        }

        public decimal BoxOfficeGross
        {
            get { return Dispatcher.Invoke(() => (decimal)GetValue(BoxOfficeGrossProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BoxOfficeGrossProperty, value); }); }
        }

        public bool ClosedCaptioned
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(ClosedCaptionedProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ClosedCaptionedProperty, value); }); }
        }

        public bool SellThruNew
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(SellThruNewProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(SellThruNewProperty, value); }); }
        }

        public int ProductTypeId
        {
            get { return Dispatcher.Invoke(() => (int)GetValue(ProductTypeIdProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ProductTypeIdProperty, value); }); }
        }

        public string MerchandiseDate
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(MerchandiseDateProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(MerchandiseDateProperty, value); }); }
        }

        public bool SellThru
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(SellThruProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(SellThruProperty, value); }); }
        }

        public string SortDate
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(SortDateProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(SortDateProperty, value); }); }
        }

        public string ReleaseDate
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(ReleaseDateProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ReleaseDateProperty, value); }); }
        }

        public int RaitingId
        {
            get { return Dispatcher.Invoke(() => (int)GetValue(RaitingIdProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(RaitingIdProperty, value); }); }
        }

        public decimal PromoValue
        {
            get { return Dispatcher.Invoke(() => (decimal)GetValue(PromoValueProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PromoValueProperty, value); }); }
        }

        public decimal ExtraNight
        {
            get { return Dispatcher.Invoke(() => (decimal)GetValue(ExtraNightProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ExtraNightProperty, value); }); }
        }

        public decimal ExpirationPrice
        {
            get { return Dispatcher.Invoke(() => (decimal)GetValue(ExpirationPriceProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ExpirationPriceProperty, value); }); }
        }

        public decimal InitialNight
        {
            get { return Dispatcher.Invoke(() => (decimal)GetValue(InitialNightProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(InitialNightProperty, value); }); }
        }

        public string PriceSetId
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(PriceSetIdProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PriceSetIdProperty, value); }); }
        }

        public decimal Purchase
        {
            get { return Dispatcher.Invoke(() => (decimal)GetValue(PurchaseProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PurchaseProperty, value); }); }
        }

        public decimal NonReturn
        {
            get { return Dispatcher.Invoke(() => (decimal)GetValue(NonReturnProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(NonReturnProperty, value); }); }
        }

        public decimal NonReturnDays
        {
            get { return Dispatcher.Invoke(() => (decimal)GetValue(NonReturnDaysProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(NonReturnDaysProperty, value); }); }
        }

        public List<long> GenreIds
        {
            get { return Dispatcher.Invoke(() => (List<long>)GetValue(GenreIdsProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(GenreIdsProperty, value); }); }
        }

        public List<string> GenreList
        {
            get { return Dispatcher.Invoke(() => (List<string>)GetValue(GenreListProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(GenreListProperty, value); }); }
        }

        public bool IsComingSoon
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(IsComingSoonProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(IsComingSoonProperty, value); }); }
        }

        public bool InStock
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(InStockProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(InStockProperty, value); }); }
        }

        public bool CanBeSold
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(CanBeSoldProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(CanBeSoldProperty, value); }); }
        }

        public string ProductFamilyName
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(ProductFamilyNameProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ProductFamilyNameProperty, value); }); }
        }

        public string ProductTypeName
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(ProductTypeNameProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ProductTypeNameProperty, value); }); }
        }

        public bool IsAvailable
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(IsAvailableProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(IsAvailableProperty, value); }); }
        }

        public bool ShowOutOfStock
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(ShowOutOfStockProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ShowOutOfStockProperty, value); }); }
        }

        public string Type
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(TypeProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(TypeProperty, value); }); }
        }

        public ObservableCollection<BarcodeProduct> BarCodes
        {
            get { return Dispatcher.Invoke(() => (ObservableCollection<BarcodeProduct>)GetValue(BarCodesProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BarCodesProperty, value); }); }
        }

        public ImageSource ImgFile
        {
            get { return Dispatcher.Invoke(() => (ImageSource)GetValue(ImgFileProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ImgFileProperty, value); }); }
        }

        public long TitleRollupProductGroupId
        {
            get { return Dispatcher.Invoke(() => (long)GetValue(TitleRollupProductGroupIdProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(TitleRollupProductGroupIdProperty, value); }); }
        }

        public Visibility AddProductWithCustomPriceButtonVisibility
        {
            get
            {
                return Dispatcher.Invoke(() => (Visibility)GetValue(AddProductWithCustomPriceButtonVisibilityProperty));
            }
            set { Dispatcher.Invoke(delegate { SetValue(AddProductWithCustomPriceButtonVisibilityProperty, value); }); }
        }

        public Visibility DisplayAddProductWithCustomPriceButtonVisibility
        {
            get
            {
                return Dispatcher.Invoke(() =>
                    (Visibility)GetValue(DisplayAddProductWithCustomPriceButtonVisibilityProperty));
            }
            set
            {
                Dispatcher.Invoke(delegate
                {
                    SetValue(DisplayAddProductWithCustomPriceButtonVisibilityProperty, value);
                });
            }
        }

        public string DiscountRestriction
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(DiscountRestrictionProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(DiscountRestrictionProperty, value); }); }
        }
    }
}