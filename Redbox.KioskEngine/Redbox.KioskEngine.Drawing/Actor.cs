using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
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

namespace Redbox.KioskEngine.Drawing
{
  public class Actor : IActor, IDisposable
  {
    private Form _mainForm;
    private bool m_clickStarted;
    private double WPFSizeConvertion = 1.3333333730697632;
    private int m_x;
    private int m_y;
    private Font m_font;
    private int m_width;
    private int m_height;
    private string m_text;
    private string m_html;
    private System.Drawing.Image m_image;
    private bool m_enabled;
    private bool m_visible;
    private bool? m_locked;
    private float m_opacity;
    private int m_frameNumber;
    private string m_errorText;
    private System.Drawing.Size? m_cornerSize;
    private System.Drawing.Color? m_borderColor;
    private float m_strokeWeight;
    private float? m_gradientAngle;
    private System.Drawing.Color m_foregroundColor;
    private System.Drawing.Color m_backgroundColor;
    private IStyleSheetStyle m_style;
    private float? m_cornerSweepAngle;
    private static bool m_drawErrorTop;
    private float? m_textRotationAngle;
    private System.Windows.Forms.WebBrowser m_browserControl;
    private System.Drawing.Color? m_gradientTargetColor;
    private System.Drawing.Point? m_textTranslationPoint;
    private RenderOptionFlags m_optionFlags;
    private FrameDimension m_frameDimension;
    private StringAlignment m_verticalAlignment;
    private StringAlignment m_horizontalAlignment;
    private FrameworkElement m_wpfFrameworkElement;
    private TextBlock m_wpfTextBlock;
    private System.Windows.Controls.Image m_wpfImage;

    public Actor()
    {
      this.Opacity = 1f;
      this.StrokeWeight = 1f;
      this.Enabled = true;
      this.Visible = true;
      this.IsDirty = true;
      this.BackgroundColor = System.Drawing.Color.Black;
      this.ForegroundColor = System.Drawing.Color.White;
      this.OptionFlags = RenderOptionFlags.Text | RenderOptionFlags.Image;
    }

    public event WPFHitHandler OnWPFHit;

    public void RaiseHit()
    {
      if (this.Hit == null)
        return;
      this.Hit((object) this, EventArgs.Empty);
    }

    public bool HitExists => this.Hit != null;

    public void ClearHit() => this.Hit = (EventHandler) null;

    public void MarkDirty()
    {
      this.NotifyScene(new Rectangle[1]
      {
        new Rectangle(this.X, this.Y, this.Width, this.Height)
      });
    }

    public virtual void Dispose() => this.ClearHit();

    public IntPtr MainHandle
    {
      get
      {
        if (this._mainForm == null)
          this._mainForm = (ServiceLocator.Instance.GetService<IEngineApplication>() as ApplicationContext).MainForm;
        return this._mainForm.Handle;
      }
    }

    public MeasureTextResult MeasureText()
    {
      IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
      using (Graphics graphics = Graphics.FromHwnd(this.MainHandle))
      {
        graphics.PixelOffsetMode = PixelOffsetMode.Half;
        graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
        Rectangle textRegion = this.GetTextRegion(false);
        StringFormat stringFormat = (StringFormat) StringFormat.GenericTypographic.Clone();
        int charactersFitted;
        int linesFilled;
        return new MeasureTextResult(graphics.MeasureString(service.ExpandProperties(this.Text), this.Font, new SizeF((float) textRegion.Width, (float) textRegion.Height), stringFormat, out charactersFitted, out linesFilled), charactersFitted, linesFilled);
      }
    }

    public void CenterHorizontally()
    {
      if (this.Scene == null)
        return;
      this.X = (this.Scene.Width - this.Width) / 2;
    }

    public void CenterVertically()
    {
      if (this.Scene == null)
        return;
      this.Y = (this.Scene.Height - this.Height) / 2;
    }

    public void Render(Graphics context) => this.OnRender(context);

    public int X
    {
      get => this.m_x;
      set
      {
        if (this.m_x == value)
          return;
        Rectangle rectangle = new Rectangle(this.m_x, this.Y, this.Width, this.Height);
        this.m_x = value;
        this.NotifyScene(new Rectangle[2]
        {
          rectangle,
          new Rectangle(this.m_x, this.Y, this.Width, this.Height)
        });
        if (this.m_wpfFrameworkElement == null)
          return;
        this.m_wpfFrameworkElement.Margin = this.Margin;
      }
    }

    public int Y
    {
      get => this.m_y;
      set
      {
        if (this.m_y == value)
          return;
        Rectangle rectangle = new Rectangle(this.X, this.m_y, this.Width, this.Height);
        this.m_y = value;
        this.NotifyScene(new Rectangle[2]
        {
          rectangle,
          new Rectangle(this.X, this.m_y, this.Width, this.Height)
        });
        if (this.m_wpfFrameworkElement == null)
          return;
        this.m_wpfFrameworkElement.Margin = this.Margin;
      }
    }

    public Font Font
    {
      get
      {
        if (this.m_font == null)
        {
          IFontCacheService service = ServiceLocator.Instance.GetService<IFontCacheService>();
          if (service != null)
            this.m_font = service.RegisterFont("DefaultActorFont", "Arial", 16f, System.Drawing.FontStyle.Regular);
        }
        return this.m_font;
      }
      set => this.SetFieldValue<Font>(ref this.m_font, value);
    }

    public int Width
    {
      get => this.m_width;
      set
      {
        this.SetFieldValue<int>(ref this.m_width, value);
        if (this.m_wpfFrameworkElement == null)
          return;
        this.m_wpfFrameworkElement.Margin = this.Margin;
      }
    }

    public int Height
    {
      get => this.m_height;
      set
      {
        this.SetFieldValue<int>(ref this.m_height, value);
        if (this.m_wpfFrameworkElement == null)
          return;
        this.m_wpfFrameworkElement.Margin = this.Margin;
      }
    }

    public string Text
    {
      get => this.m_text;
      set
      {
        this.SetFieldValue<string>(ref this.m_text, value);
        this.UpdateWPFText();
      }
    }

    public string Html
    {
      get => this.m_html;
      set
      {
        this.SetFieldValue<string>(ref this.m_html, value);
        if (this.m_html == null || !(this.m_html != string.Empty))
          return;
        this.m_browserControl = new System.Windows.Forms.WebBrowser();
        this.m_browserControl.ScrollBarsEnabled = false;
        this.m_browserControl.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(this.BrowserDocumentCompleted);
        this.m_browserControl.Height = this.Height;
        this.m_browserControl.Width = this.Width;
        this.m_browserControl.DocumentText = this.m_html;
        this.MarkDirty();
      }
    }

    public string Name { get; set; }

    public IScene Scene { get; set; }

    public bool IsDirty { get; internal set; }

    public float Opacity
    {
      get => this.m_opacity;
      set
      {
        if (!this.SetFieldValue<float>(ref this.m_opacity, value, false, false))
          return;
        if ((double) this.m_opacity < 0.0)
          this.m_opacity = 0.0f;
        if ((double) this.m_opacity >= 1.0)
          this.m_opacity = 1f;
        this.NotifyScene(new Rectangle[1]
        {
          new Rectangle(this.X, this.Y, this.Width, this.Height)
        });
      }
    }

    public int FrameCount { get; internal set; }

    public int? TabOrder { get; set; }

    public int FrameNumber
    {
      get => this.m_frameNumber;
      set
      {
        if (!this.SetFieldValue<int>(ref this.m_frameNumber, value, false, false))
          return;
        if (this.m_frameNumber >= this.FrameCount)
          this.m_frameNumber = 0;
        if (this.m_frameNumber < 0)
          this.m_frameNumber = 0;
        this.NotifyScene(new Rectangle[1]
        {
          new Rectangle(this.X, this.Y, this.Width, this.Height)
        });
      }
    }

    public System.Drawing.Image Image
    {
      get => this.m_image;
      set
      {
        if (!this.SetFieldValue<System.Drawing.Image>(ref this.m_image, value, false, false))
          return;
        if (this.m_image != null)
        {
          if (this.Width == 0)
            this.Width = this.m_image.Width;
          if (this.Height == 0)
            this.Height = this.m_image.Height;
          this.m_frameDimension = new FrameDimension(this.m_image.FrameDimensionsList[0]);
          this.FrameCount = this.m_image.GetFrameCount(this.m_frameDimension);
        }
        this.NotifyScene(new Rectangle[1]
        {
          new Rectangle(this.X, this.Y, this.Width, this.Height)
        });
        this.AssignWPFImage(this.m_image);
      }
    }

    public bool Visible
    {
      get => this.m_visible;
      set
      {
        this.SetFieldValue<bool>(ref this.m_visible, value);
        if (this.m_wpfFrameworkElement == null)
          return;
        this.m_wpfFrameworkElement.Visibility = value ? Visibility.Visible : Visibility.Hidden;
      }
    }

    public bool Enabled
    {
      get => this.m_enabled;
      set => this.SetFieldValue<bool>(ref this.m_enabled, value, true, true);
    }

    public bool? Locked
    {
      get => this.m_locked;
      set
      {
        this.m_locked = !this.m_locked.HasValue ? value : throw new ArgumentException("Locked can only be set once.");
      }
    }

    public object Tag { get; set; }

    public Rectangle Bounds => new Rectangle(this.X, this.Y, this.Width, this.Height);

    public string ErrorText
    {
      get => this.m_errorText;
      set => this.SetFieldValue<string>(ref this.m_errorText, value);
    }

    public float[] TabStops { get; set; }

    public float StrokeWeight
    {
      get => this.m_strokeWeight;
      set => this.SetFieldValue<float>(ref this.m_strokeWeight, value);
    }

    public string StyleName { get; set; }

    public Rectangle? HotSpot { get; set; }

    public Rectangle? TextRegion { get; set; }

    public HitTestFlags HitFlags { get; set; }

    public System.Drawing.Size? CornerSize
    {
      get => this.m_cornerSize;
      set => this.SetFieldValue<System.Drawing.Size?>(ref this.m_cornerSize, value);
    }

    public System.Drawing.Color? BorderColor
    {
      get => this.m_borderColor;
      set => this.SetFieldValue<System.Drawing.Color?>(ref this.m_borderColor, value);
    }

    public System.Drawing.Color ForegroundColor
    {
      get => this.m_foregroundColor;
      set => this.SetFieldValue<System.Drawing.Color>(ref this.m_foregroundColor, value);
    }

    public System.Drawing.Color BackgroundColor
    {
      get => this.m_backgroundColor;
      set => this.SetFieldValue<System.Drawing.Color>(ref this.m_backgroundColor, value);
    }

    public float? GradientAngle
    {
      get => this.m_gradientAngle;
      set => this.SetFieldValue<float?>(ref this.m_gradientAngle, value);
    }

    public float? CornerSweepAngle
    {
      get => this.m_cornerSweepAngle;
      set => this.SetFieldValue<float?>(ref this.m_cornerSweepAngle, value);
    }

    public float? TextRotationAngle
    {
      get => this.m_textRotationAngle;
      set => this.SetFieldValue<float?>(ref this.m_textRotationAngle, value);
    }

    public System.Drawing.Color? GradientTargetColor
    {
      get => this.m_gradientTargetColor;
      set => this.SetFieldValue<System.Drawing.Color?>(ref this.m_gradientTargetColor, value);
    }

    public string RelativeToActorName { get; set; }

    public System.Drawing.Point? TextTranslationPoint
    {
      get => this.m_textTranslationPoint;
      set => this.SetFieldValue<System.Drawing.Point?>(ref this.m_textTranslationPoint, value);
    }

    public RenderStateFlags StateFlags { get; set; }

    public RenderOptionFlags OptionFlags
    {
      get => this.m_optionFlags;
      set => this.SetFieldValue<RenderOptionFlags>(ref this.m_optionFlags, value);
    }

    public StringAlignment VerticalAlignment
    {
      get => this.m_verticalAlignment;
      set => this.SetFieldValue<StringAlignment>(ref this.m_verticalAlignment, value);
    }

    public StringAlignment HorizontalAlignment
    {
      get => this.m_horizontalAlignment;
      set => this.SetFieldValue<StringAlignment>(ref this.m_horizontalAlignment, value);
    }

    public IStyleSheetStyle Style
    {
      get => this.m_style;
      set => this.SetFieldValue<IStyleSheetStyle>(ref this.m_style, value);
    }

    public event EventHandler Hit;

    public FrameworkElement WPFFrameworkElement
    {
      get
      {
        if (this.m_wpfFrameworkElement == null)
          this.m_wpfFrameworkElement = this.CreateUIElement();
        return this.m_wpfFrameworkElement;
      }
      set => this.m_wpfFrameworkElement = value;
    }

    public string WPFControlName { get; set; }

    public string WPFControlAssemblyName { get; set; }

    protected FrameworkElement CreateUIElement()
    {
      FrameworkElement uiElement = string.IsNullOrEmpty(this.WPFControlName) ? this.CreateWPFActor() : this.CreateWPFControl();
      uiElement.Tag = (object) this.Name;
      uiElement.Margin = this.Margin;
      uiElement.MouseDown += new MouseButtonEventHandler(this.FrameworkElement_MouseDown);
      uiElement.MouseLeave += new System.Windows.Input.MouseEventHandler(this.FrameworkElement_MouseLeave);
      uiElement.MouseUp += new MouseButtonEventHandler(this.FrameworkElement_MouseUp);
      uiElement.IsHitTestVisible = this.WPFControlName != null || (this.HitFlags & (HitTestFlags.Enabled | HitTestFlags.DrawHotSpot)) != 0;
      return uiElement;
    }

    private FrameworkElement CreateWPFActor()
    {
      Grid wpfActor = new Grid();
      System.Drawing.Color? nullable;
      System.Drawing.Size? cornerSize;
      if ((this.OptionFlags & RenderOptionFlags.Background) != RenderOptionFlags.None || (this.OptionFlags & RenderOptionFlags.BackgroundGradient) != RenderOptionFlags.None)
      {
        Border border1 = new Border();
        border1.Opacity = (double) this.Opacity;
        Border element = border1;
        System.Windows.Media.Color mediaColor1 = this.GetMediaColor(this.BackgroundColor, (double) this.Opacity);
        if ((this.OptionFlags & RenderOptionFlags.BackgroundGradient) != RenderOptionFlags.None)
        {
          nullable = this.GradientTargetColor;
          System.Drawing.Color backgroundColor;
          if (!nullable.HasValue)
          {
            backgroundColor = this.BackgroundColor;
          }
          else
          {
            nullable = this.GradientTargetColor;
            backgroundColor = nullable.Value;
          }
          System.Windows.Media.Color mediaColor2 = this.GetMediaColor(backgroundColor, (double) this.Opacity);
          element.Background = (System.Windows.Media.Brush) new System.Windows.Media.LinearGradientBrush(mediaColor1, mediaColor2, (double) this.GradientAngle.GetValueOrDefault(180f));
        }
        else
          element.Background = (System.Windows.Media.Brush) new SolidColorBrush(mediaColor1);
        cornerSize = this.CornerSize;
        if (cornerSize.HasValue)
        {
          Border border2 = element;
          cornerSize = this.CornerSize;
          CornerRadius cornerRadius = new CornerRadius((double) cornerSize.Value.Width);
          border2.CornerRadius = cornerRadius;
        }
        wpfActor.Children.Add((UIElement) element);
      }
      if ((this.OptionFlags & RenderOptionFlags.Image) != RenderOptionFlags.None)
      {
        System.Windows.Controls.Image image = new System.Windows.Controls.Image();
        image.Opacity = (double) this.Opacity;
        this.m_wpfImage = image;
        this.AssignWPFImage(this.Image);
        this.m_wpfImage.Stretch = Stretch.Fill;
        wpfActor.Children.Add((UIElement) this.m_wpfImage);
      }
      if ((this.OptionFlags & RenderOptionFlags.DrawBorder) != RenderOptionFlags.None)
      {
        Border element = new Border();
        element.BorderThickness = new Thickness((double) this.StrokeWeight);
        Border border3 = element;
        nullable = this.BorderColor;
        SolidColorBrush solidColorBrush = new SolidColorBrush(this.GetMediaColor(nullable ?? System.Drawing.Color.Red));
        border3.BorderBrush = (System.Windows.Media.Brush) solidColorBrush;
        cornerSize = this.CornerSize;
        if (cornerSize.HasValue)
        {
          Border border4 = element;
          cornerSize = this.CornerSize;
          CornerRadius cornerRadius = new CornerRadius((double) cornerSize.Value.Width);
          border4.CornerRadius = cornerRadius;
        }
        wpfActor.Children.Add((UIElement) element);
      }
      if ((this.OptionFlags & RenderOptionFlags.Text) != RenderOptionFlags.None)
      {
        TextBlock textBlock = new TextBlock();
        textBlock.Opacity = (double) this.Opacity;
        this.m_wpfTextBlock = textBlock;
        System.Windows.Media.FontFamily fontFamily = (System.Windows.Media.FontFamily) null;
        if (fontFamily == null)
        {
          fontFamily = new System.Windows.Media.FontFamily(this.Font.FontFamily.Name);
          LogHelper.Instance.Log("Unable to find Font Family {0}.  Using default font instead.", (object) this.Font.FontFamily.Name);
        }
        this.m_wpfTextBlock.FontFamily = fontFamily;
        this.m_wpfTextBlock.FontSize = (double) this.Font.Size * this.WPFSizeConvertion;
        this.m_wpfTextBlock.FontWeight = this.Font.Style != System.Drawing.FontStyle.Bold ? (this.Font.Style != System.Drawing.FontStyle.Regular ? FontWeights.Normal : FontWeights.Regular) : FontWeights.Bold;
        this.m_wpfTextBlock.FontStyle = this.Font.Style != System.Drawing.FontStyle.Italic ? FontStyles.Normal : FontStyles.Italic;
        this.m_wpfTextBlock.HorizontalAlignment = this.WPFHorizontalAlignmnet;
        this.m_wpfTextBlock.VerticalAlignment = this.WPFVerticalAlignmnet;
        System.Windows.Media.Color mediaColor = this.GetMediaColor(this.ForegroundColor);
        if (mediaColor.A == (byte) 0 && mediaColor.R == (byte) 0 && mediaColor.B == (byte) 0 && mediaColor.G == (byte) 0)
          mediaColor.A = byte.MaxValue;
        this.m_wpfTextBlock.Foreground = (System.Windows.Media.Brush) new SolidColorBrush(mediaColor);
        this.UpdateWPFText();
        LogHelper.Instance.Log("Font for Actor {0}: Family: {1}, Size: {2}, Text: {3}", (object) this.Name, (object) fontFamily.FamilyNames[XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.Name)], (object) this.m_wpfTextBlock.FontSize, (object) this.m_wpfTextBlock.Text);
        wpfActor.Children.Add((UIElement) this.m_wpfTextBlock);
      }
      return (FrameworkElement) wpfActor;
    }

    private FrameworkElement CreateWPFControl()
    {
      FrameworkElement wpfControl = ServiceLocator.Instance.GetService<IThemeService>()?.GetNewThemedControlInstance(this.WPFControlName);
      if (wpfControl == null)
      {
        if (!string.IsNullOrEmpty(this.WPFControlName) && !string.IsNullOrEmpty(this.WPFControlAssemblyName))
        {
          string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), this.WPFControlAssemblyName);
          if (File.Exists(path))
          {
            Assembly assembly = Assembly.LoadFile(path);
            if (assembly != (Assembly) null)
            {
              Type type = assembly.GetType(this.WPFControlName);
              if (type != (Type) null)
              {
                object instance = Activator.CreateInstance(type);
                if (instance != null)
                {
                  if (instance is System.Windows.Controls.UserControl userControl)
                    wpfControl = (FrameworkElement) userControl;
                  else
                    LogHelper.Instance.Log("Type {0} is not a UserControl", (object) this.WPFControlName);
                }
                else
                  LogHelper.Instance.Log("Unable to create instance of type {0}", (object) this.WPFControlName);
              }
              else
                LogHelper.Instance.Log("Unable to get type {1} in assembly {0}", (object) path, (object) this.WPFControlName);
            }
            else
              LogHelper.Instance.Log("Unable to load assembly {0}", (object) path);
          }
          else
            LogHelper.Instance.Log("Unable to find assembly {0}", (object) path);
        }
        else
          LogHelper.Instance.Log("WPFControlName and WPFControlAssemblyName must have values");
      }
      if (wpfControl is IWPFActor wpfActor)
      {
        wpfActor.Actor = (IActor) this;
        wpfActor.OnWPFHit += (WPFHitHandler) (actor =>
        {
          WPFHitHandler onWpfHit = this.OnWPFHit;
          if (onWpfHit == null)
            return;
          onWpfHit((IActor) this);
        });
      }
      return wpfControl;
    }

    private System.Windows.HorizontalAlignment WPFHorizontalAlignmnet
    {
      get
      {
        switch (this.HorizontalAlignment)
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

    private System.Windows.VerticalAlignment WPFVerticalAlignmnet
    {
      get
      {
        switch (this.VerticalAlignment)
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

    private void AssignWPFImage(System.Drawing.Image image)
    {
      if (this.m_wpfImage == null || image == null)
        return;
      this.m_wpfImage.Source = (ImageSource) this.ToBitmapImage(new Bitmap(this.Image));
    }

    private System.Windows.Media.Color GetMediaColor(System.Drawing.Color color)
    {
      return new System.Windows.Media.Color()
      {
        R = color.R,
        G = color.G,
        B = color.B,
        A = color.A
      };
    }

    private System.Windows.Media.Color GetMediaColor(System.Drawing.Color color, double opacity)
    {
      int num = (int) Math.Ceiling((double) byte.MaxValue * opacity);
      return new System.Windows.Media.Color()
      {
        R = color.R,
        G = color.G,
        B = color.B,
        A = (byte) num
      };
    }

    private Thickness Margin
    {
      get
      {
        return new Thickness((double) this.X, (double) this.Y, this.Width == 0 ? 0.0 : (double) (this.Scene.Width - this.Width - this.X), this.Height == 0 ? 0.0 : (double) (this.Scene.Height - this.Height - this.Y));
      }
    }

    private void UpdateWPFText()
    {
      if (this.m_wpfTextBlock == null)
        return;
      this.m_wpfTextBlock.Text = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(this.Text);
    }

    private void FrameworkElement_MouseUp(object sender, MouseButtonEventArgs e)
    {
      if (!this.m_clickStarted)
        return;
      if (this.Enabled)
      {
        this.RaiseHit();
        if (this.OnWPFHit != null)
          this.OnWPFHit((IActor) this);
      }
      this.m_clickStarted = false;
    }

    private void FrameworkElement_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
      this.m_clickStarted = false;
    }

    private void FrameworkElement_MouseDown(object sender, MouseButtonEventArgs e)
    {
      this.m_clickStarted = true;
    }

    private double ToWPFScale(double value) => value * this.WPFSizeConvertion;

    private BitmapImage ToBitmapImage(Bitmap bitmap)
    {
      using (MemoryStream memoryStream = new MemoryStream())
      {
        bitmap.Save((Stream) memoryStream, ImageFormat.Png);
        memoryStream.Position = 0L;
        BitmapImage bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = (Stream) memoryStream;
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.EndInit();
        return bitmapImage;
      }
    }

    protected void NotifyScene(Rectangle[] rectangles)
    {
      if (this.Scene == null)
        return;
      this.IsDirty = this.Visible;
      this.Scene.MakeDirty(rectangles);
    }

    protected virtual void OnRender(Graphics context)
    {
      if (!this.Visible)
      {
        this.IsDirty = false;
      }
      else
      {
        context.ResetTransform();
        if ((this.OptionFlags & RenderOptionFlags.DisableAntiAlias) == RenderOptionFlags.None)
          context.SmoothingMode = SmoothingMode.AntiAlias;
        bool hasValue1 = this.TextRotationAngle.HasValue;
        bool hasValue2 = this.TextTranslationPoint.HasValue;
        if ((this.OptionFlags & RenderOptionFlags.Background) != RenderOptionFlags.None || (this.OptionFlags & RenderOptionFlags.BackgroundGradient) != RenderOptionFlags.None)
        {
          System.Drawing.Color alphaRampedColor1 = Actor.GetAlphaRampedColor(this.BackgroundColor, this.Opacity);
          System.Drawing.Size? cornerSize = this.CornerSize;
          if (cornerSize.HasValue)
          {
            using (System.Drawing.Pen pen = new System.Drawing.Pen(alphaRampedColor1, this.StrokeWeight))
            {
              Graphics g = context;
              System.Drawing.Pen p = pen;
              double x = (double) this.X;
              double y = (double) this.Y;
              double width1 = (double) this.Width;
              double height = (double) this.Height;
              cornerSize = this.CornerSize;
              double width2 = (double) cornerSize.Value.Width;
              this.DrawRoundedRect(g, p, (float) x, (float) y, (float) width1, (float) height, (float) width2, true);
            }
          }
          else
          {
            System.Drawing.Brush brush = (System.Drawing.Brush) null;
            try
            {
              if ((this.OptionFlags & RenderOptionFlags.BackgroundGradient) != RenderOptionFlags.None)
              {
                System.Drawing.Color alphaRampedColor2 = Actor.GetAlphaRampedColor(this.GradientTargetColor ?? this.BackgroundColor, this.Opacity);
                brush = (System.Drawing.Brush) new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(this.X, this.Y, this.Width, this.Height), alphaRampedColor1, alphaRampedColor2, this.GradientAngle.GetValueOrDefault(180f));
              }
              else
                brush = (System.Drawing.Brush) new SolidBrush(alphaRampedColor1);
              context.FillRectangle(brush, this.X, this.Y, this.Width, this.Height);
            }
            finally
            {
              brush?.Dispose();
            }
          }
        }
        if ((this.OptionFlags & RenderOptionFlags.Image) != RenderOptionFlags.None && this.Image != null)
        {
          ImageAttributes imageAttr = new ImageAttributes();
          if (this.FrameCount > 1)
            this.Image.SelectActiveFrame(this.m_frameDimension, this.FrameNumber);
          else
            imageAttr.SetColorMatrix(new ColorMatrix()
            {
              Matrix33 = this.Opacity
            });
          if ((this.OptionFlags & RenderOptionFlags.Tile) != RenderOptionFlags.None)
            imageAttr.SetWrapMode(WrapMode.Tile);
          context.DrawImage(this.Image, new Rectangle(this.X, this.Y, this.Width, this.Height), 0, 0, this.Image.Width, this.Image.Height, GraphicsUnit.Pixel, imageAttr);
        }
        if ((this.OptionFlags & RenderOptionFlags.DrawBorder) != RenderOptionFlags.None)
        {
          using (System.Drawing.Pen pen = new System.Drawing.Pen(this.BorderColor ?? System.Drawing.Color.Red, this.StrokeWeight))
          {
            if (this.CornerSize.HasValue)
              this.DrawRoundedRect(context, pen, (float) this.X, (float) this.Y, (float) this.Width, (float) this.Height, (float) this.CornerSize.Value.Width, false);
            else
              context.DrawRectangle(pen, new Rectangle(this.X, this.Y, this.Width, this.Height));
          }
        }
        if ((this.OptionFlags & RenderOptionFlags.Text) != RenderOptionFlags.None && !string.IsNullOrEmpty(this.Text))
        {
          using (SolidBrush solidBrush1 = new SolidBrush(Actor.GetAlphaRampedColor(this.ForegroundColor, this.Opacity)))
          {
            StringFormat format;
            if ((this.OptionFlags & RenderOptionFlags.ElideText) != RenderOptionFlags.None)
            {
              format = (StringFormat) StringFormat.GenericDefault.Clone();
              format.Trimming |= StringTrimming.EllipsisCharacter;
            }
            else
              format = (StringFormat) StringFormat.GenericTypographic.Clone();
            format.Alignment = this.HorizontalAlignment;
            format.LineAlignment = this.VerticalAlignment;
            if ((this.OptionFlags & RenderOptionFlags.DisableAntiAlias) == RenderOptionFlags.None)
            {
              context.PixelOffsetMode = PixelOffsetMode.Half;
              context.TextRenderingHint = TextRenderingHint.AntiAlias;
            }
            if (this.TabStops != null && this.TabStops.Length != 0)
              format.SetTabStops(0.0f, this.TabStops);
            Rectangle textRegion = this.GetTextRegion(hasValue2);
            string s = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(this.Text);
            if (hasValue2)
              context.TranslateTransform((float) this.TextTranslationPoint.Value.X, (float) this.TextTranslationPoint.Value.Y);
            if (hasValue1)
              context.RotateTransform(this.TextRotationAngle.Value);
            if ((this.OptionFlags & RenderOptionFlags.TextShadow) != RenderOptionFlags.None)
            {
              using (SolidBrush solidBrush2 = new SolidBrush(Actor.GetAlphaRampedColor(System.Drawing.Color.Black, this.Opacity)))
                context.DrawString(s, this.Font, (System.Drawing.Brush) solidBrush2, (RectangleF) new Rectangle(textRegion.Left + 1, textRegion.Top + 1, textRegion.Width, textRegion.Height), format);
            }
            context.DrawString(s, this.Font, (System.Drawing.Brush) solidBrush1, (RectangleF) textRegion, format);
            if (hasValue2 | hasValue1)
              context.ResetTransform();
          }
        }
        if ((this.OptionFlags & RenderOptionFlags.HtmlToImage) != RenderOptionFlags.None && this.Image != null)
        {
          ImageAttributes imageAttr = new ImageAttributes();
          imageAttr.SetColorMatrix(new ColorMatrix()
          {
            Matrix33 = this.Opacity
          });
          context.DrawImage(this.Image, new Rectangle(this.X, this.Y, this.Width, this.Height), 0, 0, this.Image.Width, this.Image.Height, GraphicsUnit.Pixel, imageAttr);
        }
        if ((this.HitFlags & HitTestFlags.DrawHotSpot) != HitTestFlags.None)
          context.DrawRectangle(Pens.HotPink, this.GetHitTestRectangle());
        if (!string.IsNullOrEmpty(this.ErrorText))
        {
          using (Font font = new Font("Tahoma", 7f, System.Drawing.FontStyle.Bold))
          {
            int y = this.Y + this.Height + 2;
            if (Actor.m_drawErrorTop)
              y = this.Y - font.Height - 2;
            context.DrawString(this.ErrorText, font, System.Drawing.Brushes.Red, (float) this.X, (float) y);
          }
          Actor.m_drawErrorTop = !Actor.m_drawErrorTop;
        }
        this.StateFlags |= RenderStateFlags.Drawn;
        this.IsDirty = false;
      }
    }

    internal Rectangle GetTextRegion(bool forTranslation)
    {
      int x = forTranslation ? 0 : this.X;
      int y = forTranslation ? 0 : this.Y;
      int width = this.Width;
      int height = this.Height;
      Rectangle? textRegion = this.TextRegion;
      if (textRegion.HasValue)
      {
        if (!forTranslation)
        {
          int num1 = x;
          textRegion = this.TextRegion;
          int left = textRegion.Value.Left;
          x = num1 + left;
          int num2 = y;
          textRegion = this.TextRegion;
          int top = textRegion.Value.Top;
          y = num2 + top;
        }
        textRegion = this.TextRegion;
        Rectangle rectangle = textRegion.Value;
        width = rectangle.Width;
        textRegion = this.TextRegion;
        rectangle = textRegion.Value;
        height = rectangle.Height;
      }
      return new Rectangle(x, y, width, height);
    }

    internal Rectangle GetHitTestRectangle()
    {
      int x = this.X;
      int y = this.Y;
      int width = this.Width;
      int height = this.Height;
      if (this.HotSpot.HasValue)
      {
        x += this.HotSpot.Value.Left;
        int num = y;
        Rectangle rectangle = this.HotSpot.Value;
        int top = rectangle.Top;
        y = num + top;
        rectangle = this.HotSpot.Value;
        width = rectangle.Width;
        rectangle = this.HotSpot.Value;
        height = rectangle.Height;
      }
      return new Rectangle(x, y, width, height);
    }

    private void BrowserDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
    {
      Bitmap bitmap = new Bitmap(this.Width, this.Height);
      this.m_browserControl.DrawToBitmap(bitmap, new Rectangle(0, 0, this.Width, this.Height));
      this.Image = (System.Drawing.Image) bitmap;
    }

    private bool SetFieldValue<T>(ref T field, T value)
    {
      return this.SetFieldValue<T>(ref field, value, true, false);
    }

    bool SetFieldValue<T>(ref T field, T value, bool notifyScene, bool processStyle)
    {
        if ((field == null && value != null) || (field != null && !field.Equals(value)))
        {
            field = value;
            if (processStyle && this.Style != null)
            {
                IStyleSheetState state = this.Style.GetState(this.Enabled ? "enabled" : "disabled");
                if (state != null)
                {
                    foreach (PropertyInfo propertyInfo in typeof(Actor).GetProperties())
                    {
                        string text = propertyInfo.Name;
                        text = text.Substring(0, 1).ToLower() + text.Substring(1);
                        object obj = state[text];
                        if (obj != null)
                        {
                            try
                            {
                                if (text != "image")
                                {
                                    object obj2 = ConversionHelper.ChangeType(obj, propertyInfo.PropertyType);
                                    propertyInfo.SetValue(this, obj2, null);
                                }
                                else
                                {
                                    string text2 = obj as string;
                                    if (text2 != null && text2.StartsWith("cache:"))
                                    {
                                        IBitmapCacheService service = ServiceLocator.Instance.GetService<IBitmapCacheService>();
                                        this.Image = service.GetImage(text2.Substring(6));
                                    }
                                    else if (text2 != null)
                                    {
                                        IResourceBundleService service2 = ServiceLocator.Instance.GetService<IResourceBundleService>();
                                        this.Image = service2.GetBitmap(text2);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Instance.Log(string.Format("Unable to set actor property '{0}' with value '{1}'.", propertyInfo.Name, obj), ex);
                            }
                        }
                    }
                }
            }
            if (notifyScene && (this.Width > 0 || this.Height > 0))
            {
                this.NotifyScene(new Rectangle[]
                {
                    new Rectangle(this.X, this.Y, this.Width, this.Height)
                });
            }
            return true;
        }
        return false;
    }

        private static System.Drawing.Color GetAlphaRampedColor(System.Drawing.Color color, float opacity)
    {
      return System.Drawing.Color.FromArgb((int) Math.Ceiling((double) byte.MaxValue * (double) opacity), color);
    }

    private void DrawRoundedRect(
      Graphics g,
      System.Drawing.Pen p,
      float x,
      float y,
      float width,
      float height,
      float radius,
      bool fill)
    {
      using (GraphicsPath path = new GraphicsPath())
      {
        if ((double) radius * 2.0 > (double) height || (double) radius * 2.0 > (double) width)
          radius = (double) height > (double) width ? (float) (int) ((double) width / 2.0) : (float) (int) ((double) height / 2.0);
        float num = radius * 2f;
        float valueOrDefault = this.CornerSweepAngle.GetValueOrDefault(90f);
        path.AddArc(x + width - num, y, num, num, 270f, valueOrDefault);
        if ((double) y + (double) height - (double) radius < (double) y + (double) radius)
          path.AddLine(x + width, y + radius, x + width, y + height - radius);
        path.AddArc(x + width - num, y + height - num, num, num, 0.0f, valueOrDefault);
        if ((double) x + (double) width - (double) radius < (double) x + (double) radius)
          path.AddLine(x + width - radius, y + height, x + radius, y + height);
        path.AddArc(x, y + height - num, num, num, 90f, valueOrDefault);
        if ((double) y + (double) height - (double) radius < (double) y + (double) radius)
          path.AddLine(x, y + height - radius, x, y + radius);
        path.AddArc(x, y, num, num, 180f, valueOrDefault);
        if ((double) x + (double) width - (double) radius > (double) x + (double) radius)
          path.AddLine(x + radius, y, x + width - radius, y);
        path.CloseFigure();
        if (fill)
        {
          System.Drawing.Brush brush = (System.Drawing.Brush) null;
          try
          {
            if ((this.OptionFlags & RenderOptionFlags.BackgroundGradient) != RenderOptionFlags.None)
            {
              System.Drawing.Color alphaRampedColor = Actor.GetAlphaRampedColor(this.GradientTargetColor ?? this.BackgroundColor, this.Opacity);
              brush = (System.Drawing.Brush) new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle((int) x, (int) y, (int) width, (int) height), p.Color, alphaRampedColor, this.GradientAngle.GetValueOrDefault(180f));
            }
            else
              brush = (System.Drawing.Brush) new SolidBrush(p.Color);
            g.FillPath(brush, path);
          }
          finally
          {
            brush?.Dispose();
          }
        }
        else
          g.DrawPath(p, path);
      }
    }
  }
}
