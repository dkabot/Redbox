using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Redbox.Rental.Model.Ads;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.UI.Models
{
    public class SimpleModel : BaseModel<SimpleModel>
    {
        public static readonly DependencyProperty BannerImageProperty =
            CreateDependencyProperty("BannerImage", TYPES.BITMAPIMAGE);

        public static readonly DependencyProperty BannerImageVisibilityProperty =
            CreateDependencyProperty("BannerImageVisibility", TYPES.VISIBILITY, Visibility.Visible);

        public static readonly DependencyProperty DisplayImageProperty =
            CreateDependencyProperty("DisplayImage", TYPES.BITMAPIMAGE);

        public static readonly DependencyProperty TitleTextProperty =
            CreateDependencyProperty("TitleText", TYPES.STRING, string.Empty);

        public static readonly DependencyProperty MessageTextProperty =
            CreateDependencyProperty("MessageText", TYPES.STRING, string.Empty);

        public static readonly DependencyProperty CommentTextProperty =
            CreateDependencyProperty("CommentText", TYPES.STRING, string.Empty);

        public static readonly DependencyProperty FooterTextProperty =
            CreateDependencyProperty("FooterText", TYPES.STRING, string.Empty);

        public static readonly DependencyProperty CancelButtonTextProperty =
            CreateDependencyProperty("CancelButtonText", TYPES.STRING, string.Empty);

        public static readonly DependencyProperty ContinueButtonTextProperty =
            CreateDependencyProperty("ContinueButtonText", TYPES.STRING, string.Empty);

        public static readonly DependencyProperty Circle1TextProperty =
            CreateDependencyProperty("Circle1Text", TYPES.STRING, string.Empty);

        public static readonly DependencyProperty Circle2TextProperty =
            CreateDependencyProperty("Circle2Text", TYPES.STRING, string.Empty);

        public static readonly DependencyProperty Circle3TextProperty =
            CreateDependencyProperty("Circle3Text", TYPES.STRING, string.Empty);

        public static readonly DependencyProperty OtherButtonTextProperty =
            CreateDependencyProperty("OtherButtonText", TYPES.STRING, string.Empty);

        public static readonly DependencyProperty CheckboxTextProperty =
            CreateDependencyProperty("CheckboxText", TYPES.STRING, string.Empty);

        public static readonly DependencyProperty CheckboxCheckedProperty =
            DependencyProperty.Register("CheckboxChecked", TYPES.BOOL, typeof(SimpleModel),
                new FrameworkPropertyMetadata(false, CheckboxPropertyChanged));

        public Action OtherAction { get; set; }

        public Action CancelAction { get; set; }

        public Action ContinueAction { get; set; }

        public Action<bool> CheckBoxChangedAction { get; set; }

        public SimpleInfo Info { get; set; }

        public DynamicRoutedCommand OtherButtonCommand { get; set; }

        public DynamicRoutedCommand CancelButtonCommand { get; set; }

        public DynamicRoutedCommand ContinueButtonCommand { get; set; }

        public BitmapImage BannerImage
        {
            get { return Dispatcher.Invoke(() => (BitmapImage)GetValue(BannerImageProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BannerImageProperty, value); }); }
        }

        public Visibility BannerImageVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(BannerImageVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BannerImageVisibilityProperty, value); }); }
        }

        public BitmapImage DisplayImage
        {
            get { return Dispatcher.Invoke(() => (BitmapImage)GetValue(DisplayImageProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(DisplayImageProperty, value); }); }
        }

        public string TitleText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(TitleTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(TitleTextProperty, value); }); }
        }

        public string MessageText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(MessageTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(MessageTextProperty, value); }); }
        }

        public string CommentText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(CommentTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(CommentTextProperty, value); }); }
        }

        public string FooterText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(FooterTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(FooterTextProperty, value); }); }
        }

        public string CancelButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(CancelButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(CancelButtonTextProperty, value); }); }
        }

        public string ContinueButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(ContinueButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ContinueButtonTextProperty, value); }); }
        }

        public string Circle1Text
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(Circle1TextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(Circle1TextProperty, value); }); }
        }

        public string Circle2Text
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(Circle2TextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(Circle2TextProperty, value); }); }
        }

        public string Circle3Text
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(Circle3TextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(Circle3TextProperty, value); }); }
        }

        public string OtherButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(OtherButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(OtherButtonTextProperty, value); }); }
        }

        public string CheckboxText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(CheckboxTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(CheckboxTextProperty, value); }); }
        }

        public bool CheckboxChecked
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(CheckboxCheckedProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(CheckboxCheckedProperty, value); }); }
        }

        public IAdImpression AdImpression { get; set; }

        private static void CheckboxPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var simpleModel = d as SimpleModel;
            if (simpleModel != null) simpleModel.CheckBoxChangedAction(simpleModel.CheckboxChecked);
        }
    }
}