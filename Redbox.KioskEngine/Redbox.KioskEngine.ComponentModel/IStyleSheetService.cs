using System.Collections.ObjectModel;

namespace Redbox.KioskEngine.ComponentModel
{
  public interface IStyleSheetService
  {
    void Reset();

    IStyleSheet New(string name);

    IStyleSheet GetStyleSheet(string name);

    ReadOnlyCollection<IStyleSheet> StyleSheets { get; }
  }
}
