using System.Windows;
using System.Windows.Controls;

namespace Redbox.Rental.UI.Controls
{
    public partial class PhoneNumberTextBlockUserControl : TextBlock
    {
        private const char DigitPlaceholder = '#';

        public static readonly DependencyProperty FormatMaskProperty = DependencyProperty.Register("FormatMask",
            typeof(string), typeof(PhoneNumberTextBlockUserControl),
            new FrameworkPropertyMetadata(null, OnFormatMaskChanged)
            {
                AffectsRender = true
            });

        public static readonly DependencyProperty DigitsProperty = DependencyProperty.Register("Digits", typeof(string),
            typeof(PhoneNumberTextBlockUserControl), new FrameworkPropertyMetadata(null, OnDigitsChanged)
            {
                AffectsRender = true
            });

        public PhoneNumberTextBlockUserControl()
        {
            InitializeComponent();
        }

        public string FormatMask
        {
            get => (string)GetValue(FormatMaskProperty);
            set => SetValue(FormatMaskProperty, value);
        }

        public string Digits
        {
            get => (string)GetValue(DigitsProperty);
            set => SetValue(DigitsProperty, value);
        }

        public new string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private static void OnFormatMaskChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PhoneNumberTextBlockUserControl).FormatText();
        }

        private static void OnDigitsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PhoneNumberTextBlockUserControl).FormatText();
        }

        private void FormatText()
        {
            var text = string.Empty;
            FormatMask = FormatMask ?? string.Empty;
            Digits = Digits ?? string.Empty;
            var i = 0;
            for (var j = 0; j < FormatMask.Length; j++)
                if (FormatMask[j] == '#')
                {
                    if (i < Digits.Length)
                    {
                        text += Digits[i].ToString();
                        i++;
                    }
                    else
                    {
                        text += " ";
                    }
                }
                else
                {
                    text += FormatMask[j].ToString();
                }

            while (i < Digits.Length) text += Digits[i++].ToString();
            Text = text;
        }
    }
}