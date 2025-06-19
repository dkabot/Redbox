using System;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Xps.Packaging;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls
{
    public partial class HelpDocViewUserControl : BaseUserControl
    {
        private static readonly DependencyProperty CurrentPageNumberProperty =
            DependencyProperty.Register("CurrentPageNumber", typeof(int), typeof(HelpDocViewUserControl),
                new PropertyMetadata(0, PageNumberChanged));

        private static readonly DependencyProperty TotalPageCountProperty =
            DependencyProperty.Register("TotalPageCount", typeof(int), typeof(HelpDocViewUserControl),
                new PropertyMetadata(0, PageNumberChanged));

        private static readonly DependencyProperty PageCounterTextProperty =
            DependencyProperty.Register("PageCounterText", typeof(string), typeof(HelpDocViewUserControl),
                new PropertyMetadata(null, PageNumberChanged));

        private static readonly DependencyProperty XpsDocumentProperty = DependencyProperty.Register("XpsDocument",
            typeof(XpsDocument), typeof(HelpDocViewUserControl), new PropertyMetadata(null, XpsDocumentChanged));

        private static readonly DependencyProperty CurrentPageTextProperty =
            DependencyProperty.Register("CurrentPageText", typeof(string), typeof(HelpDocViewUserControl),
                new PropertyMetadata(null));

        private FixedDocumentSequence m_fixedDocumentSequence;

        public HelpDocViewUserControl()
        {
            InitializeComponent();
        }

        public int CurrentPageNumber
        {
            get { return Dispatcher.Invoke(() => (int)GetValue(CurrentPageNumberProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(CurrentPageNumberProperty, value); }); }
        }

        public int TotalPageCount
        {
            get { return Dispatcher.Invoke(() => (int)GetValue(TotalPageCountProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(TotalPageCountProperty, value); }); }
        }

        public string PageCounterText
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(PageCounterTextProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(PageCounterTextProperty, value); }); }
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

        public event HelpViewModel.HelpPaddleButtonPressed OnHelpPaddleButtonPressed;

        private static void PageNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var helpDocViewUserControl = d as HelpDocViewUserControl;
            if (helpDocViewUserControl != null) helpDocViewUserControl.SetCurrentPage();
        }

        private static void XpsDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var helpDocViewUserControl = d as HelpDocViewUserControl;
            if (helpDocViewUserControl != null) helpDocViewUserControl.UpdateXpsDocument();
        }

        private void UpdateXpsDocument()
        {
            m_fixedDocumentSequence = XpsDocument != null ? XpsDocument.GetFixedDocumentSequence() : null;
            var fixedDocumentSequence = m_fixedDocumentSequence;
            int? num;
            if (fixedDocumentSequence == null)
            {
                num = null;
            }
            else
            {
                var documentPaginator = fixedDocumentSequence.DocumentPaginator;
                num = documentPaginator != null ? new int?(documentPaginator.PageCount) : null;
            }

            var num2 = num;
            TotalPageCount = num2.GetValueOrDefault();
            ChangePage(0);
        }

        private void UpdatePageCounter()
        {
            page_counter.Text = !string.IsNullOrEmpty(PageCounterText) && m_fixedDocumentSequence != null
                ? string.Format(PageCounterText, CurrentPageNumber, TotalPageCount)
                : null;
        }

        private int ChangePageNumber(int changeAmount)
        {
            var num = CurrentPageNumber + changeAmount;
            CurrentPageNumber = Math.Min(Math.Max(1, num),
                m_fixedDocumentSequence != null ? m_fixedDocumentSequence.DocumentPaginator.PageCount : 1);
            return CurrentPageNumber;
        }

        private void ChangePage(int changeAmount)
        {
            ChangePageNumber(changeAmount);
            CurrentPageText = GetCurrentPageText();
            UpdateDocumentPageVisual(CurrentPageNumber);
            UpdatePaddleVisibility();
        }

        private void SetCurrentPage()
        {
            UpdatePageCounter();
            CurrentPageText = GetCurrentPageText();
            UpdateDocumentPageVisual(CurrentPageNumber);
            UpdatePaddleVisibility();
        }

        private void UpdatePaddleVisibility()
        {
            left_arrow.Visibility = m_fixedDocumentSequence != null && CurrentPageNumber != 1
                ? Visibility.Visible
                : Visibility.Hidden;
            right_arrow.Visibility =
                m_fixedDocumentSequence != null &&
                CurrentPageNumber != m_fixedDocumentSequence.DocumentPaginator.PageCount
                    ? Visibility.Visible
                    : Visibility.Hidden;
        }

        private void PageLeftCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ChangePage(-1);
            HandleWPFHit();
            RaiseOnHelpPaddleButtonPressed("Left");
        }

        private void PageRightCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ChangePage(1);
            HandleWPFHit();
            RaiseOnHelpPaddleButtonPressed("Right");
        }

        private void RaiseOnHelpPaddleButtonPressed(string source)
        {
            var onHelpPaddleButtonPressed = OnHelpPaddleButtonPressed;
            if (onHelpPaddleButtonPressed == null) return;
            onHelpPaddleButtonPressed(source);
        }

        public string GetCurrentPageText()
        {
            var stringBuilder = new StringBuilder();
            if (XpsDocument != null)
            {
                var fixedDocumentSequenceReader = XpsDocument.FixedDocumentSequenceReader;
                if (fixedDocumentSequenceReader != null)
                {
                    var xpsFixedDocumentReader = fixedDocumentSequenceReader.FixedDocuments[0];
                    if (xpsFixedDocumentReader != null)
                    {
                        var xpsFixedPageReader = xpsFixedDocumentReader.FixedPages[Math.Max(0, CurrentPageNumber - 1)];
                        if (xpsFixedPageReader != null)
                        {
                            var xmlReader = xpsFixedPageReader.XmlReader;
                            if (xmlReader != null)
                                while (xmlReader.Read())
                                    if (xmlReader.Name == "Glyphs" && xmlReader.HasAttributes)
                                    {
                                        var attribute = xmlReader.GetAttribute("UnicodeString");
                                        stringBuilder.Append(attribute);
                                    }
                        }
                    }
                }
            }

            return stringBuilder.ToString();
        }

        private void UpdateDocumentPageVisual(int pageNumber)
        {
            VisualBrush visualBrush = null;
            if (m_fixedDocumentSequence != null && m_fixedDocumentSequence.DocumentPaginator != null)
            {
                var page = m_fixedDocumentSequence.DocumentPaginator.GetPage(Math.Max(0, CurrentPageNumber - 1));
                if (page != null) visualBrush = new VisualBrush(page.Visual);
            }

            DocumentPageVisual.Fill = visualBrush;
            InvalidateVisual();
        }
    }
}