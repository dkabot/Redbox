using System;

namespace Redbox.HAL.Component.Model
{
    public static class InventoryConstants
    {
        public const string Empty = "EMPTY";
        public const string Unknown = "UNKNOWN";
        public const string Duplicate = "(DUPLICATE)";
        public const string Backwards = "redbox";

        public static bool CodeIsUnknown(string s)
        {
            return "UNKNOWN".Equals(s, StringComparison.CurrentCultureIgnoreCase) ||
                   "redbox".Equals(s, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool IsKnownInventoryToken(string id)
        {
            return string.Compare(id, "UNKNOWN", true) == 0 || string.Compare(id, "redbox", true) == 0 ||
                   string.Compare(id, "EMPTY", true) == 0;
        }
    }
}