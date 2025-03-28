using Redbox.Controls.Utilities;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Redbox.Controls
{
    public class GridExt : Grid
    {
        public static readonly DependencyProperty SingleRadiusProperty =
            Dependency<GridExt>.CreateDependencyProperty(nameof(SingleRadius), typeof(double), (object)0.0);

        public static readonly DependencyProperty BorderRectProperty =
            Dependency<GridExt>.CreateDependencyProperty(nameof(BorderRect), typeof(Rect),
                (object)new Rect(0.0, 0.0, 0.0, 0.0));

        public static readonly DependencyProperty NumberOfRowsProperty =
            Dependency<GridExt>.CreateDependencyProperty(nameof(NumberOfRows), typeof(int), (object)1,
                new PropertyChangedCallback(OnNumberOfRowsChanged));

        public static readonly DependencyProperty NumberOfColumnsProperty =
            Dependency<GridExt>.CreateDependencyProperty(nameof(NumberOfColumns), typeof(int), (object)1,
                new PropertyChangedCallback(OnNumberOfColumnsChanged));

        public static readonly DependencyProperty RowHeightsProperty =
            Dependency<GridExt>.CreateDependencyProperty(nameof(RowHeights), typeof(string), (object)"*",
                new PropertyChangedCallback(OnRowHeightsChanged));

        public static readonly DependencyProperty ColumnWidthsProperty =
            Dependency<GridExt>.CreateDependencyProperty(nameof(ColumnWidths), typeof(string), (object)"*",
                new PropertyChangedCallback(OnColumnWidthsChanged));

        public double SingleRadius
        {
            get => (double)GetValue(SingleRadiusProperty);
            private set => SetValue(SingleRadiusProperty, (object)value);
        }

        public Rect BorderRect
        {
            get => (Rect)GetValue(BorderRectProperty);
            private set => SetValue(BorderRectProperty, (object)value);
        }

        public int NumberOfRows
        {
            get => RowDefinitions.Count;
            set => SetValue(NumberOfRowsProperty, (object)value);
        }

        public int NumberOfColumns
        {
            get => ColumnDefinitions.Count;
            set => SetValue(NumberOfColumnsProperty, (object)value);
        }

        public string RowHeights
        {
            get => (string)GetValue(RowHeightsProperty);
            set => SetValue(RowHeightsProperty, (object)value);
        }

        public string ColumnWidths
        {
            get => (string)GetValue(ColumnWidthsProperty);
            set => SetValue(ColumnWidthsProperty, (object)value);
        }

        private void SetNumberOfRows(int count)
        {
            RowDefinitions.Clear();
            for (var index = 0; index < count; ++index)
                RowDefinitions.Add(new RowDefinition());
            if (count <= 0 || ColumnDefinitions.Count != 0)
                return;
            ColumnDefinitions.Add(new ColumnDefinition());
        }

        private void SetNumberOfColumns(int count)
        {
            ColumnDefinitions.Clear();
            for (var index = 0; index < count; ++index)
                ColumnDefinitions.Add(new ColumnDefinition());
            if (count <= 0 || RowDefinitions.Count != 0)
                return;
            RowDefinitions.Add(new RowDefinition());
        }

        private void SetRowHeights(string heights)
        {
            RowDefinitions.Clear();
            ConvertToDefinitions(heights, typeof(RowDefinition));
            if (ColumnDefinitions.Count != 0)
                return;
            SetTypeValue(typeof(ColumnDefinition), new GridLength(1.0, GridUnitType.Star));
        }

        private void SetColumnWidths(string heights)
        {
            ColumnDefinitions.Clear();
            ConvertToDefinitions(heights, typeof(ColumnDefinition));
            if (RowDefinitions.Count != 0)
                return;
            SetTypeValue(typeof(RowDefinition), new GridLength(1.0, GridUnitType.Star));
        }

        private static void OnNumberOfRowsChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            ((GridExt)d).SetNumberOfRows((int)e.NewValue);
        }

        private static void OnNumberOfColumnsChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            ((GridExt)d).SetNumberOfColumns((int)e.NewValue);
        }

        private static void OnRowHeightsChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            ((GridExt)d).SetRowHeights((string)e.NewValue);
        }

        private static void OnColumnWidthsChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            ((GridExt)d).SetColumnWidths((string)e.NewValue);
        }

        private static bool IsWidthHeightValid(object value)
        {
            var d = (double)value;
            if (double.IsNaN(d))
                return true;
            return d >= 0.0 && !double.IsPositiveInfinity(d);
        }

        private void SetTypeValue(Type defType, GridLength gridLength)
        {
            if (defType == typeof(ColumnDefinition))
            {
                ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = gridLength
                });
            }
            else
            {
                if (!(defType == typeof(RowDefinition)))
                    throw new InvalidOperationException(string.Format(
                        "Invalid Type passed to GridExt:SetTypeValue: {0}",
                        defType == (Type)null ? (object)"null" : (object)defType.ToString()));
                RowDefinitions.Add(new RowDefinition()
                {
                    Height = gridLength
                });
            }
        }

        private void ConvertToDefinitions(string value, Type defType)
        {
            var str1 = value;
            var chArray = new char[1] { ',' };
            foreach (var str2 in str1.Split(chArray))
            {
                value = str2.Trim().ToLower();
                double result;
                if (double.TryParse(value, out result))
                {
                    SetTypeValue(defType, new GridLength(result, GridUnitType.Pixel));
                }
                else if (string.Empty.Equals(value))
                {
                    SetTypeValue(defType, new GridLength(1.0, GridUnitType.Star));
                }
                else if (value.Contains("*"))
                {
                    value = value.Replace("*", "");
                    if (double.TryParse(value, out result))
                        SetTypeValue(defType, new GridLength(result, GridUnitType.Star));
                    else
                        SetTypeValue(defType, new GridLength(1.0, GridUnitType.Star));
                }
                else
                {
                    if (!value.Equals("auto"))
                        throw new InvalidOperationException(!(defType == typeof(ColumnDefinition))
                            ? string.Format("RowDefinition: Height value: {0} is invalid", (object)value)
                            : string.Format("ColumnDefinition: Width value: {0} is invalid", (object)value));
                    SetTypeValue(defType, new GridLength(0.0, GridUnitType.Auto));
                }
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            BorderRect = new Rect(sizeInfo.NewSize);
            SingleRadius = ActualHeight / 2.0;
        }
    }
}