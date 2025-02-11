using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives
{
    [NativeJob(ProgramName = "lookup-barcodes")]
    internal sealed class LookupBarcodeJob : NativeJobAdapter
    {
        internal LookupBarcodeJob(ExecutionResult r, ExecutionContext c)
            : base(r, c)
        {
        }

        protected override void ExecuteInner()
        {
            var length = Context.PopTop<int>();
            var array = new string[length];
            for (var index = 0; index < length; ++index)
                array[index] = Context.PopTop<string>();
            var iis = ServiceLocator.Instance.GetService<IInventoryService>();
            Array.ForEach(array, id =>
            {
                var location = iis.Lookup(id);
                if (location == null)
                    Context.CreateInfoResult("BarcodeNotFound",
                        string.Format("The barcode {0} was not found in inventory", id), id);
                else
                    Context.CreateResult("BarcodeFound", string.Format("The barcode {0} was found in inventory", id),
                        location.Deck, location.Slot, id, new DateTime?(), null);
            });
        }
    }
}