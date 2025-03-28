using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Redbox.Controls
{
    public class RichTextBoxHelper : DependencyObject
    {
        public static readonly DependencyProperty DocumentRtfProperty;

        public static byte[] GetDocumentRtf(DependencyObject obj)
        {
            return (byte[])obj.GetValue(DocumentRtfProperty);
        }

        public static void SetDocumentRtf(DependencyObject obj, byte[] value)
        {
            obj.SetValue(DocumentRtfProperty, (object)value);
        }

        static RichTextBoxHelper()
        {
            var propertyType = typeof(byte[]);
            var ownerType = typeof(RichTextBoxHelper);
            var defaultMetadata = new FrameworkPropertyMetadata();
            defaultMetadata.BindsTwoWayByDefault = true;
            defaultMetadata.PropertyChangedCallback = (PropertyChangedCallback)((obj, e) =>
            {
                var richTextBox = (RichTextBox)obj;
                var documentRtf = GetDocumentRtf((DependencyObject)richTextBox);
                var doc = new FlowDocument();
                var range = new TextRange(doc.ContentStart, doc.ContentEnd);
                range.Load((Stream)new MemoryStream(documentRtf), DataFormats.Rtf);
                richTextBox.Document = doc;
                range.Changed += (EventHandler)((obj2, e2) =>
                {
                    if (richTextBox.Document != doc)
                        return;
                    var memoryStream = new MemoryStream();
                    range.Save((Stream)memoryStream, DataFormats.Rtf);
                    SetDocumentRtf((DependencyObject)richTextBox, memoryStream.ToArray());
                });
            });
            DocumentRtfProperty = DependencyProperty.RegisterAttached("DocumentRtf", propertyType, ownerType,
                (PropertyMetadata)defaultMetadata);
        }
    }
}