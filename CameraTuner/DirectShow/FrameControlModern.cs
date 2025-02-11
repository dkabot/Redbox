using System.IO;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Threading;

namespace Redbox.DirectShow
{
    internal sealed class FrameControlModern : AbstractDirectShowFrameControl
    {
        private readonly ManualResetEvent ImageCapturedEvent = new ManualResetEvent(false);
        private readonly AtomicFlag TriggerFlag = new AtomicFlag();

        internal FrameControlModern(string moniker, int grab, bool debug)
            : base(moniker, grab, debug)
        {
        }

        protected override bool SnapRequested => TriggerFlag.Clear();

        protected override bool OnSimulateTrigger(string file, int captureWait)
        {
            ImageCapturedEvent.Reset();
            if (!TriggerFlag.Set())
                return false;
            if (ImageCapturedEvent.WaitOne(captureWait))
                return File.Exists(file);
            LogHelper.Instance.Log("[{0}] The trigger event was not set.", ControlMoniker);
            return false;
        }

        protected override void OnImageCaptured()
        {
            ImageCapturedEvent.Set();
        }

        protected override void OnFree()
        {
            ImageCapturedEvent.Close();
        }
    }
}