namespace Redbox.KioskEngine.ComponentModel
{
    public interface ITextToSpeechService
    {
        bool TTSEnabled { get; }

        bool AudioDeviceConnected { get; set; }
        void Reset();

        void RunSpeechWorkflow(string viewName);

        void TTSRepeatSequence();

        void ClearAudioDeviceConnected();

        int GetTimeout(string timeoutType, int defaultTimeout);

        event AudioDeviceConnectionChanged OnAudioDeviceConnectionChanged;

        void ClearOnAudioDeviceConnectionChanged();
    }
}