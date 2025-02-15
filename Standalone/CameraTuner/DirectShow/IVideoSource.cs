namespace Redbox.DirectShow
{
    public interface IVideoSource
    {
        string Source { get; }

        int FramesReceived { get; }

        bool IsRunning { get; }
        bool Start();

        void SignalToStop();

        void WaitForStop();

        void Stop();

        event NewFrameEventHandler NewFrame;

        event VideoSourceErrorEventHandler VideoSourceError;

        event PlayingFinishedEventHandler PlayingFinished;
    }
}