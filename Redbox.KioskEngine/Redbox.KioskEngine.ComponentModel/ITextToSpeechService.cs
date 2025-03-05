namespace Redbox.KioskEngine.ComponentModel
{
  public interface ITextToSpeechService
  {
    void Reset();

    void RunSpeechWorkflow(string viewName);

    void TTSRepeatSequence();

    bool TTSEnabled { get; }

    bool AudioDeviceConnected { get; set; }

    void ClearAudioDeviceConnected();

    int GetTimeout(string timeoutType, int defaultTimeout);

    event AudioDeviceConnectionChanged OnAudioDeviceConnectionChanged;

    void ClearOnAudioDeviceConnectionChanged();
  }
}
