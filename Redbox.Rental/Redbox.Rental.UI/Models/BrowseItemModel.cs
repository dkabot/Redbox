using System.Windows;
using Redbox.Rental.Model.Browse;

namespace Redbox.Rental.UI.Models
{
    public class BrowseItemModel : DependencyObject, IBrowseItemModel
    {
        public static readonly DependencyProperty CanAddProperty = DependencyProperty.Register("CanAdd", typeof(bool),
            typeof(BrowseItemModel), new FrameworkPropertyMetadata(false));

        private object _data;

        private bool _isBorderItem;

        private bool _isBottomRow;

        private int _visibleItemIndex;
        public string Name { get; set; }

        public object Data
        {
            get => GetData();
            set => SetData(value);
        }

        public bool IsBorderItem
        {
            get => GetIsBorderItem();
            set => SetIsBorderItem(value);
        }

        public bool IsBottomRow
        {
            get => GetIsBottomRow();
            set => SetIsBottomRow(value);
        }

        public bool CanAdd
        {
            get => GetCanAdd();
            set => SetCanAdd(value);
        }

        public int VisibleItemIndex
        {
            get => GetVisibleItemIndex();
            set => SetVisibleItemIndex(value);
        }

        public int CurrentGridRow { get; set; }

        public int CurrentGridColumn { get; set; }

        protected virtual object GetData()
        {
            return _data;
        }

        protected virtual void SetData(object value)
        {
            _data = value;
        }

        protected virtual bool GetIsBorderItem()
        {
            return _isBorderItem;
        }

        protected virtual void SetIsBorderItem(bool value)
        {
            _isBorderItem = value;
        }

        protected virtual bool GetIsBottomRow()
        {
            return _isBottomRow;
        }

        protected virtual void SetIsBottomRow(bool value)
        {
            _isBottomRow = value;
        }

        protected virtual bool GetCanAdd()
        {
            return Dispatcher.Invoke(() => (bool)GetValue(CanAddProperty));
        }

        protected virtual void SetCanAdd(bool value)
        {
            Dispatcher.Invoke(delegate { SetValue(CanAddProperty, value); });
        }

        protected virtual int GetVisibleItemIndex()
        {
            return _visibleItemIndex;
        }

        protected virtual void SetVisibleItemIndex(int value)
        {
            _visibleItemIndex = value;
        }
    }
}