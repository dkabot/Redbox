using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Xps.Packaging;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.Model.Session;

namespace Redbox.Rental.UI.Models
{
    public class HelpViewModel : BaseModel<HelpViewModel>
    {
        public delegate void HelpDocumentChangedHandler(HelpDocuments helpDocument);

        public delegate void HelpDocumentPageChangedHandler(int pageNumber);

        public delegate void HelpPaddleButtonPressed(string source);

        public static DependencyProperty BackButtonTextProperty =
            CreateDependencyProperty("BackButtonText", TYPES.STRING);

        public static DependencyProperty CloseButtonTextProperty =
            CreateDependencyProperty("CloseButtonText", TYPES.STRING);

        public static DependencyProperty TitleTextProperty = CreateDependencyProperty("TitleText", TYPES.STRING);

        public static DependencyProperty ContactTextProperty = CreateDependencyProperty("ContactText", TYPES.STRING);

        public static DependencyProperty ShowContactTextProperty =
            CreateDependencyProperty("ShowContactText", TYPES.BOOL, false);

        public static DependencyProperty StoreNumberLabelProperty =
            CreateDependencyProperty("StoreNumberLabel", TYPES.STRING);

        public static DependencyProperty StoreNumberTextProperty =
            CreateDependencyProperty("StoreNumberText", TYPES.STRING);

        public static DependencyProperty PageCounterTextProperty =
            CreateDependencyProperty("PageCounterText", TYPES.STRING);

        private static readonly DependencyProperty CurrentHelpDocumentPageProperty =
            DependencyProperty.Register("CurrentHelpDocumentPage", typeof(int), typeof(HelpViewModel),
                new PropertyMetadata(0, CurrentHelpDocPageChanged));

        public static DependencyProperty HelpDocumentPageCountProperty =
            CreateDependencyProperty("HelpDocumentPageCount", TYPES.INT, 0);

        public static DependencyProperty XpsDocumentProperty =
            CreateDependencyProperty("XpsDocument", typeof(XpsDocument));

        public static DependencyProperty CurrentPageTextProperty =
            CreateDependencyProperty("CurrentPageText", TYPES.STRING);

        private HelpDocuments m_currentHelpDocument;

        public Func<ISpeechControl> OnGetSpeechControl;

        public HelpViewModel()
        {
            ButtonList = new ObservableCollection<HelpButtonData>();
        }

        public ObservableCollection<HelpButtonData> ButtonList { get; set; }

        public string BackButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(BackButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(BackButtonTextProperty, value); }); }
        }

        public string CloseButtonText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(CloseButtonTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(CloseButtonTextProperty, value); }); }
        }

        public string TitleText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(TitleTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(TitleTextProperty, value); }); }
        }

        public string ContactText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(ContactTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ContactTextProperty, value); }); }
        }

        public bool ShowContactText
        {
            get { return Dispatcher.Invoke(() => (bool)GetValue(ShowContactTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ShowContactTextProperty, value); }); }
        }

        public string StoreNumberLabel
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(StoreNumberLabelProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(StoreNumberLabelProperty, value); }); }
        }

        public string StoreNumberText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(StoreNumberTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(StoreNumberTextProperty, value); }); }
        }

        public string PageCounterText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(PageCounterTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PageCounterTextProperty, value); }); }
        }

        public int CurrentHelpDocumentPage
        {
            get { return Dispatcher.Invoke(() => (int)GetValue(CurrentHelpDocumentPageProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(CurrentHelpDocumentPageProperty, value); }); }
        }

        public int HelpDocumentPageCount
        {
            get { return Dispatcher.Invoke(() => (int)GetValue(HelpDocumentPageCountProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(HelpDocumentPageCountProperty, value); }); }
        }

        public XpsDocument XpsDocument
        {
            get { return Dispatcher.Invoke(() => (XpsDocument)GetValue(XpsDocumentProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(XpsDocumentProperty, value); }); }
        }

        public string CurrentPageText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(CurrentPageTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(CurrentPageTextProperty, value); }); }
        }

        public HelpDocuments CurrentHelpDocument
        {
            get => m_currentHelpDocument;
            set
            {
                m_currentHelpDocument = value;
                foreach (var helpButtonData in ButtonList)
                    helpButtonData.IsEnabled = helpButtonData.DocumentToShow != m_currentHelpDocument;
            }
        }

        public Dictionary<HelpDocuments, HelpDocumentInfo> HelpDocumentInfoList { get; set; } =
            new Dictionary<HelpDocuments, HelpDocumentInfo>();

        public event HelpDocumentChangedHandler OnHelpDocumentChanged;

        public event HelpDocumentChangedHandler OnHelpDocumentButtonPress;

        public event HelpDocumentPageChangedHandler OnHelpDocumentPageChanged;

        public event HelpPaddleButtonPressed OnHelpPaddleButtonPressed;

        public event Action OnGoBack;

        public event Action OnRedboxLogoCommand;

        private static void CurrentHelpDocPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HelpViewModel)d).ProcessHelpDocumentPageChanged((int)e.NewValue);
        }

        public void ProcessHelpDocumentButtonPress(HelpDocuments helpDocument)
        {
            var onHelpDocumentButtonPress = OnHelpDocumentButtonPress;
            if (onHelpDocumentButtonPress == null) return;
            onHelpDocumentButtonPress(helpDocument);
        }

        public void ProcessHelpDocumentChanged(HelpDocuments helpDocument)
        {
            var onHelpDocumentChanged = OnHelpDocumentChanged;
            if (onHelpDocumentChanged == null) return;
            onHelpDocumentChanged(helpDocument);
        }

        public void ProcessHelpDocumentPageChanged(int pageNumber)
        {
            var onHelpDocumentPageChanged = OnHelpDocumentPageChanged;
            if (onHelpDocumentPageChanged == null) return;
            onHelpDocumentPageChanged(pageNumber);
        }

        public void ProcessHelpPaddleButtonPressed(string source)
        {
            var onHelpPaddleButtonPressed = OnHelpPaddleButtonPressed;
            if (onHelpPaddleButtonPressed == null) return;
            onHelpPaddleButtonPressed(source);
        }

        public void ProcessGoBack()
        {
            var onGoBack = OnGoBack;
            if (onGoBack == null) return;
            onGoBack();
        }

        public void ProcessRedboxLogoCommand()
        {
            var onRedboxLogoCommand = OnRedboxLogoCommand;
            if (onRedboxLogoCommand == null) return;
            onRedboxLogoCommand();
        }

        public class HelpDocumentInfo
        {
            public string DocumentName { get; set; }

            public string DefaultDocumentName { get; set; }

            public string Title { get; set; }

            public bool Visible { get; set; }

            public string AnalyticsButtonName { get; set; }

            public string AbeTitle { get; set; }
        }
    }
}