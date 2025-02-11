using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    public sealed class VendItemResult : IVendItemResult
    {
        internal VendItemResult()
        {
        }

        public ErrorCodes Status { get; internal set; }

        public bool Presented { get; internal set; }
    }
}