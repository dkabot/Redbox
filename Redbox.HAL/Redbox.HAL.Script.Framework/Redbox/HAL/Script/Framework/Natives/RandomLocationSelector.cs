using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives
{
    internal sealed class RandomLocationSelector
    {
        private readonly List<ILocation> Locations = new List<ILocation>();

        internal RandomLocationSelector(IDecksService ds, IInventoryService iis)
        {
            var locationSelector = this;
            ds.ForAllDecksDo(_deck =>
            {
                for (var slot = 1; slot <= _deck.NumberOfSlots; ++slot)
                    locationSelector.Locations.Add(iis.Get(_deck.Number, slot));
                return true;
            });
        }

        internal ILocation SelectTargetLocation()
        {
            if (0 >= Locations.Count)
                return null;
            var index = new Random(DateTime.Now.Millisecond).Next(0, Locations.Count - 1);
            var location = Locations[index];
            Locations.RemoveAt(index);
            return location;
        }
    }
}