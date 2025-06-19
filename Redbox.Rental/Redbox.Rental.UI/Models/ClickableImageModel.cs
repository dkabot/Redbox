using System;
using System.Windows;

namespace Redbox.Rental.UI.Models
{
    public class ClickableImageModel : BaseModel<ClickableImageModel>
    {
        public static readonly DependencyProperty ImageUrlProperty =
            CreateDependencyProperty("ImageUrl", typeof(string));

        public static readonly DependencyProperty OpacityProperty =
            CreateDependencyProperty("Opacity", typeof(double), 1.0);

        public static readonly DependencyProperty ZIndexProperty =
            CreateDependencyProperty("ZIndex", typeof(double), 0.0);

        public static readonly DependencyProperty DisplayDurationProperty =
            CreateDependencyProperty("DisplayDuration", typeof(double), 5.0);

        public string ImageUrl
        {
            get { return Dispatcher.Invoke(() => (string)GetValue(ImageUrlProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ImageUrlProperty, value); }); }
        }

        public Action ClickAction { get; set; }

        public double Opacity
        {
            get { return Dispatcher.Invoke(() => (double)GetValue(OpacityProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(OpacityProperty, value); }); }
        }

        public double ZIndex
        {
            get { return Dispatcher.Invoke(() => (double)GetValue(ZIndexProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(ZIndexProperty, value); }); }
        }

        public double DisplayDuration
        {
            get { return Dispatcher.Invoke(() => (double)GetValue(DisplayDurationProperty)); }
            set { Dispatcher.Invoke(delegate { SetValue(DisplayDurationProperty, value); }); }
        }
    }
}