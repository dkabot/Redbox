using Redbox.Controls.Utilities;
using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace Redbox.Controls.BaseControls
{
    [Browsable(false)]
    [ContentProperty("Child")]
    public class BaseContainer : FrameworkElement, IAddChild
    {
        private UIElement _childElement;

        void IAddChild.AddChild(object value)
        {
            if (!(value is UIElement))
                throw new ArgumentException(
                    string.Format("UnexpectedParameterType: {0}, Parameter must be of type: {1}",
                        (object)value.GetType(), (object)typeof(UIElement)), nameof(value));
            Child = Child == null
                ? (UIElement)value
                : throw new ArgumentException(string.Format("{0} CanOnlyHaveOneChild of type: {1}", (object)GetType(),
                    (object)value.GetType()));
        }

        void IAddChild.AddText(string text)
        {
            if (!string.IsNullOrEmpty(text))
                throw new ArgumentException(string.Format("Container cannot have text: {1}", (object)GetType()));
        }

        private void SetChild(UIElement child)
        {
            if (_childElement == child)
                return;
            RemoveVisualChild((Visual)_childElement);
            RemoveLogicalChild((object)_childElement);
            _childElement = child;
            AddLogicalChild((object)child);
            AddVisualChild((Visual)child);
            InvalidateMeasure();
        }

        [DefaultValue(null)]
        public virtual UIElement Child
        {
            get => _childElement;
            set => SetChild(value);
        }

        protected override IEnumerator LogicalChildren => _childElement != null
            ? (IEnumerator)new SingleChildEnumerator((object)_childElement)
            : EmptyEnumerator.Instance;

        protected override int VisualChildrenCount => _childElement != null ? 1 : 0;

        protected override Visual GetVisualChild(int index)
        {
            return _childElement != null && index == 0
                ? (Visual)_childElement
                : throw new ArgumentOutOfRangeException(nameof(index), (object)index, "Visual_ArgumentOutOfRange");
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var child = Child;
            if (child == null)
                return new Size();
            child.Measure(constraint);
            return child.DesiredSize;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            Child?.Arrange(new Rect(arrangeSize));
            return arrangeSize;
        }

        protected UIElement IntChild
        {
            get => _childElement;
            set => _childElement = value;
        }
    }
}