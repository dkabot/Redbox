using System;
using System.Windows;
using System.Windows.Controls;

namespace Redbox.Controls
{
    public class DocumentViewerHelper : DependencyObject
    {
        public static readonly DependencyProperty DocumentScalingProperty;

        public static DocumentScaling GetDocumentScaling(DependencyObject obj)
        {
            return (DocumentScaling)obj.GetValue(DocumentScalingProperty);
        }

        public static void SetDocumentScaling(DependencyObject obj, DocumentScaling value)
        {
            obj.SetValue(DocumentScalingProperty, (object)value);
        }

        static DocumentViewerHelper()
        {
            var propertyType = typeof(DocumentScaling);
            var ownerType = typeof(DocumentViewerHelper);
            var defaultMetadata = new FrameworkPropertyMetadata();
            defaultMetadata.BindsTwoWayByDefault = true;
            defaultMetadata.PropertyChangedCallback = (PropertyChangedCallback)((obj, e) =>
            {
                var documentViewer = (DocumentViewer)obj;
                switch (GetDocumentScaling((DependencyObject)documentViewer))
                {
                    case DocumentScaling.Normal:
                        documentViewer.Zoom = 100.0;
                        break;
                    case DocumentScaling.FitHeight:
                        documentViewer.FitToHeight();
                        break;
                    case DocumentScaling.FitWidth:
                        documentViewer.FitToWidth();
                        break;
                }
            });
            DocumentScalingProperty = DependencyProperty.RegisterAttached("DocumentScaling", propertyType, ownerType,
                (PropertyMetadata)defaultMetadata);
        }
    }
}