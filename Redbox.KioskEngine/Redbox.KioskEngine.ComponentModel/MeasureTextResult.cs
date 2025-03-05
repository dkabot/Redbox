using System.Drawing;

namespace Redbox.KioskEngine.ComponentModel
{
  public class MeasureTextResult
  {
    public MeasureTextResult(SizeF size, int charactersFitted, int linesFilled)
    {
      this.Size = size;
      this.CharactersFitted = charactersFitted;
      this.LinesFilled = linesFilled;
    }

    public SizeF Size { get; private set; }

    public int CharactersFitted { get; private set; }

    public int LinesFilled { get; private set; }
  }
}
