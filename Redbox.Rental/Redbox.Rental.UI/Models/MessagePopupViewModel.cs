using System;
using System.Windows;
using System.Windows.Controls;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models
{
    public class MessagePopupViewModel : BaseModel<MessagePopupViewModel>
    {
        public static readonly DependencyProperty BorderBrushColorProperty = DependencyProperty.Register(
            "BorderBrushColor", typeof(string), typeof(MessagePopupViewModel), new FrameworkPropertyMetadata(null)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty MainGridMarginProperty = DependencyProperty.Register("MainGridMargin",
            typeof(Thickness), typeof(MessagePopupViewModel), new FrameworkPropertyMetadata(null)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty Button1TextProperty = DependencyProperty.Register("Button1Text",
            typeof(string), typeof(MessagePopupViewModel), new FrameworkPropertyMetadata(null)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty Button1VisibilityProperty = DependencyProperty.Register(
            "Button1Visibility", typeof(Visibility), typeof(MessagePopupViewModel),
            new FrameworkPropertyMetadata(Visibility.Collapsed)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty Button2TextProperty = DependencyProperty.Register("Button2Text",
            typeof(string), typeof(MessagePopupViewModel), new FrameworkPropertyMetadata(null)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty Button2VisibilityProperty = DependencyProperty.Register(
            "Button2Visibility", typeof(Visibility), typeof(MessagePopupViewModel),
            new FrameworkPropertyMetadata(Visibility.Collapsed)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty CountdownTimerGridVisibilityProperty = DependencyProperty.Register(
            "CountdownTimerGridVisibility", typeof(Visibility), typeof(MessagePopupViewModel),
            new FrameworkPropertyMetadata(Visibility.Collapsed)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty CountdownTimerTextProperty = DependencyProperty.Register(
            "CountdownTimerText", typeof(string), typeof(MessagePopupViewModel), new FrameworkPropertyMetadata(null)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty ButtonGridSeparatorLineVisibilityProperty =
            DependencyProperty.Register("ButtonGridSeparatorLineVisibility", typeof(Visibility),
                typeof(MessagePopupViewModel), new FrameworkPropertyMetadata(Visibility.Collapsed)
                {
                    AffectsRender = true
                });

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string),
            typeof(MessagePopupViewModel), new FrameworkPropertyMetadata(null)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message",
            typeof(string), typeof(MessagePopupViewModel), new FrameworkPropertyMetadata(null)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty BackgroundOpacityProperty = DependencyProperty.Register(
            "BackgroundOpacity", typeof(double), typeof(MessagePopupViewModel), new FrameworkPropertyMetadata(0.6)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty TitleStyleProperty = DependencyProperty.Register("TitleStyle",
            typeof(Style), typeof(MessagePopupViewModel), new FrameworkPropertyMetadata(null)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty MessageStyleProperty = DependencyProperty.Register("MessageStyle",
            typeof(Style), typeof(MessagePopupViewModel), new FrameworkPropertyMetadata(null)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty Button1TextStyleProperty = DependencyProperty.Register(
            "Button1TextStyle", typeof(Style), typeof(MessagePopupViewModel), new FrameworkPropertyMetadata(null)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty Button1StyleProperty = DependencyProperty.Register("Button1Style",
            typeof(Style), typeof(MessagePopupViewModel), new FrameworkPropertyMetadata(null)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty Button2TextStyleProperty = DependencyProperty.Register(
            "Button2TextStyle", typeof(Style), typeof(MessagePopupViewModel), new FrameworkPropertyMetadata(null)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty Button2StyleProperty = DependencyProperty.Register("Button2Style",
            typeof(Style), typeof(MessagePopupViewModel), new FrameworkPropertyMetadata(null)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty ButtonGridStyleProperty = DependencyProperty.Register(
            "ButtonGridStyle", typeof(Style), typeof(MessagePopupViewModel), new FrameworkPropertyMetadata(null)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty InnerGridStyleProperty = DependencyProperty.Register("InnerGridStyle",
            typeof(Style), typeof(MessagePopupViewModel), new FrameworkPropertyMetadata(null)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty FloatingButtonGridStyleProperty = DependencyProperty.Register(
            "FloatingButtonGridStyle", typeof(Style), typeof(MessagePopupViewModel), new FrameworkPropertyMetadata(null)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty MessageDockPanelStyleProperty = DependencyProperty.Register(
            "MessageDockPanelStyle", typeof(Style), typeof(MessagePopupViewModel), new FrameworkPropertyMetadata(null)
            {
                AffectsRender = true
            });

        public Action OnButton1Clicked;

        public Action OnButton2Clicked;

        public Action OnTimeOut;
        public int TimeOut { get; set; }

        public UserControl ContentUserControl { get; set; }

        public Visibility FixedSizeGridVisibility { get; set; }

        public Visibility AutoSizedGridVisibility { get; set; }

        public string Button1Text
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(Button1TextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(Button1TextProperty, value); }); }
        }

        public Visibility Button1Visibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(Button1VisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(Button1VisibilityProperty, value); }); }
        }

        public string Button2Text
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(Button2TextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(Button2TextProperty, value); }); }
        }

        public Visibility Button2Visibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(Button2VisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(Button2VisibilityProperty, value); }); }
        }

        public Visibility CountdownTimerGridVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(CountdownTimerGridVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(CountdownTimerGridVisibilityProperty, value); }); }
        }

        public string CountdownTimerText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(CountdownTimerTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(CountdownTimerTextProperty, value); }); }
        }

        public Style TitleStyle
        {
            get { return Dispatcher.Invoke(() => (Style)GetValue(TitleStyleProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(TitleStyleProperty, value); }); }
        }

        public Style MessageStyle
        {
            get { return Dispatcher.Invoke(() => (Style)GetValue(MessageStyleProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(MessageStyleProperty, value); }); }
        }

        public double BackgroundOpacity
        {
            get { return Dispatcher.Invoke(() => (double)GetValue(BackgroundOpacityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BackgroundOpacityProperty, value); }); }
        }

        public string Message
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(MessageProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(MessageProperty, value); }); }
        }

        public string Title
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(TitleProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(TitleProperty, value); }); }
        }

        public Style Button1TextStyle
        {
            get { return Dispatcher.Invoke(() => (Style)GetValue(Button1TextStyleProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(Button1TextStyleProperty, value); }); }
        }

        public Style Button1Style
        {
            get { return Dispatcher.Invoke(() => (Style)GetValue(Button1StyleProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(Button1StyleProperty, value); }); }
        }

        public Style Button2TextStyle
        {
            get { return Dispatcher.Invoke(() => (Style)GetValue(Button2TextStyleProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(Button2TextStyleProperty, value); }); }
        }

        public Style Button2Style
        {
            get { return Dispatcher.Invoke(() => (Style)GetValue(Button2StyleProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(Button2StyleProperty, value); }); }
        }

        public Style ButtonGridStyle
        {
            get { return Dispatcher.Invoke(() => (Style)GetValue(ButtonGridStyleProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ButtonGridStyleProperty, value); }); }
        }

        public Style InnerGridStyle
        {
            get { return Dispatcher.Invoke(() => (Style)GetValue(InnerGridStyleProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(InnerGridStyleProperty, value); }); }
        }

        public Style FloatingButtonGridStyle
        {
            get { return Dispatcher.Invoke(() => (Style)GetValue(FloatingButtonGridStyleProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(FloatingButtonGridStyleProperty, value); }); }
        }

        public Style MessageDockPanelStyle
        {
            get { return Dispatcher.Invoke(() => (Style)GetValue(MessageDockPanelStyleProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(MessageDockPanelStyleProperty, value); }); }
        }

        public Visibility ButtonGridSeparatorLineVisibility
        {
            get { return Dispatcher.Invoke(() => (Visibility)GetValue(ButtonGridSeparatorLineVisibilityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ButtonGridSeparatorLineVisibilityProperty, value); }); }
        }

        public string BorderBrushColor
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(BorderBrushColorProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BorderBrushColorProperty, value); }); }
        }

        public Thickness MainGridMargin
        {
            get { return Dispatcher.Invoke(() => (Thickness)GetValue(MainGridMarginProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(MainGridMarginProperty, value); }); }
        }

        public Func<ISpeechControl> OnGetSpeechControl { get; set; }
    }
}