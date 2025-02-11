using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework
{
    internal sealed class FetchThinGetObserver : IGetObserver
    {
        public void OnStuck(IGetResult result)
        {
            ServiceLocator.Instance.GetService<IInventoryService>().UpdateEmptyStuck(result.Location);
        }

        public bool OnEmpty(IGetResult result)
        {
            var service = ServiceLocator.Instance.GetService<IInventoryService>();
            result.Update(ErrorCodes.ItemStuck);
            var location = result.Location;
            service.UpdateEmptyStuck(location);
            return false;
        }
    }
}