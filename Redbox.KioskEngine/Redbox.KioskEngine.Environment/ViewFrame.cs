using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.REDS.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Xml;

namespace Redbox.KioskEngine.Environment
{
  internal sealed class ViewFrame : BaseViewFrame, IViewFrame, IBaseViewFrame, IDisposable
  {
    private readonly IDictionary<string, ViewFrame.ActorProperties> m_actors = (IDictionary<string, ViewFrame.ActorProperties>) new Dictionary<string, ViewFrame.ActorProperties>();

    public override void UpdateScene(IViewFrameInstance viewFrameInstance)
    {
      try
      {
        this.Scene.SuspendDrawing();
        this.Scene.SceneRenderType = this.ViewRenderType;
        if (this.Clear)
        {
          this.Scene.Clear();
          this.Scene.BackgroundColor = this.BackgroundColor;
          this.Scene.BackgroundImage = this.BackgroundImage;
        }
        foreach (string key in (IEnumerable<string>) this.m_actors.Keys)
        {
          ViewFrame.ActorProperties actor1 = this.m_actors[key];
          IActor actor = this.Scene.CreateActor();
          actor.Name = key;
          actor.Tag = (object) actor1.Tag;
          actor.RelativeToActorName = actor1.RelativeToActor;
          actor.X = actor1.X;
          actor.Y = actor1.Y;
          actor.Font = actor1.Font;
          actor.Text = actor1.Text;
          actor.Style = actor1.Style;
          actor.Width = actor1.Width;
          actor.Height = actor1.Height;
          actor.Opacity = actor1.Opacity;
          actor.Enabled = actor1.Enabled;
          actor.Visible = actor1.Visible;
          actor.TabOrder = actor1.TabOrder;
          actor.HotSpot = actor1.HotSpot;
          actor.HitFlags = actor1.HitFlags;
          actor.StyleName = actor1.StyleName;
          actor.ErrorText = actor1.ErrorText;
          actor.TextRegion = actor1.TextRegion;
          actor.CornerSize = actor1.CornerSize;
          actor.BorderColor = actor1.BorderColor;
          actor.OptionFlags = actor1.OptionFlags;
          actor.StrokeWeight = actor1.StrokeWeight;
          actor.GradientAngle = actor1.GradientAngle;
          actor.BackgroundColor = actor1.BackgroundColor;
          actor.ForegroundColor = actor1.ForegroundColor;
          actor.CornerSweepAngle = actor1.CornerSweepAngle;
          actor.VerticalAlignment = actor1.VerticalAlignment;
          actor.TextRotationAngle = actor1.TextRotationAngle;
          actor.GradientTargetColor = actor1.GradientTargetColor;
          actor.HorizontalAlignment = actor1.HorizontalAlignment;
          actor.TextTranslationPoint = actor1.TextTranslationPoint;
          actor.WPFControlName = actor1.WPFControlName;
          actor.WPFControlAssemblyName = actor1.WPFControlAssemblyName;
          string onHitScriptName = actor1.OnHit;
          actor.Hit += (EventHandler) ((o, args) =>
          {
            try
            {
              if (string.IsNullOrEmpty(onHitScriptName))
                return;
              ViewService.Instance.LastHitActorTag = actor.Tag;
              ViewService.Instance.LastHitActorName = actor.Name;
              ServiceLocator.Instance.GetService<IResourceBundleService>().ExecuteScript(onHitScriptName);
            }
            catch (Exception ex)
            {
              LogHelper.Instance.Log("An unhandled exception was raised in ViewFrame, actor.Hit delegate.", ex);
              LogHelper.Instance.Log("View name = {0}, actor name = {1}", (object) this.ViewName, (object) actor.Name);
            }
          });
          actor.Image = actor1.Image;
          this.Scene.AddLast(actor);
        }
        this.Scene.WPFGrid?.Focus();
        this.Scene.ResumeDrawing();
      }
      catch (Exception ex)
      {
        LogHelper.Instance.Log("An unhandled exception was raised in ViewFrame.UpdateScene.", ex);
      }
    }

    public override bool RaiseOnEnter(IViewFrameInstance viewFrameInstance)
    {
      if (string.IsNullOrEmpty(this.OnEnterResourceName))
        return false;
      LogHelper.Instance.Log("Executing on enter script ({0}) for view named: {1}", (object) this.OnEnterResourceName, (object) this.ViewName);
      ServiceLocator.Instance.GetService<IResourceBundleService>().ExecuteScript(this.OnEnterResourceName);
      return true;
    }

    public override void RaiseOnLeave(IViewFrameInstance viewFrameInstance)
    {
      if (string.IsNullOrEmpty(this.OnLeaveResourceName))
      {
        LogHelper.Instance.Log("No on leave script specified for view named: {0}", (object) this.ViewName);
      }
      else
      {
        LogHelper.Instance.Log("Executing on leave script ({0}) for view named: {1}", (object) this.OnLeaveResourceName, (object) this.ViewName);
        ServiceLocator.Instance.GetService<IResourceBundleService>().ExecuteScript(this.OnLeaveResourceName);
      }
    }

    public void Parse()
    {
      this.Errors.Clear();
      IResourceBundleService service1 = ServiceLocator.Instance.GetService<IResourceBundleService>();
      IStyleSheetService service2 = ServiceLocator.Instance.GetService<IStyleSheetService>();
      IRenderingService service3 = ServiceLocator.Instance.GetService<IRenderingService>();
      IResource viewResource = this.GetViewResource();
      this.ViewVersion = viewResource["version"] as string;
      this.ActiveFlag = viewResource["active_flag"] as string;
      XmlNode childNode = this.GetViewGraph().ChildNodes[0];
      string attributeValue1 = childNode.GetAttributeValue<string>("render_type", "GDI");
      if (!string.IsNullOrEmpty(attributeValue1))
      {
        try
        {
          this.ViewRenderType = (RenderType) Enum.Parse(typeof (RenderType), attributeValue1);
        }
        catch
        {
        }
      }
      this.Clear = childNode.GetAttributeValue<bool>("clear", true);
      this.Width = childNode.GetAttributeValue<int>("width");
      this.Height = childNode.GetAttributeValue<int>("height");
      this.Cached = childNode.GetAttributeValue<bool>("cached");
      this.SceneName = childNode.GetAttributeValue<string>("name", "Default");
      string attributeValue2 = childNode.GetAttributeValue<string>("stylesheet");
      if (!string.IsNullOrEmpty(attributeValue2))
        this.StyleSheet = service2.New(attributeValue2);
      this.ViewWindow = childNode.GetAttributeValue<Rectangle?>("viewWindow");
      this.OnEnterResourceName = childNode.GetAttributeValue<string>("onEnter");
      this.OnLeaveResourceName = childNode.GetAttributeValue<string>("onLeave");
      string attributeValue3 = childNode.GetAttributeValue<string>("backgroundImage");
      if (!string.IsNullOrEmpty(attributeValue3))
      {
        string resourceName = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(attributeValue3);
        Image bitmap = service1.GetBitmap(resourceName);
        if (bitmap != null)
          this.BackgroundImage = bitmap;
        else
          this.Errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("V002", string.Format("backgroundImage: Bitmap resource '{0}' not found.", (object) resourceName), "The resource name specified in the view attribute backgroundImage was not found."));
      }
      this.BackgroundColor = childNode.GetAttributeValue<Color>("backgroundColor", Color.FromArgb(153, 153, 152));
      XmlNodeList xmlNodeList = childNode.SelectNodes("actor");
      if (xmlNodeList == null)
        return;
      this.Scene = service3.GetScene(this.SceneName) ?? service3.GetScene("Default");
      foreach (XmlNode node in xmlNodeList)
      {
        string attributeValue4 = node.GetAttributeValue<string>("name");
        ViewFrame.ActorProperties actorProperties1 = this.m_actors.ContainsKey(attributeValue4) ? this.m_actors[attributeValue4] : new ViewFrame.ActorProperties();
        actorProperties1.Enabled = node.GetAttributeValue<bool>("enabled", true);
        IStyleSheetState styleSheetState = (IStyleSheetState) null;
        if (this.StyleSheet != null)
        {
          string attributeValue5 = node.GetAttributeValue<string>("style");
          if (!string.IsNullOrEmpty(attributeValue5))
          {
            actorProperties1.StyleName = attributeValue5;
            IStyleSheetStyle style = this.StyleSheet.GetStyle(attributeValue5);
            if (style != null)
            {
              actorProperties1.Style = style;
              styleSheetState = style.GetState(actorProperties1.Enabled ? "enabled" : "disabled");
            }
          }
        }
        foreach (FieldInfo field in typeof (ViewFrame.ActorProperties).GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
          ActorPropertyTagAttribute customAttribute = (ActorPropertyTagAttribute) Attribute.GetCustomAttribute((MemberInfo) field, typeof (ActorPropertyTagAttribute));
          if (customAttribute != null)
          {
            object attributeValue6 = node.GetAttributeValue<object>(customAttribute.Name);
            if (attributeValue6 == null)
            {
              if (styleSheetState != null)
                attributeValue6 = styleSheetState[customAttribute.Name];
              else
                continue;
            }
            if (attributeValue6 != null)
            {
              try
              {
                object obj = ConversionHelper.ChangeType(attributeValue6, field.FieldType);
                field.SetValue((object) actorProperties1, obj);
              }
              catch (Exception ex)
              {
                LogHelper.Instance.Log(string.Format("Unable to set actor property '{0}' with value '{1}'.", (object) customAttribute.Name, attributeValue6), ex);
              }
            }
          }
        }
        string attributeValue7 = node.GetAttributeValue<string>("font");
        if (string.IsNullOrEmpty(attributeValue7) && styleSheetState != null)
          attributeValue7 = styleSheetState["font"] as string;
        bool resourceFound;
        actorProperties1.Font = service1.GetFont(attributeValue7, out resourceFound);
        if (!string.IsNullOrEmpty(attributeValue7) && !resourceFound)
          this.Errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("V003", string.Format("{0}.font: Font resource '{1}' not found.", (object) attributeValue4, (object) attributeValue7), string.Format(" Actor {0} is referencing a font resource named '{1}' that was not found.", (object) attributeValue4, (object) attributeValue7)));
        string attributeValue8 = node.GetAttributeValue<string>("image");
        if (string.IsNullOrEmpty(attributeValue8) && styleSheetState != null)
          attributeValue8 = styleSheetState["image"] as string;
        if (!string.IsNullOrEmpty(attributeValue8))
        {
          string resourceName = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(attributeValue8);
          Image bitmap = service1.GetBitmap(resourceName);
          if (bitmap != null)
          {
            actorProperties1.Image = bitmap;
          }
          else
          {
            actorProperties1.BackgroundColor = Color.DarkRed;
            actorProperties1.Width = 32;
            actorProperties1.Height = 32;
            actorProperties1.OptionFlags |= RenderOptionFlags.DrawBorder;
            actorProperties1.ErrorText = string.Format("Bitmap resource '{0}' not found.", (object) resourceName);
            this.Errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("V004", string.Format("{0}.image: Bitmap resource '{1}' not found.", (object) attributeValue4, (object) resourceName), string.Format("Actor {0} is referencing a bitmap resource named '{1}' that was not found.", (object) attributeValue4, (object) resourceName)));
          }
        }
        if (this.ViewWindow.HasValue)
        {
          ViewFrame.ActorProperties actorProperties2 = actorProperties1;
          int x1 = actorProperties2.X;
          Rectangle rectangle = this.ViewWindow.Value;
          int x2 = rectangle.X;
          actorProperties2.X = x1 + x2;
          ViewFrame.ActorProperties actorProperties3 = actorProperties1;
          int y1 = actorProperties3.Y;
          rectangle = this.ViewWindow.Value;
          int y2 = rectangle.Y;
          actorProperties3.Y = y1 + y2;
        }
        this.m_actors[attributeValue4] = actorProperties1;
      }
    }

    public void Dispose()
    {
    }

    public int Width { get; internal set; }

    public int Height { get; internal set; }

    public bool Cached { get; internal set; }

    public string ViewVersion { get; internal set; }

    public RenderType ViewRenderType { get; internal set; }

    public string SceneName { get; internal set; }

    public Rectangle? ViewWindow { get; internal set; }

    public Color BackgroundColor { get; internal set; }

    public Image BackgroundImage { get; internal set; }

    public IStyleSheet StyleSheet { get; internal set; }

    public string OnEnterResourceName { get; internal set; }

    public string OnLeaveResourceName { get; internal set; }

    private XmlNode GetViewGraph()
    {
      return (XmlNode) this.GetViewResource().GetAspect("content").GetContent();
    }

    private IResource GetViewResource()
    {
      IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
      if (service == null)
        return (IResource) null;
      return service.GetResource(this.ViewName) ?? throw new ArgumentException(string.Format("View resource {0} was not found.", (object) this.ViewName));
    }

    private sealed class ActorProperties
    {
      [ActorPropertyTag(Name = "x")]
      public int X;
      [ActorPropertyTag(Name = "y")]
      public int Y;
      public Font Font;
      [ActorPropertyTag(Name = "width")]
      public int Width;
      [ActorPropertyTag(Name = "height")]
      public int Height;
      [ActorPropertyTag(Name = "tag")]
      public string Tag;
      public Image Image;
      [ActorPropertyTag(Name = "text")]
      public string Text;
      [ActorPropertyTag(Name = "onHit")]
      public string OnHit;
      public bool Enabled = true;
      [ActorPropertyTag(Name = "visible")]
      public bool Visible = true;
      [ActorPropertyTag(Name = "tabOrder")]
      public int? TabOrder;
      [ActorPropertyTag(Name = "opacity")]
      public float Opacity = 1f;
      public string StyleName;
      public IStyleSheetStyle Style;
      public string ErrorText;
      [ActorPropertyTag(Name = "cornerSize")]
      public Size? CornerSize;
      [ActorPropertyTag(Name = "strokeWeight")]
      public float StrokeWeight = 1f;
      [ActorPropertyTag(Name = "hotSpot")]
      public Rectangle? HotSpot;
      [ActorPropertyTag(Name = "borderColor")]
      public Color? BorderColor;
      [ActorPropertyTag(Name = "gradientAngle")]
      public float? GradientAngle;
      [ActorPropertyTag(Name = "hitFlags")]
      public HitTestFlags HitFlags;
      [ActorPropertyTag(Name = "textRegion")]
      public Rectangle? TextRegion;
      [ActorPropertyTag(Name = "backgroundColor")]
      public Color BackgroundColor;
      [ActorPropertyTag(Name = "foregroundColor")]
      public Color ForegroundColor;
      [ActorPropertyTag(Name = "relativeToActor")]
      public string RelativeToActor;
      [ActorPropertyTag(Name = "cornerSweepAngle")]
      public float? CornerSweepAngle;
      [ActorPropertyTag(Name = "textRotationAngle")]
      public float? TextRotationAngle;
      [ActorPropertyTag(Name = "gradientTargetColor")]
      public Color? GradientTargetColor;
      [ActorPropertyTag(Name = "textTranslationPoint")]
      public Point? TextTranslationPoint;
      [ActorPropertyTag(Name = "optionFlags")]
      public RenderOptionFlags OptionFlags;
      [ActorPropertyTag(Name = "verticalAlignment")]
      public StringAlignment VerticalAlignment;
      [ActorPropertyTag(Name = "horizontalAlignment")]
      public StringAlignment HorizontalAlignment;
      [ActorPropertyTag(Name = "WPFControlName")]
      public string WPFControlName;
      [ActorPropertyTag(Name = "WPFControlAssemblyName")]
      public string WPFControlAssemblyName;
    }
  }
}
