using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Brush = System.Drawing.Brush;
using Brushes = System.Drawing.Brushes;
using Color = System.Drawing.Color;
using FontFamily = System.Windows.Media.FontFamily;
using FontStyle = System.Drawing.FontStyle;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Image = System.Drawing.Image;
using LinearGradientBrush = System.Windows.Media.LinearGradientBrush;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Pen = System.Drawing.Pen;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using UserControl = System.Windows.Controls.UserControl;
using WebBrowser = System.Windows.Forms.WebBrowser;

namespace Redbox.KioskEngine.Drawing
{
    public class Actor : IActor, IDisposable
    {
        private static bool m_drawErrorTop;
        private readonly double WPFSizeConvertion = 1.3333333730697632;
        private Form _mainForm;
        private Color m_backgroundColor;
        private Color? m_borderColor;
        private WebBrowser m_browserControl;
        private bool m_clickStarted;
        private Size? m_cornerSize;
        private float? m_cornerSweepAngle;
        private bool m_enabled;
        private string m_errorText;
        private Font m_font;
        private Color m_foregroundColor;
        private FrameDimension m_frameDimension;
        private int m_frameNumber;
        private float? m_gradientAngle;
        private Color? m_gradientTargetColor;
        private int m_height;
        private StringAlignment m_horizontalAlignment;
        private string m_html;
        private Image m_image;
        private bool? m_locked;
        private float m_opacity;
        private RenderOptionFlags m_optionFlags;
        private float m_strokeWeight;
        private IStyleSheetStyle m_style;
        private string m_text;
        private float? m_textRotationAngle;
        private Point? m_textTranslationPoint;
        private StringAlignment m_verticalAlignment;
        private bool m_visible;
        private int m_width;
        private FrameworkElement m_wpfFrameworkElement;
        private System.Windows.Controls.Image m_wpfImage;
        private TextBlock m_wpfTextBlock;
        private int m_x;
        private int m_y;

        public Actor()
        {
            Opacity = 1f;
            StrokeWeight = 1f;
            Enabled = true;
            Visible = true;
            IsDirty = true;
            BackgroundColor = Color.Black;
            ForegroundColor = Color.White;
            OptionFlags = RenderOptionFlags.Text | RenderOptionFlags.Image;
        }

        public IntPtr MainHandle
        {
            get
            {
                if (_mainForm == null)
                    _mainForm = (ServiceLocator.Instance.GetService<IEngineApplication>() as ApplicationContext)
                        .MainForm;
                return _mainForm.Handle;
            }
        }

        private HorizontalAlignment WPFHorizontalAlignmnet
        {
            get
            {
                switch (HorizontalAlignment)
                {
                    case StringAlignment.Near:
                        return System.Windows.HorizontalAlignment.Left;
                    case StringAlignment.Center:
                        return System.Windows.HorizontalAlignment.Center;
                    case StringAlignment.Far:
                        return System.Windows.HorizontalAlignment.Right;
                    default:
                        return System.Windows.HorizontalAlignment.Left;
                }
            }
        }

        private VerticalAlignment WPFVerticalAlignmnet
        {
            get
            {
                switch (VerticalAlignment)
                {
                    case StringAlignment.Near:
                        return System.Windows.VerticalAlignment.Top;
                    case StringAlignment.Center:
                        return System.Windows.VerticalAlignment.Center;
                    case StringAlignment.Far:
                        return System.Windows.VerticalAlignment.Bottom;
                    default:
                        return System.Windows.VerticalAlignment.Top;
                }
            }
        }

        private Thickness Margin => new Thickness(X, Y, Width == 0 ? 0.0 : Scene.Width - Width - X,
            Height == 0 ? 0.0 : Scene.Height - Height - Y);

        public event WPFHitHandler OnWPFHit;

        public void RaiseHit()
        {
            if (Hit == null)
                return;
            Hit(this, EventArgs.Empty);
        }

        public bool HitExists => Hit != null;

        public void ClearHit()
        {
            Hit = null;
        }

        public void MarkDirty()
        {
            NotifyScene(new Rectangle[1]
            {
                new Rectangle(X, Y, Width, Height)
            });
        }

        public virtual void Dispose()
        {
            ClearHit();
        }

        public MeasureTextResult MeasureText()
        {
            var service = ServiceLocator.Instance.GetService<IMacroService>();
            using (var graphics = Graphics.FromHwnd(MainHandle))
            {
                graphics.PixelOffsetMode = PixelOffsetMode.Half;
                graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                var textRegion = GetTextRegion(false);
                var stringFormat = (StringFormat)StringFormat.GenericTypographic.Clone();
                int charactersFitted;
                int linesFilled;
                return new MeasureTextResult(
                    graphics.MeasureString(service.ExpandProperties(Text), Font,
                        new SizeF(textRegion.Width, textRegion.Height), stringFormat, out charactersFitted,
                        out linesFilled), charactersFitted, linesFilled);
            }
        }

        public void CenterHorizontally()
        {
            if (Scene == null)
                return;
            X = (Scene.Width - Width) / 2;
        }

        public void CenterVertically()
        {
            if (Scene == null)
                return;
            Y = (Scene.Height - Height) / 2;
        }

        public void Render(Graphics context)
        {
            OnRender(context);
        }

        public int X
        {
            get => m_x;
            set
            {
                if (m_x == value)
                    return;
                var rectangle = new Rectangle(m_x, Y, Width, Height);
                m_x = value;
                NotifyScene(new Rectangle[2]
                {
                    rectangle,
                    new Rectangle(m_x, Y, Width, Height)
                });
                if (m_wpfFrameworkElement == null)
                    return;
                m_wpfFrameworkElement.Margin = Margin;
            }
        }

        public int Y
        {
            get => m_y;
            set
            {
                if (m_y == value)
                    return;
                var rectangle = new Rectangle(X, m_y, Width, Height);
                m_y = value;
                NotifyScene(new Rectangle[2]
                {
                    rectangle,
                    new Rectangle(X, m_y, Width, Height)
                });
                if (m_wpfFrameworkElement == null)
                    return;
                m_wpfFrameworkElement.Margin = Margin;
            }
        }

        public Font Font
        {
            get
            {
                if (m_font == null)
                {
                    var service = ServiceLocator.Instance.GetService<IFontCacheService>();
                    if (service != null)
                        m_font = service.RegisterFont("DefaultActorFont", "Arial", 16f, FontStyle.Regular);
                }

                return m_font;
            }
            set => SetFieldValue(ref m_font, value);
        }

        public int Width
        {
            get => m_width;
            set
            {
                SetFieldValue(ref m_width, value);
                if (m_wpfFrameworkElement == null)
                    return;
                m_wpfFrameworkElement.Margin = Margin;
            }
        }

        public int Height
        {
            get => m_height;
            set
            {
                SetFieldValue(ref m_height, value);
                if (m_wpfFrameworkElement == null)
                    return;
                m_wpfFrameworkElement.Margin = Margin;
            }
        }

        public string Text
        {
            get => m_text;
            set
            {
                SetFieldValue(ref m_text, value);
                UpdateWPFText();
            }
        }

        public string Html
        {
            get => m_html;
            set
            {
                SetFieldValue(ref m_html, value);
                if (m_html == null || !(m_html != string.Empty))
                    return;
                m_browserControl = new WebBrowser();
                m_browserControl.ScrollBarsEnabled = false;
                m_browserControl.DocumentCompleted += BrowserDocumentCompleted;
                m_browserControl.Height = Height;
                m_browserControl.Width = Width;
                m_browserControl.DocumentText = m_html;
                MarkDirty();
            }
        }

        public string Name { get; set; }

        public IScene Scene { get; set; }

        public bool IsDirty { get; internal set; }

        public float Opacity
        {
            get => m_opacity;
            set
            {
                if (!SetFieldValue(ref m_opacity, value, false, false))
                    return;
                if (m_opacity < 0.0)
                    m_opacity = 0.0f;
                if (m_opacity >= 1.0)
                    m_opacity = 1f;
                NotifyScene(new Rectangle[1]
                {
                    new Rectangle(X, Y, Width, Height)
                });
            }
        }

        public int FrameCount { get; internal set; }

        public int? TabOrder { get; set; }

        public int FrameNumber
        {
            get => m_frameNumber;
            set
            {
                if (!SetFieldValue(ref m_frameNumber, value, false, false))
                    return;
                if (m_frameNumber >= FrameCount)
                    m_frameNumber = 0;
                if (m_frameNumber < 0)
                    m_frameNumber = 0;
                NotifyScene(new Rectangle[1]
                {
                    new Rectangle(X, Y, Width, Height)
                });
            }
        }

        public Image Image
        {
            get => m_image;
            set
            {
                if (!SetFieldValue(ref m_image, value, false, false))
                    return;
                if (m_image != null)
                {
                    if (Width == 0)
                        Width = m_image.Width;
                    if (Height == 0)
                        Height = m_image.Height;
                    m_frameDimension = new FrameDimension(m_image.FrameDimensionsList[0]);
                    FrameCount = m_image.GetFrameCount(m_frameDimension);
                }

                NotifyScene(new Rectangle[1]
                {
                    new Rectangle(X, Y, Width, Height)
                });
                AssignWPFImage(m_image);
            }
        }

        public bool Visible
        {
            get => m_visible;
            set
            {
                SetFieldValue(ref m_visible, value);
                if (m_wpfFrameworkElement == null)
                    return;
                m_wpfFrameworkElement.Visibility = value ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public bool Enabled
        {
            get => m_enabled;
            set => SetFieldValue(ref m_enabled, value, true, true);
        }

        public bool? Locked
        {
            get => m_locked;
            set => m_locked = !m_locked.HasValue ? value : throw new ArgumentException("Locked can only be set once.");
        }

        public object Tag { get; set; }

        public Rectangle Bounds => new Rectangle(X, Y, Width, Height);

        public string ErrorText
        {
            get => m_errorText;
            set => SetFieldValue(ref m_errorText, value);
        }

        public float[] TabStops { get; set; }

        public float StrokeWeight
        {
            get => m_strokeWeight;
            set => SetFieldValue(ref m_strokeWeight, value);
        }

        public string StyleName { get; set; }

        public Rectangle? HotSpot { get; set; }

        public Rectangle? TextRegion { get; set; }

        public HitTestFlags HitFlags { get; set; }

        public Size? CornerSize
        {
            get => m_cornerSize;
            set => SetFieldValue(ref m_cornerSize, value);
        }

        public Color? BorderColor
        {
            get => m_borderColor;
            set => SetFieldValue(ref m_borderColor, value);
        }

        public Color ForegroundColor
        {
            get => m_foregroundColor;
            set => SetFieldValue(ref m_foregroundColor, value);
        }

        public Color BackgroundColor
        {
            get => m_backgroundColor;
            set => SetFieldValue(ref m_backgroundColor, value);
        }

        public float? GradientAngle
        {
            get => m_gradientAngle;
            set => SetFieldValue(ref m_gradientAngle, value);
        }

        public float? CornerSweepAngle
        {
            get => m_cornerSweepAngle;
            set => SetFieldValue(ref m_cornerSweepAngle, value);
        }

        public float? TextRotationAngle
        {
            get => m_textRotationAngle;
            set => SetFieldValue(ref m_textRotationAngle, value);
        }

        public Color? GradientTargetColor
        {
            get => m_gradientTargetColor;
            set => SetFieldValue(ref m_gradientTargetColor, value);
        }

        public string RelativeToActorName { get; set; }

        public Point? TextTranslationPoint
        {
            get => m_textTranslationPoint;
            set => SetFieldValue(ref m_textTranslationPoint, value);
        }

        public RenderStateFlags StateFlags { get; set; }

        public RenderOptionFlags OptionFlags
        {
            get => m_optionFlags;
            set => SetFieldValue(ref m_optionFlags, value);
        }

        public StringAlignment VerticalAlignment
        {
            get => m_verticalAlignment;
            set => SetFieldValue(ref m_verticalAlignment, value);
        }

        public StringAlignment HorizontalAlignment
        {
            get => m_horizontalAlignment;
            set => SetFieldValue(ref m_horizontalAlignment, value);
        }

        public IStyleSheetStyle Style
        {
            get => m_style;
            set => SetFieldValue(ref m_style, value);
        }

        public event EventHandler Hit;

        public FrameworkElement WPFFrameworkElement
        {
            get
            {
                if (m_wpfFrameworkElement == null)
                    m_wpfFrameworkElement = CreateUIElement();
                return m_wpfFrameworkElement;
            }
            set => m_wpfFrameworkElement = value;
        }

        public string WPFControlName { get; set; }

        public string WPFControlAssemblyName { get; set; }

        protected FrameworkElement CreateUIElement()
        {
            var uiElement = string.IsNullOrEmpty(WPFControlName) ? CreateWPFActor() : CreateWPFControl();
            uiElement.Tag = Name;
            uiElement.Margin = Margin;
            uiElement.MouseDown += FrameworkElement_MouseDown;
            uiElement.MouseLeave += FrameworkElement_MouseLeave;
            uiElement.MouseUp += FrameworkElement_MouseUp;
            uiElement.IsHitTestVisible = WPFControlName != null ||
                                         (HitFlags & (HitTestFlags.Enabled | HitTestFlags.DrawHotSpot)) != 0;
            return uiElement;
        }

        private FrameworkElement CreateWPFActor()
        {
            var wpfActor = new Grid();
            Color? nullable;
            Size? cornerSize;
            if ((OptionFlags & RenderOptionFlags.Background) != RenderOptionFlags.None ||
                (OptionFlags & RenderOptionFlags.BackgroundGradient) != RenderOptionFlags.None)
            {
                var border1 = new Border();
                border1.Opacity = Opacity;
                var element = border1;
                var mediaColor1 = GetMediaColor(BackgroundColor, Opacity);
                if ((OptionFlags & RenderOptionFlags.BackgroundGradient) != RenderOptionFlags.None)
                {
                    nullable = GradientTargetColor;
                    Color backgroundColor;
                    if (!nullable.HasValue)
                    {
                        backgroundColor = BackgroundColor;
                    }
                    else
                    {
                        nullable = GradientTargetColor;
                        backgroundColor = nullable.Value;
                    }

                    var mediaColor2 = GetMediaColor(backgroundColor, Opacity);
                    element.Background =
                        new LinearGradientBrush(mediaColor1, mediaColor2, GradientAngle.GetValueOrDefault(180f));
                }
                else
                {
                    element.Background = new SolidColorBrush(mediaColor1);
                }

                cornerSize = CornerSize;
                if (cornerSize.HasValue)
                {
                    var border2 = element;
                    cornerSize = CornerSize;
                    var cornerRadius = new CornerRadius(cornerSize.Value.Width);
                    border2.CornerRadius = cornerRadius;
                }

                wpfActor.Children.Add(element);
            }

            if ((OptionFlags & RenderOptionFlags.Image) != RenderOptionFlags.None)
            {
                var image = new System.Windows.Controls.Image();
                image.Opacity = Opacity;
                m_wpfImage = image;
                AssignWPFImage(Image);
                m_wpfImage.Stretch = Stretch.Fill;
                wpfActor.Children.Add(m_wpfImage);
            }

            if ((OptionFlags & RenderOptionFlags.DrawBorder) != RenderOptionFlags.None)
            {
                var element = new Border();
                element.BorderThickness = new Thickness(StrokeWeight);
                var border3 = element;
                nullable = BorderColor;
                var solidColorBrush = new SolidColorBrush(GetMediaColor(nullable ?? Color.Red));
                border3.BorderBrush = solidColorBrush;
                cornerSize = CornerSize;
                if (cornerSize.HasValue)
                {
                    var border4 = element;
                    cornerSize = CornerSize;
                    var cornerRadius = new CornerRadius(cornerSize.Value.Width);
                    border4.CornerRadius = cornerRadius;
                }

                wpfActor.Children.Add(element);
            }

            if ((OptionFlags & RenderOptionFlags.Text) != RenderOptionFlags.None)
            {
                var textBlock = new TextBlock();
                textBlock.Opacity = Opacity;
                m_wpfTextBlock = textBlock;
                var fontFamily = (FontFamily)null;
                if (fontFamily == null)
                {
                    fontFamily = new FontFamily(Font.FontFamily.Name);
                    LogHelper.Instance.Log("Unable to find Font Family {0}.  Using default font instead.",
                        Font.FontFamily.Name);
                }

                m_wpfTextBlock.FontFamily = fontFamily;
                m_wpfTextBlock.FontSize = Font.Size * WPFSizeConvertion;
                m_wpfTextBlock.FontWeight = Font.Style != FontStyle.Bold
                    ? Font.Style != FontStyle.Regular ? FontWeights.Normal : FontWeights.Regular
                    : FontWeights.Bold;
                m_wpfTextBlock.FontStyle = Font.Style != FontStyle.Italic ? FontStyles.Normal : FontStyles.Italic;
                m_wpfTextBlock.HorizontalAlignment = WPFHorizontalAlignmnet;
                m_wpfTextBlock.VerticalAlignment = WPFVerticalAlignmnet;
                var mediaColor = GetMediaColor(ForegroundColor);
                if (mediaColor.A == 0 && mediaColor.R == 0 && mediaColor.B == 0 && mediaColor.G == 0)
                    mediaColor.A = byte.MaxValue;
                m_wpfTextBlock.Foreground = new SolidColorBrush(mediaColor);
                UpdateWPFText();
                LogHelper.Instance.Log("Font for Actor {0}: Family: {1}, Size: {2}, Text: {3}", Name,
                    fontFamily.FamilyNames[XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.Name)],
                    m_wpfTextBlock.FontSize, m_wpfTextBlock.Text);
                wpfActor.Children.Add(m_wpfTextBlock);
            }

            return wpfActor;
        }

        private FrameworkElement CreateWPFControl()
        {
            var wpfControl = ServiceLocator.Instance.GetService<IThemeService>()
                ?.GetNewThemedControlInstance(WPFControlName);
            if (wpfControl == null)
            {
                if (!string.IsNullOrEmpty(WPFControlName) && !string.IsNullOrEmpty(WPFControlAssemblyName))
                {
                    var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        WPFControlAssemblyName);
                    if (File.Exists(path))
                    {
                        var assembly = Assembly.LoadFile(path);
                        if (assembly != null)
                        {
                            var type = assembly.GetType(WPFControlName);
                            if (type != null)
                            {
                                var instance = Activator.CreateInstance(type);
                                if (instance != null)
                                {
                                    if (instance is UserControl userControl)
                                        wpfControl = userControl;
                                    else
                                        LogHelper.Instance.Log("Type {0} is not a UserControl", WPFControlName);
                                }
                                else
                                {
                                    LogHelper.Instance.Log("Unable to create instance of type {0}", WPFControlName);
                                }
                            }
                            else
                            {
                                LogHelper.Instance.Log("Unable to get type {1} in assembly {0}", path, WPFControlName);
                            }
                        }
                        else
                        {
                            LogHelper.Instance.Log("Unable to load assembly {0}", path);
                        }
                    }
                    else
                    {
                        LogHelper.Instance.Log("Unable to find assembly {0}", path);
                    }
                }
                else
                {
                    LogHelper.Instance.Log("WPFControlName and WPFControlAssemblyName must have values");
                }
            }

            if (wpfControl is IWPFActor wpfActor)
            {
                wpfActor.Actor = this;
                wpfActor.OnWPFHit += actor =>
                {
                    var onWpfHit = OnWPFHit;
                    if (onWpfHit == null)
                        return;
                    onWpfHit(this);
                };
            }

            return wpfControl;
        }

        private void AssignWPFImage(Image image)
        {
            if (m_wpfImage == null || image == null)
                return;
            m_wpfImage.Source = ToBitmapImage(new Bitmap(Image));
        }

        private System.Windows.Media.Color GetMediaColor(Color color)
        {
            return new System.Windows.Media.Color
            {
                R = color.R,
                G = color.G,
                B = color.B,
                A = color.A
            };
        }

        private System.Windows.Media.Color GetMediaColor(Color color, double opacity)
        {
            var num = (int)Math.Ceiling(byte.MaxValue * opacity);
            return new System.Windows.Media.Color
            {
                R = color.R,
                G = color.G,
                B = color.B,
                A = (byte)num
            };
        }

        private void UpdateWPFText()
        {
            if (m_wpfTextBlock == null)
                return;
            m_wpfTextBlock.Text = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(Text);
        }

        private void FrameworkElement_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!m_clickStarted)
                return;
            if (Enabled)
            {
                RaiseHit();
                if (OnWPFHit != null)
                    OnWPFHit(this);
            }

            m_clickStarted = false;
        }

        private void FrameworkElement_MouseLeave(object sender, MouseEventArgs e)
        {
            m_clickStarted = false;
        }

        private void FrameworkElement_MouseDown(object sender, MouseButtonEventArgs e)
        {
            m_clickStarted = true;
        }

        private double ToWPFScale(double value)
        {
            return value * WPFSizeConvertion;
        }

        private BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Png);
                memoryStream.Position = 0L;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        protected void NotifyScene(Rectangle[] rectangles)
        {
            if (Scene == null)
                return;
            IsDirty = Visible;
            Scene.MakeDirty(rectangles);
        }

        protected virtual void OnRender(Graphics context)
        {
            if (!Visible)
            {
                IsDirty = false;
            }
            else
            {
                context.ResetTransform();
                if ((OptionFlags & RenderOptionFlags.DisableAntiAlias) == RenderOptionFlags.None)
                    context.SmoothingMode = SmoothingMode.AntiAlias;
                var hasValue1 = TextRotationAngle.HasValue;
                var hasValue2 = TextTranslationPoint.HasValue;
                if ((OptionFlags & RenderOptionFlags.Background) != RenderOptionFlags.None ||
                    (OptionFlags & RenderOptionFlags.BackgroundGradient) != RenderOptionFlags.None)
                {
                    var alphaRampedColor1 = GetAlphaRampedColor(BackgroundColor, Opacity);
                    var cornerSize = CornerSize;
                    if (cornerSize.HasValue)
                    {
                        using (var pen = new Pen(alphaRampedColor1, StrokeWeight))
                        {
                            var g = context;
                            var p = pen;
                            var x = (double)X;
                            var y = (double)Y;
                            var width1 = (double)Width;
                            var height = (double)Height;
                            cornerSize = CornerSize;
                            var width2 = (double)cornerSize.Value.Width;
                            DrawRoundedRect(g, p, (float)x, (float)y, (float)width1, (float)height, (float)width2,
                                true);
                        }
                    }
                    else
                    {
                        var brush = (Brush)null;
                        try
                        {
                            if ((OptionFlags & RenderOptionFlags.BackgroundGradient) != RenderOptionFlags.None)
                            {
                                var alphaRampedColor2 =
                                    GetAlphaRampedColor(GradientTargetColor ?? BackgroundColor, Opacity);
                                brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                                    new Rectangle(X, Y, Width, Height), alphaRampedColor1, alphaRampedColor2,
                                    GradientAngle.GetValueOrDefault(180f));
                            }
                            else
                            {
                                brush = new SolidBrush(alphaRampedColor1);
                            }

                            context.FillRectangle(brush, X, Y, Width, Height);
                        }
                        finally
                        {
                            brush?.Dispose();
                        }
                    }
                }

                if ((OptionFlags & RenderOptionFlags.Image) != RenderOptionFlags.None && Image != null)
                {
                    var imageAttr = new ImageAttributes();
                    if (FrameCount > 1)
                        Image.SelectActiveFrame(m_frameDimension, FrameNumber);
                    else
                        imageAttr.SetColorMatrix(new ColorMatrix
                        {
                            Matrix33 = Opacity
                        });
                    if ((OptionFlags & RenderOptionFlags.Tile) != RenderOptionFlags.None)
                        imageAttr.SetWrapMode(WrapMode.Tile);
                    context.DrawImage(Image, new Rectangle(X, Y, Width, Height), 0, 0, Image.Width, Image.Height,
                        GraphicsUnit.Pixel, imageAttr);
                }

                if ((OptionFlags & RenderOptionFlags.DrawBorder) != RenderOptionFlags.None)
                    using (var pen = new Pen(BorderColor ?? Color.Red, StrokeWeight))
                    {
                        if (CornerSize.HasValue)
                            DrawRoundedRect(context, pen, X, Y, Width, Height, CornerSize.Value.Width, false);
                        else
                            context.DrawRectangle(pen, new Rectangle(X, Y, Width, Height));
                    }

                if ((OptionFlags & RenderOptionFlags.Text) != RenderOptionFlags.None && !string.IsNullOrEmpty(Text))
                    using (var solidBrush1 = new SolidBrush(GetAlphaRampedColor(ForegroundColor, Opacity)))
                    {
                        StringFormat format;
                        if ((OptionFlags & RenderOptionFlags.ElideText) != RenderOptionFlags.None)
                        {
                            format = (StringFormat)StringFormat.GenericDefault.Clone();
                            format.Trimming |= StringTrimming.EllipsisCharacter;
                        }
                        else
                        {
                            format = (StringFormat)StringFormat.GenericTypographic.Clone();
                        }

                        format.Alignment = HorizontalAlignment;
                        format.LineAlignment = VerticalAlignment;
                        if ((OptionFlags & RenderOptionFlags.DisableAntiAlias) == RenderOptionFlags.None)
                        {
                            context.PixelOffsetMode = PixelOffsetMode.Half;
                            context.TextRenderingHint = TextRenderingHint.AntiAlias;
                        }

                        if (TabStops != null && TabStops.Length != 0)
                            format.SetTabStops(0.0f, TabStops);
                        var textRegion = GetTextRegion(hasValue2);
                        var s = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(Text);
                        if (hasValue2)
                            context.TranslateTransform(TextTranslationPoint.Value.X, TextTranslationPoint.Value.Y);
                        if (hasValue1)
                            context.RotateTransform(TextRotationAngle.Value);
                        if ((OptionFlags & RenderOptionFlags.TextShadow) != RenderOptionFlags.None)
                            using (var solidBrush2 = new SolidBrush(GetAlphaRampedColor(Color.Black, Opacity)))
                            {
                                context.DrawString(s, Font, solidBrush2,
                                    new Rectangle(textRegion.Left + 1, textRegion.Top + 1, textRegion.Width,
                                        textRegion.Height), format);
                            }

                        context.DrawString(s, Font, solidBrush1, textRegion, format);
                        if (hasValue2 | hasValue1)
                            context.ResetTransform();
                    }

                if ((OptionFlags & RenderOptionFlags.HtmlToImage) != RenderOptionFlags.None && Image != null)
                {
                    var imageAttr = new ImageAttributes();
                    imageAttr.SetColorMatrix(new ColorMatrix
                    {
                        Matrix33 = Opacity
                    });
                    context.DrawImage(Image, new Rectangle(X, Y, Width, Height), 0, 0, Image.Width, Image.Height,
                        GraphicsUnit.Pixel, imageAttr);
                }

                if ((HitFlags & HitTestFlags.DrawHotSpot) != HitTestFlags.None)
                    context.DrawRectangle(Pens.HotPink, GetHitTestRectangle());
                if (!string.IsNullOrEmpty(ErrorText))
                {
                    using (var font = new Font("Tahoma", 7f, FontStyle.Bold))
                    {
                        var y = Y + Height + 2;
                        if (m_drawErrorTop)
                            y = Y - font.Height - 2;
                        context.DrawString(ErrorText, font, Brushes.Red, X, y);
                    }

                    m_drawErrorTop = !m_drawErrorTop;
                }

                StateFlags |= RenderStateFlags.Drawn;
                IsDirty = false;
            }
        }

        internal Rectangle GetTextRegion(bool forTranslation)
        {
            var x = forTranslation ? 0 : X;
            var y = forTranslation ? 0 : Y;
            var width = Width;
            var height = Height;
            var textRegion = TextRegion;
            if (textRegion.HasValue)
            {
                if (!forTranslation)
                {
                    var num1 = x;
                    textRegion = TextRegion;
                    var left = textRegion.Value.Left;
                    x = num1 + left;
                    var num2 = y;
                    textRegion = TextRegion;
                    var top = textRegion.Value.Top;
                    y = num2 + top;
                }

                textRegion = TextRegion;
                var rectangle = textRegion.Value;
                width = rectangle.Width;
                textRegion = TextRegion;
                rectangle = textRegion.Value;
                height = rectangle.Height;
            }

            return new Rectangle(x, y, width, height);
        }

        internal Rectangle GetHitTestRectangle()
        {
            var x = X;
            var y = Y;
            var width = Width;
            var height = Height;
            if (HotSpot.HasValue)
            {
                x += HotSpot.Value.Left;
                var num = y;
                var rectangle = HotSpot.Value;
                var top = rectangle.Top;
                y = num + top;
                rectangle = HotSpot.Value;
                width = rectangle.Width;
                rectangle = HotSpot.Value;
                height = rectangle.Height;
            }

            return new Rectangle(x, y, width, height);
        }

        private void BrowserDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var bitmap = new Bitmap(Width, Height);
            m_browserControl.DrawToBitmap(bitmap, new Rectangle(0, 0, Width, Height));
            Image = bitmap;
        }

        private bool SetFieldValue<T>(ref T field, T value)
        {
            return SetFieldValue(ref field, value, true, false);
        }

        private bool SetFieldValue<T>(ref T field, T value, bool notifyScene, bool processStyle)
        {
            if ((field == null && value != null) || (field != null && !field.Equals(value)))
            {
                field = value;
                if (processStyle && Style != null)
                {
                    var state = Style.GetState(Enabled ? "enabled" : "disabled");
                    if (state != null)
                        foreach (var propertyInfo in typeof(Actor).GetProperties())
                        {
                            var text = propertyInfo.Name;
                            text = text.Substring(0, 1).ToLower() + text.Substring(1);
                            var obj = state[text];
                            if (obj != null)
                                try
                                {
                                    if (text != "image")
                                    {
                                        var obj2 = ConversionHelper.ChangeType(obj, propertyInfo.PropertyType);
                                        propertyInfo.SetValue(this, obj2, null);
                                    }
                                    else
                                    {
                                        var text2 = obj as string;
                                        if (text2 != null && text2.StartsWith("cache:"))
                                        {
                                            var service = ServiceLocator.Instance.GetService<IBitmapCacheService>();
                                            Image = service.GetImage(text2.Substring(6));
                                        }
                                        else if (text2 != null)
                                        {
                                            var service2 = ServiceLocator.Instance.GetService<IResourceBundleService>();
                                            Image = service2.GetBitmap(text2);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.Instance.Log(
                                        string.Format("Unable to set actor property '{0}' with value '{1}'.",
                                            propertyInfo.Name, obj), ex);
                                }
                        }
                }

                if (notifyScene && (Width > 0 || Height > 0))
                    NotifyScene(new[]
                    {
                        new Rectangle(X, Y, Width, Height)
                    });
                return true;
            }

            return false;
        }

        private static Color GetAlphaRampedColor(Color color, float opacity)
        {
            return Color.FromArgb((int)Math.Ceiling(byte.MaxValue * (double)opacity), color);
        }

        private void DrawRoundedRect(
            Graphics g,
            Pen p,
            float x,
            float y,
            float width,
            float height,
            float radius,
            bool fill)
        {
            using (var path = new GraphicsPath())
            {
                if (radius * 2.0 > height || radius * 2.0 > width)
                    radius = height > (double)width ? (int)(width / 2.0) : (float)(int)(height / 2.0);
                var num = radius * 2f;
                var valueOrDefault = CornerSweepAngle.GetValueOrDefault(90f);
                path.AddArc(x + width - num, y, num, num, 270f, valueOrDefault);
                if (y + (double)height - radius < y + (double)radius)
                    path.AddLine(x + width, y + radius, x + width, y + height - radius);
                path.AddArc(x + width - num, y + height - num, num, num, 0.0f, valueOrDefault);
                if (x + (double)width - radius < x + (double)radius)
                    path.AddLine(x + width - radius, y + height, x + radius, y + height);
                path.AddArc(x, y + height - num, num, num, 90f, valueOrDefault);
                if (y + (double)height - radius < y + (double)radius)
                    path.AddLine(x, y + height - radius, x, y + radius);
                path.AddArc(x, y, num, num, 180f, valueOrDefault);
                if (x + (double)width - radius > x + (double)radius)
                    path.AddLine(x + radius, y, x + width - radius, y);
                path.CloseFigure();
                if (fill)
                {
                    var brush = (Brush)null;
                    try
                    {
                        if ((OptionFlags & RenderOptionFlags.BackgroundGradient) != RenderOptionFlags.None)
                        {
                            var alphaRampedColor = GetAlphaRampedColor(GradientTargetColor ?? BackgroundColor, Opacity);
                            brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                                new Rectangle((int)x, (int)y, (int)width, (int)height), p.Color, alphaRampedColor,
                                GradientAngle.GetValueOrDefault(180f));
                        }
                        else
                        {
                            brush = new SolidBrush(p.Color);
                        }

                        g.FillPath(brush, path);
                    }
                    finally
                    {
                        brush?.Dispose();
                    }
                }
                else
                {
                    g.DrawPath(p, path);
                }
            }
        }
    }
}