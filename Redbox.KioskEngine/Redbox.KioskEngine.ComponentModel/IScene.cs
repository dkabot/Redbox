using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace Redbox.KioskEngine.ComponentModel
{
  public interface IScene : IDisposable
  {
    IActor CreateActor();

    IActor HitTest(int x, int y);

    void Clear();

    void ResumeDrawing();

    void SuspendDrawing();

    void Remove(IActor actor);

    void AddLast(IActor actor);

    void AddFirst(IActor actor);

    IActor Remove(string name);

    IActor GetActor(string name);

    IActor GetActorByNameAndTag(string name, string tag);

    List<IActor> GetActorByTag(string tag);

    IActor GetActorByTabOrder(int tabOrder);

    void Render(Graphics context, out bool changed);

    RenderType SceneRenderType { get; set; }

    void MakeDirty(Rectangle[] rectangles);

    int ActiveHitHandlers { get; }

    int Width { get; }

    int Height { get; }

    string Name { get; }

    Color BackgroundColor { get; set; }

    System.Drawing.Image BackgroundImage { get; set; }

    LinkedList<IActor> Actors { get; }

    Grid WPFGrid { get; }

    event WPFHitHandler OnWPFHit;

    event WPFKeyEvent OnWPFKeyDown;

    event WPFKeyEvent OnWPFKeyUp;

    void PrcoessWPFHit();

    void AddWPFGridChild(FrameworkElement frameworkElement);

    void RemoveWPFGridChild(FrameworkElement frameworkElement);

    FrameworkElement GetWPFGridChild(object tag);
  }
}
