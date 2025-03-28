using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using System;

namespace Redbox.KioskEngine.Environment.TextToSpeech
{
  public class MapKeyPressToAction : SpeechPartEvent, IMapKeyPressToAction, ISpeechPartEvent
  {
    public Action Action { get; set; }

    public Func<string> ActionText { get; set; }
  }
}
