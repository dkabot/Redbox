using Redbox.Controls.Utilities;
using System.Windows;
using System.Windows.Controls;

namespace Redbox.Controls
{
    public class SimpleList : ItemsControl
    {
        public static readonly DependencyProperty ItemsHeightProperty =
            Dependency<SimpleList>.CreateDependencyProperty(nameof(ItemsHeight), typeof(int), (object)0);

        public static readonly DependencyProperty ItemsMarginProperty =
            Dependency<SimpleList>.CreateDependencyProperty(nameof(ItemsMargin), typeof(Thickness),
                (object)new Thickness());

        public int ItemsHeight
        {
            get => (int)GetValue(ItemsHeightProperty);
            set => SetValue(ItemsHeightProperty, (object)value);
        }

        public Thickness ItemsMargin
        {
            get => (Thickness)GetValue(ItemsMarginProperty);
            set => SetValue(ItemsMarginProperty, (object)value);
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var contentPresenter = element as ContentPresenter;
            var name = contentPresenter.Margin.GetType().Name;
            if (ItemsHeight > 0)
                contentPresenter.Height = (double)ItemsHeight;
            var itemsMargin = ItemsMargin;
            contentPresenter.Margin = ItemsMargin;
            base.PrepareContainerForItemOverride(element, item);
        }

        protected override bool ShouldApplyItemContainerStyle(DependencyObject container, object item)
        {
            var name = container.GetType().Name;
            return true;
        }
    }
}