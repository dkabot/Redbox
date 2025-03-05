using System.Drawing;
using System.Windows.Forms.Integration;

namespace Redbox.KioskEngine.ComponentModel
{
  public interface IRenderingService
  {
    IScene CreateScene(
      string name,
      int width,
      int height,
      Color backgroundColor,
      ElementHost elementHost);

    IScene GetScene(string name);

    void RemoveScene(string name);

    bool IsBitmapReferenced(Image image);

    IScene ActiveScene { get; set; }

    Color BackgroundColor { get; set; }
  }
}
