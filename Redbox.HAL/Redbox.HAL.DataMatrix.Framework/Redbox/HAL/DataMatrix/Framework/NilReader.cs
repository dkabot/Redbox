using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework
{
    internal sealed class NilReader : AbstractBarcodeReader
    {
        internal NilReader()
            : base(BarcodeServices.None)
        {
        }

        protected override void OnScan(ScanResult result)
        {
            result.ResetOnException();
        }
    }
}