using System.Threading;
using Redbox.HAL.Component.Model;

namespace Redbox.DirectShow;

internal sealed class FrameControlLegacy : AbstractDirectShowFrameControl
{
    private readonly ManualResetEvent FrameReceivedEvent = new(false);
    private readonly ManualResetEvent ImageCapturedEvent = new(false);
    private int m_grabImage = ControlConstants.NotTriggered;
    private int m_simulateTrigger = ControlConstants.NotTriggered;

    internal FrameControlLegacy(string moniker, int grab, bool debug)
        : base(moniker, grab, debug)
    {
    }

    protected override bool SnapRequested
    {
        get
        {
            var num = Interlocked.CompareExchange(ref m_grabImage, ControlConstants.NotTriggered,
                ControlConstants.Triggered);
            return ControlConstants.Triggered == num;
        }
    }

    protected override bool OnSimulateTrigger(string file, int captureWait)
    {
        ImageCapturedEvent.Reset();
        FrameReceivedEvent.Reset();
        if (Interlocked.Exchange(ref m_simulateTrigger, ControlConstants.Triggered) == ControlConstants.Triggered)
            return false;
        var flag = ImageCapturedEvent.WaitOne(captureWait);
        if (!flag)
            LogHelper.Instance.Log("[{0}] The trigger event was not set.", ControlMoniker);
        return flag;
    }

    protected override void OnImageCaptured()
    {
        FrameReceivedEvent.Set();
    }

    protected override bool OnGraphEvent()
    {
        var flag = true;
        if (Thread.VolatileRead(ref m_simulateTrigger) == ControlConstants.Triggered)
        {
            Interlocked.Exchange(ref m_grabImage, ControlConstants.Triggered);
            if (!FrameReceivedEvent.WaitOne(GrabWaitTime))
            {
                Interlocked.Exchange(ref m_grabImage, ControlConstants.NotTriggered);
                LogHelper.Instance.Log("[{0}] There are no captured frames from the device.", ControlMoniker);
                flag = false;
            }
            else
            {
                ImageCapturedEvent.Set();
            }

            Interlocked.Exchange(ref m_simulateTrigger, ControlConstants.NotTriggered);
        }

        return flag;
    }

    protected override void OnFree()
    {
        FrameReceivedEvent.Close();
        ImageCapturedEvent.Close();
    }
}