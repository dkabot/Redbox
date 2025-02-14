using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using Redbox.KioskEngine.ComponentModel;
using Application = System.Windows.Forms.Application;
using Image = System.Drawing.Image;

namespace Redbox.KioskEngine.Drawing
{
    public class Scene : IScene, IDisposable
    {
        private readonly object m_syncObject = new object();
        private LinkedList<IActor> m_actors;
        private Color m_backgroundColor;
        private Image m_backgroundImage;
        private List<Rectangle> m_dirtyRectangles;
        private ElementHost m_elementHost;
        private bool m_isDrawingSuspended;

        public Scene(
            string name,
            int width,
            int height,
            Color backgroundColor,
            ElementHost elementHost)
        {
            Name = name;
            Width = width;
            Height = height;
            BackgroundColor = backgroundColor;
            Bounds = new Rectangle(0, 0, Width, Height);
            InitializeWPFElementHost(elementHost);
        }

        internal Bitmap BackBuffer { get; set; }

        internal Rectangle Bounds { get; set; }

        internal bool IsDirty { get; set; }

        internal RenderType LastSceneRenderType { get; set; }

        public List<Rectangle> DirtyRectangles
        {
            get
            {
                if (m_dirtyRectangles == null)
                    m_dirtyRectangles = new List<Rectangle>();
                return m_dirtyRectangles;
            }
        }

        public void Clear()
        {
            lock (m_syncObject)
            {
                foreach (var actor in Actors)
                {
                    actor.Scene = null;
                    actor.ClearHit();
                }

                Actors.Clear();
                if (SceneRenderType == RenderType.WPF)
                    WPFGrid.Children.Clear();
                MakeDirty(null);
            }

            BackgroundImage = null;
        }

        public event WPFHitHandler OnWPFHit;

        public event WPFKeyEvent OnWPFKeyDown;

        public event WPFKeyEvent OnWPFKeyUp;

        public int ActiveHitHandlers
        {
            get
            {
                var activeHitHandlers = 0;
                for (var linkedListNode = Actors.Last; linkedListNode != null; linkedListNode = linkedListNode.Previous)
                {
                    var actor = linkedListNode.Value;
                    if ((actor.HitFlags & HitTestFlags.Enabled) != HitTestFlags.None && actor.Visible &&
                        actor.Enabled && actor.HitExists)
                        ++activeHitHandlers;
                }

                return activeHitHandlers;
            }
        }

        public IActor HitTest(int x, int y)
        {
            for (var linkedListNode = Actors.Last; linkedListNode != null; linkedListNode = linkedListNode.Previous)
            {
                var actor = linkedListNode.Value;
                if ((actor.HitFlags & HitTestFlags.Enabled) != HitTestFlags.None && actor.Visible && actor.Enabled &&
                    ((Actor)actor).GetHitTestRectangle().IntersectsWith(new Rectangle(x, y, 2, 2)))
                {
                    ((Actor)actor).RaiseHit();
                    return actor;
                }

                if ((actor.HitFlags & HitTestFlags.Obstruction) != HitTestFlags.None)
                    break;
            }

            return null;
        }

        public IActor CreateActor()
        {
            var actor = new Actor();
            actor.OnWPFHit += OnWPFActorHit;
            return actor;
        }

        public void PrcoessWPFHit()
        {
            var onWpfHit = OnWPFHit;
            if (onWpfHit == null)
                return;
            onWpfHit(null);
        }

        public void Dispose()
        {
            foreach (IDisposable actor in Actors)
                actor.Dispose();
            if (BackBuffer == null)
                return;
            BackBuffer.Dispose();
        }

        public RenderType SceneRenderType { get; set; }

        public void Render(Graphics context, out bool changed)
        {
            changed = false;
            if (!IsDirty)
                return;
            if (m_isDrawingSuspended)
                return;
            try
            {
                m_elementHost.Visible = SceneRenderType == RenderType.WPF;
                if (SceneRenderType != RenderType.GDI)
                    return;
                DrawBackground(context, Bounds);
                for (var linkedListNode = Actors.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
                    linkedListNode.Value.Render(context);
                if (LastSceneRenderType != RenderType.WPF)
                    return;
                Application.OpenForms["HostForm"]?.Focus();
            }
            finally
            {
                LastSceneRenderType = SceneRenderType;
                changed = true;
                IsDirty = false;
            }
        }

        public void ResumeDrawing()
        {
            m_isDrawingSuspended = false;
        }

        public void SuspendDrawing()
        {
            m_isDrawingSuspended = true;
            DirtyRectangles.Clear();
            IsDirty = true;
        }

        public IActor Remove(string name)
        {
            lock (m_syncObject)
            {
                var actor1 = (IActor)null;
                foreach (var actor2 in Actors)
                    if (string.Compare(actor2.Name, name, true) == 0)
                    {
                        actor1 = actor2;
                        break;
                    }

                if (actor1 == null)
                    return null;
                actor1.Scene = null;
                Actors.Remove(actor1);
                MakeDirty(new Rectangle[1] { actor1.Bounds });
                return actor1;
            }
        }

        public void Remove(IActor actor)
        {
            lock (m_syncObject)
            {
                actor.Scene = null;
                Actors.Remove(actor);
                MakeDirty(new Rectangle[1] { actor.Bounds });
            }
        }

        public void AddWPFGridChild(FrameworkElement frameworkElement)
        {
            if (frameworkElement == null)
                return;
            WPFGrid.Children.Add(frameworkElement);
        }

        public void RemoveWPFGridChild(FrameworkElement frameworkElement)
        {
            if (frameworkElement == null)
                return;
            WPFGrid.Children.Remove(frameworkElement);
        }

        public FrameworkElement GetWPFGridChild(object tag)
        {
            var result = (FrameworkElement)null;
            if (WPFGrid != null)
                WPFGrid.Dispatcher.Invoke(() =>
                {
                    foreach (var child in WPFGrid.Children)
                        if (child is FrameworkElement frameworkElement2 && frameworkElement2.Tag != null &&
                            frameworkElement2.Tag.Equals(tag))
                            result = frameworkElement2;
                });
            return result;
        }

        public void AddLast(IActor actor)
        {
            lock (m_syncObject)
            {
                actor.Scene = this;
                Actors.AddLast(actor);
                switch (SceneRenderType)
                {
                    case RenderType.GDI:
                        MakeDirty(new Rectangle[1] { actor.Bounds });
                        break;
                    case RenderType.WPF:
                        WPFGrid.Children.Add(actor.WPFFrameworkElement);
                        break;
                }
            }
        }

        public void AddFirst(IActor actor)
        {
            lock (m_syncObject)
            {
                actor.Scene = this;
                Actors.AddFirst(actor);
                MakeDirty(new Rectangle[1] { actor.Bounds });
            }
        }

        public IActor GetActor(string name)
        {
            lock (m_syncObject)
            {
                foreach (var actor in Actors)
                    if (string.Compare(actor.Name, name, true) == 0)
                        return actor;
                return null;
            }
        }

        public IActor GetActorByNameAndTag(string name, string tag)
        {
            var actorByNameAndTag = (IActor)null;
            lock (m_syncObject)
            {
                foreach (var actor in Actors)
                    if (actor.Name == name && actor.Tag as string == tag)
                    {
                        actorByNameAndTag = actor;
                        break;
                    }
            }

            return actorByNameAndTag;
        }

        public List<IActor> GetActorByTag(string tag)
        {
            var actorByTag = new List<IActor>();
            lock (m_syncObject)
            {
                foreach (var actor in Actors)
                    if (actor.Tag != null && !string.IsNullOrEmpty(actor.Tag.ToString()))
                        if (new List<string>(actor.Tag.ToString().Split(new char[1]
                            {
                                ','
                            }, StringSplitOptions.RemoveEmptyEntries)).Contains(tag))
                            actorByTag.Add(actor);
            }

            return actorByTag;
        }

        public IActor GetActorByTabOrder(int tabOrder)
        {
            var actorByTabOrder = (IActor)null;
            lock (m_syncObject)
            {
                foreach (var actor in Actors)
                {
                    var tabOrder1 = actor.TabOrder;
                    if (tabOrder1.HasValue)
                    {
                        tabOrder1 = actor.TabOrder;
                        var num = tabOrder;
                        if ((tabOrder1.GetValueOrDefault() == num) & tabOrder1.HasValue)
                        {
                            actorByTabOrder = actor;
                            break;
                        }
                    }
                }
            }

            return actorByTabOrder;
        }

        public void MakeDirty(Rectangle[] rectangles)
        {
            if (m_isDrawingSuspended)
                return;
            IsDirty = true;
            if (rectangles == null || rectangles.Length == 0)
                return;
            lock (m_syncObject)
            {
                foreach (var rectangle in rectangles)
                    if (!DirtyRectangles.Contains(rectangle))
                        DirtyRectangles.Add(rectangle);
            }
        }

        public int Width { get; }

        public int Height { get; }

        public string Name { get; }

        public Color BackgroundColor
        {
            get => m_backgroundColor;
            set
            {
                if (m_backgroundColor == value)
                    return;
                m_backgroundColor = value;
                MakeDirty(null);
            }
        }

        public Image BackgroundImage
        {
            get => m_backgroundImage;
            set
            {
                m_backgroundImage = value;
                MakeDirty(null);
            }
        }

        public Grid WPFGrid { get; private set; }

        public LinkedList<IActor> Actors
        {
            get
            {
                if (m_actors == null)
                    m_actors = new LinkedList<IActor>();
                return m_actors;
            }
        }

        internal void DrawBackground(Graphics context, Rectangle rect)
        {
            if (BackgroundImage != null)
                context.DrawImage(BackgroundImage, rect, rect.Left, rect.Top, rect.Width, rect.Height,
                    GraphicsUnit.Pixel);
            else
                using (var solidBrush = new SolidBrush(BackgroundColor))
                {
                    context.FillRectangle(solidBrush, rect.Left, rect.Top, rect.Width, rect.Height);
                }
        }

        internal List<IActor> GetDirtyActors()
        {
            var dirtyActors = new List<IActor>();
            foreach (var actor in Actors)
                if (actor.IsDirty)
                    dirtyActors.Add(actor);
            return dirtyActors;
        }

        private void InitializeWPFElementHost(ElementHost elementHost)
        {
            m_elementHost = elementHost;
            var userControl = new UserControl();
            WPFGrid = new Grid();
            WPFGrid.Focusable = true;
            WPFGrid.KeyDown += WPFGrid_KeyDown;
            WPFGrid.KeyUp += WPFGrid_KeyUp;
            userControl.Content = WPFGrid;
            m_elementHost.Child = userControl;
        }

        private void WPFGrid_KeyUp(object sender, KeyEventArgs e)
        {
            if (OnWPFKeyUp == null)
                return;
            OnWPFKeyUp(sender, e);
        }

        private void WPFGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (OnWPFKeyDown == null)
                return;
            OnWPFKeyDown(sender, e);
        }

        private void OnWPFActorHit(IActor actor)
        {
            if (OnWPFHit == null)
                return;
            OnWPFHit(actor);
        }
    }
}