using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using System.Collections.Generic;

namespace Redbox.KioskEngine.Environment.TextToSpeech
{
  public class SpeechControl : ISpeechControl
  {
    private List<ISpeechPart> _speechParts = new List<ISpeechPart>();

    public string Name { get; set; }

    public List<ISpeechPart> SpeechParts => this._speechParts;
  }
}
