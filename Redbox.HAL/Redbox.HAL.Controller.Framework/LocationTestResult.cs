using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class LocationTestResult : IPeekResult
    {
        internal LocationTestResult(int deck, int slot)
        {
            Error = ErrorCodes.Success;
            PeekLocation = ServiceLocator.Instance.GetService<IInventoryService>().Get(deck, slot);
        }

        public bool TestOk => Error == ErrorCodes.Success;

        public bool IsFull { get; internal set; }

        public ErrorCodes Error { get; internal set; }

        public ILocation PeekLocation { get; }
    }
}